using ProtocolWrapper.Protocols.Tcp;
using ProtocolWrapper.Protocols.Udp;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace ProtocolWrapper
{
    public enum Mode
    {
        Multithreading, Asynchronous
    }
    public enum ProtocolType
    {
        TCP, UDP
    }
    internal class Protocol
    {
        internal static int BufferLength = 400;
        internal static char Separator = '*';

        internal static bool DevelopmentDebug = true;
        internal static bool ShowGeneralEvent = true;

        /// <summary>
        /// 标识，所有的Listener和Protocol的id都不一样
        /// </summary>
        internal static int id = 0;

        //Events
        internal static ProtocolEventBuffer OnClientConnectFailed = new ProtocolEventBuffer();
        internal static ProtocolEventBuffer OnClientReceiveFailed = new ProtocolEventBuffer();
        internal static ProtocolEventBuffer OnClientSendFailed = new ProtocolEventBuffer();
        internal static ProtocolEventBuffer OnServerReceiveFailed = new ProtocolEventBuffer();
        internal static ProtocolEventBuffer OnServerSendFailed = new ProtocolEventBuffer();

        internal static Action<ProtocolBase, int> OnRecvConnection;

        internal static Func<string, bool> ClientRecvImmediateInvoke;
        internal static Func<string, bool> ConnectionRecvImmediateInvoke;

        internal static Mode mode=Mode.Multithreading;
        internal static ProtocolType type;

        internal static object basesLock=new object();
        internal static List<ProtocolBase> ProtocolBases = new List<ProtocolBase>();
        internal static List<ListenerBase> ListenerBases= new List<ListenerBase>();

        internal static List<IProtocolCommandBuffer> Triggers;

        

        
        private static bool initialized = false;
        private static void Init()
        {
            if (initialized) return;
            Utils.Time.Init();
            initialized = true;
        }
        internal static void Update()
        {
            if (!initialized) Init();

            DisposeCheck();//必须在Update之前
            Utils.Time.Update();
            foreach(var t in Triggers)t.InMainThread();
            UpdateListener();

            Broadcast.Update();
        }
        private static void DisposeCheck()
        {
            lock (basesLock)
            {
                for (int i = ProtocolBases.Count - 1; i >= 0; i--)
                {
                    if (ProtocolBases[i] == null || ProtocolBases[i].Cancelled)
                    {
                        ProtocolBases[i].Dispose();
                        ProtocolBases.RemoveAt(i);
                    }
                }
                for (int i = ListenerBases.Count - 1; i >= 0; i--)
                {
                    if (ListenerBases[i] == null || ListenerBases[i].Cancelled)
                    {
                        ListenerBases[i].Dispose();
                        ListenerBases.RemoveAt(i);
                    }
                }
            }
        }
        private static void UpdateListener()
        {
            foreach (var b in ListenerBases) b.Update();
        }


        internal static ProtocolBase GetClient(string ip,int port)
        {
            switch (type)
            {
                case ProtocolType.TCP: return ModuleTcp.GetProtocolBase(ip,port);
                case ProtocolType.UDP: return ModuleUdp.GetProtocolBase(ip, port);
            }
            Debug.LogError("未注册的协议");
            return null;
        }
        internal static ListenerBase GetListener(IPAddress ip,int port)
        {
            switch (type)
            {
                case ProtocolType.TCP: return ModuleTcp.GetListener(ip, port);
                case ProtocolType.UDP: return ModuleUdp.GetListener(ip, port);
            }
            Debug.LogError("未注册的协议");
            return null;
        }
    }
}
