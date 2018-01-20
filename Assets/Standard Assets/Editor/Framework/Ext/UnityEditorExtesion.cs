
using System;
using UnityEditor;
using WWFramework.Editor.Helper;

namespace WWFramework.Editor.Extension
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