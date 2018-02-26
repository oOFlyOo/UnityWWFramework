
using UnityEngine;

namespace WWFramework.Helper
{
    public static class AssetHelper
    {
        #region dll
        public const string AssemblyCSharpfirstpass = "Assembly-CSharp-firstpass";
        #endregion

        #region DefaultCommand
        public static string DefaultCommand
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        {
                            return "cmd";
                        }
                    case RuntimePlatform.OSXEditor:
                        {
                            return "/bin/bash";
                        }
                }
                return string.Empty;
            }
        }
        #endregion
    }
}