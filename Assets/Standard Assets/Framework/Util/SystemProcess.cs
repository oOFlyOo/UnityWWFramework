
using System.Collections.Generic;
using System.Diagnostics;
using WWFramework.Helper;

namespace WWFramework.Util
{
    public class SystemProcess
    {
        private string _fileName;
        private List<string> _arguments;
        private bool _useShell;

        private ProcessStartInfo _processStartInfo;

        private SystemProcess()
        {
        }

        public static SystemProcess CreateProcess(bool useShell)
        {
            return CreateProcess(AssetHelper.DefaultCommand, useShell);
        }

        public static SystemProcess CreateProcess(string fileName, bool useShell)
        {
            var process = new SystemProcess()
            {
                _fileName = fileName,
                _useShell = useShell,
                _arguments = new List<string>(),
            };

            return process;
        }

        public void AppendArgument(string argument)
        {
            _arguments.Add(argument);
        }

        private void InitProcessStartInfo()
        {
            _processStartInfo = new ProcessStartInfo()
            {
                FileName = _fileName,
                Arguments = string.Join(" ", _arguments.ToArray()),
                UseShellExecute = _useShell,
            };
        }

        public void Start()
        {
            if (_processStartInfo != null)
            {
                InitProcessStartInfo();

                Process.Start(_processStartInfo);
            }
        }


        public int StartAndWaitForExit(out string result, out string error)
        {
            var exitCode = 0;

            if (_processStartInfo != null)
            {
                InitProcessStartInfo();

                var process = Process.Start(_processStartInfo);
                result = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            else
            {
                result = string.Empty;
                error = string.Empty;
            }

            return exitCode;
        }
    }
}