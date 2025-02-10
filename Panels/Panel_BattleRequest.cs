using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrossChessServer.MessageClasses;
using UnityEngine;
using UnityEngine.UI;

public class Panel_BattleRequest : MonoBehaviour
{
    private Text Text_Request;
    private Button Btn_Accept;
    private Button Btn_Reject;

    private string riverName;
    public string RiverName {get{return this.riverName;} set{this.riverName = value;}}
    private int riverClientID;
    public int RiverClientID {get{return this.riverClientID;} set{this.riverClientID = value;}}

    void OnEnable()
    {
        this.Text_Request = transform.GetChild(0).GetComponent<Text>();
        this.Btn_Accept = transform.GetChild(1).GetComponent<Button>();
        this.Btn_Reject = transform.GetChild(2).GetComponent<Button>();

        NetManager.Instance.RegisterHandler(MessageID.HallClients, OnReceiveHallClients);
    }

    void OnDisable()
    {
        NetManager.Instance.UnregisterHandler(MessageID.HallClients, OnReceiveHallClients);
    }

    void Start()
    {
        if(this.riverClientID != 0)
        {
            this.Text_Request.text = $"收到来自客户端[{this.riverClientID}]{this.riverName}的对战请求，是否接受？";
            // 绑定接受和拒绝按钮方法
            Btn_Accept.onClick.AddListener(() => {
                NetManager.Instance.Send(new ReplyBattleRequest(this.riverClientID, true));
            });
            Btn_Reject.onClick.AddListener(() => {
                NetManager.Instance.Send(new ReplyBattleRequest(this.riverClientID, false));
                // 点拒绝之后按钮消失
                this.gameObject.SetActive(false);
            });
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnReceiveHallClients(object data)
    {
        HallClients hallClients = data as HallClients;
        // 大厅用户状态更新，却不包含对战请求发送方，说明发送方已经退出大厅，弹窗消失
        if(hallClients.clientIds.Contains(this.riverClientID) == false)
        {
            this.gameObject.SetActive(false);
        }
    }
}
