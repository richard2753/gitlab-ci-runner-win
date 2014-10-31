using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using gitlab_ci_runner.conf;
using gitlab_ci_runner.runner;
using gitlab_ci_runner.setup;
using Topshelf;

namespace gitlab_ci_runner
{
    public class RunnerService : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            ServicePointManager.DefaultConnectionLimit = 999;

            if (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Substring(0, 1) == @"\")
            {
                Console.WriteLine("Can't run on UNC Path");
            }
            else
            {
                Console.WriteLine("Starting Gitlab CI Runner for Windows");
                Config.LoadConfig();

                if (!Config.IsConfigured())
                {
                    Setup.Run();
                }
                Runner.Run();
            }
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Runner.Stop();
            return true;
        }
    }
}