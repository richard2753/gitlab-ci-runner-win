using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Reflection;
using gitlab_ci_runner.conf;
using gitlab_ci_runner.runner;
using gitlab_ci_runner.setup;
using Topshelf;

namespace gitlab_ci_runner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("-sslbypass"))
            {
                RegisterSecureSocketsLayerBypass();
            }
            HostFactory.Run(host =>
            {
                host.SetDisplayName("GitLab CI Windows Runner");
                host.RunAsLocalSystem();
                host.Service<RunnerService>();
            });
        }

		static void RegisterSecureSocketsLayerBypass()
		{
			ServicePointManager.ServerCertificateValidationCallback +=
			    (sender, certificate, chain, sslPolicyErrors) => true;
		}
    }

    public class RunnerService : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            Console.InputEncoding = Encoding.Default;
            Console.OutputEncoding = Encoding.Default;
            ServicePointManager.DefaultConnectionLimit = 999;

            if (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Substring(0, 1) == @"\")
            {
                Console.WriteLine("Can't run on UNC Path");
            }
            else
            {
                Console.WriteLine("Starting Gitlab CI Runner for Windows");
                Config.loadConfig();

                if (!Config.isConfigured())
                {
                    // Load the setup
                    Setup.run();
                }

                if (Config.isConfigured())
                {
                    // Load the runner
                    Runner.run();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Runner quit. Press any key to exit!");
            //Console.ReadKey();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Runner.stop();
            return true;
        }
    }


}
