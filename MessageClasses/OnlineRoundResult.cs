using System.Text;

public class OnlineRoundResult : BaseMessage
{
    
    public int roundID; // 战局的ID
    public string playerName; // 用户名字
    public bool isPrevPlayer; // 是否是先手
    public int result; // 对战结果，1表示先手胜，2表示后手胜，0表示平局
    public int[] steps; // 每一步的位置

    public OnlineRoundResult()
    {
        steps = new int[9]; // 初始化 steps 数组
    }

    public OnlineRoundResult(int roundID, string playerName, bool isPrevPlayer, int result, int[] steps)
    {
        this.roundID = roundID;
        this.playerName = playerName;
        this.isPrevPlayer = isPrevPlayer;
        this.result = result;
        this.steps = steps;
    }

    public override MessageID GetMessageID()
    {
        return MessageID.OnlineRoundResult;
    }

        public override int GetBytesNum()
    {
        return MESSAGE_ID_LENGTH+
            sizeof(int)+ // 战局ID
                sizeof(int)+Encoding.UTF8.GetBytes(playerName).Length+ // 用户名字
                    sizeof(bool)+ // 是否是先手
                        sizeof(int)+ // 战局结果
                            sizeof(int) * 9; // 步骤
    }

    public override byte[] ConvertToByteArray()
    {
        int index = 0;
        byte[] bytes = new byte[MESSAGE_ID_LENGTH + GetBytesNum()];

        WriteInt(bytes, (int)GetMessageID(), ref index);
        WriteInt(bytes, roundID, ref index);
        WriteString(bytes, playerName, ref index);
        WriteBool(bytes, isPrevPlayer, ref index);
        WriteInt(bytes, result, ref index);

        for(int i=0; i<9; i++) {
            WriteInt(bytes, steps[i], ref index);
        }

        return bytes;
    }

    public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        roundID = ReadInt(bytes, ref index);
        playerName = ReadString(bytes, ref index);
        isPrevPlayer = ReadBool(bytes, ref index);
        result = ReadInt(bytes, ref index);

        for(int i=0; i<9; i++)
        {
            steps[i] = ReadInt(bytes, ref index);
        }

        return index - beginIndex;
    }
}