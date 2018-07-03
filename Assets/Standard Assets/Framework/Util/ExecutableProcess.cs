
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WWFramework.Helper;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace WWFramework.Util
{
    public class ExecutableProcess
    {
        private string _fileName;
        private List<string> _arguments;
        private bool _writeSelf;
        private bool _useShell;
        private bool _createNoWindow;

        private bool _hasInit;

        private ExecutableProcess()
        {
        }

        public static ExecutableProcess CreateProcess(string fileName = null, bool useShell = false,
            bool writeSelf = false, bool createNoWindow = true)
        {
            var process = new ExecutableProcess()
            {
                _fileName = fileName,
                _writeSelf = writeSelf,
                _useShell = !writeSelf && useShell,
                _createNoWindow = !useShell && createNoWindow,
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
                FileName = string.IsNullOrEmpty(_fileName) ? AssetHelper.DefaultCommand : _fileName,
                UseShellExecute = _useShell,
                // 如果开启，则会影响Shell
                ErrorDialog = _useShell,
                // 当开启Shell的时候，怎么都是true
                CreateNoWindow = _createNoWindow,
                RedirectStandardInput = !_useShell,
                RedirectStandardOutput = !_useShell,
                RedirectStandardError = !_useShell,
            };

            Console.InputEncoding = System.Text.Encoding.UTF8;
            Process process;
            var args = string.Join(" ", _arguments.ToArray());
            if (_writeSelf)
            {
                process = Process.Start(info);
                if (_arguments != null)
                {
                    process.StandardInput.WriteLine(args);
                }
            }
            else
            {
                info.Arguments = args;

                process = Process.Start(info);
            }

            if (info.FileName == AssetHelper.DefaultWinCommand && !_useShell)
            {
                process.StandardInput.WriteLine("exit");
            }

            if (!_writeSelf)
            {
                Debug.Log(string.Format("{0}\n{1}", info.FileName, args));
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
                if (!_useShell)
                {
                    result = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                }
                else
                {
                    result = string.Empty;
                    error = string.Empty;
                }

                process.WaitForExit();
                exitCode = process.ExitCode;
                process.Close();
            }
            else
            {
                result = string.Empty;
                error = string.Empty;
            }

            return exitCode;
        }


        public string StartAndCheck(int exitCode = 0)
        {
            string result;
            string error;
            var code = StartAndWaitForExit(out result, out error);
            if (code != exitCode || !string.IsNullOrEmpty(error))
            {
                throw new Exception(string.Format("Process Exit: {0} \n{1}", code, error));
            }

            return result;
        }

        public string StartAndCheckLog(int exitCode = 0)
        {
            var result = StartAndCheck(exitCode);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log(result);
            }

            return result;
        }
    }
}
