using System;
using System.Collections.Generic;

namespace Ens.Request
{
    namespace Client
    {
        public class GetInfo : RequestClient
        {
            protected internal override string Header => "R7";

            public static Action<Dictionary<int,string>> OnRecvReply;
            public static Action OnTimeOut;
            public static Action RoomNotFoundError;

            private static GetInfo Instance;
            internal GetInfo() : base()
            {
                Instance = this;
            }
            public static void SendRequest(List<int>ids)//提供静态方法用于调用
            {
                Instance.SendRequest(Format.ListToString(ids));
            }
            protected override void Error(int code, string data)
            {
                RoomNotFoundError?.Invoke();
            }
            protected override void HandleReply(string data)
            {
                OnRecvReply?.Invoke(Format.StringToDictionary(data,int.Parse,s=>s));
            }
            protected internal override void TimeOut()
            {
                OnTimeOut?.Invoke();
            }
        }
    }
    namespace Server
    {
        internal class GetInfo : RequestServer
        {
            protected internal override string Header => "R7";
            protected internal override string HandleRequest(EnsConnection conn, string data)
            {
                List<int> ids = Format.StringToList(data, int.Parse);
                Dictionary<int, string> r = new Dictionary<int, string>();
                foreach (var i in ids)
                {
                    if (EnsRoomManager.Instance.rooms.TryGetValue(i, out var room))
                    {
                        r.Add(i, Format.DictionaryToString(room.Info));
                    }
                }
                return Format.DictionaryToString(r);
            }
        }
    }
}
