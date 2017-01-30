using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking.NetworkSystem;

public class NetworkRelay //: MonoBehaviour
{
    const short m_msgTypeStringToServer = 1000;
    const short m_msgTypeString         = 1001;

    public bool m_connectServerOnStart = false;

    //NetworkMatch m_networkMatch;
    //List<MatchDesc> m_matchList = new List<MatchDesc>();

    //string m_matchName;
    //MatchInfo m_matchInfo;
    static bool m_isServer;
    //static bool m_matchCreated;  // only valid if IsServer
    //static bool m_matchJoined;

    static NetworkClient m_client;
    static bool m_connectionEstablished;

    public delegate void MessageEventHandler(object sender, string message);

    static List<MessageEventHandler> m_messageCallbacks = new List<MessageEventHandler>();


    //public List<MatchDesc> MatchList { get { return m_matchList; } }
    public bool IsServer { get { return m_isServer; } }
    //public bool MatchCreated { get { return m_matchCreated; } }
    //public bool MatchJoined { get { return m_matchJoined; } }
    //public MatchInfo MatchInfo { get { return m_matchInfo; } }
    public static bool ConnectionEstablished { get { return m_connectionEstablished; } }
    //public string MatchName { get { return m_matchName; } }


    void Awake()
    {
        //m_networkMatch = gameObject.AddComponent<NetworkMatch>();
        //m_networkMatch.baseUri = new System.Uri("https://mm.unet.unity3d.com/");
    }

    void Start()
    {
        //if (m_connectServerOnStart)
        //    ConnectServer();
    }

