using System;
using UnityEngine;

namespace ProtocolWrapper
{
    internal abstract class ProtocolBase : Disposable
    {
        public string IP;
        public int Port;
        protected CircularQueue<string> SendBuffer;
        protected CircularQueue<string> ReceiveBuffer;

        public bool Initialized=false;
        public bool Cancelled = false;
        public bool On
        {
            get
            {
                return Initialized && !Cancelled;
            }
        }

        public int Id;

        /// <summary>
        /// Fill data only
        /// </summary>
        protected void Init(string ip, int port)
        {
            SendBuffer = new CircularQueue<string>();
            ReceiveBuffer = new CircularQueue<string>();
            IP = ip;
            Port = port;

            Id = Protocol.id++;
        }

        //均为非阻塞，线程安全的
        public abstract void SendData(string data);
        public abstract void RefreshSendBuffer();
        public abstract CircularQueue<string> RefreshRecvBuffer();
        public abstract void ShutDown();

        protected override void ReleaseUnmanagedMenory()
        {
            SendBuffer=null; 
            ReceiveBuffer=null;
        }
    }
}
