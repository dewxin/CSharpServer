using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class CheckTimeHelper
    {
        public delegate void OnErrorFormat(string format, params object[] args);

        public static OnErrorFormat ErrorHandler { get; set; }

        public double startTick { get; set; }
        public double checkTick { get; set; }

        //todo 做成 using的形式
        public static CheckTimeHelper StartTime()
        {
            CheckTimeHelper timeHelper = new CheckTimeHelper();
            //todo 使用stopwatch
            timeHelper.startTick = Environment.TickCount;
            return timeHelper;
        }

        public bool IntervalGreaterThan(double intervalSecond = 0.1)
        {
            checkTick = (Environment.TickCount - startTick) / 1000.0;
            if (checkTick > intervalSecond)
            {
                return true;
            }
            return false;
        }

        public void Error(string info)
        {
            if (ErrorHandler == null)
                Console.WriteLine(string.Format("check: {0}, timeout: {1}", info, checkTick));
            else
            {
                //ErrorHandler.Invoke("check: {0}, timeout: {1}", info, checkTick);
                ErrorHandler("check: {0}, timeout: {1}", info, checkTick);
            }

        }
    }
}
