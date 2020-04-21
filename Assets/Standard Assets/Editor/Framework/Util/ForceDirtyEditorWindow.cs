


using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class ForceDirtyEditorWindow: BaseEditorWindow
    {
        [MenuItem("WWFramework/ForceDirty/Window")]
        private static ForceDirtyEditorWindow GetWindow()
        {
            return GetWindowExt<ForceDirtyEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.Space();
            EditorUIHelper.LabelField("只有代码相关的会重新序列化，材质球不会，但是材质球也会更新serializedVersion！");

            EditorUIHelper.Space();
            EditorUIHelper.Button("更新被依赖的资源", UpdateSelectionsDependencies);

            EditorUIHelper.Space();
            EditorUIHelper.Button("更新依赖的资源", UpdateSelectionsRevertDependencies);

            EditorUIHelper.Space();
            EditorUIHelper.Button("更新选中的文件夹", UpdateSelectionsFolder);
        }

        private void UpdateSelectionsDependencies()
        {
            var paths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            foreach (var path in AssetDatabase.GetDependencies(paths))
            {
                EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(path));
            }
            AssetDatabase.SaveAssets();
        }

        private void UpdateSelectionsRevertDependencies()
        {
            var paths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            foreach (var path in Helper.Editor.EditorAssetHelper.GetReverseDependencies(paths))
            {
                EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(path));
            }
            AssetDatabase.SaveAssets();
        }

        private void UpdateSelectionsFolder()
        {
            var targets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            foreach (var target in targets)
            {
                EditorUtility.SetDirty(target);
            }
            AssetDatabase.SaveAssets();
        }
    }
}