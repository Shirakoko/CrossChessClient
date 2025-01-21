public class ReplyBattleRequest : BaseMessage
{
    public int riverClientID;
    public bool accept;

    public ReplyBattleRequest()
    {

    }

    public ReplyBattleRequest(int riverClientID, bool accept)
    {
        this.riverClientID = riverClientID;
        this.accept = accept;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, riverClientID, ref index);
        WriteBool(bytes, accept, ref index);
        return bytes;
    }

    public override int GetBytesNum()
    {
        return sizeof(int) + sizeof(bool);
    }

    public override MessageID GetMessageID()
    {
        return MessageID.ReplyBattleRequest;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        riverClientID = ReadInt(bytes, ref index);
        accept = ReadBool(bytes, ref index);

        return index - beginIndex;
    }
}