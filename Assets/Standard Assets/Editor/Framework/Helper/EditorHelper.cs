using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper;
using WWFramework.Reflection;
using WWFramework.Util;
using Object = UnityEngine.Object;

namespace WWFramework.Helper.Editor
{
    public static class EditorHelper
    {
        #region 通用
        public static readonly Dictionary<BuildTarget, string> BuildTargetStrDict = new Dictionary<BuildTarget, string>()
        {
            {BuildTarget.StandaloneWindows, "Standalone"},
            {BuildTarget.Android, "Android"},
            {BuildTarget.iOS, "iPhone"},
        };

        public static byte[] HttpRequest(string url, int timeOut = 3000)
        {
            var request = WebRequest.Create(url);
            request.Timeout = timeOut;

            var response = request.GetResponse();
            var datas = new byte[response.ContentLength];
            var value = 0;
            using (var sResp = response.GetResponseStream())
            {
                const int bufSize = 2048;
                byte[] srcbuff = new byte[bufSize];
                int size = sResp.Read(srcbuff, 0, bufSize);
                while (size > 0)
                {
                    Buffer.BlockCopy(srcbuff, 0, datas, value, size);
                    value += size;
                    size = sResp.Read(srcbuff, 0, bufSize);
                }
            }

            return datas;
        }


        public static void ClearConsole()
        {
            var type = typeof(ActiveEditorTracker).GetSameAssemblyType("UnityEditorInternal.LogEntries");
            type.InvokeStaticMethod("Clear");
        }
        #endregion


        #region 提示的辅助
        public enum Result
        {
            Success,
            Error,
        }

        public static void DisplayResultDialog(Result result = Result.Success,
            string msg = "完成！")
        {
            EditorUtility.DisplayDialog(result.ToString(), msg, "确定");
        }

        public static void DisplayAndThrowError(string msg)
        {
            DisplayAndThrowError(new Exception(msg));
        }

        public static void DisplayAndThrowError(Exception e)
        {
            DisplayResultDialog(Result.Error, e.Message);
            throw e;
        }

        public static void Run(Action action, bool askConfirm = true, bool displayResultDialog = true)
        {
            try
            {
                if (askConfirm)
                {
                    if (!EditorUtility.DisplayDialog("确认：", "谨慎操作啊亲！",
                        "确认", "取消"))
                    {
                        return;
                    }
                }

                action();

                if (displayResultDialog)
                {
                    DisplayResultDialog();
                }
            }
            catch (Exception e)
            {
                DisplayAndThrowError(e);
            }
        }
        #endregion

        #region 进度条，Mac下巨慢
        public static void DisplayProgressBar(string title, string info, float progress, bool force = false)
        {
            if (Application.platform != RuntimePlatform.OSXEditor || force)
            {
                EditorUtility.DisplayProgressBar(title, info, progress);
            }
        }


        public static bool DisplayCancelableProgressBar(string title, string info, float progress, bool force = false)
        {
            if (Application.platform != RuntimePlatform.OSXEditor || force)
            {
                return EditorUtility.DisplayCancelableProgressBar(title, info, progress);
            }

            return false;
        }

        public static void ClearProgressBar(bool force = false)
        {
            if (Application.platform != RuntimePlatform.OSXEditor || force)
            {
                EditorUtility.ClearProgressBar();
            }
        }
        #endregion

        #region 光照
        public static void ChangeAutoGenerateLightingState(bool auto)
        {
            var settingsType = typeof(LightmapEditorSettings);
            var settings = settingsType.InvokeStaticMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic) as
                UnityEngine.Object;
            var so = new SerializedObject(settings);
            so.FindProperty("m_GIWorkflowMode").intValue = auto ? 0 : 1;
            so.ApplyModifiedProperties();
        }
        #endregion

        #region 视窗更改

        private const string CustomGameViewSizeName = "Custom";

