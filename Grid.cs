using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Grid : MonoBehaviour, IPointerDownHandler
{
    public int ID; // 格子的ID
    public COLOR COLOR; // 各自的状态，0白子，1黑子，2没子
    private Image stone; // 棋子的图片

    void Awake()
    {
        stone = transform.Find("Img_Stone").GetComponent<Image>();
    }

    // 格子被按下后执行的函数
    public void OnPointerDown(PointerEventData eventData)
    {
        if(COLOR != COLOR.G){return;} // 格子中有棋子则不响应
        if(GameController.Instance.canClick==false){return;}
        
        // PvP行动一步
        if(GameController.Instance.isPVP==true)
        {
            GameController.Instance.PvPMove(this);
        }
        // 人机对战行动一步，在PvPMove的基础上要增加AIPlayer的行动
        else
        {
            GameController.Instance.PvEMove(this);
        }
    }

    /// <summary>
    /// 初始化格子
    /// </summary>
    /// <param name="ID">格子的ID</param>
    public void InitGrid(int ID)
    {
        this.ID = ID;
        stone.gameObject.SetActive(false);
        COLOR = COLOR.G; // 表示没棋子
    }

    public void ShowStone(Sprite sprite)
    {
        stone.sprite = sprite;
        stone.gameObject.SetActive(true);
    }
}

public enum COLOR
{
    W=0,
    B=1,
    G=2,
}
