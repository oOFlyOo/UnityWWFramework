using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper;
using WWFramework.Reflection;

namespace WWFramework.Helper.Editor
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
            var type = typeof(ActiveEditorTracker).GetSameAssemblyType("UnityEditorInternal.LogEntries");
            type.InvokeStaticMethod("Clear");
        }


        public static List<string> GetReverseDependencies(string[] paths, string[] searchPaths = null)
        {
            var includeList = new HashSet<string>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var guid in AssetDatabase.FindAssets("t:Material t:Model t:Prefab t:Scene", searchPaths))
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                var dependencies = AssetDatabase.GetDependencies(filePath, false);
                foreach (var dependency in dependencies)
                {
                    if (paths.Any(dependency.Contains))
                    {
                        includeList.Add(filePath);
                        break;
                    }
                }
            }

            stopWatch.Stop();
            UnityEngine.Debug.Log(string.Format("反向查找依赖耗时：{0}", stopWatch.Elapsed.Seconds));

            return includeList.ToList();
        }


        public static void ChangeAutoGenerateLightingState(bool auto)
        {
            var settingsType = typeof(LightmapEditorSettings);
            var settings = settingsType.InvokeStaticMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic) as
                    UnityEngine.Object;
            var so = new SerializedObject(settings);
            so.FindProperty("m_GIWorkflowMode").intValue = auto ? 0 : 1;
            so.ApplyModifiedProperties();
        }


        public static readonly Dictionary<BuildTarget, string> BuildTargetStrDict = new Dictionary<BuildTarget, string>()
        {
            {BuildTarget.StandaloneWindows, "Standalone"},
            {BuildTarget.Android, "Android"},
            {BuildTarget.iOS, "iPhone"},
        };

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

            var index = (int) gameViewSizeGroupType.InvokeMethod("IndexOf", curSizeGroup, ReflectionExtension.DefaultFlags,
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
    }
}
