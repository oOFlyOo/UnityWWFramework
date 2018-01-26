using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class BuiltinResourceEditorWindow: BaseEditorWindow
    {            
        [MenuItem("WWFramework/BuiltinResource/Window")]
        private static BuiltinResourceEditorWindow GetWindow()
        {
            return GetWindow<BuiltinResourceEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.Button("显示内置资源", () =>
            {
                SelectingEditorWindow.Show(EditorAssetHelper.BuiltinAssets);
            });
        }
    }
}