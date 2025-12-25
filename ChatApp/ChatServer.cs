using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ChatServer
{
    private TcpListener listener;
    private Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
    private Dictionary<string, string> publicKeys = new Dictionary<string, string>();

    private Dictionary<int, GroupInfo> groups = new Dictionary<int, GroupInfo>();
    private int nextGroupId = 1;

    private bool running = false;

    public event Action<string> OnLog;
    public event Action<string, string, string> OnMessageForwarded;

    public void Start(int port = 5000)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        OnLog?.Invoke($"Server started on port {port}");

        Task.Run(() =>
        {
            while (running)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
                catch { break; }
            }
        });
    }

    public void Stop()
    {
        running = false;
        listener?.Stop();
        OnLog?.Invoke("Server stopped");
    }

    private void SendPacketToClient(NetworkStream stream, byte[] data)
    {
        try
        {
            byte[] lengthHeader = BitConverter.GetBytes(data.Length);
            lock (stream)
            {
                stream.Write(lengthHeader, 0, lengthHeader.Length);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }
        catch { }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        string clientName = null;

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

                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) return;
                    totalBytesRead += read;
                }

                var msg = Message.Deserialize(messageBuffer);

                switch (msg.Type)
                {
                    case MessageType.RegisterPublicKey:
                        clientName = msg.Sender;
                        lock (clients)
                        {
                            clients[clientName] = client;
                            publicKeys[clientName] = Encoding.UTF8.GetString(msg.Data);
                        }
                        OnLog?.Invoke($"✅ {clientName} connected");

                        BroadcastUserList();
                        System.Threading.Thread.Sleep(50);
                        BroadcastPublicKey(clientName, msg.Data);
                        SendExistingKeys(client, clientName);
                        SendAllGroupsToUser(clientName, stream);
                        break;

                    case MessageType.EncryptedMessage:
                        HandlePrivateMessage(msg);
                        break;

                    case MessageType.EncryptedBroadcast:
                        HandleBroadcast(msg);
                        break;

                    case MessageType.CreateGroup:
                        HandleCreateGroup(msg, stream);
                        break;

                    case MessageType.JoinGroup:
                        HandleJoinGroup(msg, stream);
                        break;

                    case MessageType.GroupMessage:
                        HandleGroupMessage(msg, stream);
                        break;

                    case MessageType.TypingStatus:
                        if (msg.GroupID > 0)
                            BroadcastToGroup(msg.GroupID, msg); // báo cho cả nhóm
                        else
                            HandlePrivateMessage(msg); // báo cho 1 người
                        break;

                    case MessageType.VideoCallRequest:
                    case MessageType.VideoCallAccept:
                    case MessageType.VideoCallReject:
                    case MessageType.VideoCallEnd:
                        HandlePrivateMessage(msg);
                        break;

                    case MessageType.LeaveGroup:
                        HandleLeaveGroup(msg, stream);
                        break;

                    case MessageType.StatusUpdate:
                        HandlePrivateMessage(msg);
                        break;

                    case MessageType.LikeMessage:
                        if (msg.GroupID > 0)
                        {
                            HandleGroupLike(msg);
                        }
                        else
                        {
                            HandlePrivateMessage(msg);
                        }
                        break;
                }
            }
        }
        catch { }
        finally
        {
            if (clientName != null)
            {
                bool removed = false;
                lock (clients)
                {
                    // so sánh Socket, mình chỉ xóa nếu socket đang đóng == socket trong danh sách
                    // cái này chặn việc log out cái phiên cũ (khi user mở lại tab/cửa sổ) xóa mất user khỏi danh sách
                    if (clients.ContainsKey(clientName) && clients[clientName] == client)
                    {
                        clients.Remove(clientName);
                        publicKeys.Remove(clientName);
                        removed = true;
                    }
                }

                if (removed)
                {
                    OnLog?.Invoke($"❌ {clientName} disconnected");
                    BroadcastUserList();
                }
                else
                {
                    // log để biết rằng ngắt kết nối này bị bỏ qua do là phiên cũ
                    OnLog?.Invoke($"ℹ️ Ignored disconnect from old session of {clientName}");
                }
            }
            client.Close();
        }
    }

    private void HandleGroupLike(Message msg)
    {
        try
        {
            lock (groups)
            {
                if (!groups.ContainsKey(msg.GroupID)) return;
                var group = groups[msg.GroupID];

                lock (clients)
                {
                    var data = msg.Serialize();
                    foreach (var member in group.Members)
                    {
                        if (clients.ContainsKey(member))
                        {
                            try
                            {
                                var stream = clients[member].GetStream();
                                SendPacketToClient(stream, data);
                            }
                            catch { }
                        }
                    }
                }
            }
        }
        catch { }
    }

    private void HandlePrivateMessage(Message msg)
    {
        lock (clients)
        {
            if (clients.ContainsKey(msg.Receiver))
            {
                var receiverStream = clients[msg.Receiver].GetStream();
                var data = msg.Serialize();
                SendPacketToClient(receiverStream, data);

                string contentInfo = GetContentTypeInfo(msg.ContentType);
                OnLog?.Invoke($"📤 {msg.Sender} → {msg.Receiver}: {contentInfo}");
                OnMessageForwarded?.Invoke(msg.Sender, msg.Receiver, contentInfo);
            }
        }
    }

    private void HandleBroadcast(Message msg)
    {
        lock (clients)
        {
            var data = msg.Serialize();
            int sentCount = 0;
            foreach (var kvp in clients)
            {
                if (kvp.Key != msg.Sender)
                {
                    try
                    {
                        var receiverStream = kvp.Value.GetStream();
                        SendPacketToClient(receiverStream, data);
                        sentCount++;
                    }
                    catch { }
                }
            }
            string contentInfo = GetContentTypeInfo(msg.ContentType);
            OnLog?.Invoke($"📢 {msg.Sender} → ALL ({sentCount} users): {contentInfo}");
            OnMessageForwarded?.Invoke(msg.Sender, "ALL", contentInfo);
        }
    }

    private void HandleCreateGroup(Message msg, NetworkStream senderStream)
    {
        try
        {
            string groupName = Encoding.UTF8.GetString(msg.Data);
            lock (groups)
            {
                int groupId = nextGroupId++;
                var group = new GroupInfo
                {
                    GroupID = groupId,
                    GroupName = groupName,
                    CreatedBy = msg.Sender,
                    Members = new List<string> { msg.Sender }
                };
                groups[groupId] = group;

                var response = new Message
                {
                    Type = MessageType.GroupCreated,
                    GroupID = groupId,
                    Sender = "Server",
                    Data = Encoding.UTF8.GetBytes(groupName)
                };
                var data = response.Serialize();
                SendPacketToClient(senderStream, data);

                OnLog?.Invoke($"👥 Group '{groupName}' created by {msg.Sender} (ID: {groupId})");
                BroadcastNewGroup(group);
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"❌ Error creating group: {ex.Message}");
        }
    }

    private void HandleJoinGroup(Message msg, NetworkStream senderStream)
    {
        try
        {
            int groupId = msg.GroupID;
            lock (groups)
            {
                if (groups.ContainsKey(groupId))
                {
                    var group = groups[groupId];
                    // đ phải là thành viên thì mới làm, làm ơn ạ!!!
                    if (!group.Members.Contains(msg.Sender))
                    {
                        group.Members.Add(msg.Sender);
                        group.MemberCount = group.Members.Count;

                        // 1. chỉ thông báo tham gia 1 lần duy nhất
                        NotifyGroupMembers(groupId, $"{msg.Sender} joined the group");

                        // 2. chỉ Broadcast danh sách mới khi có người mới thực sự
                        BroadcastNewGroup(group);
                    }
                }
            }
        }
        catch (Exception ex) { OnLog?.Invoke($"Join Error: {ex.Message}"); }
    }

    private void HandleGroupMessage(Message msg, NetworkStream senderStream)
    {
        try
        {
            lock (groups)
            {
                if (!groups.ContainsKey(msg.GroupID)) return;
                var group = groups[msg.GroupID];

                lock (clients)
                {
                    var data = msg.Serialize();
                    foreach (var member in group.Members)
                    {
                        // làm ơn chỉ gửi cho những người KHÁC người gửi
                        if (member != msg.Sender && clients.ContainsKey(member))
                        {
                            try
                            {
                                var stream = clients[member].GetStream();
                                SendPacketToClient(stream, data);
                            }
                            catch { }
                        }
                    }
                }
            }
            OnLog?.Invoke($"[Group {msg.GroupID}] {msg.Sender}: {Encoding.UTF8.GetString(msg.Data)}");
        }
        catch (Exception ex) { OnLog?.Invoke($"❌ Error: {ex.Message}"); }
    }

    // hàm ni là gửi tin cho 1 nhóm 

    private void BroadcastToGroup(int groupId, Message packet)
    {
        List<TcpClient> targetClients = new List<TcpClient>();

        // Bước 1: Thu thập danh sách Client cần gửi (Làm nhanh trong lock)
        lock (groups)
        {
            if (groups.ContainsKey(groupId))
            {
                var group = groups[groupId];
                lock (clients)
                {
                    foreach (var memberName in group.Members)
                    {
                        if (memberName != packet.Sender && clients.ContainsKey(memberName))
                            targetClients.Add(clients[memberName]);
                    }
                }
            }
        }

        // Bước 2: Gửi dữ liệu (Thực hiện ngoài lock để tránh làm treo Server)
        var data = packet.Serialize();
        foreach (var targetClient in targetClients)
        {
            try { SendPacketToClient(targetClient.GetStream(), data); }
            catch { /* Bỏ qua nếu client đã thoát */ }
        }
    }

    private void HandleLeaveGroup(Message msg, NetworkStream senderStream)
    {

        try
        {
            lock (groups)
            {
                if (groups.ContainsKey(msg.GroupID))
                {
                    var group = groups[msg.GroupID];
                    group.Members.Remove(msg.Sender);
                    OnLog?.Invoke($"👤 {msg.Sender} left Group '{group.GroupName}'");

                    if (group.Members.Count == 0)
                    {
                        groups.Remove(msg.GroupID);
                        OnLog?.Invoke($"🗑️ Group '{group.GroupName}' deleted (no members)");
                    }
                    else
                    {
                        NotifyGroupMembers(msg.GroupID, $"{msg.Sender} left the group");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"❌ Error leaving group: {ex.Message}");
        }
    }



    private void SendAllGroupsToUser(string username, NetworkStream stream)
    {
        try
        {
            lock (groups)
            {
                foreach (var group in groups.Values)
                {
                    // CHỈ GỬI NẾU USERNAME NẰM TRONG DANH SÁCH MEMBERS
                    if (group.Members.Contains(username))
                    {
                        var groupData = $"{group.GroupID}|{group.GroupName}|{group.CreatedBy}|{group.Members.Count}";
                        var msg = new Message
                        {
                            Type = MessageType.GroupMemberList,
                            GroupID = group.GroupID,
                            Sender = "Server",
                            Data = Encoding.UTF8.GetBytes(groupData)
                        };
                        var data = msg.Serialize();
                        SendPacketToClient(stream, data);
                    }
                }
            }
        }
        catch (Exception ex) { OnLog?.Invoke($"❌ Error sending groups: {ex.Message}"); }
    }

    private void BroadcastNewGroup(GroupInfo group)
    {
        var groupData = $"{group.GroupID}|{group.GroupName}|{group.CreatedBy}|{group.Members.Count}";
        var msg = new Message
        {
            Type = MessageType.GroupList,
            GroupID = group.GroupID,
            Sender = "Server",
            Data = Encoding.UTF8.GetBytes(groupData)
        };
        var data = msg.Serialize();
        lock (clients)
        {
            foreach (var client in clients.Values)
            {
                try { SendPacketToClient(client.GetStream(), data); } catch { }
            }
        }
    }

    private void NotifyGroupMembers(int groupId, string notification)
    {
        lock (groups)
        {
            if (!groups.ContainsKey(groupId)) return;
            var group = groups[groupId];
            var msg = new Message
            {
                Type = MessageType.GroupMessage,
                GroupID = groupId,
                Sender = "System",
                Data = Encoding.UTF8.GetBytes(notification),
                ContentType = ContentType.Text
            };
            var data = msg.Serialize();
            lock (clients)
            {
                foreach (var member in group.Members)
                {
                    if (clients.ContainsKey(member))
                    {
                        try { SendPacketToClient(clients[member].GetStream(), data); } catch { }
                    }
                }
            }
        }
    }

    private string GetContentTypeInfo(ContentType type)
    {
        return type switch
        {
            ContentType.Text => "Text message",
            ContentType.Image => "📷 Image",
            ContentType.Audio => "🎵 Audio",
            _ => "Unknown"
        };
    }

    private void BroadcastPublicKey(string clientName, byte[] publicKey)
    {
        var msg = new Message
        {
            Type = MessageType.PublicKeyBroadcast,
            Sender = clientName,
            Data = publicKey
        };
        var data = msg.Serialize();
        lock (clients)
        {
            foreach (var kvp in clients)
            {
                if (kvp.Key != clientName)
                {
                    try { SendPacketToClient(kvp.Value.GetStream(), data); } catch { }
                }
            }
        }
    }

    private void SendExistingKeys(TcpClient newClient, string newClientName)
    {
        var stream = newClient.GetStream();
        lock (clients)
        {
            foreach (var kvp in publicKeys)
            {
                if (kvp.Key != newClientName)
                {
                    var msg = new Message
                    {
                        Type = MessageType.PublicKeyBroadcast,
                        Sender = kvp.Key,
                        Data = Encoding.UTF8.GetBytes(kvp.Value)
                    };
                    var data = msg.Serialize();
                    SendPacketToClient(stream, data);
                }
            }
        }
    }

    private void BroadcastUserList()
    {
        string userList;
        lock (clients) { userList = string.Join(",", clients.Keys); }
        var msg = new Message { Type = MessageType.UserList, Data = Encoding.UTF8.GetBytes(userList) };
        var data = msg.Serialize();
        lock (clients)
        {
            foreach (var client in clients.Values)
            {
                try { SendPacketToClient(client.GetStream(), data); } catch { }
            }
        }
    }
}