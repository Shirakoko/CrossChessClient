using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineGameController : MonoBehaviour
{
    private static OnlineGameController _instance;
    public static OnlineGameController Instance{get{return _instance;}}

    [Header("棋子图片")]
    public Sprite[] stoneSprites; // 先后手的不同棋子的图片

    #region 棋局状态相关变量
    private bool isPrevPlayer; // 是否是先手玩家
    private int lastPlayerID = 0; // 刚才执行操作的玩家ID
    private OnlineGrid[] grids; // 棋盘格的状态
    private int moveCount; // 棋局走过的总步数
    private int[] steps; // 每一步的位置，-1表示该步没有落子
    public bool canClick; // 是否能点击格子
    # endregion

    private COLOR selfColor => isPrevPlayer?COLOR.W:COLOR.B;
    private COLOR riverColor => isPrevPlayer?COLOR.B:COLOR.W;

    void Awake()
    {
        if(_instance==null){_instance=this;}
        // TODO 是否是先手
        isPrevPlayer = NetManager.Instance._isPrevPlayer;
        moveCount = 0;
        grids = new OnlineGrid[9];
        steps = new int[9]{-1,-1,-1,-1,-1,-1,-1,-1,-1};
    }

    void OnEnable()
    {
        NetManager.Instance.RegisterHandler(MessageID.MoveInfo, OnReceiveMoveInfo);    
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

        grids[pos].State = riverColor;
        grids[pos].ShowStone(GetCurrentSprite(isSelf: false)); // 展示对手的棋子
        canClick = true; // 恢复可点击状态
    }

    public void Move(OnlineGrid grid)
    {
        // TODO 播放音效

        grid.State = selfColor;

        grid.ShowStone(GetCurrentSprite()); // 展示棋子
        steps[moveCount] = grid.ID; moveCount++; // 记录步数
        canClick = false; // 设置为不可点击

        // TODO把刚才的落子信息发给服务器
        NetManager.Instance.Send(new MoveInfo(grid.ID, NetManager.Instance._onlineRoundIndex));
        // 第5步开始要检查胜负情况
        if(moveCount >= 5)
        {
            COLOR result = CheckWin();
            // 处理结果
        }
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
}
