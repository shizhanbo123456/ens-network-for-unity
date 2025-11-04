using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ClientUdp : WrapperUdp
    {
        public new void Init(string ip,int port)
        {
            Init(new UdpClient(port), IPAddress.Parse(ip), port);
            Initialized = true;

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
            while (On)
            {
                try
                {
                    var b = Client.Receive(ref remoteEp);
                    if (!(remoteEp.Address.Equals(ipAddress) && remoteEp.Port == Port)) continue;
                    string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);
                    foreach (var d in data) RecvBuffer.Write(d);
                }
                catch
                {
                    OnReceiveFailed();
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
                    if (r.RemoteEndPoint.Address != ipAddress || r.RemoteEndPoint.Port != Port) continue;
                    var b = r.Buffer;
                    string s = Format.DeFormat(Format.GetString(b), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);
                    foreach (var d in data) RecvBuffer.Write(d);
                }
                catch
                { 
                    OnReceiveFailed(); 
                }
            }
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