
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using WWFramework.Extension;
using WWFramework.Reflection;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace WWFramework.Helper.Editor
{
    public static class EditorAssetHelper
    {
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

        private static string _projectTempPath;

        public static string ProjectTempPath
        {
            get
            {
                if (String.IsNullOrEmpty(_projectTempPath))
                {
                    _projectTempPath = IOHelper.ProjectDirectory + "/Temp";
                }

                return _projectTempPath;
            }
        }

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

        private static Dictionary<SearchFilter, Type> _searchFilterTypes = new Dictionary<SearchFilter, Type>()
        {
            {SearchFilter.AnimationClip, typeof(AnimationClip)},
            {SearchFilter.AudioClip, typeof(AudioClip)},
            {SearchFilter.AudioMixer, typeof(AudioMixer)},
            {SearchFilter.Font, typeof(Font)},
            {SearchFilter.GUISkin, typeof(GUISkin)},
            {SearchFilter.Material, typeof(Material)},
            {SearchFilter.Mesh, typeof(Mesh)},
            {SearchFilter.PhysicMaterial, typeof(PhysicMaterial)},
            {SearchFilter.Scene, typeof(SceneAsset)},
//            {SearchFilter.Script, typeof(MonoScript) },
            {SearchFilter.Shader, typeof(Shader)},
            {SearchFilter.Sprite, typeof(Sprite)},
            {SearchFilter.Texture, typeof(Texture)},
        };

        public static bool IsMatch(Object obj, SearchFilter filter)
        {
            if (obj == null)
            {
                return false;
            }

            if (filter == SearchFilter.All)
            {
                return true;
            }
            else
            {
                var objType = obj.GetType();

                if (typeof(Component).IsAssignableFrom(objType) && !typeof(MonoBehaviour).IsAssignableFrom(objType))
                {
                    return false;
                }

                foreach (var keyValue in _searchFilterTypes)
                {
                    if (keyValue.Value.IsAssignableFrom(objType))
                    {
                        return keyValue.Key == filter;
                    }
                }

                return IsMatch(AssetDatabase.GetAssetPath(obj), filter);
            }
        }

        private static bool IsMatch(string path, SearchFilter filter)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            switch (filter)
            {
                case SearchFilter.Model:
                    {
                        return path.EndsWith(".FBX");
                    }
                case SearchFilter.Prefab:
                    {
                        return path.EndsWith(".prefab");
                    }
                case SearchFilter.Script:
                    {
                        return path.EndsWith(".cs");
                    }
            }

            return false;
        }

        public static string[] FindAssets(SearchFilter filter, params string[] searchInFolders)
        {
            var filterStr = filter != SearchFilter.All ? String.Format("t:{0}", filter) : null;
            return AssetDatabase.FindAssets(filterStr, searchInFolders.ParamsFixing());
        }

        public static List<string> FindAssetsPaths(SearchFilter filter, params string[] searchInFolders)
        {
            return FindAssets(filter, searchInFolders.ParamsFixing()).Select(s => AssetDatabase.GUIDToAssetPath(s))
                .ToList();
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
                if (type != null && type.IsSubclassOf(scriptObjType) && !type.IsSubclassOf(editorType) &&
                    !type.IsSubclassOf(editorWindowType))
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

        #region 资源引用

        private const string RevertDependenciesSearch =
            "t:AnimatorController t:AnimatorOverrideController t:Material t:Prefab t:Scene t:ScriptableObject";

        private class AssetDependence
        {
            private static readonly string[] CacheNull = new string[0];

            public Hash128 Hash;
            public string[] Dependencies = CacheNull;
        }

        private static readonly Dictionary<string, AssetDependence> AssetDependenceDict =
            new Dictionary<string, AssetDependence>();

        public static List<string> GetReverseDependencies(string[] paths, string[] searchPaths = null)
        {
            var includeList = new HashSet<string>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var guid in AssetDatabase.FindAssets(RevertDependenciesSearch, searchPaths))
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                var hash = AssetDatabase.GetAssetDependencyHash(filePath);
                AssetDependence assetDepend;
                if (!AssetDependenceDict.TryGetValue(filePath, out assetDepend))
                {
                    assetDepend = new AssetDependence();
                    AssetDependenceDict[filePath] = assetDepend;
                }

                if (assetDepend.Hash != hash)
                {
                    assetDepend.Hash = hash;
                    assetDepend.Dependencies = AssetDatabase.GetDependencies(filePath, false);
                }

                var dependencies = assetDepend.Dependencies;

                foreach (var dependence in dependencies)
                {
                    if (paths.Any(dependence.Contains))
                    {
                        includeList.Add(filePath);
                        break;
                    }
                }
            }

            stopWatch.Stop();
            UnityEngine.Debug.Log(String.Format("反向查找依赖耗时：{0}", stopWatch.Elapsed.Seconds));

            return includeList.ToList();
        }

        private class AssetDependenceObjects
        {
            private static readonly Object[] CacheNull = new Object[0];

            public Hash128 Hash;
            public Object[] Dependencies = CacheNull;
        }

        private static readonly Dictionary<string, AssetDependenceObjects> AssetDependenceObjectDict =
            new Dictionary<string, AssetDependenceObjects>();

        private static readonly Object[] CacheSearchAsset = new Object[1];

        private static Object[] GetCacheSearchAsset(Object obj)
        {
            CacheSearchAsset[0] = obj;

            return CacheSearchAsset;
        }

        public static List<Object> CollectReverseDependencies(Object[] objs, string[] searchPaths = null)
        {
            var includeList = new HashSet<Object>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var searchObjs = new HashSet<Object>();
            foreach (var guid in AssetDatabase.FindAssets(RevertDependenciesSearch, searchPaths))
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                var mainObj = AssetDatabase.LoadMainAssetAtPath(filePath);
                // EditorUtility.CollectDependencies 是 recursive 的
                if (searchObjs.Contains(mainObj))
                {
                    continue;
                }

                var hash = AssetDatabase.GetAssetDependencyHash(filePath);
                AssetDependenceObjects assetDepend;
                if (!AssetDependenceObjectDict.TryGetValue(filePath, out assetDepend))
                {
                    assetDepend = new AssetDependenceObjects();
                    AssetDependenceObjectDict[filePath] = assetDepend;
                }

                if (assetDepend.Hash != hash)
                {
                    assetDepend.Hash = hash;
                    assetDepend.Dependencies =
                        EditorUtility.CollectDependencies(GetCacheSearchAsset(mainObj));
                }

                var dependencies = assetDepend.Dependencies;

                var isMatch = false;
                foreach (var dependence in dependencies)
                {
                    if (Array.IndexOf(objs, dependence) >= 0)
                    {
                        includeList.Add(mainObj);
                        isMatch = true;

                        break;
                    }
                }

                if (!isMatch)
                {
                    // 如果该对象没搜到，那该对象包含的对象都不需要搜了
                    searchObjs.UnionWith(dependencies);
                }
            }

            // 字体会错乱？
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            stopWatch.Stop();
            UnityEngine.Debug.Log(String.Format("反向查找依赖耗时：{0}", stopWatch.Elapsed.Seconds));

            return includeList.ToList();
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

                        object id = null;
                        if (unityType != null)
                        {
                            var type = unityType.InvokeStaticMethod("FindTypeByName", ReflectionExtension.DefaultFlags,
                                name);
                            id = unityType.GetPropertyValue("persistentTypeID", type);
                        }
                        else
                        {
                            var baseObjectTools =
                                utilityType.GetSameAssemblyType("UnityEditorInternal.BaseObjectTools");
                            id = baseObjectTools.InvokeStaticMethod("StringToClassID", ReflectionExtension.DefaultFlags,
                                name);
                        }

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
            return (int)typeof(AssetDatabase).InvokeStaticMethod("GetInstanceIDFromGUID",
                BindingFlags.Static | BindingFlags.NonPublic, guid);
        }

        public static Object GUIDToObject(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                return null;
            }

            var id = GetInstanceIDFromGUID(guid);
            if (id != 0)
            {
                return EditorUtility.InstanceIDToObject(id);
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadMainAssetAtPath(path);
        }

        #endregion

        #region 获取一些路径相关

        [MenuItem("Assets/Path/CopyRelativePath", true)]
        [MenuItem("Assets/Path/CopyAbsolutionPath", true)]
        private static bool CopyPathCheck()
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

        [MenuItem("GameObject/Path/CopyRelativePath", true)]
        private static bool CopyGameObjectPathCheck()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/Path/CopyGameObjectPath")]
        private static void CopyGameObjectPath()
        {
            GUIUtility.systemCopyBuffer = Selection.activeGameObject.transform.GetHierarchyPath();
        }

        [MenuItem("Assets/Save Assets %#&s")]
        private static void SaveAssets()
        {
            AssetDatabase.SaveAssets();
        }


        public static string GetLibraryDll(string name)
        {
            return String.Format("{0}/Library/ScriptAssemblies/{1}.dll", IOHelper.ProjectDirectory, name);
        }

        #endregion

        #region 图片相关

        public static void GetTextureSize(TextureImporter importer, out int width, out int height)
        {
            object[] args = new object[2] { 0, 0 };
            var method =
                typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(importer, args);
            width = (int)args[0];
            height = (int)args[1];
        }

        public enum TextureSizeFeatureLevel
        {
            None,
            Multiple4,
            POT,
            SquarePOT,
        }

        public static TextureSizeFeatureLevel GetTextureSizeFearture(string assetPath)
        {
            return GetTextureSizeFearture((TextureImporter)AssetImporter.GetAtPath(assetPath));
        }

        public static TextureSizeFeatureLevel GetTextureSizeFearture(TextureImporter importer)
        {
            int width;
            int height;
            GetTextureSize(importer, out width, out height);

            if (width % 4 == 0 && height % 4 == 0)
            {
                var potW = (width & (width - 1)) == 0;
                var potH = (height & (height - 1)) == 0;

                if (potW && potH)
                {
                    if (width == height)
                    {
                        return TextureSizeFeatureLevel.SquarePOT;
                    }

                    return TextureSizeFeatureLevel.POT;
                }

                return TextureSizeFeatureLevel.Multiple4;
            }

            return TextureSizeFeatureLevel.None;
        }

        private const int AstcAlphaInterval = 6;

        public static TextureImporterFormat FixAstcAlphaFormat(TextureImporterFormat format, bool hasAlpha)
        {
            if (hasAlpha)
            {
                if (format >= TextureImporterFormat.ASTC_RGB_4x4 && format <= TextureImporterFormat.ASTC_RGB_12x12)
                {
                    return format + AstcAlphaInterval;
                }
            }
            else
            {
                if (format >= TextureImporterFormat.ASTC_RGBA_4x4 && format <= TextureImporterFormat.ASTC_RGBA_12x12)
                {
                    return format - AstcAlphaInterval;
                }
            }

            return format;
        }

        public static readonly string[] TextureSizeStrs = { "32", "64", "128", "256", "512", "1024", "2048" };
        public static readonly int[] TextureSizes = { 32, 64, 128, 256, 512, 1024, 2048 };

        #endregion

        #region 模型

        [MenuItem("Assets/Model/LogModelBonesCount")]
        private static void LogModelBonesCount()
        {
            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
            {
                var count = GetModelBonesCount(obj);
                if (count > 0)
                {
                    UnityEngine.Debug.Log(String.Format("{0} Bones:{1}", obj.name, count));
                }
            }
        }

        public static int GetModelBonesCount(Object obj)
        {
            var count = 0;
            var path = AssetDatabase.GetAssetPath(obj);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                //                return importer.transformPaths.Length;

                byte[] metaFileBytes = null;
                var optimize = importer.optimizeGameObjects;
                if (optimize)
                {
                    metaFileBytes = File.ReadAllBytes(AssetDatabase.GetTextMetaFilePathFromAssetPath(path));
                    importer.optimizeGameObjects = false;
                    importer.SaveAndReimport();
                }

                var skinnedMesh = ((GameObject)obj).GetComponentInChildren<SkinnedMeshRenderer>(true);
                if (skinnedMesh != null)
                {
                    count = skinnedMesh.bones.Length;
                }

                if (metaFileBytes != null)
                {
                    File.WriteAllBytes(AssetDatabase.GetTextMetaFilePathFromAssetPath(path), metaFileBytes);
                }
            }

            return count;
        }

        #endregion

        #region 资源类型

        public static bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }

        #endregion
    }
}