using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
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

            EditorUIHelper.Space();
            EditorUIHelper.Button("内置引用", ShowBuiltinDependences);
        }

        private void ShowDependences()
        {
            SelectingEditorWindow.Show(EditorUtility.CollectDependencies(GetSelections()).ToList(), "引用信息：");
        }

        private void ShowReverseDependences()
        {
            var selections = GetSelections();
            var paths = selections.Select(obj => AssetDatabase.GetAssetPath(obj)).ToArray();
            var objPaths = EditorHelper.GetReverseDependencies(paths);

            SelectingEditorWindow.Show(objPaths.ConvertAll(input => AssetDatabase.LoadMainAssetAtPath(input)), "反向引用：");
        }

        private void ShowBuiltinDependences()
        {
            var dependences = EditorUtility.CollectDependencies(GetSelections());
            var builtin = dependences.Where(o => AssetHelper.IsBuiltinAsset(o)).ToList();

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
    }
}