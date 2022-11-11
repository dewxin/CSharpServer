using System;
using System.Diagnostics;
using System.Threading;

namespace ProjectCommon.Unit
{
    public abstract class ThreadBase
    {
        public bool NeedStop { get; protected set; }

        //https://docs.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=net-6.0
        AutoResetEvent shutdownEvent = new AutoResetEvent(false);

        //可能会有多余的CPU空跑
        //用户A的数据唤醒了逻辑线程(已消耗)，逻辑线程开始取数据包。
        //此时用户B的数据抵达，再次唤醒(未消耗)，数据包也一并取走。
        //当逻辑处理完成，逻辑线程由于B导致的唤醒没有消耗，会再多跑一遍，但B的逻辑其实已经处理。
        AutoResetEvent wakeupEvent = new AutoResetEvent(false);

        public ThreadBase()
        {
        }

        public void Setup()
        {
            NeedStop = false;


            AfterSetup();
        }

        protected virtual void AfterSetup()
        {
        }

        public void Run(Object threadContext)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var lastTimeMs = stopwatch.Elapsed.TotalMilliseconds; 

            while (!NeedStop)
            {
                var nowTimeMs = stopwatch.Elapsed.TotalMilliseconds; 
                var elapsedMs = nowTimeMs - lastTimeMs;
                lastTimeMs = nowTimeMs;

#if DEBUG
                DoWorkInThreadLoop(elapsedMs);
#else
                try
                {
                    DoWorkInThreadLoop(elapsedMs);
                }
                catch (Exception ex)
                {
                    OnError(ex.ToString(), true);
                }
#endif

                wakeupEvent.WaitOne();
            }

            shutdownEvent.Set();
        }

        public void Wakeup()
        {
            wakeupEvent.Set();
        }

        protected abstract void DoWorkInThreadLoop(double elapsed);

        protected abstract void OnError(string errorInfo, bool needNotify = false);

        public void Stop()
        {
            NeedStop = true;
            shutdownEvent.WaitOne();
        }
    }
}
