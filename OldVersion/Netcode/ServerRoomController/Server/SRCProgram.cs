using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class SRCProgram:MonoBehaviour
{
    public static SRCProgram Instance;

    public float DisconnectThreshold = 3f;
    public float HeartbeatMsgInterval = 0.2f;

    public int delay=1;
    public int round=5;

    public bool PrintRoomData = true;
    public bool RoomDebug=true;

    [Tooltip("为空则视为IPAddress.Any")]
    public string IP;
    public int port = 12345;

    public ProtocolWrapper.ProtocolType ProtocolType= ProtocolWrapper.ProtocolType.TCP;

    private void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        ENCInstance.DisconnectThreshold = DisconnectThreshold;
        ENCInstance.HeartbeatMsgInterval = HeartbeatMsgInterval;

        Utils.Time.Init();

        ProtocolWrapper.Protocol.type = ProtocolType;

        IPAddress ip=IP==string.Empty ? IPAddress.Any :IPAddress.Parse(IP);
        new SRCDedicatedServer(ip,port);

        SRCDedicatedServer.Instance.StartListening();
    }
    public void Update()
    {
        for(int i = 0; i < round; i++)
        {
            Utils.Time.Update();
            SRCDedicatedServer.Instance.Update();

            Thread.Sleep(delay);
        }
    }
}
