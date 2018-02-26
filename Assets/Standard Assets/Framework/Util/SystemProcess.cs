
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WWFramework.Helper;

namespace WWFramework.Util
{
    public class SystemProcess
    {
        private string _fileName;
        private List<string> _arguments;
        private bool _createNoWindow;
        private bool _useShell;

        private bool _hasInit;

        private SystemProcess()
        {
        }

        public static SystemProcess CreateProcess(bool createNoWindow = false, bool useShell = false)
        {
            return CreateProcess(null, createNoWindow, useShell);
        }

        public static SystemProcess CreateProcess(string fileName, bool createNoWindow = false, bool useShell = false)
        {
            var process = new SystemProcess()
            {
                _fileName = fileName,
                _createNoWindow = createNoWindow,
                _useShell = useShell,
                _arguments = new List<string>(),
            };

            return process;
        }

        public void AppendArgument(string argument)
        {
            _arguments.Add(argument);
        }

        public void AppendPath(string path)
        {
            AppendArgument(string.Format("\"{0}\"", path));
        }

        private Process InitProcess()
        {
            if (_hasInit)
            {
                return null;
            }
            _hasInit = true;

            var info = new ProcessStartInfo()
            {
                CreateNoWindow = _createNoWindow,
                UseShellExecute = _useShell,
                ErrorDialog = true,
                RedirectStandardOutput = !_useShell,
                RedirectStandardError = !_useShell,
            };

            Process process;
            if (string.IsNullOrEmpty(_fileName))
            {
                info.FileName = AssetHelper.DefaultCommand;
                info.RedirectStandardInput = true;

                process = Process.Start(info);

                // win下加个换行否则乱码
                process.StandardInput.WriteLine();
                process.StandardInput.WriteLine("chcp 65001");
                if (_arguments != null)
                {
                    process.StandardInput.WriteLine(string.Join(" ", _arguments.ToArray()));
                }
                // 运行为退出程序
                process.StandardInput.WriteLine("exit");
            }
            else
            {
                info.FileName = _fileName;
                info.Arguments = string.Join(" ", _arguments.ToArray());

                process = Process.Start(info);
            }

            return process;
        }

        public void Start()
        {
            if (!_hasInit)
            {
                InitProcess();
            }
        }


        public int StartAndWaitForExit(out string result, out string error)
        {
            var exitCode = 0;

            if (!_hasInit)
            {
                var process = InitProcess();
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


        public string StartAndChek(int exitCode = 0)
        {
            string result;
            string error;
            var code = StartAndWaitForExit(out result, out error);
            if (code != exitCode)
            {
                throw new Exception(string.Format("Process Exit {0} {1}\n{2}", code, error, result));
            }

            return result;
        }
    }
}