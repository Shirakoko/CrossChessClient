public class RequestHallClients : BaseMessage
{
    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        return bytes;
    }

    public override int GetBytesNum()
    {
        return 0;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.RequestHallClients;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        return 0;
    }
}