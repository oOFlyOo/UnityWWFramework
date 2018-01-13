
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Editor.UI;

namespace WWFramework.Editor.Util
{
    public class ReferenceEditorWindow: BaseEditorWindow
    {
        private Object _searchObject;

        [MenuItem("WWFramework/Reference/Window")]
        private static ReferenceEditorWindow GetWindow()
        {
            return GetWindow<ReferenceEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            _searchObject = EditorUIHelper.ObjectField("搜索：", _searchObject, typeof(Object), true);

            EditorUIHelper.Space();
            EditorUIHelper.Button("获取引用", ShowDependences);

            EditorUIHelper.Space();
            EditorUIHelper.Button("反向引用", ShowReverseDependences);
        }

        private void ShowDependences()
        {
            SelectingEditorWindow.Show(EditorUtility.CollectDependencies(GetSelections()).ToList(), "引用信息：");
        }

        private void ShowReverseDependences()
        {
            var selections = GetSelections();
            var paths = selections.Select(obj => AssetDatabase.GetAssetPath(obj)).ToList();
            var objPaths = new List<string>();

            foreach (var dependency in AssetDatabase.GetDependencies("Assets/Test/Scenes"))
            {
                Debug.Log(dependency);
            }

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                foreach (var dependency in AssetDatabase.GetDependencies(assetPath))
                {
                    foreach (var path in paths)
                    {
                        if (dependency.Contains(path))
                        {
                            objPaths.Add(assetPath);   
                        }
                    }
                }
            }

            SelectingEditorWindow.Show(objPaths.ConvertAll(input => AssetDatabase.LoadMainAssetAtPath(input)), "反向引用：");
        }

        private Object[] GetSelections()
        {
            if (_searchObject != null)
            {
                return new[] { _searchObject };
            }
            else
            {
                return Selection.objects;
            }
        }
    }
}