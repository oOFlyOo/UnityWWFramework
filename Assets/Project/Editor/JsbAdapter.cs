
#if ENABLE_JSB

using UnityEngine;
using System.Collections;

public class JsbAdapter
{
    [UnityEditor.InitializeOnLoadMethod]
    private static void Adapting()
    {
        var oldScripts = JSBCodeGenSettings.PathsNotToJavaScript.ToList();
        oldScripts.Add("ZPH/");
        JSBCodeGenSettings.PathsNotToJavaScript = oldScripts.ToArray();
    }
}

#endif