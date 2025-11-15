using System;

namespace Utils
{
    public class Time
    {
        private static DateTime Now;
        private static DateTime LastFrame;
        private static float m_deltaTime;


        public static float time
        {
            get
            {
                return (float)DateTime.Now.Subtract(Now).TotalSeconds;
            }
        }
        public static float deltaTime
        {
            get
            {
                return m_deltaTime;
            }
        }


        public static void Init()
        {
            Now = DateTime.Now;
            LastFrame = DateTime.Now;
        }
        public static void Update()
        {
            m_deltaTime = (float)DateTime.Now.Subtract(LastFrame).TotalSeconds;
            LastFrame = DateTime.Now;
        }
    }
}
