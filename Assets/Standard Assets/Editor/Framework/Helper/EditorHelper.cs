using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEditor;
using WWFramework.Helper;

namespace WWFramework.Editor.Helper
{
    public static class EditorHelper
    {
        #region 通用

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
            var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
            var type = assembly.GetType("UnityEditorInternal.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }


        public static bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }


        public static List<string> GetReverseDependencies(string[] paths, string[] searchPaths = null)
        {
            var includeList = new List<string>();
            searchPaths = searchPaths ?? new[] {IOHelper.CurrentDirectory};
            foreach (var searchPath in searchPaths)
            {
                foreach (var file in Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories))
                {
                    if (IsMetaFile(file))
                    {
                        continue;
                    }

                    var relativePath = IOHelper.GetRelativePath(file.Replace("\\", "/"));
                    foreach (var dependency in AssetDatabase.GetDependencies(relativePath))
                    {
                        foreach (var path in paths)
                        {
                            if (dependency.Contains(path))
                            {
                                includeList.Add(relativePath);
                            }
                        }
                    }
                }
            }

            return includeList;
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
    }
}