        public static void SetEditorGameViewSize(int width, int height)
        {
            var ssType = typeof(ScriptableSingleton<>);
            var gameViewSizesType = ssType.GetSameAssemblyType("UnityEditor.GameViewSizes");
            var ssgvsType = ssType.MakeGenericType(gameViewSizesType);
            var gameViewSizes = ssgvsType.GetStaticPropertyValue("instance");
            var curSizeGroup = gameViewSizesType.GetPropertyValue("currentGroup", gameViewSizes);
            var gameViewSizeGroupType = ssType.GetSameAssemblyType("UnityEditor.GameViewSizeGroup");
            var customGameSizeList = gameViewSizeGroupType.GetFieldValue("m_Custom", curSizeGroup, BindingFlags.Instance | BindingFlags.NonPublic) as IList;
            var gameViewSizeType = ssType.GetSameAssemblyType("UnityEditor.GameViewSize");

            object curGameSize = null;
            foreach (var obj in customGameSizeList)
            {
                if (gameViewSizeType.GetPropertyValue("baseText", obj).ToString() == CustomGameViewSizeName)
                {
                    curGameSize = obj;
                    break;
                }
            }

            if (curGameSize == null)
            {
                var gameViewSizeTypeType = ssType.GetSameAssemblyType("UnityEditor.GameViewSizeType");
                curGameSize = Activator.CreateInstance(gameViewSizeType, Enum.GetValues(gameViewSizeTypeType).GetValue(1), width, height, CustomGameViewSizeName);
                gameViewSizeGroupType.InvokeMethod("AddCustomSize", curSizeGroup, ReflectionExtension.DefaultFlags, curGameSize);
            }
            else
            {
                gameViewSizeType.SetPropertyValue("width", curGameSize, width);
                gameViewSizeType.SetPropertyValue("height", curGameSize, height);
            }

            var builtinCount = (int)gameViewSizeGroupType.InvokeMethod("GetBuiltinCount", curSizeGroup);

            var index = (int)gameViewSizeGroupType.InvokeMethod("IndexOf", curSizeGroup, ReflectionExtension.DefaultFlags,
                        curGameSize);

            var editorWindowType = ssType.GetSameAssemblyType("UnityEditor.GameView");
            var editorWindow = EditorWindow.GetWindow(editorWindowType);
            editorWindowType.InvokeMethod("SizeSelectionCallback", editorWindow,
                BindingFlags.Instance | BindingFlags.NonPublic, index + builtinCount);
        }

        public static void GetEditorGameViewSize(out int width, out int height)
        {
            var editorType = typeof(UnityEditor.Editor);

            var editorWinType = editorType.GetSameAssemblyType("UnityEditor.GameView");
            var editorWindow = EditorWindow.GetWindow(editorWinType);
            var curGameViewSize = editorWinType.GetPropertyValue("currentGameViewSize", editorWindow, BindingFlags.Instance | BindingFlags.NonPublic);
            var gameViewSizeType = editorType.GetSameAssemblyType("UnityEditor.GameViewSize");

            width = (int)gameViewSizeType.GetPropertyValue("width", curGameViewSize);
            height = (int)gameViewSizeType.GetPropertyValue("height", curGameViewSize);
        }
        #endregion

        #region WinRAR
        private const string WinRAR_RegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe";

