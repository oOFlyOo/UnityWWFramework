using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class CrossDomainDelegateEditorWindow: BaseEditorWindow
    {
        private Regex _unSafeRegex = new Regex("OnDestroy.+?{((?<Open>{)|(?<-Open>})|[^{}])*(?(Open)(?!))}", RegexOptions.Singleline);

        private Dictionary<MonoScript, List<FieldInfo>> _monoFieldDict;

        private Dictionary<MonoScript, List<FieldInfo>> MonoFieldDict
        {
            get
            {
                if (_monoFieldDict == null)
                {
                    _monoFieldDict = Calculate();
                }
                return _monoFieldDict;
            }
        }

        private Dictionary<MonoScript, List<FieldInfo>> _unSafeMonoFieldDict;

        private Dictionary<MonoScript, List<FieldInfo>> UnSafeMonoFieldDict
        {
            get
            {
                if (_unSafeMonoFieldDict == null)
                {
                    _unSafeMonoFieldDict = CalculateUnSafe(MonoFieldDict);
                }
                return _unSafeMonoFieldDict;
            }
        }

        private enum ShowType
        {
            All,
            UnSafe,
        }

        private ShowType _showType;
        private Vector2 _scrool;


        [MenuItem("WWFramework/CrossDomainDelegate/Window")]
        private static CrossDomainDelegateEditorWindow GetWindow()
        {
            return GetWindow<CrossDomainDelegateEditorWindow>();
        }


        protected override void CustomOnGUI()
        {
            _showType = EditorUIHelper.EnumPopup<ShowType>("显示:", _showType);

            EditorUIHelper.Space();
            var dict = _showType == ShowType.All ? MonoFieldDict : UnSafeMonoFieldDict;

            _scrool = EditorUIHelper.BeginScrollView(_scrool);
            {
                foreach (var keyValue in dict)
                {
                    EditorUIHelper.Space();
                    EditorUIHelper.ObjectField(string.Empty, keyValue.Key);
                    foreach (var fieldInfo in keyValue.Value)
                    {
                        EditorUIHelper.LabelField(fieldInfo.Name);
                    }
                }
            }
            EditorUIHelper.EndScrollView();
        }

        private Dictionary<MonoScript, List<FieldInfo>> Calculate()
        {
            var dict = new Dictionary<MonoScript, List<FieldInfo>>();
            var monoType = typeof (MonoBehaviour);
            var listType = typeof (List<>);
            var delegateType = typeof (Delegate);
            var eventType =
                Assembly.LoadFile(EditorAssetHelper.GetLibraryDll(AssetHelper.AssemblyCSharpfirstpass))
                    .GetType("EventDelegate");

            foreach (var findAsset in EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Script))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(findAsset);
                var scriptType = script.GetClass();
                if (scriptType != null && scriptType.IsSubclassOf(monoType) && !scriptType.IsNestedAssembly &&
                    scriptType.Assembly.FullName.Contains(AssetHelper.AssemblyCSharpfirstpass))
                {
                    foreach (var fieldInfo in scriptType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        var filedType = fieldInfo.FieldType;
                        if (filedType.IsGenericType && filedType.GetGenericTypeDefinition() == listType)
                        {
                            filedType = filedType.GetGenericArguments()[0];
                        }

                        if (filedType.IsSubclassOf(delegateType) ||
                            (eventType != null && filedType.IsSubclassOf(eventType)))
                        {
                            if (!dict.ContainsKey(script))
                            {
                                dict[script] = new List<FieldInfo>();
                            }

                            dict[script].Add(fieldInfo);
                        }
                    }
                }
            }

            return dict;
        }


        private Dictionary<MonoScript, List<FieldInfo>> CalculateUnSafe(Dictionary<MonoScript, List<FieldInfo>> allDict)
        {
            var dict = new Dictionary<MonoScript, List<FieldInfo>>();
            foreach (var keyValue in allDict)
            {
                var destroy = _unSafeRegex.Match(keyValue.Key.text).Value;
                if (string.IsNullOrEmpty(destroy))
                {
                    dict[keyValue.Key] = keyValue.Value.ToList();
                    continue;
                }

                foreach (var fieldInfo in keyValue.Value)
                {
                    if (!destroy.Contains(fieldInfo.Name))
                    {
                        if (!dict.ContainsKey(keyValue.Key))
                        {
                            dict[keyValue.Key] = new List<FieldInfo>();
                        }

                        dict[keyValue.Key].Add(fieldInfo);
                    }
                }
            }

            return dict;
        }
    }
}