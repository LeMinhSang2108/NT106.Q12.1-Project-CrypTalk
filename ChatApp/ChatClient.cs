using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Net;

public class ChatClient
{
    private TcpClient client;
    private NetworkStream stream;
    private string username;
    private RSA rsa;
    private string privateKey;
    private string publicKey;
    private Dictionary<string, string> peerPublicKeys = new Dictionary<string, string>();
    private bool running = false;
    public bool IsConnected => client != null && client.Connected && running;
    private Dictionary<int, GroupInfo> availableGroups = new Dictionary<int, GroupInfo>();

    public event Action<string, string, ContentType, string> OnMessageReceived;
    public event Action<List<string>> OnUserListUpdated;
    public event Action<string> OnVideoCallRequestReceived;
    public event Action<List<GroupInfo>> OnGroupListUpdated;
    public event Action<int, string> OnGroupCreated;
    public event Action<string, string> OnMessageLiked;
    public event Action<string, string> OnMessageStatusUpdated;
    public event Action<int, string, string, ContentType, string> OnGroupMessageReceived;
    public event Action<int, string, bool> OnTypingStatusReceived;
    public event Action<string> OnVideoCallAccepted;



    public void Connect(string host, int port, string username)
    {
        this.username = username;
        client = new TcpClient(host, port);
        stream = client.GetStream();

        rsa = RSA.Create(2048);
        privateKey = rsa.ToXmlString(true);
        publicKey = rsa.ToXmlString(false);

        var regMsg = new Message
        {
            Type = MessageType.RegisterPublicKey,
            Sender = username,
            Data = Encoding.UTF8.GetBytes(publicKey)
        };
        SendMessagePacket(regMsg);

        running = true;
        Task.Run(() => ReceiveMessages());
    }


    public void SendStatusUpdate(string receiver, string messageId, string status)
    {
        var msg = new Message
        {
            Type = MessageType.StatusUpdate,
            Sender = username,
            Receiver = receiver,
            Data = Encoding.UTF8.GetBytes($"{status}|{messageId}"),
            ContentType = ContentType.Text
        };
        SendMessagePacket(msg);
    }
    private void ReceiveMessages()
    {
        try
        {
            while (running)
            {
                byte[] lengthBuffer = new byte[4];
                int headerBytesRead = 0;
                while (headerBytesRead < 4)
                {
                    int read = stream.Read(lengthBuffer, headerBytesRead, 4 - headerBytesRead);
                    if (read == 0) return;
                    headerBytesRead += read;
                }
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] buffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(buffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) return;
                    totalBytesRead += read;
                }

                var msg = Message.Deserialize(buffer);

                switch (msg.Type)
                {



                    case MessageType.PublicKeyBroadcast:
                        lock (peerPublicKeys)
                        {
                            peerPublicKeys[msg.Sender] = Encoding.UTF8.GetString(msg.Data);
                        }
                        break;

                    case MessageType.EncryptedMessage:
                    case MessageType.EncryptedBroadcast:
                        DecryptAndDisplayMessage(msg);
                        SendStatusUpdate(msg.Sender, msg.MessageID, "DELIVERED");
                        break;

                    case MessageType.UserList:
                        var users = Encoding.UTF8.GetString(msg.Data).Split(',')
                            .Where(u => !string.IsNullOrEmpty(u) && u != username)
                            .ToList();
                        OnUserListUpdated?.Invoke(users);
                        break;

                    case MessageType.GroupCreated:
                        HandleGroupCreated(msg);
                        break;

                    case MessageType.GroupMemberList:
                        HandleGroupList(msg);
                        break;

                    case MessageType.GroupMessage:
                        HandleGroupMessage(msg);
                        SendStatusUpdate(msg.Sender, msg.MessageID, "DELIVERED");
                        break;

                    case MessageType.GroupList:
                        HandleGroupList(msg);
                        break;

                    case MessageType.TypingStatus:
                        bool isTyping = msg.Data[0] == 1;
                        // Phải truyền cả GroupID và Sender
                        OnTypingStatusReceived?.Invoke(msg.GroupID, msg.Sender, isTyping);
                        break;
                    case MessageType.VideoCallRequest:
                        OnVideoCallRequestReceived?.Invoke(msg.Sender);
                        break;

                    case MessageType.VideoCallAccept:
                        OnVideoCallAccepted?.Invoke(msg.Sender);
                        break;

                    case MessageType.StatusUpdate:
                        string statusData = Encoding.UTF8.GetString(msg.Data);
                        var parts = statusData.Split('|');
                        if (parts.Length == 2)
                        {
                            string status = parts[0];
                            string msgId = parts[1];
                            OnMessageStatusUpdated?.Invoke(msgId, status);
                        }
                        break;

                    case MessageType.LikeMessage:
                        string messageId = Encoding.UTF8.GetString(msg.Data);
                        OnMessageLiked?.Invoke(messageId, msg.Sender);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Disconnect();
        }
    }



    public void CreateGroup(string groupName)
    {
        var msg = new Message
        {
            Type = MessageType.CreateGroup,
            Sender = username,
            Data = Encoding.UTF8.GetBytes(groupName)
        };
        SendMessagePacket(msg);
    }

