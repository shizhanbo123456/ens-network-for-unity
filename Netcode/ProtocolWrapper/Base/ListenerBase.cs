using System;
using System.Collections.Generic;
using System.Net;

namespace ProtocolWrapper
{
    internal abstract class ListenerBase:Disposable
    {
        protected IPAddress IP;
        protected int Port;
        protected int connectionIndex=0;
        public bool Listening=false;
        public bool Cancelled=false;

        public int Id;
        public ListenerBase(IPAddress iP, int port)
        {
            IP = iP;
            Port = port;

            Id=Protocol.id++;

            Protocol.ListenerBases.Add(this);
        }

        public abstract void StartListening();
        public abstract void EndListening();
        public virtual void Update()
        {
            
        }
        public virtual void ShutDown()
        {
            Listening = false;
            Cancelled = true;
        }
        public abstract void Broadcast(string data);
        public abstract void Broadcast(string data,int ignore);
        public abstract void PTP(string data,List<int>targets);
        public abstract void PTP(string data, int target);
    }
}
