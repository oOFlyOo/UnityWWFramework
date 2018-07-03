
using UnityEngine;

namespace WWFramework.Helper
{
    public static class AssetHelper
    {
        #region dll
        public const string AssemblyCSharpfirstpass = "Assembly-CSharp-firstpass";
        public const string AssemblyCSharp = "Assembly-CSharp";
        #endregion

        #region DefaultCommand
        public const string DefaultWinCommand = "cmd";
        public const string DefaultMacCommand = "/bin/bash";

        public static string DefaultCommand
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        {
                            return DefaultWinCommand;
                        }
                    case RuntimePlatform.OSXEditor:
                        {
                            return DefaultMacCommand;
                        }
                }
                return string.Empty;
            }
        }
        #endregion
    }
}
