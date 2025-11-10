using ProtocolWrapper.Protocols.Tcp;
using ProtocolWrapper.Protocols.Udp;
using System;
using System.Net;

namespace ProtocolWrapper
{
    public enum ConcurrentType
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

        internal static bool DevelopmentDebug = true;

        /// <summary>
        /// 标识，所有的Listener和Protocol的id都不一样
        /// </summary>
        internal static int id = 0;

        internal static Action<ProtocolBase, int> OnRecvConnection;
        internal static Action OnClientInitialized;//只会在客户端调用

        internal static Func<string, bool> ClientRecvImmediateInvoke;
        internal static Func<string, bool> ConnectionRecvImmediateInvoke;

        internal static ConcurrentType mode=ConcurrentType.Multithreading;
        internal static ProtocolType type;


        /// <summary>
        /// 失败会返回null
        /// </summary>
        internal static ProtocolBase GetClient(string ip,int port)
        {
            switch (type)
            {
                case ProtocolType.TCP: return ModuleTcp.GetProtocolBase(ip,port);
                case ProtocolType.UDP: return ModuleUdp.GetProtocolBase(ip, port);
            }
            Utils.Debug.LogError("未注册的协议");
            return null;
        }
        /// <summary>
        /// 失败会返回null
        /// </summary>
        internal static ListenerBase GetListener(IPAddress ip,int port)
        {
            switch (type)
            {
                case ProtocolType.TCP: return ModuleTcp.GetListener(ip, port);
                case ProtocolType.UDP: return ModuleUdp.GetListener(ip, port);
            }
            Utils.Debug.LogError("未注册的协议");
            return null;
        }
    }
}
