using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using gitlab_ci_runner.helper;

namespace gitlab_ci_runner.conf
{
    class Config
    {
        /// <summary>
        /// URL to the Gitlab CI coordinator
        /// </summary>
        public static string url {
            get { return ConfigurationManager.AppSettings["gitlab-ci-url"]; }
        }

        /// <summary>
        /// Gitlab CI runner auth token
        /// </summary>
        public static string token;

        /// <summary>
        /// Configuration Path
        /// </summary>
        private const string confPath = @"token.cfg";

        /// <summary>
        /// Load the configuration
        /// </summary>
        public static void loadConfig()
        {
            if (File.Exists(confPath))
            {
                token = File.ReadAllText(confPath);
            }
        }

        /// <summary>
        /// Save the configuration
        /// </summary>
        public static void saveConfig()
        {
            if (File.Exists(confPath))
            {
                File.Delete(confPath);
            }

            File.WriteAllText(confPath, token);
        }

        /// <summary>
        /// Is the runner already configured?
        /// </summary>
        /// <returns>true if configured, false if not</returns>
        public static bool isConfigured()
        {
            if (url != null && url != "" && token != null && token != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
