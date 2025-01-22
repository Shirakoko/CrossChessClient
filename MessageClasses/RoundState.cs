public class RoundState : BaseMessage
{
    public int[] grids;
    public RoundState()
    {
        this.grids = new int[9];
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[sizeof(int) + GetBytesNum()];
        WriteIntList(bytes, grids, ref index);

        return bytes;
    }

    public override int GetBytesNum()
    {
        int size = sizeof(int);
        for (int i = 0; i < grids.Length; i++)
        {
            size += sizeof(int);
        }

        return size;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.RoundState;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        grids = ReadIntList(bytes, ref index);

        return index- beginIndex;
    }
}