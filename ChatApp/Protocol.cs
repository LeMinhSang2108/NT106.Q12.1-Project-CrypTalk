using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum MessageType : byte
{
    RegisterPublicKey = 1,
    PublicKeyBroadcast = 2,
    EncryptedMessage = 3,
    UserList = 4,
    EncryptedBroadcast = 5,
    CreateGroup = 6,
    GroupCreated = 7,
    JoinGroup = 8,
    LeaveGroup = 9,
    GroupMessage = 10,
    GroupMemberList = 11,
    GroupList = 12,
    VideoCallRequest = 20,
    VideoCallAccept = 21,
    VideoCallReject = 22,
    VideoCallEnd = 23,
    LikeMessage = 30,
    StatusUpdate = 40,
    TypingStatus = 50
}

public enum ContentType : byte
{
    Text = 1,
    Image = 2,
    Audio = 3
}

public class Message
{
    public MessageType Type { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public byte[] Data { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Text;
    public int GroupID { get; set; } = 0;
    public string MessageID { get; set; } = Guid.NewGuid().ToString();

    public byte[] Serialize()
    {
        var senderBytes = Encoding.UTF8.GetBytes(Sender ?? "");
        var receiverBytes = Encoding.UTF8.GetBytes(Receiver ?? "");
        var messageIdBytes = Encoding.UTF8.GetBytes(MessageID ?? "");

        var result = new List<byte>();
        result.Add((byte)Type);
        result.Add((byte)ContentType);
        result.AddRange(BitConverter.GetBytes(GroupID));
        result.AddRange(BitConverter.GetBytes(senderBytes.Length));
        result.AddRange(senderBytes);
        result.AddRange(BitConverter.GetBytes(receiverBytes.Length));
        result.AddRange(receiverBytes);
        result.AddRange(BitConverter.GetBytes(messageIdBytes.Length));
        result.AddRange(messageIdBytes);
        result.AddRange(BitConverter.GetBytes(Data?.Length ?? 0));
        if (Data != null) result.AddRange(Data);

        return result.ToArray();
    }

    public static Message Deserialize(byte[] data)
    {
        int offset = 0;
        var msg = new Message { Type = (MessageType)data[offset++] };
        msg.ContentType = (ContentType)data[offset++];
        msg.GroupID = BitConverter.ToInt32(data, offset); offset += 4;

        int senderLen = BitConverter.ToInt32(data, offset); offset += 4;
        msg.Sender = Encoding.UTF8.GetString(data, offset, senderLen); offset += senderLen;

        int receiverLen = BitConverter.ToInt32(data, offset); offset += 4;
        msg.Receiver = Encoding.UTF8.GetString(data, offset, receiverLen); offset += receiverLen;

        int messageIdLen = BitConverter.ToInt32(data, offset); offset += 4;
        msg.MessageID = Encoding.UTF8.GetString(data, offset, messageIdLen); offset += messageIdLen;

        int dataLen = BitConverter.ToInt32(data, offset); offset += 4;
        if (dataLen > 0)
        {
            msg.Data = new byte[dataLen];
            Array.Copy(data, offset, msg.Data, 0, dataLen);
        }

        return msg;
    }
}

public class GroupInfo
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public string CreatedBy { get; set; }
    public List<string> Members { get; set; }
    public int MemberCount { get; set; }

    public GroupInfo()
    {
        Members = new List<string>();
    }

    public override string ToString()
    {
        return $"# {GroupName} [ID:{GroupID}] - ({MemberCount} mems)";
    }
}