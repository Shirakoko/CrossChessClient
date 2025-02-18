# 客户端部分

客户端用Unity引擎开发，其中网络模块的核心是NetManager.cs。

## 1.NetManager功能拆解

核心类`NetManager`作为单例组件，挂载于Unity游戏物体，且**过场景不销毁**，负责管理客户端全生命周期网络交互，包括连接管理、消息路由、状态同步及异常处理。

### 单例模式

`NetManager`使用单例模式确保全局只有一个实例，方便在游戏的其他模块中调用。

```csharp
private static NetManager _instance;
public static NetManager Instance { get { return _instance; } }
```

### 客户端状态记录

`NetManager`中的变量记录客户端的状态，包括客户端ID、用户名、是否先手、当前对战ID等。

```csharp
public int _clientID = 0;
public string _userName = "";
public bool _isPrevPlayer;
public int _onlineRoundIndex;
```

### 消息处理

`NetManager`字典`messageHandlers`存储和调用不同消息ID对应的回调函数。

```csharp
private Dictionary<MessageID, Action<object>> messageHandlers = new Dictionary<MessageID, Action<object>>();
```

- 注册消息回调函数：`RegisterHandler`方法注册消息回调函数。

```csharp
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
```

- 解绑消息回调函数：`UnregisterHandler`方法解绑消息回调函数。

```csharp
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
```

- 触发消息回调函数：`InvokeMessageCallback`方法触发消息回调函数，支持延迟触发。

```csharp
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
```

### 网络连接管理

`NetManager`负责与服务器的连接、断开连接以及心跳消息的发送。

- 启动客户端：`StartClient`方法启动客户端并连接服务器。

```csharp
public void StartClient()
{
    if (isConnected)
    {
        return;
    }

    if (socket == null)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.endPoint);
    try
    {
        socket.Connect(ipPoint);
        isConnected = true;
        ThreadPool.QueueUserWorkItem(SendMsg);
        ThreadPool.QueueUserWorkItem(ReceiveMsg);
    }
    catch (SocketException e)
    {
        Debug.Log("连接失败: " + e.ErrorCode + e.Message);
    }
}
```

- 关闭客户端：`CloseClient`方法关闭客户端并发送退出消息。

```csharp
public void CloseClient()
{
    if (isConnected && socket != null)
    {
        socket.Send(new ClientQuit().ConvertToByteArray());
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        CancelInvoke("SendHeartMessage");
        socket = null;
    }
    isConnected = false;
}
```

### 发送心跳消息

- `SendHeartMessage`方法定期心跳消息。

```
private void SendHeartMessage()
{
    if (isConnected)
    {
        socket.Send(heartMessage.ConvertToByteArray());
    }
}
```

- 在 `Awake`生命周期函数中开启`InvokeRepeating`定期发送心跳消息。

```csharp
InvokeRepeating("SendHeartMessage", 0, HEART_MESSAGE_INTERVAL);
```

### 消息发送与接收

`NetManager`使用两个队列`sendMsgQueue`和`receiveMsgQueue`来管理消息的发送和接收；在 `StartClient`方法中将这两个方法加入**线程池**。

- 发送消息：

`Send` 方法将消息加入发送队列。

```csharp
public void Send(BaseMessage message)
{
    sendMsgQueue.Enqueue(message);
}
```

线程方法遍历消息队列，发送消息

```csharp
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
```

- 接收消息：线程方法 `ReceiveMsg` 方法接收消息并加入接收队列

```csharp
private void ReceiveMsg(object obj)
{
    while (socket != null && isConnected)
    {
        if (socket.Available > 0)
        {
            receiveNum = socket.Receive(receiveBytes);
            byte[] truncatedBytes = new byte[receiveNum];
            Array.Copy(receiveBytes, 0, truncatedBytes, 0, receiveNum);
            lock (receiveMsgQueue)
            {
                receiveMsgQueue.Enqueue(truncatedBytes);
            }
        }
    }
}
```

- 消息处理： `Update` 生命周期函数中处理接收到的消息，并根据消息ID调用相应的回调函数。

