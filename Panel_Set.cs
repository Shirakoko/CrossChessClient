using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Set : MonoBehaviour
{
    private Dropdown Dropdown_INI;
    private Dropdown Dropdown_SUB;
    private InputField InputField_INI;
    private InputField InputField_SUB;

    private Dropdown Dropdown_AI1;
    private Dropdown Dropdown_AI2;

    void Awake()
    {
        Dropdown_INI = transform.Find("Dropdown_INI").GetComponent<Dropdown>();
        Dropdown_SUB = transform.Find("Dropdown_SUB").GetComponent<Dropdown>();
        InputField_INI = transform.Find("InputField_INI").GetComponent<InputField>();
        InputField_SUB = transform.Find("InputField_SUB").GetComponent<InputField>();

        Dropdown_AI1 = transform.Find("Dropdown_AI1").GetComponent<Dropdown>();
        Dropdown_AI2 = transform.Find("Dropdown_AI2").GetComponent<Dropdown>();
    }

    void Start()
    {
        InputField_INI.gameObject.SetActive(true);
        InputField_SUB.gameObject.SetActive(true);

        Dropdown_AI1.gameObject.SetActive(false);
        Dropdown_AI2.gameObject.SetActive(false);
    }

    public void DropDown_INI()
    {
        if(Dropdown_INI.value==0) // 选择了玩家
        {
            InputField_INI.gameObject.SetActive(true);
            Dropdown_AI1.gameObject.SetActive(false);
        }
        else // 选择了电脑
        {
            InputField_INI.gameObject.SetActive(false);
            Dropdown_AI1.gameObject.SetActive(true);
        }
    }

    public void DropDown_SUB()
    {
        if(Dropdown_SUB.value==0) // 选择了玩家
        {
            InputField_SUB.gameObject.SetActive(true);
            Dropdown_AI2.gameObject.SetActive(false);
        }
        else // 选择了电脑
        {
            InputField_SUB.gameObject.SetActive(false);
            Dropdown_AI2.gameObject.SetActive(true);
        }
    }

    public void Btn_Back()
    {
        GameManager.Instance.ToMenuScene();
    }


    public void Btn_Play()
    {
        // 先把对战信息传递给GameManager单例
        // 先手
        if(Dropdown_INI.value==0) // 先手是人
        {
            GameManager.Instance.isAIPlayer1 = false;
            GameManager.Instance.player1 = InputField_INI.text==""?"玩家":InputField_INI.text;
        }  
        else // 先手是电脑
        {
            GameManager.Instance.isAIPlayer1 = true;
            GameManager.Instance.player1 = "电脑";
            // 设置电脑难度
            GameManager.Instance.isAIPlayer1Hard = Dropdown_AI1.value==1;
        }

        if(Dropdown_SUB.value==0) // 后手是人
        {
            GameManager.Instance.isAIPlayer2 = false;
            GameManager.Instance.player2 = InputField_SUB.text==""?"玩家":InputField_SUB.text;
        }
        else // 后手是电脑
        {
            GameManager.Instance.isAIPlayer2 = true;
            GameManager.Instance.player2 = "电脑";
            // 设置电脑难度
            GameManager.Instance.isAIPlayer2Hard = Dropdown_AI2.value==1;
        }

        // 再切换场景
        GameManager.Instance.ToGameScene();
    }
}
