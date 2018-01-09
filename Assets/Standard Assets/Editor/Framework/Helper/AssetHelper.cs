
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WWFramework.Editor.Helper
{
    public static class AssetHelper
    {
        #region 资源查找
        public enum SearchFilter
        {
            AnimationClip,
            AudioClip,
            AudioMixer,
            Font,
            GUISkin,
            Material,
            Mesh,
            Model,
            PhysicMaterial,
            Prefab,
            Scene,
            Script,
            Shader,
            Sprite,
            Texture,
            VideoClip,
        }

        public static string[] FindAssets(SearchFilter filter, string[] searchInFolders = null)
        {
            return AssetDatabase.FindAssets(String.Format("t:{0}", filter), searchInFolders);
        }

        public static List<string> FindAssetsPaths(SearchFilter filter, string[] searchInFolders = null)
        {
            return FindAssets(filter, searchInFolders).Select(AssetDatabase.GUIDToAssetPath).ToList();
        }

        public static MonoScript FindScriptableObject(Type type)
        {
            foreach (var monoScript in FindScriptableObjects())
            {
                if (monoScript.GetClass() == type)
                {
                    return monoScript;
                }
            }
            return null;
        }

        public static List<MonoScript> FindScriptableObjects()
        {
            var scriptList = new List<MonoScript>();
            var scripts = FindAssetsPaths(SearchFilter.Script);
            var scriptObjType = typeof(ScriptableObject);
            var editorType = typeof(UnityEditor.Editor);
            var editorWindowType = typeof(EditorWindow);

            foreach (var path in scripts)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script.GetClass();
                if (type != null && type.IsSubclassOf(scriptObjType) && !type.IsSubclassOf(editorType) && !type.IsSubclassOf(editorWindowType))
                {
                    scriptList.Add(script);
                }
            }

            return scriptList;
        }
        #endregion


        #region 资源创建
        public static T CreateScriptableObjectAsset<T>(string path) where T: ScriptableObject
        {
            return CreateScriptableObjectAsset(typeof (T), path) as T;
        }

        public static ScriptableObject CreateScriptableObjectAsset(Type type, string path)
        {
            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);

            return asset;
        }
        #endregion

        public static void GetTextureSize(TextureImporter importer, out int width, out int height)
        {
            object[] args = new object[2] { 0, 0 };
            var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(importer, args);

            width = (int)args[0];
            height = (int)args[1];
        }

        public static bool IsTextureMultipleOfFour(TextureImporter importer)
        {
            int width;
            int height;
            GetTextureSize(importer, out width, out height);
            int result1;
            Math.DivRem(width, 4, out result1);
            int result2;
            Math.DivRem(height, 4, out result2);

            return result1 == 0 && result2 == 0;
        }

        public static void SelectObject(Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        public static void RevealInFinder(string path)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = path.Replace("/", "\\");
                Process.Start("explorer.exe", "/select," + path);
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }
    }
}