using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProtocolWrapper.Protocols.Udp
{
    /// <summary>
    /// 需要调用StartListening/EndListening<br></br>
    /// </summary>
    internal class ListenerUdp : ListenerBase
    {
        public UdpClient client;

        public Dictionary<IPEndPoint,ConnectionUdp>Connections=new Dictionary<IPEndPoint, ConnectionUdp>();

        public ListenerUdp(IPAddress ip,int port):base(ip,port)
        {
            client = new UdpClient(port);
            Listening = false;
        }
        public override void StartListening()
        {
            Listening = true; 
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
        public override void EndListening()
        {
            Listening = false;
        }


        public void Recv()
        {
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (Listening)
            {
                try
                {
                    var b = client.Receive(ref remoteEp);
                    if (!Listening) return;
                    OnRecvData(b, remoteEp);
                }
                catch
                {
                    
                }
            }
        }
        public async Task RecvAsync()
        {
            while (Listening)
            {
                try
                {
                    var r = await client.ReceiveAsync();
                    if (!Listening) return;
                    var b = r.Buffer;
                    OnRecvData(b,r.RemoteEndPoint);
                }
                catch
                {

                }
            }
        }
        private void OnRecvData(byte[] b,IPEndPoint ep)
        {
            string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
            if (!rightFormat) return;
            var data = Format.Split(s);
            if (!Connections.ContainsKey(ep))
            {
                var c = new ConnectionUdp();
                c.Init(this, ep);
                Connections.Add(ep, c);
                Protocol.OnRecvConnection?.Invoke(c, ++connectionIndex);
                if (Protocol.DevelopmentDebug) Utils.Debug.Log("[W]有新客户端连接");
            }
            var conn = Connections[ep];
            foreach (var d in data) conn.RecvBuffer.Write(d);
        }


        public override void ShutDown()
        {
            Listening = false;
            Cancelled = true;
            foreach (var c in Connections)c.Value.ShutDown();
        }


        protected override void ReleaseManagedMenory()
        {
            client.Dispose();
            foreach (var c in Connections) c.Value.Dispose();
            Connections.Clear();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            client = null;
            Connections = null;
        }
    }
}