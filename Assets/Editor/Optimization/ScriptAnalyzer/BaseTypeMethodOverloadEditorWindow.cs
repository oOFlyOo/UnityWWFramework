using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using WWFramework.Helper;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class BaseTypeMethodOverloadEditorWindow: BaseEditorWindow
    {
        private string _searchType;

        private Dictionary<string, HashSet<string>> _monoMethodDict = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, HashSet<string>> _monoFieldDict = new Dictionary<string, HashSet<string>>();

        private Vector2 _scroll;

        [MenuItem("WWFramework/BaseTypeMethodOverload/Window")]
        private static BaseTypeMethodOverloadEditorWindow GetWindow()
        {
            return GetWindowExt<BaseTypeMethodOverloadEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.BeginHorizontal();
            {
                _searchType = EditorUIHelper.SearchCancelTextField(_searchType);
                EditorUIHelper.Button("Caculate", Calculate);
                EditorUIHelper.Button("CaculateAll", CaulateAll);
            }
            EditorUIHelper.EndHorizontal();

            _scroll = EditorUIHelper.BeginScrollView(_scroll);
            {
                foreach (var keyValue in _monoFieldDict)
                {
                    EditorUIHelper.DrawLine();
                    EditorUIHelper.TextField("Type:", keyValue.Key);

                    foreach (var method in keyValue.Value)
                    {
                        EditorUIHelper.TextField("Field:", method);
                    }
                }

                EditorUIHelper.Space();
                foreach (var keyValue in _monoMethodDict)
                {
                    EditorUIHelper.DrawLine();
                    EditorUIHelper.TextField("Type:", keyValue.Key);

                    foreach (var method in keyValue.Value)
                    {
                        EditorUIHelper.TextField("Method:", method);
                    }
                }
            }
            EditorUIHelper.EndScrollView();
        }

        private void Calculate()
        {
            if (string.IsNullOrEmpty(_searchType))
            {
                return;
            }

            var dll = Assembly.LoadFile(EditorAssetHelper.GetLibraryDll(AssetHelper.AssemblyCSharp));
            var type = dll.GetType(_searchType);

            if (type == null)
            {
                return;
            }

            var baseType = type;
            var types = dll.GetTypes();
            var subTypes = new HashSet<Type>();
            foreach (var t in types)
            {
                // 其实这里把自己也算进去了
                if (baseType.IsAssignableFrom(t))
                {
                    subTypes.Add(t);
                }
            }

            CaulateTypes(subTypes, dll);
        }

        private void CaulateAll()
        {
            var dll = Assembly.LoadFile(EditorAssetHelper.GetLibraryDll(AssetHelper.AssemblyCSharp));
            var types = new HashSet<Type>();
            foreach (var type in dll.GetTypes())
            {
                if (type.IsEnum)
                {
                    continue;
                }

                types.Add(type);
            }
            CaulateTypes(types, dll);
        }

        /// <summary>
        /// 有遗漏，会漏掉私有函数
        /// </summary>
        /// <param name="types"></param>
        /// <param name="dll"></param>
        private void CaulateTypes(HashSet<Type> types, Assembly dll)
        {
            _monoFieldDict.Clear();
            _monoMethodDict.Clear();

            var allMethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var allFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var subTypeOverloadDict = new Dictionary<string, bool>();
            var baseTypeOverloadDict = new Dictionary<string, bool>();
            foreach (var type in types)
            {
                CaulateOverloadMethod(subTypeOverloadDict, type,  allMethodFlags);

                var typeName = type.FullName;
                var baseType = type.BaseType;
                if (baseType != null && baseType.Assembly == dll)
                {
                    CaulateHideField(type, baseType, fieldFlags, allFieldFlags, _monoFieldDict);

                    CaulateOverloadMethod(baseTypeOverloadDict, type.BaseType, allMethodFlags);
                    GetFlattenHierarchyOverloadMethod(subTypeOverloadDict, baseTypeOverloadDict, _monoMethodDict, typeName);
                }

                foreach (var inter in type.GetInterfaces())
                {
                    // 父类型继承的接口不管
                    if (baseType != null && baseType.GetInterface(inter.Name) != null)
                    {
                        continue;
                    }

                    if (inter.Assembly != dll)
                    {
                        continue;
                    }

                    CaulateOverloadMethod(baseTypeOverloadDict, inter, allMethodFlags);
                    GetFlattenHierarchyOverloadMethod(subTypeOverloadDict, baseTypeOverloadDict, _monoMethodDict, typeName);
                }
            }
        }

        private void CaulateHideField(Type type, Type baseType, BindingFlags flags, BindingFlags baseFlags,
            Dictionary<string, HashSet<string>> monoFieldDict)
        {
            var fields = new HashSet<string>(type.GetFields(flags).Select(info => info.Name));
            var baseFields = new HashSet<string>(baseType.GetFields(baseFlags).Select(info => info.Name));
            foreach (var field in fields)
            {
                if (baseFields.Contains(field))
                {
                    HashSet<string> hashSet;
                    if (!monoFieldDict.TryGetValue(field, out hashSet))
                    {
                        hashSet = new HashSet<string>();
                        monoFieldDict[type.FullName] = hashSet;
                    }

                    hashSet.Add(field);
                }
            }
        }

        private void CaulateOverloadMethod(Dictionary<string, bool> methodOverideDict, Type type, BindingFlags flags)
        {
            methodOverideDict.Clear();
            var methods = type.GetMethods(flags);
            foreach (var method in methods)
            {
                var name = method.Name;
                methodOverideDict[name] = methodOverideDict.ContainsKey(name);
            }
        }

        private void GetFlattenHierarchyOverloadMethod(Dictionary<string, bool> subOverloadDict,
            Dictionary<string, bool> baseOverloadDict, Dictionary<string, HashSet<string>> monoMethoDict, string typeName)
        {
            foreach (var keyValue in subOverloadDict)
            {
                // 说明有重载
                if (keyValue.Value)
                {
                    // 父类型有同名函数，并且没有重载
                    if (baseOverloadDict.ContainsKey(keyValue.Key) && !baseOverloadDict[keyValue.Key])
                    {
                        HashSet<string> methodNames;
                        if (!monoMethoDict.TryGetValue(typeName, out methodNames))
                        {
                            methodNames = new HashSet<string>();
                            monoMethoDict[typeName] = methodNames;
                        }

                        methodNames.Add(keyValue.Key);
                    }
                }
            }

        }
    }
}