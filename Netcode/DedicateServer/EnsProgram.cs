using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class EnsProgram:MonoBehaviour
{
    public static EnsProgram Instance;
    public EnsDedicatedServer server;

    public float DisconnectThreshold = 3f;
    public float HeartbeatMsgInterval = 0.2f;

    public int delay=1;
    public int round=5;

    public bool PrintRoomData = true;
    public bool RoomDebug=true;

    //为空则视为IPAddress.Any
    public string IP;
    public int port = 12345;

    public ProtocolWrapper.ProtocolType ProtocolType= ProtocolWrapper.ProtocolType.TCP;

    private void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        EnsInstance.DisconnectThreshold = DisconnectThreshold;
        EnsInstance.HeartbeatMsgInterval = HeartbeatMsgInterval;

        Utils.Time.Init();

        ProtocolWrapper.Protocol.type = ProtocolType;

        IPAddress ip=IP==string.Empty ? IPAddress.Any :IPAddress.Parse(IP);
        server=new EnsDedicatedServer(ip,port);

        server.StartListening();
    }
    public void Update()
    {
        for(int i = 0; i < round; i++)
        {
            EnsEventRegister.LoopCommon();
            EnsEventRegister.LoopServer();

            server.Update();

            Thread.Sleep(delay);
        }
    }
}