        private static string GetWinRARPath()
        {
            string winrarPath = null;
            try
            {
                var regKey = Registry.LocalMachine.OpenSubKey(WinRAR_RegKeyPath);
                if (regKey != null)
                {
                    winrarPath = regKey.GetValue("").ToString();
                    regKey.Close();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (string.IsNullOrEmpty(winrarPath))
            {
                winrarPath = "WinRAR.exe";
            }

            return winrarPath;
        }

        public static void CompressByWinRAR(string fromPath, string toPath, bool ignoreBaseFolder = true, bool zipMode = true, string extraRootFolder = null)
        {
            if (Application.platform != RuntimePlatform.OSXEditor)
            {
                var process = ExecutableProcess.CreateProcess(GetWinRARPath());

                // 添加到压缩文件，不会删除旧的
                process.AppendArgument("a");
                // 包括子文件夹
                process.AppendArgument("-r");
                // 不会将整个路径添加进去
                process.AppendArgument("-ep1");
                if (zipMode)
                {
                    process.AppendArgument("-afzip");
                }

                if (!string.IsNullOrEmpty(extraRootFolder))
                {
                    process.AppendArgument(string.Format("-ap{0}", extraRootFolder));
                }
                process.AppendPath(toPath);
                // 是否包含传进来的文件夹，还是只对子文件和文件夹
                process.AppendArgument(ignoreBaseFolder ? fromPath + "/*.*" : fromPath);

                process.StartAndCheckLog();
            }
            else
            {
                fromPath = Path.GetFullPath(fromPath);
                toPath = Path.GetFullPath(toPath);

                var process = ExecutableProcess.CreateProcess(writeSelf: true);
                var fileName = Path.GetFileName(fromPath);

                process.AppendArgument("cd");
                process.AppendArgument(Path.GetDirectoryName(fromPath));
                process.AppendArgument("\n");
                process.AppendArgument("zip");
                process.AppendArgument("-r");
                if (!string.IsNullOrEmpty(extraRootFolder))
                {
                    Directory.CreateDirectory(string.Format("{0}/{1}", Path.GetDirectoryName(fromPath), extraRootFolder));
                    FileUtil.CopyFileOrDirectory(fromPath,
                        string.Format("{0}/{1}/{2}", Path.GetDirectoryName(fromPath), extraRootFolder, fileName));
                    process.AppendArgument("-m");
                    fileName = extraRootFolder;
                }
                process.AppendArgument(toPath);
                process.AppendArgument(fileName);

                process.StartAndCheckLog();
            }
        }
        #endregion

        #region Win文件签名，否则容易被误当病毒
        private static string MakeCertPath
        {
            get
            {
                return string.Format("{0}/MonoBleedingEdge/lib/mono/4.5/makecert.exe",
                    EditorApplication.applicationContentsPath);
            }
        }
        private static string Cert2SpcPath
        {
            get
            {
                return string.Format("{0}/MonoBleedingEdge/lib/mono/4.5/cert2spc.exe",
                    EditorApplication.applicationContentsPath);
            }
        }
        private static string SignCodePath
        {
            get
            {
                return string.Format("{0}/MonoBleedingEdge/lib/mono/4.5/signcode.exe",
                    EditorApplication.applicationContentsPath);
            }
        }

        private const string PvkPath = "WWFramework/PC/subject.pvk";
        private const string CerPath = "WWFramework/PC/pc.cer";
        private const string SpcPath = "WWFramework/PC/pc.spc";


        public static void GenerateSignFiles()
        {
            var process = ExecutableProcess.CreateProcess(MakeCertPath);
            process.AppendArgument("-n");
            process.AppendArgument("CN=Windows,E=microsoft,O=微软");
            process.AppendArgument("-r");
            process.AppendArgument("-sv");
            IOHelper.CreateFileDirectory(PvkPath);
            process.AppendArgument(PvkPath);
            IOHelper.CreateFileDirectory(CerPath);
            process.AppendArgument(CerPath);
            process.StartAndCheckLog();

            process = ExecutableProcess.CreateProcess(Cert2SpcPath);
            process.AppendArgument(CerPath);
            IOHelper.CreateFileDirectory(SpcPath);
            process.AppendArgument(SpcPath);
            process.StartAndCheckLog();
        }

        public static void SignWinFile(string path, string spcPath = SpcPath, string pvkPath = PvkPath)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var process = ExecutableProcess.CreateProcess(SignCodePath);
            process.AppendArgument("-spc");
            process.AppendArgument(spcPath);
            process.AppendArgument("-v");
            process.AppendArgument(pvkPath);
            process.AppendArgument(path);
            process.StartAndCheckLog();

            // 会备份原来的文件，删除掉
            var backFile = path + ".bak";
            IOHelper.DeleteFile(backFile);
        }
        #endregion
    }
}