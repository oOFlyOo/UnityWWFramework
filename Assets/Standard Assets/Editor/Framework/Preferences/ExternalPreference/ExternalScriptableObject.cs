
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Editor.Preference
{
    public class ExternalScriptableObject: ScriptableObject
    {
        [Serializable]
        public class ExternalTool
        {
            public string ToolPath;
            public string Extension;
        }

        public List<ExternalTool> ToolList;
    }
}