```csharp
void Update()
{
    if (receiveMsgQueue.Count > 0)
    {
        byte[] messageBytes = receiveMsgQueue.Dequeue();
        int messageID = BitConverter.ToInt32(messageBytes, 0);
        switch (messageID)
        {
            case (int)MessageID.ProvideRoundList:
                ProvideRoundList provideRoundList = new ProvideRoundList();
                provideRoundList.ReadFromBytes(messageBytes, BaseMessage.MESSAGE_ID_LENGTH);
                this.InvokeMessageCallback(MessageID.ProvideRoundList, provideRoundList.Rounds);
                break;
            // 其他消息处理...
        }
    }
}
```

## 2. OnlineGameController功能拆解

`OnlineGameController`是客户端联机对战场景的核心控制器，负责管理对战过程中的**棋盘状态、玩家交互、胜负判定及结果处理**;与网络模块`NetManager`深度集成，实现实时对战数据的同步。

### 棋盘状态管理

通过`OnlineGrid`数组来管理棋盘的状态；`OnlineGrid`对象代表格子。

```csharp
private OnlineGrid[] grids; // 棋盘格的状态
```

- 更新棋盘状态：当玩家或对手落子时，`OnlineGameController`会更新对应格子的状态，并在UI上显示相应的棋子。

```csharp
public void Move(OnlineGrid grid)
{
    grid.State = selfColor; // 自己下一步
    grid.ShowStone(GetCurrentSprite()); // 展示棋子
    steps[moveCount] = grid.ID; moveCount++; // 记录步数
    canClick = false; // 设置为不可点击
    NetManager.Instance.Send(new MoveInfo(grid.ID, NetManager.Instance._onlineRoundIndex)); // 给服务端发消息
}
```

### 玩家交互

通过`canClick`变量来控制玩家是否可以点击棋盘格子。当轮到玩家落子时，`canClick`为`true`，玩家可以点击棋盘上的空格子进行落子。

```
public bool canClick; // 是否能点击格子
```

- **玩家落子**：当玩家点击一个空格子时，`OnlineGameController`会调用`Move`方法，更新棋盘状态，并将落子信息发送给服务器。

```csharp
public void Move(OnlineGrid grid)
{
    grid.State = selfColor; // 自己下一步
    grid.ShowStone(GetCurrentSprite()); // 展示棋子
    steps[moveCount] = grid.ID; moveCount++; // 记录步数
    canClick = false; // 设置为不可点击
    NetManager.Instance.Send(new MoveInfo(grid.ID, NetManager.Instance._onlineRoundIndex)); // 给服务端发消息
}
```

- **对手落子**：当接收到对手的落子信息时，`OnlineGameController`更新棋盘状态，并恢复玩家的点击权限。

```csharp
private void OnReceiveMoveInfo(object obj)
{
    MoveInfo moveInfo = obj as MoveInfo;
    int pos = moveInfo.pos;
    grids[pos].State = riverColor; // 对手下一步
    steps[moveCount] = pos; moveCount++; // 记录步数
    grids[pos].ShowStone(GetCurrentSprite(isSelf: false)); // 展示对手的棋子
    canClick = true; // 恢复可点击状态
}
```

### 胜负判定

从第5步开始，在每一步落子后都会检查当前棋盘状态，判断是否有玩家获胜或是否平局。

- 胜负检查：`CheckWin`检查棋盘的所有可能获胜情况，包括横向、纵向和斜向。

```csharp
private COLOR CheckWin()
{
    // 横向三种情况
    if(grids[0].State != COLOR.G && grids[0].State == grids[1].State && grids[1].State == grids[2].State){return grids[0].State;}
    if(grids[3].State != COLOR.G && grids[3].State == grids[4].State && grids[4].State == grids[5].State){return grids[3].State;}
    if(grids[6].State != COLOR.G && grids[6].State == grids[7].State && grids[7].State == grids[8].State){return grids[6].State;}

    // 纵向三种情况
    if(grids[0].State != COLOR.G && grids[0].State == grids[3].State && grids[3].State == grids[6].State){return grids[0].State;}
    if(grids[1].State != COLOR.G && grids[1].State == grids[4].State && grids[4].State == grids[7].State){return grids[1].State;}
    if(grids[2].State != COLOR.G && grids[2].State == grids[5].State && grids[5].State == grids[8].State){return grids[2].State;}
    // 斜向两种情况
    if(grids[0].State != COLOR.G && grids[0].State == grids[4].State && grids[4].State == grids[8].State){return grids[0].State;}
    if(grids[2].State != COLOR.G && grids[2].State == grids[4].State && grids[4].State == grids[6].State){return grids[2].State;}

    return COLOR.G; // 平手
}
```

