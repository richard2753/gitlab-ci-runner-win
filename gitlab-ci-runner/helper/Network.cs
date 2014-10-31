using System;
using System.Net;
using System.Text;
using System.Threading;
using gitlab_ci_runner.api;
using gitlab_ci_runner.conf;
using gitlab_ci_runner.runner;
using ServiceStack;

namespace gitlab_ci_runner.helper
{
    class Network
    {
        /// <summary>
        /// Gitlab CI API URL
        /// </summary>
        private static string ApiUrl
        {
            get
            {
                return Config.Url + "/api/v1/";
            }
        }

        /// <summary>
        /// Register the runner with the coordinator
        /// </summary>
        /// <param name="sPubKey">SSH Public Key</param>
        /// <param name="sToken">Token</param>
        /// <returns>Token</returns>
        public static string RegisterRunner(string sPubKey, string sToken)
        {
            var client = new JsonServiceClient(ApiUrl);
            try
            {
                var authToken = client.Post(new RegisterRunner
                {
                    token = Uri.EscapeDataString(sToken),
                    public_key = Uri.EscapeDataString(sPubKey)
                });

                if (!authToken.token.IsNullOrEmpty())
                {
                    Console.WriteLine("Runner registered with id {0}", authToken.id);
                    return authToken.token;
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error while registering runner :", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get a new build
        /// </summary>
        /// <returns>BuildInfo object or null on error/no build</returns>
        public static BuildInfo GetBuild()
        {
            Console.WriteLine("* Checking for builds...");
            var client = new JsonServiceClient(ApiUrl);
            try
            {
                var buildInfo = client.Post(new CheckForBuild
                {
                    token = Uri.EscapeDataString(Config.Token)
                });

                if (buildInfo != null)
                {
                    return buildInfo;
                }
            }
            catch (WebServiceException ex)
            {
                Console.WriteLine(ex.StatusCode == 404 ? "* Nothing" : "* Failed");
            }

            return null;
        }

        /// <summary>
        /// PUSH the Build to the Gitlab CI Coordinator
        /// </summary>
        /// <param name="iId">Build ID</param>
        /// <param name="state">State</param>
        /// <param name="sTrace">Command output</param>
        /// <returns></returns>
        public static bool PushBuild(int iId, State state, string sTrace)
        {
            Console.WriteLine("[" + DateTime.Now + "] Submitting build " + iId + " to coordinator ...");

            var trace = new StringBuilder();
            foreach (string t in sTrace.Split('\n'))
                trace.Append(t).Append("\n");

            int iTry = 0;
            while (iTry <= 5)
            {
                try
                {
                    var client = new JsonServiceClient(ApiUrl);
                    var resp = client.Put(new PushBuild
                    {
                        id = iId + ".json",
                        token = Uri.EscapeDataString(Config.Token),
                        state = GetStateString(state),
                        trace = trace.ToString()
                    });

                    if (resp != null)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.ToString());
                }

                iTry++;
                Thread.Sleep(1000);
            }

            return false;
        }

        private static string GetStateString(State state)
        {
            return state.ToString().ToLowerInvariant();
        }
    }
}