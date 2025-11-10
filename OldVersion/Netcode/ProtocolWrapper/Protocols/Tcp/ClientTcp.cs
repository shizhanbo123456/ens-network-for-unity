using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProtocolWrapper.Protocols.Tcp
{
    /// <summary>
    /// 需要单独调用Init，需要等待初始化，若初始化失败会调用OnClientConnectFailed<br></br>
    /// 接收时报错会调用OnClientReceiveFailed，需要调用Update
    /// </summary>
    internal class ClientTcp : WrapperTcp
    {
        public new void Init(string ip, int port)
        {
            IP=ip;
            Port=port;
            Initialized = true;
            if (Protocol.mode == Mode.Multithreading)
            {
                Thread init = new Thread(new ThreadStart(Init));
                init.Start();
            }
            else
            {
                _ = InitAsync();
            }
        }

        private void Init()
        {
            try
            {
                Client = new TcpClient(IP, Port);
            }
            catch
            {
                Protocol.OnClientConnectFailed.SetTrigger();
                return;
            }

            Init(Client);
            
            Thread Recv = new Thread(new ThreadStart(Receive));
            Recv.Start();
        }
        private async Task InitAsync()
        {
            Client = new TcpClient();
            try
            {
                await Client.ConnectAsync(IP, Port);
            }
            catch
            {
                Protocol.OnClientConnectFailed.SetTrigger();
                return;
            }


            Init(Client);

            _ = ReceiveAsync();
        }

        public override bool OnRecvPreProcess(string data)
        {
            if (Protocol.ClientRecvImmediateInvoke != null)
            {
                return Protocol.ClientRecvImmediateInvoke.Invoke(data);
            }
            return false;
        }
        public override void ShutDown()
        {
            base.ShutDown();
        }
    }
}
