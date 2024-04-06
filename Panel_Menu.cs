using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Menu : MonoBehaviour
{
    public GameObject Panel_RoundsInfo;
    public void Btn_Start()
    {
        GameManager.Instance.ToSetScene();
    }

    public void Btn_Info()
    {
        Panel_RoundsInfo.gameObject.SetActive(true);
    }
}
