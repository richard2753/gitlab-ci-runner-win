using System.Configuration;
using System.IO;

namespace gitlab_ci_runner.conf
{
    class Config
    {
        /// <summary>
        /// URL to the Gitlab CI coordinator
        /// </summary>
        public static string Url {
            get { return ConfigurationManager.AppSettings["gitlab-ci-url"]; }
        }

        /// <summary>
        /// Gitlab CI runner auth token
        /// </summary>
        public static string Token;

        /// <summary>
        /// Configuration Path
        /// </summary>
        private const string ConfPath = @"token.cfg";

        /// <summary>
        /// Load the configuration
        /// </summary>
        public static void LoadConfig()
        {
            if (File.Exists(ConfPath))
            {
                Token = File.ReadAllText(ConfPath);
            }
        }

        /// <summary>
        /// Save the configuration
        /// </summary>
        public static void SaveConfig()
        {
            if (File.Exists(ConfPath))
            {
                File.Delete(ConfPath);
            }

            File.WriteAllText(ConfPath, Token);
        }

        /// <summary>
        /// Is the runner already configured?
        /// </summary>
        /// <returns>true if configured, false if not</returns>
        public static bool IsConfigured()
        {
            if (!string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Token))
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
