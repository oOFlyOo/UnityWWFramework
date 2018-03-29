using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class MissingReferenceEditorWindow: BaseEditorWindow
    {
        [MenuItem("WWFramework/MissingReference/Window")]
        public static MissingReferenceEditorWindow GetWindow()
        {
            return GetWindowExt<MissingReferenceEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.Button("搜索 Project", FindAllAssets);

            EditorUIHelper.Space();
            EditorUIHelper.Button("搜索当前场景", FindCurrentScene);
        }

        private void FindAllAssets()
        {
            var checkList =
                AssetDatabase.GetAllAssetPaths()
                    .Select(s => AssetDatabase.LoadMainAssetAtPath(s));

            var resultList = EditorAssetHelper.FindMissingReferences(checkList.ToList());

            SelectingEditorWindow.Show(resultList);
        }

        private void FindCurrentScene()
        {
            var checkList = SceneManager.GetActiveScene().GetRootGameObjects();

            var resultList = EditorAssetHelper.FindMissingReferences(checkList.Select(o => o as Object).ToList());

            SelectingEditorWindow.Show(resultList);
        }
    }
}