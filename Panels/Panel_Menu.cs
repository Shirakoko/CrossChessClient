using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Menu : MonoBehaviour
{
    [Header("对战信息面板")]
    public Panel_RoundsInfo Panel_RoundsInfo;
    [Header("联机对战面板")]
    public Panel_Login Panel_Login;

    // 开始游戏按钮
    public void Btn_Start()
    {
        GameManager.Instance.ToSetScene();
    }

    // 战局信息按钮
    public void Btn_Info()
    {
        Panel_RoundsInfo.gameObject.SetActive(true);
    }

    // 联机对战按钮
    public void Btn_OnlineBattle()
    {
        Panel_Login.gameObject.SetActive(true);
    }
}
