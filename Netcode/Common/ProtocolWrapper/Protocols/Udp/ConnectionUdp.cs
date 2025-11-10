using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ConnectionUdp : ProtocolBase
    {
        public CircularQueue<string> RecvBuffer
        {
            get { return ReceiveBuffer; }
        }
        private IPEndPoint EP;
        private ListenerUdp Listener;
        protected UdpClient Client;
        protected IPAddress ipAddress;
        public void Init(ListenerUdp listener, IPEndPoint ep)
        {
            EP = ep;
            Listener = listener;
            Client = listener.client;
            ipAddress=ep.Address;
            Init(ep.Address.ToString(),ep.Port);
            Initialized = true;
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
                Client.Send(SendData, SendData.Length, new IPEndPoint(ipAddress, Port));
            }
            catch
            {

            }
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


        public override void ShutDown()
        {
            Cancelled = true;
            Listener.Connections.Remove(EP);
        }
        protected override void ReleaseManagedMenory()
        {
            Client.Dispose();
            base.ReleaseManagedMenory();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            EP = null;
            Listener = null;
            Client = null;
            ipAddress = null;
            base.ReleaseUnmanagedMenory();
        }
    }
}