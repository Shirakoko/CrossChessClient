using System.Collections;
using System.IO;
using UnityEngine;
using System.Text;
using UnityEngine.UI;


public enum RESULT {
    DRAW = 0, // 平局
    PREV = 1, // 白子（先手）胜利
    LATE = 2, // 黑子（后手）胜利
}
public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance{get{return _instance;}}

    private int _ROUNDCOUNT; // 保存的对战的局数
    
    public bool isPVP; // 是否是人和人对战

    private AIPlayer aiPlayer; // AI
    private bool isSaved; // 是否点过保存信息

    public Player player1; // 先手玩家
    public Player player2; // 后手玩家
    private int moveCount; // 棋局走过的总步数
    private bool currentPlayer; // 当前下棋的玩家，true表示先手，false表示后手
    private int[] steps; // 每一步的位置

    private RESULT result;

    public Sprite[] stoneSprites; // 先后手的不同棋子的图片

    public Grid[] grids;
    public int[][] gridArray; // 用来表示棋盘状态的数组

    public bool canClick; // 能否点击格子（在AI思考时不能点击）

    Coroutine eveCoroutine = null; // 存放EVE对战协程的变量

    # region "外部引用"
    public Text Text_Result;
    # endregion
    private GameObject Panel_OverGo;
    

    void Awake()
    {
        if(_instance==null){_instance=this;}
        moveCount = 0;
        currentPlayer = true; // 开局先手先下
        grids = new Grid[9]; // 实例化格子对象的数组4
        gridArray = new int[3][]{new int[3]{2,2,2},new int[3]{2,2,2},new int[3]{2,2,2}}; // 初始化棋盘状态
        
        // 一开始隐藏游戏结束面板
        Panel_OverGo = transform.Find("Panel_Over").gameObject;
        Panel_OverGo.SetActive(false);
    }

    void Start()
    {
        player1 = new Player(GameManager.Instance.player1, GameManager.Instance.isAIPlayer1, GameManager.Instance.isAIPlayer1Hard);
        player2 = new Player(GameManager.Instance.player2, GameManager.Instance.isAIPlayer2, GameManager.Instance.isAIPlayer2Hard);

        // 设置对局情况
        isPVP = !player1.isAI && !player2.isAI;
        canClick = player1.isAI?false:true; // 先手若是AI，一开始无法点击格子，否则可以点
        steps = new int[9]{-1,-1,-1,-1,-1,-1,-1,-1,-1}; // 每一步的位置先都设成-1

        // 设置当前的局数（相对于保存过的数据）
        GameManager.Instance.UpdateRoundItemCount();
        _ROUNDCOUNT = GameManager.Instance.mRoundItemCount;
        // Debug.Log("当前的ID："+_ROUNDCOUNT);

        // 先设置成没保存过
        isSaved = false;

        if(!isPVP){aiPlayer = new AIPlayer();} // 若是人机对战要实例化一个AI玩家

        // 把格子对象加入数组，并初始化
        Transform gridParent = transform.Find("Panel_Game").Find("Grids");
        for(int i=0; i<9; i++)
        {
            grids[i] = gridParent.GetChild(i).GetComponent<Grid>();
            grids[i].InitGrid(i);
        }

        // 如果先手后手都是AI，直接走完整个棋
        if(player1.isAI && player2.isAI)
        {
            eveCoroutine = StartCoroutine(EVESimulate());
            return;
        }

        // 先手是AI，要线先下第一步
        if(player1.isAI)
        {
            int movePos;
            System.Random random = new System.Random();
            if(player1.isHardAI) // 困难模式只有占据中心或者角才能赢
            {
                int[] poses = new int[]{0,2,4,6,8};
                movePos = poses[random.Next(5)];
            }
            else // 非困难模式随机抽一个边上的位置占据
            {
                int[] poses = new int[]{1,3,5,7};
                movePos = poses[random.Next(4)];
            }
            StartCoroutine(AIDelayedMove(movePos/3, movePos%3));
        }
    }

    private Sprite GetCurrentSprite()
    {
        if(currentPlayer)
        {
            return stoneSprites[0];
        }
        else
        {
            return stoneSprites[1];
        }
    }

    // 人人对战，每次只有一步
    public void PvPMove(Grid grid)
    {
        // TODO 播放音效

        grid.COLOR = currentPlayer?COLOR.W:COLOR.B; // 赋值颜色
        gridArray[grid.ID/3][grid.ID%3] = (int)grid.COLOR; // 记录到棋盘状态数组gridArray中

        grid.ShowStone(GetCurrentSprite()); // 显示棋子
        steps[moveCount] = grid.ID; // 记录当前步骤
        // Debug.Log("第"+moveCount+"步下在："+grid.ID);

        // 发送消息给服务器
        // NetManager.Instance.Send($"第{moveCount}步，{grid.COLOR}棋，下在{grid.ID}");
        moveCount++; currentPlayer = !currentPlayer; // 增加步骤并切换当前行动方

        // 第五步开始要检查胜负情况
        if(moveCount>=5)
        {
            COLOR winColor = CheckWin();
            if(moveCount<9 && winColor==COLOR.G){return;} // 还没下满且未分胜负时继续游戏

            if(winColor == COLOR.W)
            {
                Debug.Log("白先手获胜，弹出结束菜单");
                result = RESULT.PREV; canClick = false;
            }
            else if(winColor == COLOR.B)
            {
                Debug.Log("黑后手获胜，弹出结束菜单");
                result = RESULT.LATE; canClick = false;
            }
            else if(moveCount>=9)
            {
                Debug.Log("打平了，弹出结束菜单");
                result = RESULT.DRAW; canClick = false;
            }
            StartCoroutine(ShowGameOverPanel());
        }
    }

    // 人机对战，人走完之后还要AI走一步
    public void PvEMove(Grid grid)
    {
        // 播放音效

        grid.COLOR = currentPlayer?COLOR.W:COLOR.B; // 赋值颜色
        gridArray[grid.ID/3][grid.ID%3] = 1; // 玩家落子，玩家用1表示
        // PrintState();

        grid.ShowStone(GetCurrentSprite()); // 显示棋子
        steps[moveCount] = grid.ID; // 记录当前步骤
        // Debug.Log("第"+moveCount+"步下在："+grid.ID);
        moveCount++; currentPlayer = !currentPlayer; // 增加步骤并切换当前行动方

        // 第五步开始要检查胜负情况
        if(moveCount>=5)
        {
            COLOR winColor = CheckWin();

            if(winColor == COLOR.W)
            {
                // Debug.Log("白先手获胜，弹出结束菜单");
                result = RESULT.PREV; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                return;
            }
            else if(winColor == COLOR.B)
            {
                // Debug.Log("黑后手获胜，弹出结束菜单");
                result = RESULT.LATE; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                return;
            }
            else if(moveCount>=9)
            {
                // Debug.Log("打平了，弹出结束菜单");
                result = RESULT.DRAW; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                return;
            }
        }

        // 之后是AI行动，根据困难模式和简单模式调用相应函数
        MOVEPOS movePos;
        Player current = currentPlayer?player1:player2;

        if(current.isHardAI)
        {
            movePos = aiPlayer.FindBestMove(gridArray);
        }
        else
        {
            movePos = aiPlayer.FindRandomMove(gridArray);
        }
        
        if(movePos.X!=-1) // 找到了最佳落子位置
        {
            // Debug.Log("最佳位置: "+movePos.X+", "+movePos.Y);
            StartCoroutine(AIDelayedMove(movePos.X, movePos.Y));
        }
        else
        {
            Debug.Log("没有找到最佳位置");
        }
    }

    # region "协程"
    // 0.75秒后显示游戏结束面板
    private IEnumerator ShowGameOverPanel()
    {
        string resultStr = "";
        switch(result)
        {
            case RESULT.DRAW: resultStr = "打成平手"; break;
            case RESULT.PREV: resultStr = player1.name+"获胜"; break;
            case RESULT.LATE: resultStr = player2.name+"获胜"; break;
            default: break;
        }
        Text_Result.text = resultStr;
        // 等待一会儿后再显示
        yield return new WaitForSeconds(0.5f);
        Panel_OverGo.SetActive(true);
    }

    // 直接模拟两个电脑互博
    private IEnumerator EVESimulate()
    {
        canClick = false;
        for(int i=0; i<9;i++)
        {
            yield return new WaitForSeconds(0.5f);
            MOVEPOS movePos;
            Player current = currentPlayer?player1:player2;

            if(current.isHardAI)
            {
                movePos = aiPlayer.FindBestMove(gridArray);
            }
            else
            {
                movePos = aiPlayer.FindRandomMove(gridArray);
            }

            if(movePos.X!=-1) // 找到了最佳落子位置
            {
                int index = 3*movePos.X+movePos.Y;
                grids[index].COLOR = currentPlayer?COLOR.W:COLOR.B; // 赋值颜色
                gridArray[movePos.X][movePos.Y] = 0; // AI下了
                // PrintState();

                grids[index].ShowStone(GetCurrentSprite()); // 显示棋子
                steps[moveCount] = index; // 记录当前步骤的位置
                // Debug.Log("第"+moveCount+"步下在："+index);
                moveCount++; currentPlayer = !currentPlayer; // 增加步骤并切换当前行动方
                

                // 第五步开始要检查胜负情况
                if(moveCount>=5)
                {
                    COLOR winColor = CheckWin();
                    if(winColor == COLOR.W)
                    {
                        // Debug.Log("白先手获胜，弹出结束菜单");
                        result = RESULT.PREV; canClick = false;
                        StopCoroutine(eveCoroutine); // 停止协程
                        StartCoroutine(ShowGameOverPanel());
                        yield return null;
                    }
                    else if(winColor == COLOR.B)
                    {
                        // Debug.Log("黑后手获胜，弹出结束菜单");
                        result = RESULT.LATE; canClick = false;
                        StopCoroutine(eveCoroutine); // 停止协程
                        StartCoroutine(ShowGameOverPanel());
                        yield return null;
                    }
                    else if(moveCount>=9)
                    {
                        // Debug.Log("打平了，弹出结束菜单");
                        result = RESULT.DRAW; canClick = false;
                        StopCoroutine(eveCoroutine); // 停止协程
                        StartCoroutine(ShowGameOverPanel());
                        yield return null;
                    }
                }
            }
            else
            {
                Debug.Log("没有找到最佳位置");
            }
        }
    }

    // AI在人下完后0.5秒再下
    private IEnumerator AIDelayedMove(int movePosX, int movePosY)
    {
        canClick = false; // AI思考时玩家不能点击
        yield return new WaitForSeconds(0.5f);
        int index = 3*movePosX+movePosY;
        grids[index].COLOR = currentPlayer?COLOR.W:COLOR.B; // 赋值颜色
        gridArray[movePosX][movePosY] = 0; // AI下了
        // PrintState();

        grids[index].ShowStone(GetCurrentSprite()); // 显示棋子
        steps[moveCount] = index; // 记录当前步骤的位置
        // Debug.Log("第"+moveCount+"步下在："+index);
        moveCount++; currentPlayer = !currentPlayer; // 增加步骤并切换当前行动方

        // 第五步开始要检查胜负情况
        if(moveCount>=5)
        {
            COLOR winColor = CheckWin();

            if(winColor == COLOR.W)
            {
                // Debug.Log("白先手获胜，弹出结束菜单");
                result = RESULT.PREV; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                yield return null;
            }
            else if(winColor == COLOR.B)
            {
                // Debug.Log("黑后手获胜，弹出结束菜单");
                result = RESULT.LATE; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                yield return null;
            }
            else if(moveCount>=9)
            {
                // Debug.Log("打平了，弹出结束菜单");
                result = RESULT.DRAW; canClick = false;
                StartCoroutine(ShowGameOverPanel());
                yield return null;
            }
        }
        canClick = true; // 最后才能点击
    }
    # endregion

    private COLOR CheckWin()
    {
        // 横向三种情况
        if(grids[0].COLOR != COLOR.G && grids[0].COLOR == grids[1].COLOR && grids[1].COLOR == grids[2].COLOR){return grids[0].COLOR;}
        if(grids[3].COLOR != COLOR.G && grids[3].COLOR == grids[4].COLOR && grids[4].COLOR == grids[5].COLOR){return grids[3].COLOR;}
        if(grids[6].COLOR != COLOR.G && grids[6].COLOR == grids[7].COLOR && grids[7].COLOR == grids[8].COLOR){return grids[6].COLOR;}

        // 纵向三种情况
        if(grids[0].COLOR != COLOR.G && grids[0].COLOR == grids[3].COLOR && grids[3].COLOR == grids[6].COLOR){return grids[0].COLOR;}
        if(grids[1].COLOR != COLOR.G && grids[1].COLOR == grids[4].COLOR && grids[4].COLOR == grids[7].COLOR){return grids[1].COLOR;}
        if(grids[2].COLOR != COLOR.G && grids[2].COLOR == grids[5].COLOR && grids[5].COLOR == grids[8].COLOR){return grids[2].COLOR;}
        // 斜向两种情况
        if(grids[0].COLOR != COLOR.G && grids[0].COLOR == grids[4].COLOR && grids[4].COLOR == grids[8].COLOR){return grids[0].COLOR;}
        if(grids[2].COLOR != COLOR.G && grids[2].COLOR == grids[4].COLOR && grids[4].COLOR == grids[6].COLOR){return grids[2].COLOR;}

        return COLOR.G; // 平手
    }

    # region "按钮方法"
    /// <summary>
    /// 重玩按钮的方法
    /// </summary>
    public void Btn_Replay()
    {
        GameManager.Instance.ToGameScene();
    }

    /// <summary>
    /// 返回按钮的方法
    /// </summary>
    public void Btn_Back()
    {
        GameManager.Instance.ToMenuScene();
    }

    /// <summary>
    /// 保存当前对战的数据
    /// </summary>
    public void Btn_Save()
    {
        if(isSaved){return;}
        Round round = new Round();
        round.roundID = _ROUNDCOUNT++;
        round.player1 = player1.name;
        round.player2 = player2.name;
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
    # endregion

}

public class Player
{
    public string name;
    public bool isAI;
    public bool isHardAI; // 如果是AI，是否是困难模式

    public Player(string name, bool isAI, bool isHardAI)
    {
        this.name = name;
        this.isAI = isAI;
        this.isHardAI = isHardAI;
    }
}

