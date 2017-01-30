using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <remarks>
/// Implements VHMsg style connect/send/receive functionality, but using Unity's Network classes
/// instead of VHMsg itself.  Used in VHMsgEmulator, and used in the standalone app for acting as the middleman between VHMsg and Unity.
/// </remarks>
public class VHMsgNetwork : NetworkBehaviour
{
    public DebugConsole console;
    public VHMsgBase vhmsg;

    public void Start()
    {
        Application.runInBackground = true;
        console = DebugConsole.Get();

        if (vhmsg == null)
        {
            vhmsg = VHMsgBase.Get();
        }

        if (console == null)
        {
            console = FindObjectOfType<DebugConsole>();
        }

        if (vhmsg != null)
        {
#if !UNITY_WEBGL
            if (Network.isClient)
            {
                vhmsg.SubscribeMessage("*");
                vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));
                console.AddCommandCallback("vhmsg", new DebugConsole.ConsoleCallback(HandleConsoleMessage));
            }
#endif
        }
        else
        {
            Debug.LogError("vhmsgGO needs to have a monobehaviour script that implements the IVHMsg interface");
        }
        //Network.logLevel = NetworkLogLevel.Full;
    }

    void VHMsg_MessageEvent(object sender, VHMsgBase.Message message)
    {
        RpcReceiveVHMsgFromServer(message.s);
        Debug.Log("Server Received vhmsg " + message.s);
    }

    [ClientRpc]
    public void RpcReceiveVHMsgFromServer(string opandarg)
    {
        vhmsg.ReceiveVHMsg(opandarg);
    }

    [Command]
    public void CmdSendVHMsgToServer(string opandarg)
    {
        vhmsg.SendVHMsg(opandarg);
    }


    /// <summary>
    /// called from the console when a 'vhmsg' prefixed command is sent
    /// </summary>
    /// <param name="commandEntered"></param>
    /// <param name="console"></param>
    void HandleConsoleMessage(string commandEntered, DebugConsole console)
    {
        /*if (commandEntered.IndexOf("vhmsg") != -1)
        {
            string opCode = string.Empty;
            string args = string.Empty;
            if (console.ParseVHMSG(commandEntered, ref opCode, ref args))
            {
#if !UNITY_WEBGL
                if (Network.isServer)
                {
                    vhmsg.SendVHMsg(opCode, args);
                }
                else
                {
                    CmdSendVHMsgToServer(opCode + " " + args);
                }
#endif
            }
            else
            {
                console.AddTextToLog(commandEntered + " requires an opcode string and can have an optional argument string");
            }
        }*/
    }

    public override void OnStartClient ()
    {
        base.OnStartClient ();
        VHMsgEmulator emulator = FindObjectOfType<VHMsgEmulator>();
        emulator.network = this;
    }
}
