using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gitlab_ci_runner.api;
using gitlab_ci_runner.helper;
using Magnum.Extensions;
using Timer = System.Timers.Timer;

namespace gitlab_ci_runner.runner
{
    class Runner
    {
        /// <summary>
        /// Build process
        /// </summary>
        private static Build build = null;
        private static bool stopRequested = false;
        private static readonly Timer _poller = new Timer(5.Seconds().TotalMilliseconds);

        /// <summary>
        /// Start the configured runner
        /// </summary>
        public static void run()
        {
            stopRequested = false;
            _poller.Elapsed += (o, e) => PollForBuild();
            _poller.Start();
            Console.WriteLine("* Gitlab CI Runner started");
            Console.WriteLine("* Waiting for builds");
            //waitForBuild();
        }

        private static void PollForBuild()
        {
            if (completed || running)
            {
                updateBuild();
            }
            else
            {
                if (!stopRequested) getBuild();
            }
        }

        public static void stop()
        {
            _poller.Stop();
            stopRequested = true;
            while (running)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Build completed?
        /// </summary>
        public static bool completed
        {
            get
            {
                return running && build.completed;
            }
        }

        /// <summary>
        /// Build running?
        /// </summary>
        public static bool running
        {
            get
            {
                return build != null;
            }
        }

        /// <summary>
        /// Update the current running build progress
        /// </summary>
        private static void updateBuild()
        {
            if (build.completed)
            {
                // Build finished
                if (pushBuild())
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Completed build " + build.buildInfo.id);
                    build = null;
                }
            }
            else
            {
                // Build is currently running
                pushBuild();
            }
        }

        /// <summary>
        /// PUSH Build Status to Gitlab CI
        /// </summary>
        /// <returns>true on success, false on fail</returns>
        private static bool pushBuild()
        {
            return Network.pushBuild(build.buildInfo.id, build.state, build.output);
        }

        /// <summary>
        /// Get a new build job
        /// </summary>
        private static void getBuild()
        {
            var binfo = Network.getBuild();
            if (binfo == null) return;

            // Create Build Job
            build = new Build(binfo);
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Build " + binfo.id + " started...");
            var t = new Task(build.run);
            t.Start();
        }
    }
}
