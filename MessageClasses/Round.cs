using System.Text;

/// <summary>
/// 战局信息
/// </summary>
public class Round: BaseMessage
{
    public int roundID; // 战局ID
    public string player1; // 先手的昵称
    public string player2; // 后手的昵称
    public int result; // 对战结果，1表示先手胜，2表示后手胜，0表示平局
    public int[] steps; // 每一步的位置

    public Round()
    {
        steps = new int[9]; // 初始化 steps 数组
    }

    public override MessageID GetMessageID()
    {
        return MessageID.RoundInfo;
    }

    // 把对局信息做成一个字符串
    public string GetWriteString()
    {
        StringBuilder stringBuilder= new StringBuilder();
        stringBuilder.Append(roundID.ToString());
        stringBuilder.Append("#");
        stringBuilder.Append(player1);
        stringBuilder.Append("#");
        stringBuilder.Append(player2);
        stringBuilder.Append("#");
        stringBuilder.Append(result.ToString());
        stringBuilder.Append("#");

        // 记录结果
        for (int i = 0; i < 9; i++)
        {
            stringBuilder.Append(steps[i].ToString());
        }

        return stringBuilder.ToString();
    }

    # region "二进制序列化"
    /// <summary>
    /// 得到字节数组长度
    /// </summary>
    /// <returns></returns>
    public override int GetBytesNum()
    {
        return MESSAGE_ID_LENGTH+ // 战局roundID
                    sizeof(int)+Encoding.UTF8.GetBytes(player1).Length+ // player1的名字
                        sizeof(int)+Encoding.UTF8.GetBytes(player2).Length+ // player2的名字
                            sizeof(int)+ // 战局result
                                sizeof(int) * 9; // 步骤
    }

    /// <summary>
    /// 从字节数组中读取对象
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="beginIndex">起始位置</param>
    /// <returns></returns>
    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;

        roundID = ReadInt(bytes, ref index);
        player1 = ReadString(bytes, ref index);
        player2 = ReadString(bytes, ref index);
        result = ReadInt(bytes, ref index);

        for (int i = 0; i < 9; i++)
        {
            steps[i] = ReadInt(bytes, ref index);
        }

        return index - beginIndex;
    }

    /// <summary>
    /// 转换成字节数组
    /// </summary>
    /// <returns>字节数组</returns>
    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        // 字节数组长度为消息ID + 消息内容的总长度
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, roundID, ref index);
        WriteString(bytes, player1, ref index);
        WriteString(bytes, player2, ref index);
        WriteInt(bytes, result, ref index);

        for (int i = 0; i < 9; i++)
        {
            WriteInt(bytes, steps[i], ref index);
        }

        return bytes;
    }
    #endregion
}
