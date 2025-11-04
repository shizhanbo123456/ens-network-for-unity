using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ListenerUdp : ListenerBase
    {
        public UdpClient client;
        private int port;

        public Dictionary<IPEndPoint,ConnectionUdp>Connections=new Dictionary<IPEndPoint, ConnectionUdp>();

        public ListenerUdp(IPAddress ip,int port):base(ip,port)
        {
            this.port = port;
            client = new UdpClient(port);

            if (Protocol.mode == Mode.Multithreading)
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
            while (!Cancelled)
            {
                try
                {
                    var b = client.Receive(ref remoteEp);
                    string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);

                    if (!Connections.ContainsKey(remoteEp))
                    {
                        if (Listening)
                        {
                            var c = new ConnectionUdp();
                            c.Init(this, remoteEp);
                            Connections.Add(remoteEp, c);
                            Protocol.OnRecvConnection?.Invoke(c, ++connectionIndex);
                        }
                        else continue;
                    }
                    var conn = Connections[remoteEp];
                    foreach (var d in data)
                    {
                        if (conn.OnRecvPreProcess(d)) continue;
                        conn.RecvBuffer.Write(d);
                    }
                }
                catch
                {
                    
                }
            }
        }
        public async Task RecvAsync()
        {
            while (!Cancelled)
            {
                try
                {
                    var r = await client.ReceiveAsync();
                    var b = r.Buffer;
                    string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);

                    if (!Connections.ContainsKey(r.RemoteEndPoint))
                    {
                        if (Listening)
                        {
                            var c = new ConnectionUdp();
                            c.Init(this, r.RemoteEndPoint);
                            Connections.Add(r.RemoteEndPoint, c);
                            Protocol.OnRecvConnection?.Invoke(c, ++connectionIndex);
                        }
                        else continue;
                    }
                    var conn = Connections[r.RemoteEndPoint];
                    foreach (var d in data) conn.RecvBuffer.Write(d);
                }
                catch
                {

                }
            }
        }








        public override void StartListening()
        {
            Listening = true;
        }
        public override void EndListening()
        {
            Listening = false;
        }

        public override void Broadcast(string data)
        {
            throw new System.NotImplementedException();
        }
        public override void Broadcast(string data, int ignore)
        {
            throw new System.NotImplementedException();
        }
        public override void PTP(string data, List<int> targets)
        {
            throw new System.NotImplementedException();
        }
        public override void PTP(string data, int target)
        {
            throw new System.NotImplementedException();
        }


        public override void ShutDown()
        {
            base.ShutDown();
            foreach(var c in Connections)c.Value.ShutDown();
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