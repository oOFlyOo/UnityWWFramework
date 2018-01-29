
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
#if UNITY_STANDALONE_WIN
                return "cmd";
#elif UNITY_STANDALONE_OSX
                return "/bin/bash";
#else
                return string.Empty;
#endif
            }
        }
        #endregion
    }
}