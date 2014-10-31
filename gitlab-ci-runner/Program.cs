using System.Configuration;
using System.Linq;
using System.Net;
using Topshelf;

namespace gitlab_ci_runner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains(ConfigurationManager.AppSettings["gitlab-ci-sslbypass"]))
            {
                RegisterSecureSocketsLayerBypass();
            }
            HostFactory.Run(host =>
            {
                host.SetInstanceName(ConfigurationManager.AppSettings["service-instance-name"]);
                host.SetDisplayName("GitLab CI Windows Runner");
                host.SetDescription("GitLab CI Windows Runner");
                host.RunAs(ConfigurationManager.AppSettings["service-username"], ConfigurationManager.AppSettings["service-password"]);
                host.Service<RunnerService>();
            });
        }

        static void RegisterSecureSocketsLayerBypass()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }
}
