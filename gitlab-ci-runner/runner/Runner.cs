using System;
using System.Threading;
using System.Threading.Tasks;
using gitlab_ci_runner.helper;
using Magnum.Extensions;
using Timer = System.Timers.Timer;

namespace gitlab_ci_runner.runner
{
    class Runner
    {
        private static Build _build = null;
        private static bool _stopRequested = false;
        private static bool _polling = false;
        private static readonly Timer Poller = new Timer(5.Seconds().TotalMilliseconds);

        public static void Run()
        {
            _stopRequested = false;
            Poller.Elapsed += (o, e) => PollForBuild();
            Poller.Start();
            Console.WriteLine("* Gitlab CI Runner started");
            Console.WriteLine("* Waiting for builds");
        }

        private static void PollForBuild()
        {
            if (_polling)
                return;
            _polling = true;
            try
            {
                if (Running)
                {
                    UpdateBuild();
                }
                else
                {
                    if (!_stopRequested) GetBuild();
                }
            }
            finally
            {
                _polling = false;
            }
        }

        public static void Stop()
        {
            Poller.Stop();
            _stopRequested = true;
            while (Running)
            {
                Thread.Sleep(1000);
            }
        }

        public static bool Completed
        {
            get
            {
                return Running && _build.Completed;
            }
        }

        public static bool Running
        {
            get
            {
                return _build != null;
            }
        }

        private static void UpdateBuild()
        {
            if (Completed)
            {
                if (_build.PushProgress())
                {
                    Console.WriteLine("[" + DateTime.Now + "] Completed build " + _build.Id);
                    _build = null;
                }
            }
            else
            {
                _build.PushProgress();
            }
        }

        private static void GetBuild()
        {
            var binfo = Network.GetBuild();
            if (binfo == null) return;

            _build = new Build(binfo);
            Console.WriteLine("[" + DateTime.Now + "] Build " + _build.Id + " started...");
            var t = new Task(_build.Run);
            t.Start();
        }
    }
}
