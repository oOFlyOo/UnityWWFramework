
using System.Collections.Generic;
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
            return GetWindow<MissingReferenceEditorWindow>();
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
                AssetHelper.FindAssetsPaths(AssetHelper.SearchFilter.Prefab)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>);

            var resultList = AssetHelper.FindMissingReferences(checkList.ToList());

            SelectingEditorWindow.Show(resultList.ConvertAll(input => (Object)input));
        }

        private void FindCurrentScene()
        {
            var checkList = SceneManager.GetActiveScene().GetRootGameObjects();

            var resultList = AssetHelper.FindMissingReferences(checkList.ToList());

            SelectingEditorWindow.Show(resultList.ConvertAll(input => (Object)input));
        }
    }
}