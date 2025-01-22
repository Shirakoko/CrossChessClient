public class MoveInfo : BaseMessage
{
    // 落子位置，0-8
    public int pos;
    // 在服务器中的战局ID
    public int onlineRoundIndex;

    public MoveInfo()
    {

    }

    public MoveInfo(int pos, int onlineRoundIndex)
    {
        this.pos = pos;
        this.onlineRoundIndex = onlineRoundIndex;
    }
    
    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[sizeof(int) + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, pos, ref index);
        WriteInt(bytes, onlineRoundIndex, ref index);
        return bytes;
    }

    public override int GetBytesNum()
    {
        return sizeof(int) + sizeof(int);
    }

    public override MessageID GetMessageID()
    {
        return MessageID.MoveInfo;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        pos = ReadInt(bytes, ref index);
        onlineRoundIndex = ReadInt(bytes, ref index);

        return index - beginIndex;
    }
}