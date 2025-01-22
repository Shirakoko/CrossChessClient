using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CrossChessServer.MessageClasses;
using UnityEngine;

/// <summary>
/// 用户信息结构体
/// </summary>
public struct UserInfo
{
    public string Name; // 用户名
    public bool IsIdle; // 用户是否空闲

    public UserInfo(string name, bool isIdle)
    {
        Name = name;
        IsIdle = isIdle;
    }
}

public class NetManager: MonoBehaviour
{
    private const float HEART_MESSAGE_INTERVAL = 4.0f;

    private static NetManager _instance;
    public static NetManager Instance{get {return _instance;}}

    /// <summary>
    /// 客户端ID，进入大厅时获得，不在大厅内为0
    /// </summary>
    public int _clientID = 0;
    public string _userName = "";
    public bool _isPrevPlayer;
    public int _onlineRoundIndex;

    /// <summary>
    /// 接收到服务端消息后的回调函数
    /// </summary>
    private Dictionary<MessageID, Action<object>> messageHandlers = new Dictionary<MessageID, Action<object>>();

    /// <summary>
    /// 注册消息回调函数
    /// </summary>
    /// <param name="messageId">消息Id</param>
    /// <param name="handler">回调函数</param>
    public void RegisterHandler(MessageID messageId, Action<object> handler)
    {
        if (!messageHandlers.ContainsKey(messageId))
        {
            messageHandlers[messageId] = handler;
        }
        else
        {
            messageHandlers[messageId] += handler;
        }
    }

    /// <summary>
    /// 解绑回调函数
    /// </summary>
    /// <param name="messageId">消息Id</param>
    /// <param name="handler">回调函数</param>
    public void UnregisterHandler(MessageID messageId, Action<object> handler)
    {
        if (messageHandlers.ContainsKey(messageId))
            {
            messageHandlers[messageId] -= handler;
            if (messageHandlers[messageId] == null)
            {
                messageHandlers.Remove(messageId);
            }
        }
    }

    /// <summary>
    /// 发送事件给事件监听者
    /// </summary>
    /// <param name="messageId">消息Id</param>
    /// <param name="messageData">消息数据</param>
    /// <param name="delaySeconds">延迟触发委托</param>
    public void InvokeMessageCallback(MessageID messageId, object messageData, float delaySeconds = 0)
    {
        if (messageHandlers.ContainsKey(messageId))
        {
            StartCoroutine(DelayedInvoke(messageHandlers[messageId], messageData, delaySeconds));
        }
        else
        {
            Debug.Log("消息ID: " + messageId + "未注册事件监听");
        }
    }

    private IEnumerator DelayedInvoke(Action<object> handler, object messageData, float delaySeconds)
    {
        if (delaySeconds > 0) {
            yield return new WaitForSeconds(delaySeconds);
        } else {
            yield return null;
        }
        
        handler?.Invoke(messageData);
    }

    /** IP地址和端口号 */
    [Header("服务器IP地址")]
    public string ip;
    [Header("服务器端口号")]
    public int endPoint;

    // 客户端Socket
    private Socket socket;
    // 是否连接
    private bool isConnected = false;
    public bool IsConnected => isConnected;

    // 心跳消息对象
    private HeartMessage heartMessage = new HeartMessage();

    /// <summary>
    /// 发送消息队列
    /// </summary>
    private Queue<BaseMessage> sendMsgQueue = new Queue<BaseMessage>();

    /// <summary>
    /// 接收消息队列
    /// </summary>
    private Queue<byte[]> receiveMsgQueue = new Queue<byte[]>();

    // 收消息的水桶（容器）
    private byte[] receiveBytes = new byte[1024 * 1024];
    // 收到的字节数
    private int receiveNum;

    void Awake()
    {
        _instance = this;
        InvokeRepeating("SendHeartMessage", 0, HEART_MESSAGE_INTERVAL);
    }

    private void SendHeartMessage()
    {
        if(isConnected)
        {
            // print("发送心跳消息");
            // 直接使用socket发送字节，不加入消息发送队列
            socket.Send(heartMessage.ConvertToByteArray());
        }
    }

