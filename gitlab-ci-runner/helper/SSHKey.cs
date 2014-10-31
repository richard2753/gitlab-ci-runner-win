﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace gitlab_ci_runner.helper
{
    class SshKey
    {
        /// <summary>
        /// Generate a keypair
        /// </summary>
        public static void GenerateKeypair()
        {
            // We need both, the Public and Private Key
            // Public Key to send to Gitlab
            // Private Key to connect to Gitlab later
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa.pub") &&
                File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa"))
            {
                return;
            }

            try {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh");
            } catch (Exception) {}
            Process p = new Process();
            p.StartInfo.FileName = "ssh-keygen";
            p.StartInfo.Arguments = "-t rsa -f \"" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.ssh\\id_rsa\" -N \"\"";
            p.Start();
            Console.WriteLine();
            Console.WriteLine("Waiting for SSH Key to be generated ...");
            p.WaitForExit();
            while (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa.pub") &&
                   !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa"))
            { 
                Thread.Sleep(1000);
            }
            Console.WriteLine("SSH Key generated successfully!");
        }

        /// <summary>
        /// Get the public key
        /// </summary>
        /// <returns></returns>
        public static string GetPublicKey()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa.pub"))
            {
                return File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ssh\id_rsa.pub");
            }
            else
            {
                return null;
            }
        }
    }
}
