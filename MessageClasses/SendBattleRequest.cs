using System.Text;

public class SendBattleRequest : BaseMessage
{
    // 请求接收方法客户端ID
    public int riverClientID;
    // 请求发送方的名字
    public string senderClientName;

    public SendBattleRequest()
    {

    }

    public SendBattleRequest(int riverClientID, string riverClientName)
    {
        this.riverClientID = riverClientID;
        this.senderClientName = riverClientName;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[sizeof(int) + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, riverClientID, ref index);
        WriteString(bytes, senderClientName, ref index);
        return bytes;

    }

    public override int GetBytesNum()
    {
        return sizeof(int) + sizeof(int) + Encoding.UTF8.GetBytes(senderClientName).Length;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.SendBattleRequest;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        riverClientID = ReadInt(bytes, ref index);
        senderClientName = ReadString(bytes, ref index);

        return index - beginIndex;
    }
}