- 处理胜负结果：当检测到有玩家获胜或平局时，调用`CheckAndHandleGameResult`处理游戏结束的逻辑，包括显示结果面板、发送结果给服务器。

```csharp
private void CheckAndHandleGameResult()
{
    COLOR winColor = CheckWin();

    if (moveCount < 9 && winColor == COLOR.G) { return; } // 还没下满且未分胜负时继续游戏

    if (winColor == COLOR.W)
    {
        Debug.Log("白先手获胜，弹出结束菜单");
        result = RESULT.PREV; canClick = false;
    }
    else if (winColor == COLOR.B)
    {
        Debug.Log("黑后手获胜，弹出结束菜单");
        result = RESULT.LATE; canClick = false;
    }
    else if (moveCount >= 9)
    {
        Debug.Log("打平了，弹出结束菜单");
        result = RESULT.DRAW; canClick = false;
    }
    // 给服务端发送对战结果
    NetManager.Instance.Send(
	    new OnlineRoundResult(
		    NetManager.Instance._onlineRoundIndex, // 战局ID
			    NetManager.Instance._userName, this.isPrevPlayer, // 客户端名字、是否是先手 
				    (int)this.result, this.steps)); // 对战结果、步骤
    
    // 开启协程显示游戏结束面板
    StartCoroutine(ShowGameOverPanel());
}
```

### 游戏结束处理

当游戏结束时，显示游戏结束面板，并根据胜负结果更新UI。

- 显示游戏结束面板：`ShowGameOverPanel`方法会在0.5秒后显示游戏结束面板，并根据胜负结果更新`Text_Result`的文本内容。

```csharp
private IEnumerator ShowGameOverPanel()
{
    string resultStr = "";
    switch (result)
    {
        case RESULT.DRAW: resultStr = "打成平手"; break;
        case RESULT.PREV:
            if (isPrevPlayer)
            {
                resultStr = "你获胜";
            }
            else
            {
                resultStr = "对手获胜";
            }
            break;
        case RESULT.LATE:
            if (!isPrevPlayer)
            {
                resultStr = "你获胜";
            }
            else
            {
                resultStr = "对手获胜";
            }
            break;
        default: break;
    }

    Text_Result.text = resultStr;
    // 等待一会儿后再显示
    yield return new WaitForSeconds(0.5f);
    Panel_OverGo.SetActive(true);
}
```

- **保存对战数据**：玩家可以点击保存按钮，将当前对战的数据保存到本地或发送给服务器。

```csharp
public void Btn_Save()
{
    if(isSaved){return;}
    Round round = new Round();
    round.roundID = NetManager.Instance._onlineRoundIndex;
    round.player1 = NetManager.Instance._userName;
    round.player2 = "";
    round.result = (int)this.result;
    for(int i=0; i<9;i++)
    {
        round.steps[i] = this.steps[i];
    }

# if UNITY_WEBGL
    GameManager.Instance.mRoundList.Add(round);
# endif

# if UNITY_STANDALONE_WIN
    string filePath = Path.Combine(Application.streamingAssetsPath, "TXTs", "rounds.txt");
    //得到字符串的UTF8 数据流
    byte[] bytes = Encoding.UTF8.GetBytes(round.GetWriteString());
    // 文件流创建一个文本文件
    FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
    // 文件写入数据流
    fs.Write(bytes, 0, bytes.Length); fs.WriteByte(0x0A);
    if (bytes != null)
    {
        //清空缓存
        fs.Flush();
        // 关闭流
        fs.Close();
        //销毁资源
        fs.Dispose();
        Debug.Log("保存到：" + filePath);
    }
# endif

    isSaved = true;

    // 发送战局结果给服务器
    Debug.Log("发送战局结果给服务器");
    NetManager.Instance.Send(round);
}
```
