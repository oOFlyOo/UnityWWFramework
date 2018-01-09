

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

namespace WWFramework.Editor.Helper
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
            var monoScript = AssetHelper.FindScriptableObject(typeof (RecompileScriptableObject));
            var path = string.Format("{0}/{1}.asset", Path.GetDirectoryName(AssetDatabase.GetAssetPath(monoScript)),
                monoScript.name);
            _scriptObj = AssetDatabase.LoadAssetAtPath<RecompileScriptableObject>(path);
            if (_scriptObj == null)
            {
                _scriptObj = AssetHelper.CreateScriptableObjectAsset<RecompileScriptableObject>(path);
            }
        }

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
                    RecompileScriptList[0].Execute();
                    RecompileScriptList.RemoveAt(0);
                    DidRecompilingScripts();
                }
                catch (Exception)
                {
                    RecompileScriptList.Clear();
                    throw;
                }
            }
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

        public static void WaitIfCompiling<T>(Action<T> callback, object param)
        {
            if (ShouldAdd())
            {
                AddRecompileScript(callback.Method, param);
            }
            else
            {
                callback((T)param);
            }
        }

        private static void AddRecompileScript(MethodInfo method, object param = null)
        {
            if (!method.IsStatic)
            {
                throw new NotSupportedException(method.ToString());
            }
            RecompileScriptList.Add(new RecompileScript(method, param));
            EditorUtility.SetDirty(_scriptObj);
//            AssetDatabase.SaveAssets();
        }

        private static bool ShouldAdd()
        {
            return EditorApplication.isCompiling || RecompileScriptList.Count > 0;
        }
    }
}