using System;
using System.Collections.Generic;

namespace Ens.Request
{
    namespace Client
    {
        public class SetInfo : RequestClient
        {
            protected internal override string Header => "R6";

            public static Action OnRecvReply;
            public static Action OnTimeOut;
            public static Action NotInRoomError;

            private static SetInfo Instance;
            internal SetInfo() : base()
            {
                Instance = this;
            }
            public static void SendRequest(Dictionary<string,string> pairs)//提供静态方法用于调用
            {
                Instance.SendRequest(Format.DictionaryToString(pairs));
            }
            protected override void Error(int code, string data)
            {
                NotInRoomError?.Invoke();
            }
            protected override void HandleReply(string data)
            {
                OnRecvReply?.Invoke();
            }
            protected internal override void TimeOut()
            {
                OnTimeOut?.Invoke();
            }
        }
    }
    namespace Server
    {
        internal class SetInfo : RequestServer
        {
            protected internal override string Header => "R6";
            protected internal override string HandleRequest(EnsConnection conn, string data)
            {
                if (conn.room == null) return ThrowError(0);
                var info=Format.StringToDictionary(data, t => t, t => t);
                foreach(var i in info.Keys)
                {
                    if (conn.room.Info.ContainsKey(i))conn.room.Info[i] = info[i];
                    else conn.room.Info.Add(i, info[i]);
                }
                return "empty";
            }
        }
    }
}