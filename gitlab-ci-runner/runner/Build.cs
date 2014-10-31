using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using gitlab_ci_runner.api;
using gitlab_ci_runner.helper;

namespace gitlab_ci_runner.runner
{
    class Build
    {
        public State State { get; private set; }
        public bool Completed { get; private set; }
        public int Id { get { return _buildInfo.id; } }

        private readonly ConcurrentQueue<string> _outputList;
        private readonly string _projectsDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\projects";
        private readonly string _projectDir;
        private readonly BuildInfo _buildInfo;

        public Build(BuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
            _projectDir = _projectsDir + @"\project-" + buildInfo.project_id;
            _outputList = new ConcurrentQueue<string>();
            State = State.Waiting;
            Completed = false;
        }

        public void Run()
        {
            State = State.Running;

            try
            {
                InitProjectDir();

                State = _buildInfo.GetCommands().All(Execute)
                    ? State.Success
                    : State.Failed;
            }
            catch (Exception rex)
            {
                Log("A runner exception occoured: " + rex.Message);
                State = State.Failed;
            }

            Completed = true;
        }

        public bool PushProgress()
        {
            return Network.PushBuild(_buildInfo.id, State, GetOutput());
        }

        private void InitProjectDir()
        {
            FileSystem.EnsureFolderExists(_projectDir);

            var git = new GitCommandBuilder(_projectDir, _buildInfo);

            if (CanDoGitFetch())
            {
                Execute(git.FetchCommand());
            }
            else
            {
                if (Directory.Exists(_projectDir))
                    FileSystem.DeleteDirectory(_projectDir);

                Execute(git.CloneCommand());
            }
            Execute(git.CheckoutCommand());
        }

        private bool CanDoGitFetch()
        {
            return Directory.Exists(_projectDir + @"\.git") && _buildInfo.allow_git_fetch;
        }

        private bool Execute(string command)
        {
            try
            {
                var cmd = command.Trim();

                Log(cmd);

                using (var process = new Process { StartInfo = GetStartInfo(cmd) })
                {
                    process.OutputDataReceived += OutputHandler;
                    process.ErrorDataReceived += OutputHandler;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(_buildInfo.timeout * 1000))
                    {
                        process.Kill();
                    }
                    return process.ExitCode == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private ProcessStartInfo GetStartInfo(string command)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            if (Directory.Exists(_projectDir))
            {
                startInfo.WorkingDirectory = _projectDir; // Set Current Working Directory to project directory
            }
            startInfo.FileName = "cmd.exe"; // use cmd.exe so we dont have to split our command in file name and arguments
            startInfo.Arguments = "/C \"" + command + "\""; // pass full command as arguments

            // Environment variables
            startInfo.EnvironmentVariables["HOME"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            startInfo.EnvironmentVariables["BUNDLE_GEMFILE"] = _projectDir + @"\Gemfile";
            startInfo.EnvironmentVariables["BUNDLE_BIN_PATH"] = "";
            startInfo.EnvironmentVariables["RUBYOPT"] = "";

            startInfo.EnvironmentVariables["CI_SERVER"] = "yes";
            startInfo.EnvironmentVariables["CI_SERVER_NAME"] = "GitLab CI";
            startInfo.EnvironmentVariables["CI_SERVER_VERSION"] = null; // GitlabCI Version
            startInfo.EnvironmentVariables["CI_SERVER_REVISION"] = null; // GitlabCI Revision

            startInfo.EnvironmentVariables["CI_BUILD_REF"] = _buildInfo.sha;
            startInfo.EnvironmentVariables["CI_BUILD_REF_NAME"] = _buildInfo.ref_name;
            startInfo.EnvironmentVariables["CI_BUILD_ID"] = _buildInfo.id.ToString(CultureInfo.InvariantCulture);

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            return startInfo;
        }

        private void Log(string command)
        {
            _outputList.Enqueue("");
            _outputList.Enqueue(command);
            _outputList.Enqueue("");
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                _outputList.Enqueue(outLine.Data);
            }
        }

        private string GetOutput()
        {
            string t;
            while (_outputList.TryPeek(out t) && string.IsNullOrEmpty(t))
            {
                _outputList.TryDequeue(out t);
            }
            return String.Join("\n", _outputList.ToArray()) + "\n";
        }

    }
}