    public void JoinGroup(int groupId)
    {
        var msg = new Message
        {
            Type = MessageType.JoinGroup,
            GroupID = groupId,
            Sender = username,
            Data = new byte[0]
        };
        SendMessagePacket(msg);
    }


    public string SendGroupMessage(int groupId, string plaintext, ContentType contentType = ContentType.Text)
    {
        var msg = new Message
        {
            Type = MessageType.GroupMessage,
            GroupID = groupId,
            Sender = username,
            Data = Encoding.UTF8.GetBytes(plaintext),
            ContentType = contentType
        };
        SendMessagePacket(msg);
        return msg.MessageID;
    }

    public void SendMessagePacket(Message msg)
    {
        if (client == null || !client.Connected) return;

        byte[] data = msg.Serialize();
        byte[] lengthHeader = BitConverter.GetBytes(data.Length);

        lock (stream)
        {
            stream.Write(lengthHeader, 0, lengthHeader.Length);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }

    public string SendGroupFile(int groupId, byte[] fileData, ContentType contentType)
    {
        var msg = new Message
        {
            Type = MessageType.GroupMessage,
            GroupID = groupId,
            Sender = username,
            Data = fileData,
            ContentType = contentType
        };
        SendMessagePacket(msg);
        return msg.MessageID;
    }

    public void SendLike(string receiver, string messageId)
    {
        var msg = new Message
        {
            Type = MessageType.LikeMessage,
            Sender = username,
            Receiver = receiver,
            Data = Encoding.UTF8.GetBytes(messageId)
        };
        SendMessagePacket(msg);
    }

    public void SendGroupLike(int groupId, string messageId)
    {
        var msg = new Message
        {
            Type = MessageType.LikeMessage,
            GroupID = groupId,
            Sender = username,
            Data = Encoding.UTF8.GetBytes(messageId)
        };
        SendMessagePacket(msg);
    }


    public List<GroupInfo> GetAvailableGroups()
    {
        lock (availableGroups)
        {
            return availableGroups.Values.ToList();
        }
    }

    private void HandleGroupCreated(Message msg)
    {
        int groupId = msg.GroupID;
        string groupName = Encoding.UTF8.GetString(msg.Data);

        OnGroupCreated?.Invoke(groupId, groupName);
    }

    private void HandleGroupList(Message msg)
    {
        try
        {
            string data = Encoding.UTF8.GetString(msg.Data);
            var parts = data.Split('|');
            if (parts.Length >= 4)
            {
                var group = new GroupInfo
                {
                    GroupID = int.Parse(parts[0]),
                    GroupName = parts[1],
                    CreatedBy = parts[2],
                    MemberCount = int.Parse(parts[3])
                };
                lock (availableGroups) { availableGroups[group.GroupID] = group; }
                OnGroupListUpdated?.Invoke(GetAvailableGroups());
            }
        }
        catch { }
    }

    private void HandleGroupMessage(Message msg)
    {
        int groupId = msg.GroupID;
        string sender = msg.Sender;
        string content = msg.ContentType == ContentType.Text
            ? Encoding.UTF8.GetString(msg.Data)
            : Convert.ToBase64String(msg.Data);

        OnGroupMessageReceived?.Invoke(groupId, sender, content, msg.ContentType, msg.MessageID);
    }

    public string SendMessage(string receiver, string plaintext, ContentType contentType = ContentType.Text)
    {
        return SendMessageInternal(receiver, Encoding.UTF8.GetBytes(plaintext), contentType, false);
    }

    public string SendFile(string receiver, byte[] fileData, ContentType contentType)
    {
        return SendMessageInternal(receiver, fileData, contentType, false);
    }

    public string BroadcastMessage(string plaintext, ContentType contentType = ContentType.Text)
    {
        return SendMessageInternal("ALL", Encoding.UTF8.GetBytes(plaintext), contentType, true);
    }

    public string BroadcastFile(byte[] fileData, ContentType contentType)
    {
        return SendMessageInternal("ALL", fileData, contentType, true);
    }

    private string SendMessageInternal(string receiver, byte[] data, ContentType contentType, bool isBroadcast)
    {
        if (isBroadcast)
        {
            List<string> allUsers;
            lock (peerPublicKeys) { allUsers = peerPublicKeys.Keys.ToList(); }

            if (allUsers.Count == 0) return null;

            byte[] aesKey = CryptoHelper.GenerateAESKey();
            byte[] encryptedData = CryptoHelper.EncryptAES(data, aesKey);
            var encryptedKeys = new Dictionary<string, byte[]>();

            lock (peerPublicKeys)
            {
                foreach (var user in allUsers)
                {
                    if (peerPublicKeys.ContainsKey(user))
                        encryptedKeys[user] = CryptoHelper.EncryptRSA(aesKey, peerPublicKeys[user]);
                }
            }

            var combined = new List<byte>();
            combined.AddRange(BitConverter.GetBytes(encryptedKeys.Count));
            foreach (var kvp in encryptedKeys)
            {
                byte[] usernameBytes = Encoding.UTF8.GetBytes(kvp.Key);
                combined.AddRange(BitConverter.GetBytes(usernameBytes.Length));
                combined.AddRange(usernameBytes);
                combined.AddRange(BitConverter.GetBytes(kvp.Value.Length));
                combined.AddRange(kvp.Value);
            }
            combined.AddRange(encryptedData);

            var msg = new Message
            {
                Type = MessageType.EncryptedBroadcast,
                Sender = username,
                Receiver = "ALL",
                Data = combined.ToArray(),
                ContentType = contentType
            };
            SendMessagePacket(msg);


            return msg.MessageID;
        }
        else
        {
            lock (peerPublicKeys) { if (!peerPublicKeys.ContainsKey(receiver)) return null; }

            byte[] aesKey = CryptoHelper.GenerateAESKey();
            string receiverPublicKey;
            lock (peerPublicKeys) { receiverPublicKey = peerPublicKeys[receiver]; }

            byte[] encryptedAesKey = CryptoHelper.EncryptRSA(aesKey, receiverPublicKey);
            byte[] encryptedMessage = CryptoHelper.EncryptAES(data, aesKey);

            var combined = new List<byte>();
            combined.AddRange(BitConverter.GetBytes(encryptedAesKey.Length));
            combined.AddRange(encryptedAesKey);
            combined.AddRange(encryptedMessage);

            var msg = new Message
            {
                Type = MessageType.EncryptedMessage,
                Sender = username,
                Receiver = receiver,
                Data = combined.ToArray(),
                ContentType = contentType
            };
            SendMessagePacket(msg);
            return msg.MessageID;
        }
    }


    private void DecryptAndDisplayMessage(Message msg)
    {
        try
        {
            byte[] decryptedData;
            if (msg.Type == MessageType.EncryptedBroadcast)
            {
                int offset = 0;
                int userCount = BitConverter.ToInt32(msg.Data, offset); offset += 4;
                byte[] myEncryptedKey = null;

                for (int i = 0; i < userCount; i++)
                {
                    int usernameLen = BitConverter.ToInt32(msg.Data, offset); offset += 4;
                    string user = Encoding.UTF8.GetString(msg.Data, offset, usernameLen); offset += usernameLen;
                    int keyLen = BitConverter.ToInt32(msg.Data, offset); offset += 4;
                    byte[] encKey = msg.Data.Skip(offset).Take(keyLen).ToArray(); offset += keyLen;

                    if (user == username) myEncryptedKey = encKey;
                }

                if (myEncryptedKey == null) return;
                byte[] aesKey = CryptoHelper.DecryptRSA(myEncryptedKey, privateKey);
                byte[] encryptedMessage = msg.Data.Skip(offset).ToArray();
                decryptedData = CryptoHelper.DecryptAES(encryptedMessage, aesKey);
            }
            else
            {
                int offset = 0;
                int aesKeyLen = BitConverter.ToInt32(msg.Data, offset); offset += 4;
                byte[] encryptedAesKey = msg.Data.Skip(offset).Take(aesKeyLen).ToArray(); offset += aesKeyLen;
                byte[] encryptedMessage = msg.Data.Skip(offset).ToArray();
                byte[] aesKey = CryptoHelper.DecryptRSA(encryptedAesKey, privateKey);
                decryptedData = CryptoHelper.DecryptAES(encryptedMessage, aesKey);
            }

            if (msg.ContentType == ContentType.Text)
            {
                string plaintext = Encoding.UTF8.GetString(decryptedData);
                OnMessageReceived?.Invoke(msg.Sender, plaintext, ContentType.Text, msg.MessageID);
            }
            else
            {
                string base64Data = Convert.ToBase64String(decryptedData);
                OnMessageReceived?.Invoke(msg.Sender, base64Data, msg.ContentType, msg.MessageID);
            }
        }
        catch { }
    }

    public void RequestVideoCall(string receiver)
    {
        var msg = new Message
        {
            Type = MessageType.VideoCallRequest,
            Sender = username,
            Receiver = receiver,
            Data = new byte[0]
        };
        SendMessagePacket(msg);
    }

    public void AcceptVideoCall(string sender)
    {
        var msg = new Message
        {
            Type = MessageType.VideoCallAccept,
            Sender = username,
            Receiver = sender,
            Data = new byte[0]
        };
        SendMessagePacket(msg);

    }


    public void SendTypingStatus(string receiver, int groupId, bool isTyping)
    {
        var msg = new Message
        {
            Type = MessageType.TypingStatus,
            Sender = username,
            Receiver = receiver,
            GroupID = groupId,
            Data = new byte[] { (byte)(isTyping ? 1 : 0) }
        };
        SendMessagePacket(msg);
    }



    public void LeaveGroup(int groupId)
    {
        var packet = new Message
        {
            Type = MessageType.LeaveGroup,
            GroupID = groupId,
            Sender = username,
            Data = new byte[0]
        };
        SendMessagePacket(packet);
    }

    public void Disconnect()
    {
        running = false;
        client?.Close();
        rsa?.Dispose();
    }
}