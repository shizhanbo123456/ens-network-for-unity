using Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtocolWrapper.Protocols.Tcp
{
    internal abstract class WrapperTcp : ProtocolBase
    {
        protected TcpClient Client;
        protected NetworkStream Stream;

        /// <summary>
        /// 需要client已经初始化
        /// </summary>
        protected void Init(TcpClient client)
        {
            Client = client;
            Stream = client.GetStream();
            Init((Client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (Client.Client.RemoteEndPoint as IPEndPoint).Port);
        }
        protected virtual void Receive()
        {
            byte[] buffer = new byte[Protocol.BufferLength];
            while (!Cancelled)
            {
                try
                {
                    int bytesRead = Stream.Read(buffer, 0, buffer.Length);
                    OnRecvData(buffer, bytesRead);
                }
                catch (Exception)
                {
                    
                }
            }
        }

        protected async Task ReceiveAsync()
        {
            byte[] buffer = new byte[Protocol.BufferLength];
            while (!Cancelled)
            {
                try
                {
                    int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
                    OnRecvData(buffer, bytesRead);
                }
                catch (Exception)
                {
                    
                }
            }
        }
        private void OnRecvData(byte[] buffer,int bytesRead)
        {
            if (bytesRead == 0) return;
            string s = Format.DeFormat(Format.GetString(buffer, 0, bytesRead), out bool rightFormat);
            if (!rightFormat) return;
            var data = Format.Split(s);
            foreach (var d in data)
            {
                ReceiveBuffer.Write(d);
            }
        }

        public override void SendData(string data)
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return;
            }
            SendBuffer.Write(data);
        }

        public override CircularQueue<string> RefreshRecvBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return null;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return null;
            }
            return ReceiveBuffer;
        }

        public override void RefreshSendBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return;
            }
            if (SendBuffer==null||SendBuffer.Empty()) return;
            
            string data = Format.EnFormat(Format.Combine(SendBuffer));

            byte[] SendData = Format.GetBytes(data);
            try
            {
                Stream.Write(SendData, 0, SendData.Length);
            }
            catch (Exception)
            {
                
            }
        }
        public override void ShutDown()
        {
            Cancelled= true;
            Stream?.Close();
            Client?.Close();
        }
        protected override void ReleaseManagedMenory()
        {
            Stream?.Dispose();
            Client?.Dispose();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            base.ReleaseUnmanagedMenory();
            Stream = null;
            Client = null;
        }
    }
}
