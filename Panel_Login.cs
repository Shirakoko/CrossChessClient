using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Login : MonoBehaviour
{

    // 输入框
    [Header("用户名输入框")]
    public InputField inputField;
    // 输入的用户名
    private string userName = "";

    void OnEnable()
    {
        // 订阅接收消息事件
        NetManager.Instance.RegisterHandler(MessageID.AllowEnterHall, OnAllowEnterHall);
    }

    void OnDisable()
    {
        // 取消订阅接收消息事件
        NetManager.Instance.UnregisterHandler(MessageID.AllowEnterHall, OnAllowEnterHall);
    }

    private void OnAllowEnterHall(object data)
    {
        // TODO 接收到服务端准许进入大厅的消息，关闭当前Panel并展示大厅Panel
        Debug.Log("接收到服务端准许进入大厅的消息");
    }

    // 进入大厅按钮
    public void Btn_Enter()
    {
        if(inputField.textComponent.text.Length > 0)
        {
            NetManager.Instance.Send(new EnterHall(userName));
        }
        else
        {
            Debug.Log("用户名长度不能为0");
        }
    }
}
