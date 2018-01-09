﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using WWFramework.Editor.Helper;
using WWFramework.Editor.UI;

namespace WWFramework.Editor.Util
{
    public class ScriptableObjectEditorWindow:BaseEditorWindow
    {
        [NonSerialized]
        private List<MonoScript> _scriptList;

        private List<MonoScript> ScriptList
        {
            get
            {
                if (_scriptList == null)
                {
                    _scriptList = AssetHelper.FindScriptableObjects();
                }
                return _scriptList;
            }
        }


        [MenuItem("Util/ScriptableObject/Window")]
        public static ScriptableObjectEditorWindow GetWindow()
        {
            return GetWindow<ScriptableObjectEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            foreach (var monoScript in ScriptList)
            {
                EditorUIHelper.BeginHorizontal();
                EditorUIHelper.ObjectField("", monoScript);
                EditorUIHelper.Button("创建", () => CreateScriptObject(monoScript));
                EditorUIHelper.EndHorizontal();
                EditorUIHelper.Space();
            }
        }


        private void CreateScriptObject(MonoScript script)
        {
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));
            var asset = AssetHelper.CreateScriptableObjectAsset(script.GetClass(),
                string.Format("{0}/{1}.asset", path, script.name));

            AssetDatabase.Refresh();
            AssetHelper.SelectObject(asset);
        }
    }
}