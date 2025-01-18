
public class RequestRoundList: BaseMessage
{
    public override MessageID GetMessageID()
    {
        return MessageID.RequestRoundList;
    }

    public override int GetBytesNum()
    {
        return 0;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;

        byte[] bytes = new byte[MESSAGE_ID_LENGTH];
        WriteInt(bytes, (int)GetMessageID(), ref index);

        return bytes;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        return 0;
    }
}

public class ProvideRoundList : BaseMessage
{
    private Round[] rounds;
    public Round[] Rounds => rounds;

    public ProvideRoundList()
    {

    }

    public ProvideRoundList(Round[] rounds)
    {
        this.rounds = rounds;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.ProvideRoundList;
    }

    public override int GetBytesNum()
    {
        int size = sizeof(int); // 数组长度
        foreach (Round round in rounds)
        {
            size += round.GetBytesNum();
        }

        return size;
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];
        
        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteDataList(bytes, rounds, ref index);

        return bytes;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        rounds = ReadDataList<Round>(bytes, ref index);
        return index - beginIndex;
    }
}
