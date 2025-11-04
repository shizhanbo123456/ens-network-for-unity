using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ProtocolWrapper.Protocols.Tcp
{
    /// <summary>
    /// 成功连接后立即初始化,不会出现初始化失败<br></br>
    /// 接收时报错会调用OnServerReceiveFailed，需要调用Update
    /// </summary>
    internal class ConnectionTcp : WrapperTcp
    {
        public ListenerBase Server;


        public void Init(TcpClient client,ListenerBase server)
        {
            Server=server;
            Init(client);
            Initialized = true;
            if (Protocol.mode == Mode.Multithreading)
            {
                Thread recv = new Thread(new ThreadStart(Receive));
                recv.Start();
            }
            else
            {
                _ = ReceiveAsync();
            }
        }
        public override bool OnRecvPreProcess(string data)
        {
            if (Protocol.ConnectionRecvImmediateInvoke != null)
            {
                return Protocol.ConnectionRecvImmediateInvoke.Invoke(data);
            }
            return false;
        }
        protected override void ReleaseUnmanagedMenory()
        {
            base.ReleaseUnmanagedMenory();
            Server = null;
        }
    }
}