    void Update()
    {
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        //Disconnect();
    }

#if false
    void OnConnectionDropped(BasicResponse callback)
    {
        Debug.Log("Connection has been dropped on matchmaker server");

        m_isServer = false;
        m_matchName = null;
        m_matchInfo = null;
        m_matchCreated = false;
        m_matchJoined = false;
        m_connectionEstablished = false;
        m_client.Disconnect();
        m_client = null;

        if (IsServer)
        {
            NetworkServer.Shutdown();
        }
    }
#endif

#if false
    void OnMatchCreated(CreateMatchResponse matchResponse)
    {
        if (!matchResponse.success)
        {
            Debug.LogError("Create match failed");
            return;
        }

        Debug.Log("Create match succeeded");

        try
        {
            // when called multiple times (disconnect/reconnect), you get this error:
            //  ArgumentException: An element with the same key already exists in the dictionary.
            //  System.Collections.Generic.Dictionary`2[UnityEngine.Networking.Types.NetworkID,UnityEngine.Networking.Types.NetworkAccessToken].Add (NetworkID key, UnityEngine.Networking.Types.NetworkAccessToken value) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Collections.Generic/Dictionary.cs:404)
            //  UnityEngine.Networking.Utility.SetAccessTokenForNetwork (NetworkID netId, UnityEngine.Networking.Types.NetworkAccessToken accessToken) (at C:/buildslave/unity/build/Runtime/Networking/Managed/UNETTypes.cs:119)
            //  NetworkRelay.OnMatchJoined (UnityEngine.Networking.Match.JoinMatchResponse matchJoin) (at Assets/Scripts/NetworkRelay.cs:186)
            //  UnityEngine.Networking.Match.NetworkMatch+<ProcessMatchResponse>c__Iterator0`1[UnityEngine.Networking.Match.JoinMatchResponse].MoveNext () (at C:/buildslave/unity/build/Runtime/Networking/Managed/MatchMakingClient.cs:302)

            // according to http://forum.unity3d.com/threads/where-is-the-documentation-for-utility-setaccesstokenfornetwork.401489/
            // this is a bug.  And it's safe to ignore the exception, which is what we are doing here.

            Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessTokenString));
        }
        catch (Exception)
        {
        }

        NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
        NetworkServer.RegisterHandler(m_msgTypeStringToServer, OnStringToServer);

        m_client = ClientScene.ConnectLocalServer();
        m_client.RegisterHandler(MsgType.Connect, OnConnected);
        m_client.RegisterHandler(m_msgTypeString, OnString);

        m_isServer = true;
        m_matchCreated = true;
        m_matchJoined = true;
        m_matchInfo = new MatchInfo(matchResponse);
        m_connectionEstablished = true;
    }

    void OnMatchJoined(JoinMatchResponse matchJoin)
    {
        if (!matchJoin.success)
        {
            Debug.LogError("Join match failed");
            return;
        }

        Debug.Log("Join match succeeded");

        try
        {
            // when called multiple times (disconnect/reconnect), you get this error:
            //  ArgumentException: An element with the same key already exists in the dictionary.
            //  System.Collections.Generic.Dictionary`2[UnityEngine.Networking.Types.NetworkID,UnityEngine.Networking.Types.NetworkAccessToken].Add (NetworkID key, UnityEngine.Networking.Types.NetworkAccessToken value) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Collections.Generic/Dictionary.cs:404)
            //  UnityEngine.Networking.Utility.SetAccessTokenForNetwork (NetworkID netId, UnityEngine.Networking.Types.NetworkAccessToken accessToken) (at C:/buildslave/unity/build/Runtime/Networking/Managed/UNETTypes.cs:119)
            //  NetworkRelay.OnMatchJoined (UnityEngine.Networking.Match.JoinMatchResponse matchJoin) (at Assets/Scripts/NetworkRelay.cs:186)
            //  UnityEngine.Networking.Match.NetworkMatch+<ProcessMatchResponse>c__Iterator0`1[UnityEngine.Networking.Match.JoinMatchResponse].MoveNext () (at C:/buildslave/unity/build/Runtime/Networking/Managed/MatchMakingClient.cs:302)

            // according to http://forum.unity3d.com/threads/where-is-the-documentation-for-utility-setaccesstokenfornetwork.401489/
            // this is a bug.  And it's safe to ignore the exception, which is what we are doing here.

            Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
        }
        catch (Exception)
        {
        }

        Debug.Log(string.Format("Connecting to Address: '{0}' Port: '{1}' NetworkID: '{2}' NodeID: '{3}'", matchJoin.address, matchJoin.port, matchJoin.networkId, matchJoin.nodeId));

        try
        {
            // when called multiple times (disconnect/reconnect), you get this error:
            //  ArgumentException: An element with the same key already exists in the dictionary.
            //  System.Collections.Generic.Dictionary`2[UnityEngine.Networking.Types.NetworkID,UnityEngine.Networking.Types.NetworkAccessToken].Add (NetworkID key, UnityEngine.Networking.Types.NetworkAccessToken value) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Collections.Generic/Dictionary.cs:404)
            //  UnityEngine.Networking.Utility.SetAccessTokenForNetwork (NetworkID netId, UnityEngine.Networking.Types.NetworkAccessToken accessToken) (at C:/buildslave/unity/build/Runtime/Networking/Managed/UNETTypes.cs:119)
            //  NetworkRelay.OnMatchJoined (UnityEngine.Networking.Match.JoinMatchResponse matchJoin) (at Assets/Scripts/NetworkRelay.cs:186)
            //  UnityEngine.Networking.Match.NetworkMatch+<ProcessMatchResponse>c__Iterator0`1[UnityEngine.Networking.Match.JoinMatchResponse].MoveNext () (at C:/buildslave/unity/build/Runtime/Networking/Managed/MatchMakingClient.cs:302)

            // according to http://forum.unity3d.com/threads/where-is-the-documentation-for-utility-setaccesstokenfornetwork.401489/
            // this is a bug.  And it's safe to ignore the exception, which is what we are doing here.

            Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
        }
        catch (Exception)
        {
        }

        m_client = new NetworkClient();
        m_client.RegisterHandler(MsgType.Connect, OnConnected);
        m_client.RegisterHandler(m_msgTypeString, OnString);
        m_client.Connect(new MatchInfo(matchJoin));

        m_isServer = false;
        m_matchJoined = true;
        m_matchInfo = new MatchInfo(matchJoin);
        m_connectionEstablished = true;
    }

    void OnMatchList(ListMatchResponse matchListResponse)
    {
        if (matchListResponse.success && matchListResponse.matches != null)
        {
            m_matchList = matchListResponse.matches;
        }
    }
