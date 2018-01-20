using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Preference.Editor
{
    [InitializeOnLoad]
    public static class LightingPreference
    {
        private const string AutoGenerateLighting = "AutoGenerateLighting";

        private static bool _autoGenerateLighting;

        static LightingPreference()
        {
            _autoGenerateLighting = EditorPrefs.GetBool(AutoGenerateLighting, false);

            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }


        [PreferenceItem("Lighting")]
        private static void PreferenceOnGUI()
        {
            EditorUIHelper.BeginChangeCheck();
            _autoGenerateLighting = EditorUIHelper.Toggle("自动烘焙", _autoGenerateLighting);
            if (EditorUIHelper.EndChangeCheck())
            {
                EditorPrefs.SetBool(AutoGenerateLighting, _autoGenerateLighting);
            }
        }


        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (mode == NewSceneMode.Single && !_autoGenerateLighting)
            {
                EditorHelper.ChangeAutoGenerateLightingState(false);
            }
        }
    }
}