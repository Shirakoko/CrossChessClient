public class EnterRound : BaseMessage
{
    public bool isPrevPlayer;
    public int onlineRoundIndex;

    public EnterRound()
    {

    }

    public EnterRound(bool isPrevPlayer, int onlineRoundIndex)
    {
        this.isPrevPlayer = isPrevPlayer;
        this.onlineRoundIndex = onlineRoundIndex;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteBool(bytes, isPrevPlayer, ref index);
        WriteInt(bytes, onlineRoundIndex, ref index);
        return bytes;
    }

    public override int GetBytesNum()
    {
        return sizeof(bool) + sizeof(int);
    }

    public override MessageID GetMessageID()
    {
        return MessageID.EnterRound;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        isPrevPlayer = ReadBool(bytes, ref index);
        onlineRoundIndex = ReadInt(bytes, ref index);

        return index - beginIndex;
    }
}