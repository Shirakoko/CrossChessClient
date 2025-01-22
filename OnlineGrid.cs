using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnlineGrid : MonoBehaviour, IPointerDownHandler
{
    public int ID; //格子ID
    public COLOR State; // 格子的状态，W白子，B黑子，G没子
    private Image Img_Stone; // 图片组件

    void Awake()
    {
        Img_Stone = transform.Find("Img_Stone").GetComponent<Image>();
    }

    /// <summary>
    /// 初始化格子
    /// </summary>
    /// <param name="ID">格子的ID</param>
    public void InitGrid(int ID)
    {
        this.ID = ID;
        Img_Stone.gameObject.SetActive(false);
        State = COLOR.G; // 表示没棋子
    }

    /// <summary>
    /// 展示棋子
    /// </summary>
    /// <param name="sprite">棋子图片</param>
    public void ShowStone(Sprite sprite)
    {
        Img_Stone.sprite = sprite;
        Img_Stone.gameObject.SetActive(true);
    }

    // 格子被按下后执行的函数
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        if(State != COLOR.G){return;} // 格子中有棋子则不响应
        if(OnlineGameController.Instance.canClick==false){return;}
        
        // 行动一步
        OnlineGameController.Instance.Move(this);
    }
}
