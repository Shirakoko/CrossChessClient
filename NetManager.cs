using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CrossChessServer.MessageClasses;
using UnityEngine;

public class NetManager: MonoBehaviour
{
    const int MESSAGE_ID_LENGTH = 4;

    private static NetManager _instance;
    public static NetManager Instance{get {return _instance;}}

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
    /// <param name="messageData"></param>
    public void InvokeMessageCallback(MessageID messageId, object messageData)
    {
        if (messageHandlers.ContainsKey(messageId))
        {
            messageHandlers[messageId]?.Invoke(messageData);
        }
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
    }

    void Update()
    {
        // 处理收到的消息
        if(receiveMsgQueue.Count > 0)
        {
            byte[] messageBytes = receiveMsgQueue.Dequeue();
            int messageID = BitConverter.ToInt32(messageBytes, 0);
            Debug.Log("处理服务端消息，消息ID: " + (MessageID)messageID);
            switch(messageID)
            {
                // 提供战局信息
                case (int)MessageID.ProvideRoundList:
                    ProvideRoundList provideRoundList = new ProvideRoundList();
                    provideRoundList.ReadFromBytes(messageBytes, MESSAGE_ID_LENGTH);
                    this.InvokeMessageCallback(MessageID.ProvideRoundList, provideRoundList.Rounds);
                    break;
                // 准许进入大厅
                case (int)MessageID.AllowEnterHall:
                    this.InvokeMessageCallback(MessageID.AllowEnterHall, null);
                    break;
                // 大厅用户数据
                case (int)MessageID.HallClients:
                    HallClients hallClients = new HallClients();
                    hallClients.ReadFromBytes(messageBytes, MESSAGE_ID_LENGTH);
                    Debug.Log("大厅用户个数: " + hallClients.clientIds.Length);
                    this.InvokeMessageCallback(MessageID.HallClients, hallClients);
                    break;
                default:
                    break;
            }
        }
    }

    // 开启客户端Socket并连接服务端
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
