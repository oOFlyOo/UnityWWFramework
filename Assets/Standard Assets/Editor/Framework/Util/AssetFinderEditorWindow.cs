using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class AssetFinderEditorWindow : BaseEditorWindow
    {
        private string _searchText;

        [MenuItem("WWFramework/AssetFinder/Window")]
        private static AssetFinderEditorWindow GetWindow()
        {
            return GetWindowExt<AssetFinderEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            _searchText = EditorUIHelper.SearchCancelTextField(_searchText);

            EditorUIHelper.Space();
            EditorUIHelper.Button("InstanceID", () =>
            {
                int id;
                if (int.TryParse(_searchText, out id))
                {
                    EditorAssetHelper.SelectObject(EditorUtility.InstanceIDToObject(id));
                }
            });


            EditorUIHelper.Space();
            EditorUIHelper.Button("GUID", () =>
            {
                EditorAssetHelper.SelectObject(EditorAssetHelper.GUIDToObject(_searchText));
            });
        }
    }
}