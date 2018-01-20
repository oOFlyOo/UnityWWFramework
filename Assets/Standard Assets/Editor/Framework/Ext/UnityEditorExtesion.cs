using UnityEditor;

namespace WWFramework.Extension.Editor
{
    public static class UnityEditorExtesion
    {
        public static string GetScriptableObjectPathByMonoScript(this MonoScript monoScript)
        {
            var path = AssetDatabase.GetAssetPath(monoScript);

            return path.Replace(".cs", ".asset");
        }
    }
}