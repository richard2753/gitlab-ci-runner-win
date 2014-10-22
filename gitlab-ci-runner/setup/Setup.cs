﻿using System;
using System.Configuration;
using gitlab_ci_runner.conf;
using gitlab_ci_runner.helper;

namespace gitlab_ci_runner.setup
{
    class Setup
    {
        /// <summary>
        /// Start the Setup
        /// </summary>
        public static void run()
        {
            // Generate SSH Keys
            SSHKey.generateKeypair();

            // Register Runner
            registerRunner();
        }

        /// <summary>
        /// Register the runner with the coordinator
        /// </summary>
        private static void registerRunner()
        {
            // Read Token
            //string sToken = "";
            //while (sToken == "")
            //{
            //    Console.WriteLine("Please enter the gitlab-ci token for this runner:");
            //    sToken = Console.ReadLine();
            //}

            // Register Runner
            string sTok = Network.registerRunner(SSHKey.getPublicKey(), ConfigurationManager.AppSettings["gitlab-ci-token"]);
            if (sTok != null)
            {
                // Save Config
                Config.token = sTok;
                Config.saveConfig();

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
