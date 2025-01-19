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

    [Header("大厅面板")]
    public Panel_Hall Panel_Hall;

    // 进入大厅按钮
    public void Btn_Enter()
    {
        if(inputField.textComponent.text.Length > 0)
        {
            userName = inputField.textComponent.text; // 赋值 userName
            Panel_Hall.gameObject.SetActive(true);
            Panel_Hall.ClearAllClientItems();
            NetManager.Instance.Send(new EnterHall(userName));
        }
        else
        {
            Debug.Log("用户名长度不能为0");
        }
    }
}
