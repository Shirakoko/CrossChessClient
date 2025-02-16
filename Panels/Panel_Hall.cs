using CrossChessServer.MessageClasses;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Hall : MonoBehaviour
{
    [Header("对战请求条目")]
    public GameObject emp_BattleRequest;

    [Header("不可对战按钮图片")]
    public Sprite image_CannotBattle;
    private Transform contentTrans; // 内容节点

    void Awake()
    {
        contentTrans = this.transform.Find("SV_Clients/Viewport/Content");
    }

    void OnEnable()
    {
        NetManager.Instance.RegisterHandler(MessageID.HallClients, OnReceiveHallClients);
        NetManager.Instance.RegisterHandler(MessageID.SendBattleRequest, OnReceiveBattleRequest);
        // 清除所有大厅用户
        ClearAllClientItems();
        // NetManager.Instance.Send(new RequestHallClients());
    }

    void OnDisable()
    {
        NetManager.Instance.UnregisterHandler(MessageID.HallClients, OnReceiveHallClients);
        NetManager.Instance.UnregisterHandler(MessageID.SendBattleRequest, OnReceiveBattleRequest);
    }

    private void ClearAllClientItems()
    {
        // 先清除contentTrans的所有游戏物体
        foreach (Transform child in contentTrans) {
            Destroy(child.gameObject);
        }
    }

    private void OnReceiveHallClients(object data)
    {
        // 清除所有用户
        ClearAllClientItems();
        // 在准入之前不显示发起对战UI
        if(NetManager.Instance._clientID == 0)
        {
            return;
        }
        // 根据大厅用户个数生成UI
        HallClients hallClients = data as HallClients;
        int clientsCount = hallClients.clientIds.Length;
        for(int i=0; i<clientsCount; i++)
        {
            int clientId = hallClients.clientIds[i];
            string clientName = hallClients.clientNames[i];
            bool isClientIdle = hallClients.clientIdleStates[i];

            GameObject itemObj = Instantiate(emp_BattleRequest, contentTrans);
            itemObj.transform.GetChild(0).GetComponent<Text>().text = clientId.ToString();
            itemObj.transform.GetChild(1).GetComponent<Text>().text = clientName;
            

            // 如果是自己出现在联机大厅，对战按钮禁用
            if (clientId == NetManager.Instance._clientID) {
                itemObj.transform.GetChild(2).GetComponent<Button>().enabled = false;
                itemObj.transform.GetChild(2).GetComponent<Button>().image.sprite = image_CannotBattle;
            }
            else
            {
                if (isClientIdle)
                {
                    // 其他客户端空闲时，绑定按钮方法，向服务器发送对战请求消息
                    itemObj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(()=>{
                        NetManager.Instance.Send(new SendBattleRequest(clientId, NetManager.Instance._userName));
                    });
                }
                else
                {
                    // 其他用户繁忙时，对战按钮禁用
                    itemObj.transform.GetChild(2).GetComponent<Button>().enabled = false;
                    itemObj.transform.GetChild(2).GetComponent<Button>().image.sprite = image_CannotBattle;
                }
            }

            itemObj.transform.localPosition -= new Vector3(0, 100 * i, 0);
        }
    }

    private void OnReceiveBattleRequest(object data)
    {
        SendBattleRequest sendBattleRequest = data as SendBattleRequest;
        int riverClientID = sendBattleRequest.riverClientID;
        string riverClientName = sendBattleRequest.senderClientName;

        Debug.Log("--------------------弹窗提示-----------------------");
        Debug.Log($"收到客户端: {riverClientID} {riverClientName}的对战请求，是否接收对战？");
        ShowBattleRequestPanel(riverClientName, riverClientID);
    }

    // 退出大厅按钮
    public void Btn_Quit()
    {
        if (NetManager.Instance._clientID != 0)
        {
            NetManager.Instance.Send(new QuitHall());
            NetManager.Instance._clientID = 0;
            NetManager.Instance._userName = "";
        }

        this.gameObject.SetActive(false);
    }

    private void ShowBattleRequestPanel(string riverClientName, int riverClientID)
    {
        Panel_BattleRequest panel_BattleRequest = transform.parent.Find("Panel_BattleRequest").GetComponent<Panel_BattleRequest>();
        panel_BattleRequest.RiverName = riverClientName; panel_BattleRequest.RiverClientID = riverClientID;
        panel_BattleRequest.gameObject.SetActive(true);
    }
}
