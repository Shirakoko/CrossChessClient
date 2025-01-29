# CrossChess-井字棋小游戏（V1.0）
备注：库洛游戏技术策划笔试

## 安装和运行方法

- Windows版本（Github上）的安装运行方法：
  - 访问：访问游戏的Github页面：[Shirakoko/CrossChess: 技术策划笔试 (github.com)](https://github.com/Shirakoko/CrossChess)
  - 下载： 可以从main分支下载，也可从Compressed分支下载
  - 解压：右键点击CrossChess.zip，选择“解压到当前文件夹”
  - 打开解压后的文件夹，找到游戏的启动程序，即CrossChess.exe
  - 启动：双击启动程序

- WebGL版本（发布在itch.io上）的运行方法
  - 打开您的网络浏览器
  - 输入或粘贴游戏在itch.io上的网址：[井字棋小游戏 by Yuki (itch.io)](https://yukilovesgames.itch.io/cross-chess)
  - 在游戏页面上，找到游戏启动按钮“Run game”，游戏将在浏览器窗口加载

请注意，运行WebGL版本的游戏可能需要一个兼容的浏览器和足够的系统资源。如果游戏在加载时出现问题，尝试更新浏览器或检查网络连接

## 功能拆解

### 已实现的功能

- 人人对战模式：允许两名玩家在同一设备上轮流进行游戏
  - 每位玩家都可以在空闲的格子中放置自己的标记（黑子或白子）
  - 游戏会在一名玩家获胜或所有格子被填满时结束
- 人机对战模式：玩家可以选择与电脑AI对战
  - 玩家可选择先手或后手
  - 游戏会在玩家或电脑获胜，或所有格子被填满时结束
- 电脑对战模式：两个AI程序自动进行对战，玩家作为观战者
  - 此模式可用于展示电脑AI的能力或供玩家学习策略
- 游戏中的电脑AI提供两种AI难度选项：
  - 简单AI：执行在空白位置随机落子
  -  困难AI：使用更复杂的算法（Minimax）来计算最佳落子位置
- 战局数据保存和回放功能：
  - 在每局游戏结束后，可以将游戏结果和相关数据保存到本地文件
  - V1.0中保存的数据包括双方玩家名称和获胜者
  - 战局数据保存为二进制格式，以便于存储和读取（Windows版本支持战局数据本地化，WebGL版本只支持单次运行时保存）
  - 对局的每一步记录和模拟回放

### 其他待实现的潜在功能

- 支持账号密码的登陆系统

- 基于账号的玩家统计信息（如胜率）
- 在线多人对战
- 自定义游戏界面和标记
- ......

## 类和脚本UML图

- 控制管理的Mono脚本
  - GameManager：游戏的总管理，生命周期是从程序运行到退出，存在于每个场景
  - GameController：游戏逻辑控制者，仅存在于Game场景，实现核心玩法逻辑
- UI的Mono脚本
  - Panel_Manu：游戏主页面板
  - Panel_Set：设置对局双方信息的面板
  - Panel_RoundsInfo：显示历史战局信息的面板
  - Panel_Replay：（正确命名应该是Panel_Replicate）复现战局的面板
- 信息类
  - Player：玩家信息类，在GameController的Start()函数中实例化，表示对局双方
  - Round：战局信息类，在GameController的Btn_Save()函数中实例化，用于占据信息保存
- 其他功能类
  - Grid：Mono脚本，挂在在格子UI游戏物体上，用于鼠标点击响应和棋子UI显隐
  - AIPlayer：非Mono脚本，若对战双方含电脑AI，则在GameController的Start()函数中实例化

![未命名文件](https://github.com/Shirakoko/CrossChess/assets/97279549/5924763c-b5dd-4893-8254-bdaa9088915e)

## 场景和用户界面UML图

- 游戏共有三个场景，它们之间的通过Button实现切换（由于资源体量小，本游戏全都采用同步加载）
  - Scenes/Menu：入口场景，主页面
  - Scenes/Set：设置对局双方信息（是电脑还是玩家、玩家昵称、电脑难度）
  - Scenes/Game：游玩场景

![未命名文件 (3)](https://github.com/Shirakoko/CrossChess/assets/97279549/8270d9de-8f49-4275-bf10-6a85db20c475)


## 玩法流程图

- 游戏玩法分为三种：
  - PvP：人人对战，先手玩家和后手玩家轮流落子
  - PvE：人机对战，可自行设置玩家是先手还是后手以及电脑AI难度
  - EvE：电脑对战，可分别设置先手电脑和后手电脑的难度

![未命名文件 (5)](https://github.com/Shirakoko/CrossChess/assets/97279549/ff6b59e6-4523-44e7-b7cf-af5b4ca59ec1)

## 电脑AI代码实现

- 所有的脚本代码均可在Scripts分支中找到，这里展示部分脚本
- 找到最佳落子位置

```csharp
// 找到最佳落子位置
public MOVEPOS FindBestMove(int[][] grids)
{
    int minScore = 10;
    MOVEPOS move = new MOVEPOS{X = -1, Y = -1};
    List<int> possibleBestPos = new List<int>(); // 可以和棋的所有位置
    System.Random random = new System.Random();

    // 遍历所有可能的落子位置
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            // 检查是否为空格子
            if (grids[i][j] == 2)
            {
                grids[i][j] = 0; // AI 落子
                int moveScore = Minimax(grids, true); // 计算此时的评分
                grids[i][j] = 2; // 撤销落子

                if (moveScore == 0) // 能达到和棋是最佳位置
                {
                    possibleBestPos.Add(i*3+j);
                }
                else if (moveScore < minScore)
                {
                    move.X = i; move.Y = j;
                    minScore = moveScore;
                }
            }
        }
    }

    if(possibleBestPos.Count > 0)
    {
        int movePos = possibleBestPos[random.Next(possibleBestPos.Count)];
        move.X = movePos/3; move.Y = movePos%3;
    }

    return move; // 返回最佳落子位置
}
```

- 找到随机落子位置

```csharp
public MOVEPOS FindRandomMove(int[][] grids)
{
    MOVEPOS move = new MOVEPOS{X = -1, Y = -1}; 
    List<int> possiblePos = new List<int>();
    System.Random random = new System.Random();

    // 遍历所有可能的落子位置
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            if(grids[i][j] == 2)
            {
                possiblePos.Add(i*3+j);
            }
        }
    }

    // 从可能的位置中随机抽取一个下
    if(possiblePos.Count > 0)
    {
        int movePos = possiblePos[random.Next(possiblePos.Count)];
        move.X = movePos/3; move.Y = movePos%3;
    }

    return move;
}
```

- Minimax方法该处落子的评分

```csharp
private int Minimax(int[][] grids, bool isMax)
{
    int score = Evaluate(grids);

    // 如果游戏结束返回评分，为-1或1
    if (score != 0){return score;}
    // 如果棋盘已满，返回0
    if(IsFull(grids)){return 0;}

    if (isMax) // 轮到玩家落子
    {
        int maxScore = -10; // AI的最大得分

        // 遍历每个玩家可能下的位置
        for (int i = 0; i < 3; i++)
        {
        	for(int j = 0; j < 3; j++)
            {
            	if (grids[i][j] == 2) 
                {
                    grids[i][j] = 1; // 模拟玩家落子
                    // 计算该情况下得分并取较大值（玩家最优解）
                    maxScore = Mathf.Max(maxScore, Minimax(grids, !isMax)); 
                    grids[i][j] = 2;
                }
            }
        }
        return maxScore;
    }
    else // 轮到AI落子
    {
    	int minScore = 10; // 玩家的最小得分

		// 遍历每个AI可能下的位置
        for (int i = 0; i < 3; i++) 
        {
            for(int j = 0; j < 3; j++)
            {
                if (grids[i][j] == 2)
                {
                    grids[i][j] = 0; // 模拟AI落子
                    // 计算该情况下得分并取较小值（AI最优解）
                    minScore = Mathf.Min(minScore, Minimax(grids, !isMax));
                    grids[i][j] = 2;
                }
            }
        }
        return minScore;
    }
}
```

- 评估棋盘得分的函数

```c#
private int Evaluate(int[][] grids)
{
    // 检查所有可能的组合，1表示玩家赢，-1 表示AI赢，0表示平局或游戏未结束

    // 检查行
    for (int i = 0; i < 3; i++)
    {
        if(grids[i][0] == grids[i][1] && grids[i][0] == grids[i][2])
        {
            if(grids[i][0]==1){return 1;} // 玩家获胜
            if(grids[i][0]==0){return -1;} // AI获胜
        }
    }

    // 检查列
    for (int i = 0; i < 3; i++)
    {
        if(grids[0][i] == grids[1][i] && grids[1][i] == grids[2][i])
        {
            if(grids[0][i]==1){return 1;} // 玩家获胜
            if(grids[0][0]==0){return -1;} // AI获胜
        }
    }

    // 检查对角线
    if (grids[0][0] == grids[1][1] && grids[1][1] == grids[2][2])
    {
        if (grids[0][0] == 1) return 1; // 玩家获胜
        if (grids[0][0] == 0) return -1; // AI获胜
    }
    if (grids[0][2] == grids[1][1] && grids[1][1] == grids[2][0])
    {
        if (grids[0][2] == 1) return 1; // 玩家获胜
        if (grids[0][2] == 0) return -1; // AI获胜
    }

    // 如果没有获胜组合，返回平局或游戏未结束
    return 0;
}
```
