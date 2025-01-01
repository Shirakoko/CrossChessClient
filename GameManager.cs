using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance{get{return _instance;}}

    public int mRoundItemCount;

    public List<Round> mRoundList;

    # region "对战信息"
    public string player1;
    public string player2;
    public bool isAIPlayer1;
    public bool isAIPlayer2;
    public bool isAIPlayer1Hard;
    public bool isAIPlayer2Hard;
    # endregion

    // public List<Round> roundsInfo = new List<Round>(); // 对战数据


    void Awake()
    {
        if(_instance==null)
        {
            _instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        mRoundItemCount = 0;
        mRoundList = new List<Round>();

        // 保证网络模块存在，已通过Script Execution Order设置
        if (NetManager.Instance == null) {
            this.gameObject.AddComponent<NetManager>();
        }
    }

    void Start()
    {
        // 开启客户端Socket并连接服务端
        NetManager.Instance.StartClient();
    }

    # region "场景管理"
    public void ToSetScene()
    {
        SceneManager.LoadScene("Set");
    }

    public void ToGameScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void ToMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }
    # endregion

#if UNITY_WEBGL
    public void UpdateRoundItemCount()
    {
        mRoundItemCount = mRoundList.Count;
    }
# endif
# if UNITY_STANDALONE_WIN
    public void UpdateRoundItemCount()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "TXTs", "rounds.txt");
        int lines = 0;  //用来统计txt行数
        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        StreamReader sr = new StreamReader(fs);
        while (sr.ReadLine() != null)
        {
            lines++;
        }
        sr.Close();
        fs.Close();
        mRoundItemCount = lines;
    }
#endif
}
