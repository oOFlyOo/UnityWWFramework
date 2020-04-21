

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WWFramework.Extension.Editor;

namespace WWFramework.Helper.Editor
{
    public static class ScriptRecompileHelper
    {
        #region 记录待触发函数
        private static RecompileScriptableObject _scriptObj;

        private static List<RecompileScript> RecompileScriptList
        {
            get
            {
                if (_scriptObj == null)
                {
                    CreateOrLoadReloadAsset();
                }
                return _scriptObj.ReloadScriptList;
            }
        }
        #endregion

        private static void CreateOrLoadReloadAsset()
        {
            var monoScript = EditorAssetHelper.FindScriptableObject(typeof(RecompileScriptableObject));
            var path = monoScript.GetScriptableObjectPathByMonoScript();
            _scriptObj = AssetDatabase.LoadAssetAtPath<RecompileScriptableObject>(path);
            if (_scriptObj == null)
            {
                _scriptObj = EditorAssetHelper.CreateScriptableObjectAsset<RecompileScriptableObject>(path);
            }
        }

        // [DidReloadScripts] 这个依赖代码编译，启动的时候不会执行，而下面那个启动的时候就会执行，包括代码编译过
//        [InitializeOnLoadMethod]
        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            DidRecompilingScripts();
        }

        private static void DidRecompilingScripts()
        {
            if (!EditorApplication.isCompiling && RecompileScriptList.Count > 0)
            {
                try
                {
                    if (!RecompileScriptList[0].Executing)
                    {
                        var obj = RecompileScriptList[0];
                        obj.Execute();
                        // 有可能执行途中进行了清空操作
                        if (RecompileScriptList.IndexOf(obj) == 0)
                        {
                            RecompileScriptList.RemoveAt(0);
                        }

                        SetDirtyAndSaveScriptObj();

                        DidRecompilingScripts();
                    }
                    else
                    {
                        // 往往是因为没执行完就关闭了 Unity
                        ClearRecompileScriptList();
                    }
                }
                catch (Exception)
                {
                    ClearRecompileScriptList();

                    throw;
                }
            }
        }

        public static void CheckBeforeUsing()
        {
            ClearRecompileScriptList();
        }

        private static void ClearRecompileScriptList()
        {
            RecompileScriptList.Clear();
            SetDirtyAndSaveScriptObj();
        }

        private static void SetDirtyAndSaveScriptObj()
        {
            EditorUtility.SetDirty(_scriptObj);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static void WaitIfCompiling(Action callback)
        {
            if (ShouldAdd())
            {
                AddRecompileScript(callback.Method);
            }
            else
            {
                callback();
            }
        }

    public static void WaitIfCompiling<T>(Action<T> callback, object arg)
    {
        if (ShouldAdd())
        {
            AddRecompileScript(callback.Method, arg);
        }
        else
        {
            callback((T)arg);
        }
    }

    public static void WaitIfCompiling<T1, T2>(Action<T1, T2> callback, object arg1, object arg2)
    {
        if (ShouldAdd())
        {
            AddRecompileScript(callback.Method, arg1, arg2);
        }
        else
        {
            callback((T1)arg1, (T2)arg2);
        }
    }

    private static void AddRecompileScript(MethodInfo method, params object[] args)
    {
        if (!method.IsStatic)
        {
            throw new NotSupportedException(method.ToString());
        }
        RecompileScriptList.Add(new RecompileScript(method, args));
        SetDirtyAndSaveScriptObj();
    }

        private static bool ShouldAdd()
        {
            return EditorApplication.isCompiling || RecompileScriptList.Count > 0;
        }
    }
}