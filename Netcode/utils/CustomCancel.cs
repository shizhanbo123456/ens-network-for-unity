using System;

namespace Utils
{
    public class CustomCancel
    {
        public bool Valid
        {
            get
            {
                if (cancelByTime && Time.time >= targetCancelTime) return false;
                if (cancelByCondition && condition.Invoke()) return false;
                return true;
            }
        }
        private bool cancelByTime = false;
        private float targetCancelTime;
        public void CancelAfter(float time)
        {
            cancelByTime= true;
            targetCancelTime=Time.time+time;
        }
        private bool cancelByCondition = false;
        private Func<bool> condition;
        public void CancelWhen(Func<bool> cancelwhen)
        {
            cancelByCondition = true;
            condition = cancelwhen;
        }
    }
}