#endif

    public static void ConnectServer()
    {
        Debug.LogFormat("ConnectServer()");

        //StartCoroutine(DisconnectAndConnectServerInternal());

        Disconnect();


        NetworkServer.Listen(9000);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnectServer);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnectServer);
        NetworkServer.RegisterHandler(MsgType.Error, OnErrorServer);
        NetworkServer.RegisterHandler(m_msgTypeStringToServer, OnStringToServer);

        m_client = ClientScene.ConnectLocalServer();
        m_client.RegisterHandler(MsgType.Connect, OnConnect);
        m_client.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        m_client.RegisterHandler(MsgType.Error, OnError);
        m_client.RegisterHandler(m_msgTypeString, OnString);

        m_isServer = true;
        //m_matchCreated = true;
        //m_matchJoined = true;
        //m_matchInfo = new MatchInfo(matchResponse);
        //m_connectionEstablished = true;
    }

    public static void ConnectClient()
    {
        Debug.LogFormat("ConnectClient()");

        //StartCoroutine(DisconnectAndConnectClientInternal(match));

        Disconnect();


        m_client = new NetworkClient();
        m_client.RegisterHandler(MsgType.Connect, OnConnect);
        m_client.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        m_client.RegisterHandler(MsgType.Error, OnError);
        m_client.RegisterHandler(m_msgTypeString, OnString);
        m_client.Connect("127.0.0.1", 9000);

        m_isServer = false;
        //m_matchJoined = true;
        //m_matchInfo = new MatchInfo(matchJoin);
        //m_connectionEstablished = true;
    }

    public static void Disconnect()
    {
        Debug.LogFormat("Disconnect()");

        //if (m_matchInfo != null)
        //    m_networkMatch.DropConnection(m_matchInfo.networkId, m_matchInfo.nodeId, OnConnectionDropped);

        if (m_client != null)
        {
            m_client.Disconnect();
            m_client.Shutdown();
            m_client = null;
        }

        if (m_isServer)
            NetworkServer.Shutdown();

        m_isServer = false;
        //m_matchCreated = false;
        //m_matchJoined = false;
        m_connectionEstablished = false;
    }

#if false
    public void RefreshRoomList()
    {
        m_networkMatch.ListMatches(0, 100, "", OnMatchList);
    }
#endif

    public static void SendNetworkMessage(string msg)
    {
        if (m_client == null)
            return;

        Debug.LogFormat("SendNetworkMessage() - {0}", msg);
        var stringMessage = new StringMessage(msg);
        m_client.Send(m_msgTypeStringToServer, stringMessage);
    }

    public static void AddMessageCallback(MessageEventHandler callback)
    {
        m_messageCallbacks.Add(callback);
    }

    public static void RemoveMessageCallback(MessageEventHandler callback)
    {
        m_messageCallbacks.Remove(callback);
    }

    public static void RemoveAllMessageCallbacks()
    {
        m_messageCallbacks.Clear();
    }

    static void OnConnect(NetworkMessage msg)
    {
        Debug.Log("OnConnect()");

        m_connectionEstablished = true;
    }

    static void OnDisconnect(NetworkMessage msg)
    {
        Debug.Log("OnDisconnect()");

        m_connectionEstablished = false;
    }

    static void OnError(NetworkMessage msg)
    {
        Debug.LogFormat("OnError() - {0}", msg);
    }

    static void OnConnectServer(NetworkMessage msg)
    {
        Debug.Log("OnConnectServer()");
    }

    static void OnDisconnectServer(NetworkMessage msg)
    {
        Debug.Log("OnDisconnectServer()");
    }

    static void OnErrorServer(NetworkMessage msg)
    {
        Debug.LogFormat("OnErrorServer() - {0}", msg);
    }

    static void OnStringToServer(NetworkMessage msg)
    {
        var stringMessage = msg.ReadMessage<StringMessage>();

        Debug.Log("OnStringToServer() - received: " + stringMessage.value);

        NetworkServer.SendToAll(m_msgTypeString, stringMessage);
    }

    static void OnString(NetworkMessage msg)
    {
        var stringMessage = msg.ReadMessage<StringMessage>();

        Debug.Log("OnString() - received: " + stringMessage.value);

        HandleNetworkMessage(stringMessage.value);
    }

#if false
    IEnumerator DisconnectAndConnectServerInternal()
    {
        Disconnect();

        if (m_connectionEstablished)
            yield return new WaitForEndOfFrame();

        string deviceNameFiltered = SystemInfo.deviceName;
        deviceNameFiltered = deviceNameFiltered.Replace("<", ""); // on android SystemInfo.deviceName is <unknown>
        deviceNameFiltered = deviceNameFiltered.Replace(">", "");
        deviceNameFiltered = Char.ToUpper(deviceNameFiltered[0]) + deviceNameFiltered.Substring(1).ToLower();
        string machineName = deviceNameFiltered;
        string dateTime = DateTime.Now.ToString("MM/dd h:mm:ss");
        m_matchName = string.Format("'{0}' {1}", machineName, dateTime);
        m_networkMatch.CreateMatch(m_matchName, 8, true, "", OnMatchCreated);
    }
#endif

#if false
    IEnumerator DisconnectAndConnectClientInternal(MatchDesc match)
    {
        Disconnect();

        if (m_connectionEstablished)
            yield return new WaitForEndOfFrame();

        m_matchName = match.name;
        m_networkMatch.JoinMatch(match.networkId, "", OnMatchJoined);
    }
#endif


    static void HandleNetworkMessage(string message)
    {
        foreach (var callback in m_messageCallbacks)
        {
            if (callback != null)
                callback(null, message);
        }
    }
}
