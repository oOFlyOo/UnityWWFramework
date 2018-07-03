using System;
using System.Linq;
using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;
using Object = UnityEngine.Object;

namespace WWFramework.Util.Editor
{
    public class ReferenceEditorWindow: BaseEditorWindow
    {
        private Object _searchObject;

        [MenuItem("WWFramework/Reference/Window")]
        private static ReferenceEditorWindow GetWindow()
        {
            return GetWindowExt<ReferenceEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            _searchObject = EditorUIHelper.ObjectField(_searchObject, typeof(Object), "搜索：", true);

            EditorUIHelper.Space();
            EditorUIHelper.Button("获取引用", CollectDependences);

            EditorUIHelper.Space();
            EditorUIHelper.Button("获取引用（非内置）", ShowDependences);

//            EditorUIHelper.Space();
//            EditorUIHelper.Button("反向引用", CollectReverseDependences);

            EditorUIHelper.Space();
            EditorUIHelper.Button("反向引用（非内置）", ShowReverseDependences);

            EditorUIHelper.Space();
            EditorUIHelper.Button("内置引用", ShowBuiltinDependences);
        }

        private void CollectDependences()
        {
            SelectingEditorWindow.Show(EditorUtility.CollectDependencies(GetSelections()).ToList(), "引用信息：");
        }

        private void ShowDependences()
        {
            SelectingEditorWindow.Show(AssetDatabase.GetDependencies(GetSelectionPaths()).Select(path => AssetDatabase.LoadMainAssetAtPath(path)).ToList(), "引用信息：");
        }

        private void ShowReverseDependences()
        {
            var selections = GetSelections();
            var paths = selections.Select(obj => AssetDatabase.GetAssetPath(obj)).ToArray();
            var objPaths = WWFramework.Helper.Editor.EditorHelper.GetReverseDependencies(paths);

            SelectingEditorWindow.Show(objPaths.ConvertAll(input => AssetDatabase.LoadMainAssetAtPath(input)), "反向引用：");
        }

        private void ShowBuiltinDependences()
        {
            var dependences = EditorUtility.CollectDependencies(GetSelections());
            var builtin = dependences.Where(o => EditorAssetHelper.IsBuiltinAsset(o)).ToList();

            SelectingEditorWindow.Show(builtin, "引用的内置资源");
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

        private string[] GetSelectionPaths()
        {
            if (_searchObject != null)
            {
                return new[] {AssetDatabase.GetAssetPath(_searchObject)};
            }
            else
            {
                return Selection.assetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            }
        }
    }
}