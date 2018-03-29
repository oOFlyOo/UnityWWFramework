using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
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
                    _scriptList = EditorAssetHelper.FindScriptableObjects();
                }
                return _scriptList;
            }
        }


        [MenuItem("WWFramework/ScriptableObject/Window")]
        public static ScriptableObjectEditorWindow GetWindow()
        {
            return GetWindowExt<ScriptableObjectEditorWindow>();
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
            var asset = EditorAssetHelper.CreateScriptableObjectAsset(script.GetClass(),
                string.Format("{0}/{1}.asset", path, script.name));

            AssetDatabase.Refresh();
            EditorAssetHelper.SelectObject(asset);
        }
    }
}