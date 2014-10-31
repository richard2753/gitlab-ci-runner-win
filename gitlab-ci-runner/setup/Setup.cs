using System;
using System.Configuration;
using gitlab_ci_runner.conf;
using gitlab_ci_runner.helper;

namespace gitlab_ci_runner.setup
{
    class Setup
    {
        public static void Run()
        {
            SshKey.GenerateKeypair();

            RegisterRunner();
        }

        private static void RegisterRunner()
        {
            string token = Network.RegisterRunner(SshKey.GetPublicKey(), ConfigurationManager.AppSettings["gitlab-ci-token"]);
            if (token != null)
            {
                Config.Token = token;
                Config.SaveConfig();

                Console.WriteLine();
                Console.WriteLine("Runner registered successfully. Feel free to start it!");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Failed to register this runner. Perhaps your SSH key is invalid or you are having network problems");
                throw new Exception("Failed to register runner");
            }
        }
    }
}
