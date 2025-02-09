using UnityEngine;
using UnityEngine.UI;

public class Panel_Login : MonoBehaviour
{

    // 输入框
    [Header("用户名输入框")]
    public InputField inputField;

    [Header("大厅面板")]
    public Panel_Hall Panel_Hall;

    private void OnEnable()
    {
        // 登录面板激活时，开启客户端Socket以连接服务端
        NetManager.Instance.StartClient();
    }

    // 进入大厅按钮
    public void Btn_Enter()
    {
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

    // 返回主菜单按钮
    public void Btn_Back()
    {
        // 关闭客户端socket
        NetManager.Instance.CloseClient();
        this.gameObject.SetActive(false);
    }
}
