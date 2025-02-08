using UnityEngine;
using UnityEngine.UI;

public class Panel_Login : MonoBehaviour
{

    // 输入框
    [Header("用户名输入框")]
    public InputField inputField;

    [Header("大厅面板")]
    public Panel_Hall Panel_Hall;

    // 进入大厅按钮
    public void Btn_Enter()
    {
        // TODO
        // 修复点击“开始游戏”后再选择联机，无法进入大厅的bug
        if(inputField.textComponent.text.Length > 0)
        {
            NetManager.Instance._userName = inputField.textComponent.text; // 赋值 userName
            Panel_Hall.gameObject.SetActive(true);
            NetManager.Instance.Send(new EnterHall(NetManager.Instance._userName));
        }
        else
        {
            Debug.Log("用户名长度不能为0");
        }
    }
}
