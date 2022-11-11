using ProjectCommon.Unit;
using System.Collections.Concurrent;
using System.Timers;

namespace ServerCommon.Timer
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-6.0
    public class TimerManager : ServerComponentBase
    {
        //已经到期可以在severThread中执行的事件
        //改为FastSafeQueue
        ConcurrentQueue<MyTimer> eventReadyToExecuteQueue { get; set; } = new();

        public TimerManager(ServerBase server) : base(server)
        {
        }

        //TimerManager.CreateTimerServerThread(ServerCheckTime, null, TryConnectWorldAndLogicServer);
        public void CreateTimerServerThread(int msInterval, object data, TimerCallback timerCallback)
        {
            var timer = new MyTimer(msInterval);
            timer.Data = data;
            timer.Callback = timerCallback;

            timer.Elapsed += (source, e) => 
            { 
                eventReadyToExecuteQueue.Enqueue(timer);
                ServerThread.Instance.Wakeup();
            };

            timer.AutoReset = true;
            timer.Enabled = true;
        }

        //public void CreateTimer(int msInterval, object data, TimerCallback timerCallback)
        //{
        //    var timer = new MyTimer(msInterval);
        //    timer.Data = data;

        //    timer.Elapsed += (source, e) => { timerCallback(data); };
        //    timer.AutoReset = true;
        //    timer.Enabled = true;
        //}


        //serverThread 同步执行
        public override void Tick(double elapsed)
        {
            if (eventReadyToExecuteQueue.IsEmpty)
                return;

            //同步执行
            MyTimer timer;
            while (eventReadyToExecuteQueue.TryDequeue(out timer!))
            {
                try
                {
                    timer.Callback(timer);
                }
                catch (Exception e)
                {
                    server.Logger.Error(e.ToString());
                }
            }
        }

    }

    public  class MyTimer : System.Timers.Timer 
    {
        public MyTimer(int msInterval) : base(msInterval) { }

        public object Data { get; set; }
        public TimerCallback Callback { get; set; }
    }
}