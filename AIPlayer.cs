using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MOVEPOS
{
    public int X;
    public int Y;
}
public class AIPlayer
{
    // 寻找最佳走法(最聪明的AI)
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

    // 寻找随机走法（最笨的AI）
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

    /// <summary>
    /// 用博弈算法得到得分（越小则说明AI容易胜）
    /// </summary>
    /// <param name="grids">棋盘情况</param>
    /// <param name="depth">棋盘已经填满的个数</param>
    /// <param name="isMax">是否是玩家的轮次</param>
    /// <returns></returns>
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
            // int maxScore = 0;

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
                        // maxScore += Minimax(grids, !isMax);
                        grids[i][j] = 2;
                    }
                }
            }
            return maxScore;
        }
        else // 轮到AI落子
        {
            int minScore = 10; // 玩家的最小得分
            // int minScore = 0;

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
                        // minScore += Minimax(grids, !isMax);
                        grids[i][j] = 2;
                    }
                }
            }
            return minScore;
        }
        
    }

    // 评估棋盘得分的函数
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

    private bool IsFull(int[][] grids)
    {
        for(int i=0; i<3; i++)
        {
            for(int j=0; j<3; j++)
            {
                if(grids[i][j]==2){return false;}
            }
        }

        return true;
    }
}