    void Update()
    {
        // 处理收到的消息
        if(receiveMsgQueue.Count > 0)
        {
            byte[] messageBytes = receiveMsgQueue.Dequeue();
            int messageID = BitConverter.ToInt32(messageBytes, 0);
            // Debug.Log("处理服务端消息，消息ID: " + (MessageID)messageID);
            switch(messageID)
            {
                // 提供战局信息
                case (int)MessageID.ProvideRoundList:
                    ProvideRoundList provideRoundList = new ProvideRoundList();
                    provideRoundList.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    this.InvokeMessageCallback(MessageID.ProvideRoundList, provideRoundList.Rounds);
                    break;
                // 准许进入大厅
                case (int)MessageID.AllowEnterHall:
                    AllowEnterHall allowEnterHall = new AllowEnterHall();
                    allowEnterHall.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    this._clientID = allowEnterHall.clientID;
                    // Debug.Log("收到服务端的准许进入大厅的消息，获得_clinetID: " + this._clientID);
                    // 请求大厅用户数据
                    this.Send(new RequestHallClients());
                    break;
                // 大厅用户数据
                case (int)MessageID.HallClients:
                    HallClients hallClients = new HallClients();
                    hallClients.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    // Debug.Log("收到服务器发送的大厅用户数据，个数: " + hallClients.clientIds.Length);
                    this.InvokeMessageCallback(MessageID.HallClients, hallClients);
                    break;
                // 对战请求
                case (int)MessageID.SendBattleRequest:
                    SendBattleRequest sendBattleRequest = new SendBattleRequest();
                    sendBattleRequest.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    Debug.Log("收到来自客户端: " + sendBattleRequest.riverClientID 
                        + sendBattleRequest.senderClientName + "的对战请求");
                    this.InvokeMessageCallback(MessageID.SendBattleRequest, sendBattleRequest);
                    break;
                // 进入对战
                case (int)MessageID.EnterRound:
                    EnterRound enterRound = new EnterRound();
                    enterRound.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    this._isPrevPlayer = enterRound.isPrevPlayer; // 赋值是否是先手
                    Debug.Log("---------进入对战---------是否先手: " + enterRound.isPrevPlayer);
                    this._onlineRoundIndex = enterRound.onlineRoundIndex; // 赋值在服务器上的对战ID
                    GameManager.Instance.ToOnlineGameScene();
                    break;
                case (int)MessageID.MoveInfo:
                    MoveInfo moveInfo = new MoveInfo();
                    moveInfo.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                    Debug.Log($"对手下在了{moveInfo.pos}位置");
                    this.InvokeMessageCallback(MessageID.MoveInfo, moveInfo);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 开启客户端Socket并连接服务端
    /// </summary>
    public void StartClient()
    {
        // 如果是连接状态 直接返回
        if (isConnected)
        {
            return;
        }  

        if (socket == null)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
            
        //连接服务端
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.endPoint);
        try
        {
            socket.Connect(ipPoint);
            isConnected = true;
            //开启发送线程
            ThreadPool.QueueUserWorkItem(SendMsg);
            //开启接收线程
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
        }
        catch (SocketException e)
        {
            if (e.ErrorCode == 10061)
            {
                print("服务器拒绝连接: " + e.ErrorCode + e.Message);
            }
            else
            {
                print("连接失败: " + e.ErrorCode + e.Message);
            }                
        }
    }

    /// <summary>
    /// 关闭客户端Socket，并给服务端发送退出消息
    /// </summary>
    public void CloseClient()
    {    
        if(isConnected && socket != null)
        {
            // 直接给服务端发送退出消息（不使用消息队列）
            socket.Send(new ClientQuit().ConvertToByteArray());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            // 关闭定期发送心跳消息
            CancelInvoke("SendHeartMessage");
            socket = null;
        }
        isConnected = false;
    }

    /// <summary>
    /// 发送字符串消息给服务器
    /// </summary>
    /// <param name="message">字符串消息</param>
    public void Send(BaseMessage message)
    {
        sendMsgQueue.Enqueue(message);
    }

    # region "线程方法"
    private void SendMsg(object obj)
    {
        while(socket != null && isConnected)
        {
            if(sendMsgQueue.Count > 0)
            {
                Debug.Log("发送消息, ID: " + sendMsgQueue.Peek().GetMessageID());
                socket.Send(sendMsgQueue.Dequeue().ConvertToByteArray());
            }
        }
    }

    private void ReceiveMsg(object obj)
    {
        while(socket != null && isConnected)
        {
            if(socket.Available > 0)
            {
                receiveNum = socket.Receive(receiveBytes);
                // 根据接收到的长度从容器中截取，构成新的字节数组并放入接收消息队列
                byte[] truncatedBytes = new byte[receiveNum];
                Array.Copy(receiveBytes, 0, truncatedBytes, 0, receiveNum);
                lock (receiveMsgQueue)
                {
                    receiveMsgQueue.Enqueue(truncatedBytes);
                }
            }
        }
    }
    #endregion
}
