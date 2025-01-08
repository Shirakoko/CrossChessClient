using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class Panel_RoundsInfo : MonoBehaviour
{
    public Panel_Replay Panel_Replay;
    public GameObject emp_Item;
    public Transform contentTrans; // 内容节点

    void OnEnable()
    {
        // 订阅接收消息事件
        NetManager.Instance.RegisterHandler(MessageID.ProvideRoundList, OnReceiveProvideRoundList);
    
        if(NetManager.Instance.IsConnected)
        {
            // 向服务器发送RequestRoundList消息，请求获取战局列表
            NetManager.Instance.Send(new RequestRoundList());
        }
        else
        {
            // 读取本地txt
            ReadInfoFromTXT();
        } 
    }

    void OnDisable()
    {
        // 取消订阅接收消息事件
        NetManager.Instance.UnregisterHandler(MessageID.ProvideRoundList, OnReceiveProvideRoundList);
    }

    private void OnReceiveProvideRoundList(object data)
    {
        Round[] rounds = data as Round[];
        StartCoroutine(ShowRoundInfo(rounds));
    }

    private IEnumerator ShowRoundInfo(Round[] rounds)
    {
        for(int i = 0; i < rounds.Length; i++)
        {
            Round round = rounds[i];
            GameObject itemObj = Instantiate(emp_Item, contentTrans);
            itemObj.transform.GetChild(0).GetComponent<Text>().text = round.player1;
            itemObj.transform.GetChild(1).GetComponent<Text>().text = round.player2;
            string result = round.result.ToString();
            switch (result)
            {
                case "0": result = "平手"; break;
                case "1": result = "先手胜"; break;
                case "2": result = "后手胜"; break;
                default: break;
            }
            itemObj.transform.GetChild(2).GetComponent<Text>().text = result;
            itemObj.transform.localPosition -= new Vector3(0, 100 * i, 0);

            int[] steps = round.steps;
            itemObj.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => { Replicate(steps); });

            yield return null; // 等待下一帧继续执行
        }

        RectTransform rectTrans = contentTrans.GetComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector2(0, 100 * rounds.Length); // 改变内容框的长度
    }

# if UNITY_STANDALONE_WIN
    private void ReadInfoFromTXT()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "TXTs", "rounds.txt");
        //逐行读取返回的为数组数据
        string[] strs = File.ReadAllLines(filePath);
        if(strs.Length == 0){return;}
        for(int i = 0; i < strs.Length; i++)
        {
            string[] infos = strs[i].Split("#");
            if(infos.Length == 5)
            {
                GameObject itemObj = Instantiate(emp_Item, contentTrans);
                int ID = int.Parse(infos[0]);
                itemObj.transform.GetChild(0).GetComponent<Text>().text = infos[1];
                itemObj.transform.GetChild(1).GetComponent<Text>().text = infos[2];
                string result = infos[3];
                switch(result)
                {
                    case "0": result = "平手"; break;
                    case "1": result = "先手胜"; break;
                    case "2": result = "后手胜"; break;
                    default: break;
                }
                itemObj.transform.GetChild(2).GetComponent<Text>().text = result;
                // 设置位置
                itemObj.transform.localPosition -= new Vector3(0, 100*ID, 0);
                // 绑定按钮事件
                int[] steps = new int[9]{-1,-1,-1,-1,-1,-1,-1,-1,-1}; int index = 0;
                while(infos[4][index]!='-')
                {
                    steps[index] = infos[4][index]-'0';
                    index++;
                }
                itemObj.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(()=>{Replicate(steps);});
            }
        }
        RectTransform rectTrans = contentTrans.GetComponent<RectTransform>();
        rectTrans.sizeDelta =  new Vector2(0, 100*strs.Length); // 改变内容框的长度
    }
# endif

# if UNITY_WEBGL
    private void ReadInfoFromTXT()
    {
        List<Round> roundList = GameManager.Instance.mRoundList;
        for(int i = 0; i < roundList.Count; i++)
        {
            GameObject itemObj = Instantiate(emp_Item, contentTrans);
            int ID = roundList[i].roundID;
            itemObj.transform.GetChild(0).GetComponent<Text>().text = roundList[i].player1;
            itemObj.transform.GetChild(1).GetComponent<Text>().text = roundList[i].player2;
            int res = roundList[i].result; string result = "";
            switch(res)
            {
                case 0: result = "平手"; break;
                case 1: result = "先手胜"; break;
                case 2: result = "后手胜"; break;
                default: break;
            }
            itemObj.transform.GetChild(2).GetComponent<Text>().text = result;
            // 设置位置
            itemObj.transform.localPosition -= new Vector3(0, 100*ID, 0);
            // 绑定按钮事件
            int[] steps = GameManager.Instance.mRoundList[ID].steps;
            itemObj.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(()=>{Replicate(steps);});
        }

        RectTransform rectTrans = contentTrans.GetComponent<RectTransform>();
        rectTrans.sizeDelta =  new Vector2(0, 100*roundList.Count); // 改变内容框的长度
    }
# endif

    // 复现棋局
    private void Replicate(int[] steps)
    {
        Panel_Replay.transform.localPosition = Vector3.zero; // 出现棋盘，瞬移到左侧
        Panel_Replay.ReplicateRound(steps);
    }

    public void Btn_Cross()
    {
        this.gameObject.SetActive(false);
    }

# if UNITY_STANDALONE_WIN
    public void Btn_Clear()
    {
        // 清空txt
        string filePath = Path.Combine(Application.streamingAssetsPath, "TXTs", "rounds.txt");
        FileStream fs = new FileStream(filePath, FileMode.Truncate, FileAccess.ReadWrite);
        fs.Close();

        // 销毁content下的游戏物体
        Transform transform;
        for(int i = 0;i < contentTrans.childCount; i++)
        {
            transform = contentTrans.GetChild(i);
            Destroy(transform.gameObject);
        }
    }
# endif

# if UNITY_WEBGL
    public void Btn_Clear()
    {
        GameManager.Instance.mRoundList.Clear();
        Transform transform;
        for(int i = 0;i < contentTrans.childCount; i++)
        {
            transform = contentTrans.GetChild(i);
            Destroy(transform.gameObject);
        }
    }
#endif
}
