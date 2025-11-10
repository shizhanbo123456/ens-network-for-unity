using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// SRC框架用于实现远程服务器的交互和房间管理<br></br>
/// 对于SRC层，加入时不会获取id或调用ENC事件(仅仅触发SRC事件)，而是加入房间时获取房间内id并触发ENC事件<br></br>
/// 连接成功和加入房间(分配id)之间只会发送请求信息<br></br>
/// ENC层的服务器相当于SRC层的房间<br></br>
/// 连接时，先触发SRC连接，加入房间后，先分配id(房间和客户端的，防bug)，再促发ENC连接和Id分配<br></br>
/// 断开时如果退出了房间则触发ENC断开，之后触发SRC断开
/// </summary>
public class SRCCorrespondent : NOMCorrespondent
{
    [Space]
    [Header("SRC")]
    [SerializeField] private string serverIP = "127.0.0.1";
    public string ServerIP
    {
        get { return serverIP; }
        set 
        { 
            serverIP = value;
            if(ENCInstance.ShowGeneralEvent)Debug.Log("[S]SRC IP设置为" + value); 
        }
    }
    public int ServerPort = 12345;
    protected override void ApplySettings()
    {
        SRCInstance.SRCCorrespondent = this;
        base.ApplySettings();
    }
    protected override void ApplyEvents()
    {
        new SRCEventRegister().RegistEvents();
    }
    public virtual void StartSRCClient()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[S]已启动，关闭后才可调用");
            return;
        }
        if (!SRCDataCheck())
        {
            Debug.Log("[S]输入的IP或端口有误");
            return;
        }

        try
        {
            SRCInstance.SRCOn = true;
            networkMode = NetworkMode.Client;
            ENCClient = new ENCClient(ServerIP, ServerPort);
        }
        catch (Exception e)
        {
            Debug.LogError("[S]客户端启动失败，IP="+ServerIP+" Port="+ServerPort+" Log:"+e.ToString());
        }
    }
    private bool SRCDataCheck()
    {
        return IPAddress.TryParse(ServerIP, out _) && ServerPort >= 0 && ServerPort <= 65535;
    }

    public override void ShutDown()
    {
        base.ShutDown();
        if (SRCInstance.DisconnectInvoke == false)
        {
            SRCInstance.OnServerDisconnect.Invoke();
        }
        SRCInstance.SRCOn = false;
        SRCInstance.SRCHost = false;
        SRCInstance.PresentRoomId = 0;
    }

    /// <summary>
    /// >0加入 ==0创建 <0离开
    /// </summary>
    public void JoinRoom(int id)
    {
        if(networkMode != NetworkMode.Client)
        {
            Debug.Log("[S]未启动客户端模式");
            return;
        }
        if (id >= 0 && SRCInstance.PresentRoomId != 0)
        {
            Debug.LogError("[S]已加入房间，无法再加入");
            return;
        }
        if (id < 0 && SRCInstance.PresentRoomId == 0)
        {
            Debug.LogError("[S]未加入房间，无法离开");
            return;
        }
        ENCClient.SendData("kR]"+id);
    }
}
