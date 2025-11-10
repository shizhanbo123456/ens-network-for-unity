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
        }

        public abstract void StartListening();
        public abstract void EndListening();
        public virtual void ShutDown()
        {
            Listening = false;
            Cancelled = true;
        }
    }
}
