using System;
using System.Text;


public enum MessageID
{
    // 战局信息
    RoundInfo = 6,

    // 请求战局信息列表
    RequestRoundList = 7,

    // 返回战局信息列表
    ProvideRoundList = 8,

    // 进入大厅
    EnterHall = 1,

    // 准许进入大厅
    AllowEnterHall = 11,

    // 退出大厅
    QuitHall = 2,

    // 客户端退出
    ClientQuit = 99,
}

public abstract class BaseMessage
{
    /// <summary>
    /// 子类重写，消息ID
    /// </summary>
    public abstract MessageID GetMessageID();

    /// <summary>
    /// 子类重写，获取类序列化后的字节数组的长度
    /// </summary>
    /// <returns>字节数组长度</returns>
    public abstract int GetBytesNum();

    /// <summary>
    /// 把类中的成员变量转换成字节数组
    /// </summary>
    /// <returns>字节数组</returns>
    public abstract byte[] ConvertToByteArray();

    /// <summary>
    /// 从字节数组中读取成员变量，并赋值给类变量
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="beginIndex">读取的起始位置</param>
    /// <returns>读取的字节数长度</returns>
    public abstract int ReadFromBytes(byte[] bytes, int beginIndex = 0);

    #region "各数据类型的序列化方法"
    protected void WriteInt(byte[] bytes, int value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(int);
    }

    protected void WriteFloat(byte[] bytes, float value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(float);
    }

    protected void WriteBool(byte[] bytes, bool value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(bool);
    }

    protected void WriteString(byte[] bytes, string value, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        WriteInt(bytes, strBytes.Length, ref index);
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }

    protected void WriteData(byte[] bytes, BaseMessage data, ref int index)
    {
        byte[] dataBytes = data.ConvertToByteArray();
        int dataLengthWithoutID = data.GetBytesNum(); // 去掉消息ID的长度
        Array.Copy(dataBytes, sizeof(int), bytes, index, dataLengthWithoutID);
        index += dataLengthWithoutID;
    }

    protected void WriteDataList(byte[] bytes, BaseMessage[] data, ref int index)
    {
        WriteInt(bytes, data.Length, ref index);
        for (int i = 0; i < data.Length; i++)
        {
            WriteData(bytes, data[i], ref index);
        }
    }

    # endregion

    #region "各数据类型的反序列化方法"
    protected int ReadInt(byte[] bytes, ref int index)
    {
        int result = BitConverter.ToInt32(bytes, index);
        index += sizeof(int);
        return result;
    }
    protected float ReadFloat(byte[] bytes, ref int index)
    {
        float result = BitConverter.ToSingle(bytes, index);
        index += sizeof(float);
        return result;
    }

    protected bool ReadBool(byte[] bytes, ref int index)
    {
        bool result = BitConverter.ToBoolean(bytes, index);
        index += sizeof(bool);
        return result;
    }

    protected string ReadString(byte[] bytes, ref int index)
    {
        int length = ReadInt(bytes, ref index);
        string result = Encoding.UTF8.GetString(bytes, index, length);
        index += length;
        return result;
    }

    protected T ReadData<T>(byte[] bytes, ref int index) where T : BaseMessage, new()
    {
        T data = new T();
        data.ReadFromBytes(bytes, index);
        index += data.GetBytesNum();
        return data;
    }

    protected T[] ReadDataList<T>(byte[] bytes, ref int index) where T : BaseMessage, new()
    {
        int length = ReadInt(bytes, ref index);
        T[] data = new T[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = ReadData<T>(bytes, ref index);
        }

        return data;
    }

    #endregion
}