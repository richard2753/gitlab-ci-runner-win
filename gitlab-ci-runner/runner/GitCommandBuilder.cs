using System;
using gitlab_ci_runner.api;

namespace gitlab_ci_runner.runner
{
    public class GitCommandBuilder
    {
        private readonly string _buildPath;
        private readonly BuildInfo _buildInfo;

        public GitCommandBuilder(string buildPath, BuildInfo buildInfo)
        {
            _buildPath = buildPath;
            _buildInfo = buildInfo;
        }

        public string CheckoutCommand()
        {
            var command = "";
            // Change to drive
            command = _buildPath.Substring(0, 1) + ":";
            // Change to directory
            command += " && cd " + _buildPath;
            // Git Reset
            command += " && git reset --hard";
            // Git Checkout
            command += " && git checkout " + _buildInfo.sha;

            return command;
        }

        public string CloneCommand()
        {
            var command = "";
            // Change to drive
            command = _buildPath.Substring(0, 1) + ":";
            // Change to directory
            command += " && cd " + _buildPath;
            // Git Clone
            command += " && git clone " + _buildInfo.repo_url + " project-" + _buildInfo.project_id;
            // Change to directory
            command += " && cd " + _buildPath;
            // Git Checkout
            command += " && git checkout " + _buildInfo.sha;

            return command;
        }

        public string FetchCommand()
        {
            String command = "";

            // Change to drive
            command = _buildPath.Substring(0, 1) + ":";
            // Change to directory
            command += " && cd " + _buildPath;
            // Git Reset
            command += " && git reset --hard";
            // Git Clean
            command += " && git clean -f";
            // Git fetch
            command += " && git fetch";

            return command;
        }
    }
}