using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Udp
{
    internal abstract class WrapperUdp : ProtocolBase
    {
        public CircularQueue<string> RecvBuffer
        {
            get { return ReceiveBuffer; } 
        }
        protected UdpClient Client;
        protected IPAddress ipAddress;
        public void Init(UdpClient client,IPAddress ip,int port)
        {
            Client= client;
            Init(ip.ToString(), port);
            ipAddress=ip;
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

            string data = Format.EnFormat(Format.Combine(SendBuffer, Protocol.Separator));
            byte[] SendData = Format.GetBytes(data);
            try
            {
                Client.Send(SendData, SendData.Length, new IPEndPoint(ipAddress, Port));
            }
            catch
            {
                OnSendFailed();
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
            Client=null;
            base.ReleaseUnmanagedMenory();
        }
    }
}