
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WWFramework.Extension;
using WWFramework.Reflection;
using Object = UnityEngine.Object;

namespace WWFramework.Helper.Editor
{
    public static class EditorAssetHelper
    {
        #region 资源查找
        public enum SearchFilter
        {
            All,
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

        public static string[] FindAssets(SearchFilter filter, params string[] searchInFolders)
        {
            var filterStr = filter != SearchFilter.All ? string.Format("t:{0}", filter) : null;
            return AssetDatabase.FindAssets(filterStr, searchInFolders.ParamsFixing());
        }

        public static List<string> FindAssetsPaths(SearchFilter filter, params string[] searchInFolders)
        {
            return FindAssets(filter, searchInFolders.ParamsFixing()).Select(AssetDatabase.GUIDToAssetPath).ToList();
        }

        public static MonoScript FindScriptableObject(Type type)
        {
            foreach (var monoScript in EditorAssetHelper.FindScriptableObjects())
            {
                if (monoScript.GetClass() == type)
                {
                    return monoScript;
                }
            }
            return null;
        }

        public static MonoScript FindScriptableObjectQuickly(Type type)
        {
            var so = ScriptableObject.CreateInstance(type);
            var script = MonoScript.FromScriptableObject(so);
            Object.DestroyImmediate(so);

            return script;
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


        public static List<Object> FindMissingReferences(List<Object> checksObjects)
        {
            var resultList = new List<Object>();
            var hasCheckList = new List<Object>();
            foreach (var checksObject in checksObjects)
            {
                FindMissingReferences(checksObject, resultList, hasCheckList);
            }

            return resultList;
        }

        public static void FindMissingReferences(Object obj, List<Object> resultList, List<Object> hasCheckList)
        {
            if (obj == null || resultList.Contains(obj) || hasCheckList.Contains(obj))
            {
                return;
            }

            hasCheckList.Add(obj);

            var go = obj as GameObject;
            var objMissing = false;
            if (go != null)
            {
                foreach (var com in go.GetComponents<Component>())
                {
                    if (com == null)
                    {
                        if (!objMissing)
                        {
                            objMissing = true;
                            resultList.AddIfNotExist(obj);
                        }
                    }
                    else
                    {
                        FindMissingReferences(com, resultList, hasCheckList);
                    }
                }

                foreach (Transform trans in go.transform)
                {
                    FindMissingReferences(trans.gameObject, resultList, hasCheckList);
                }
            }
            else
            {
                var so = new SerializedObject(obj);
                var sp = so.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null)
                        {
                            if (sp.objectReferenceInstanceIDValue != 0 && !objMissing)
                            {
                                objMissing = true;
                                resultList.AddIfNotExist(obj);
                            }
                        }
                        else
                        {
                            FindMissingReferences(sp.objectReferenceValue, resultList, hasCheckList);
                        }
                    }
                }
            }


        }
        #endregion


        #region 资源创建
        public static T CreateScriptableObjectAsset<T>(string path) where T : ScriptableObject
        {
            return CreateScriptableObjectAsset(typeof(T), path) as T;
        }

        public static ScriptableObject CreateScriptableObjectAsset(Type type, string path)
        {
            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);

            return asset;
        }
        #endregion

        #region 内置资源
        public static Type[] BuiltinAssetTypes =
        {
            typeof(Mesh),
            typeof(Material),
            typeof(Texture2D),
            typeof(Font),
            typeof(Shader),
            typeof(Sprite),
            typeof(LightmapParameters),
        };


        private static List<Object> _builtinAssets;

        public static List<Object> BuiltinAssets
        {
            get
            {
                if (_builtinAssets == null)
                {
                    _builtinAssets = new List<Object>();
                    var utilityType = typeof(EditorGUIUtility);
                    var unityType = utilityType.GetSameAssemblyType("UnityEditor.UnityType");
                    var builinType = utilityType.GetSameAssemblyType("UnityEditor.BuiltinResource");

                    foreach (var builtinAssetType in BuiltinAssetTypes)
                    {
                        var name = builtinAssetType.ToString().Substring(builtinAssetType.Namespace.Length + 1);
                        var type = unityType.InvokeStaticMethod("FindTypeByName", ReflectionExtension.DefaultFlags, name);
                        var id = unityType.GetPropertyValue("persistentTypeID", type);
                        var resArray = utilityType.InvokeStaticMethod("GetBuiltinResourceList",
                            BindingFlags.Static | BindingFlags.NonPublic, id) as Array;
                        foreach (var res in resArray)
                        {
                            var insId = (int)builinType.GetFieldValue("m_InstanceID", res);
                            _builtinAssets.Add(EditorUtility.InstanceIDToObject(insId));
                        }
                    }
                }

                return _builtinAssets;
            }
        }


        public static bool IsBuiltinAsset(Object obj)
        {
            return BuiltinAssets.Contains(obj);
        }

        public static int GetInstanceIDFromGUID(string guid)
        {
            return (int) typeof (AssetDatabase).InvokeStaticMethod("GetInstanceIDFromGUID",
                BindingFlags.Static | BindingFlags.NonPublic, guid);
        }

        public static Object GUIDToObject(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var id = GetInstanceIDFromGUID(guid);
            if (id != 0)
            {
                return EditorUtility.InstanceIDToObject(id);
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadMainAssetAtPath(path);
        }
        #endregion

        #region 获取一些路径相关
        [MenuItem("Assets/Path/CopyRelativePath", true)]
        [MenuItem("Assets/Path/CopyAbsolutionPath", true)]
        private static bool CopyRelativePathCheck()
        {
            return Selection.assetGUIDs.Length == 1;
        }

        [MenuItem("Assets/Path/CopyRelativePath")]
        private static void CopyRelativePath()
        {
            GUIUtility.systemCopyBuffer = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        }

        [MenuItem("Assets/Path/CopyAbsolutionPath")]
        private static void CopyAbsolutionPath()
        {
            GUIUtility.systemCopyBuffer = IOHelper.GetFullPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }


        public static string GetLibraryDll(string name)
        {
            return string.Format("{0}/Library/ScriptAssemblies/{1}.dll", IOHelper.CurrentDirectory, name);
        }
        #endregion

        public static void GetTextureSize(TextureImporter importer, out int width, out int height)
        {
            object[] args = new object[2] { 0, 0 };

            importer.GetType().InvokeMethod("GetWidthAndHeight", importer, BindingFlags.NonPublic | BindingFlags.Instance, args);

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