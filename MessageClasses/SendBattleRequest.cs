public class SendBattleRequest : BaseMessage
{
    // 对手客户端ID
    public int riverClientID;

    public SendBattleRequest()
    {

    }

    public SendBattleRequest(int riverClientID)
    {
        this.riverClientID = riverClientID;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[sizeof(int) + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, riverClientID, ref index);
        return bytes;

    }

    public override int GetBytesNum()
    {
        return sizeof(int);
    }

    public override MessageID GetMessageID()
    {
        return MessageID.SendBattleRequest;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        riverClientID = ReadInt(bytes, ref index);

        return index - beginIndex;
    }
}