using System;
using System.Collections.Generic;

namespace Ens.Request
{
    namespace Client
    {
        public class SetRule : RequestClient
        {
            protected internal override string Header => "R1";

            public static Action OnRecvReply;
            public static Action OnTimeOut;
            public static Action NotInRoomError;

            private static SetRule Instance;
            internal SetRule() : base()
            {
                Instance = this;
            }
            public static void SendRequest(Dictionary<string,(char,int)>info)//提供静态方法用于调用
            {
                string s = Format.DictionaryToString(info, valueconverter: t => t.Item1.ToString() + t.Item2);
                Instance.SendRequest(s);
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
}