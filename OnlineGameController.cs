using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGameController : MonoBehaviour
{
    private static OnlineGameController _instance;
    public static OnlineGameController Instance{get{return _instance;}}

    [Header("棋子图片")]
    public Sprite[] stoneSprites; // 先后手的不同棋子的图片

    #region 棋局状态相关变量
    private bool isPrevPlayer; // 是否是先手玩家
    private OnlineGrid[] grids; // 棋盘格的状态
    private int moveCount; // 棋局走过的总步数
    private int[] steps; // 每一步的位置，-1表示该步没有落子
    public bool canClick; // 是否能点击格子
    private RESULT result; // 对战结果
    # endregion

    private bool isSaved; // 是否点过保存信息

    private COLOR selfColor => isPrevPlayer?COLOR.W:COLOR.B;
    private COLOR riverColor => isPrevPlayer?COLOR.B:COLOR.W;

    private GameObject Panel_OverGo;
    public Text Text_Result;


    void Awake()
    {
        if(_instance==null){_instance=this;}
        
        isPrevPlayer = NetManager.Instance._isPrevPlayer;
        moveCount = 0;
        grids = new OnlineGrid[9];
        steps = new int[9]{-1,-1,-1,-1,-1,-1,-1,-1,-1};
    }

    void OnEnable()
    {
        NetManager.Instance.RegisterHandler(MessageID.MoveInfo, OnReceiveMoveInfo);

        // 一开始隐藏游戏结束面板
        Panel_OverGo = transform.Find("Panel_Over").gameObject;
        Panel_OverGo.SetActive(false);

        Text_Result = Panel_OverGo.transform.Find("Text_Result").GetComponent<Text>();
    }

    void OnDisable()
    {
        NetManager.Instance.UnregisterHandler(MessageID.MoveInfo, OnReceiveMoveInfo);
    }

    void Start()
    {
        // 把格子对象加入数组，并初始化
        Transform gridParent = transform.Find("Panel_Game").Find("Grids");
        for(int i=0; i<9; i++)
        {
            grids[i] = gridParent.GetChild(i).GetComponent<OnlineGrid>();
            grids[i].InitGrid(i);
        }

        // 如果是先手，一开始能点击
        canClick = isPrevPlayer;
    }

    private void OnReceiveMoveInfo(object obj)
    {
        MoveInfo moveInfo = obj as MoveInfo;
        int pos = moveInfo.pos;
        // TODO 播放音效

        grids[pos].State = riverColor; // 对手下一步
        steps[moveCount] = pos; moveCount++; // 记录步数
        grids[pos].ShowStone(GetCurrentSprite(isSelf: false)); // 展示对手的棋子
        canClick = true; // 恢复可点击状态

        // 第5步开始要检查胜负情况
        if (moveCount >= 5)
        {
            CheckAndHandleGameResult();
        }
    }

    public void Move(OnlineGrid grid)
    {
        // TODO 播放音效

        grid.State = selfColor; // 自己下一步

        grid.ShowStone(GetCurrentSprite()); // 展示棋子
        steps[moveCount] = grid.ID; moveCount++; // 记录步数
        canClick = false; // 设置为不可点击

        NetManager.Instance.Send(new MoveInfo(grid.ID, NetManager.Instance._onlineRoundIndex)); // 给服务端发消息

        // 第5步开始要检查胜负情况
        if (moveCount >= 5)
        {
            CheckAndHandleGameResult();
        }
    }

    // 检查并处理胜负情况
    private void CheckAndHandleGameResult()
    {
        COLOR winColor = CheckWin();

        if (moveCount < 9 && winColor == COLOR.G) { return; } // 还没下满且未分胜负时继续游戏

        if (winColor == COLOR.W)
        {
            Debug.Log("白先手获胜，弹出结束菜单");
            result = RESULT.PREV; canClick = false;
        }
        else if (winColor == COLOR.B)
        {
            Debug.Log("黑后手获胜，弹出结束菜单");
            result = RESULT.LATE; canClick = false;
        }
        else if (moveCount >= 9)
        {
            Debug.Log("打平了，弹出结束菜单");
            result = RESULT.DRAW; canClick = false;
        }

        // 开启协程
        StartCoroutine(ShowGameOverPanel());
    }

    // 0.75秒后显示游戏结束面板
    private IEnumerator ShowGameOverPanel()
    {
        string resultStr = "";
        switch (result)
        {
            case RESULT.DRAW: resultStr = "打成平手"; break;
            case RESULT.PREV:
                if (isPrevPlayer)
                {
                    resultStr = "你获胜";
                }
                else
                {
                    resultStr = "对手获胜";
                }
                break;
            case RESULT.LATE:
                if (!isPrevPlayer)
                {
                    resultStr = "你获胜";
                }
                else
                {
                    resultStr = "对手获胜";
                }
                break;
            default: break;
        }

        Text_Result.text = resultStr;
        // 等待一会儿后再显示
        yield return new WaitForSeconds(0.5f);
        Panel_OverGo.SetActive(true);
    }

    private Sprite GetCurrentSprite(bool isSelf = true)
    {
        if(isPrevPlayer)
        {
            return isSelf? stoneSprites[0] : stoneSprites[1];
        }
        else
        {
            return isSelf? stoneSprites[1] : stoneSprites[0];
        }
    }

    private COLOR CheckWin()
    {
        // 横向三种情况
        if(grids[0].State != COLOR.G && grids[0].State == grids[1].State && grids[1].State == grids[2].State){return grids[0].State;}
        if(grids[3].State != COLOR.G && grids[3].State == grids[4].State && grids[4].State == grids[5].State){return grids[3].State;}
        if(grids[6].State != COLOR.G && grids[6].State == grids[7].State && grids[7].State == grids[8].State){return grids[6].State;}

        // 纵向三种情况
        if(grids[0].State != COLOR.G && grids[0].State == grids[3].State && grids[3].State == grids[6].State){return grids[0].State;}
        if(grids[1].State != COLOR.G && grids[1].State == grids[4].State && grids[4].State == grids[7].State){return grids[1].State;}
        if(grids[2].State != COLOR.G && grids[2].State == grids[5].State && grids[5].State == grids[8].State){return grids[2].State;}
        // 斜向两种情况
        if(grids[0].State != COLOR.G && grids[0].State == grids[4].State && grids[4].State == grids[8].State){return grids[0].State;}
        if(grids[2].State != COLOR.G && grids[2].State == grids[4].State && grids[4].State == grids[6].State){return grids[2].State;}

        return COLOR.G; // 平手
    }

    /// <summary>
    /// 保存当前对战的数据
    /// </summary>
    public void Btn_Save()
    {
        // TODO 保存战局信息需增加对手名字
        if(isSaved){return;}
        Round round = new Round();
        round.roundID = NetManager.Instance._onlineRoundIndex;
        round.player1 = NetManager.Instance._userName;
        round.player2 = "";
        round.result = (int)this.result;
        for(int i=0; i<9;i++)
        {
            round.steps[i] = this.steps[i];
        }

# if UNITY_WEBGL
        GameManager.Instance.mRoundList.Add(round);
# endif

# if UNITY_STANDALONE_WIN
        string filePath = Path.Combine(Application.streamingAssetsPath, "TXTs", "rounds.txt");
        //得到字符串的UTF8 数据流
        byte[] bytes = Encoding.UTF8.GetBytes(round.GetWriteString());
        // 文件流创建一个文本文件
        FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
        // 文件写入数据流
        fs.Write(bytes, 0, bytes.Length); fs.WriteByte(0x0A);
        if (bytes != null)
        {
            //清空缓存
            fs.Flush();
            // 关闭流
            fs.Close();
            //销毁资源
            fs.Dispose();
            Debug.Log("保存到：" + filePath);
        }
# endif

        isSaved = true;

        // 发送战局结果给服务器
        Debug.Log("发送战局结果给服务器");
        NetManager.Instance.Send(round);
    }
}
