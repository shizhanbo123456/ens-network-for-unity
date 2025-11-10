using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ClientUdp : ProtocolBase
    {
        protected UdpClient Client;
        private IPAddress ipAddress;
        private IPEndPoint ep;
        public new void Init(string ip,int port)
        {
            Client = new UdpClient(port);
            ipAddress=IPAddress.Parse(ip);
            ep=new IPEndPoint(ipAddress, port);
            Init(ip, port);
            Initialized = true;

            if (Protocol.mode == ConcurrentType.Multithreading)
            {
                Thread t = new Thread(new ThreadStart(Recv));
                t.Start();
            }
            else
            {
                _ = RecvAsync();
            }
        }
        public void Recv()
        {
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (On)
            {
                try
                {
                    var b = Client.Receive(ref remoteEp);
                    if (!(remoteEp.Address.Equals(ipAddress) && remoteEp.Port == Port)) continue;
                    OnRecvData(b);
                }
                catch
                {
                    
                }
            }
        }
        public async Task RecvAsync()
        {
            while (On)
            {
                try
                {
                    var r = await Client.ReceiveAsync();
                    if (r.RemoteEndPoint.Address.Equals(ipAddress) || r.RemoteEndPoint.Port != Port) continue;
                    var b = r.Buffer;
                    OnRecvData(b);
                }
                catch
                { 
                    
                }
            }
        }
        private void OnRecvData(byte[]b)
        {
            string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
            if (!rightFormat) return;
            var data = Format.Split(s);
            foreach (var d in data) ReceiveBuffer.Write(d);
        }
        public override CircularQueue<string> RefreshRecvBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperUdp未完成初始化");
                return null;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperUdp已被取消");
                return null;
            }
            return ReceiveBuffer;
        }


        public override void SendData(string data)
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperUdp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperUdp已被取消");
                return;
            }
            SendBuffer.Write(data);
        }
        public override void RefreshSendBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperUdp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperUdp已被取消");
                return;
            }
            if (SendBuffer == null || SendBuffer.Empty()) return;

            string data = Format.EnFormat(Format.Combine(SendBuffer));
            byte[] SendData = Format.GetBytes(data);
            try
            {
                Client.Send(SendData, SendData.Length,ep);
            }
            catch
            {

            }
        }

        public override void ShutDown()
        {
            Cancelled = true;
        }
        protected override void ReleaseManagedMenory()
        {
            Client.Dispose();
            base.ReleaseManagedMenory();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            Client = null;
            base.ReleaseUnmanagedMenory();
        }
    }
}