using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ProtocolWrapper.Protocols.Tcp
{
    internal class ConnectionTcp : WrapperTcp
    {
        public ListenerTcp Server;

        public void Init(TcpClient client,ListenerTcp server)
        {
            Server=server;
            Init(client);
            Initialized = true;

            if (Protocol.mode == ConcurrentType.Multithreading)
            {
                Thread recv = new Thread(new ThreadStart(Receive));
                recv.Start();
            }
            else
            {
                _ = ReceiveAsync();
            }
        }
        public override void ShutDown()
        {
            base.ShutDown();
            Server.Connections.Remove(Id);
        }
        protected override void ReleaseUnmanagedMenory()
        {
            base.ReleaseUnmanagedMenory();
            Server = null;
        }
    }
}
