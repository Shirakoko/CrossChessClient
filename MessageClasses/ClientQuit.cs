public class ClientQuit : BaseMessage
{
    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[sizeof(int) + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        return bytes;
    }

    public override int GetBytesNum()
    {
        return 0;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.ClientQuit;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        return 0;
    }
}

// TODO 心跳消息