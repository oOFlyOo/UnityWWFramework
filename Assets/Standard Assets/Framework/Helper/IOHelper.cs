
using System.IO;

namespace WWFramework.Helper
{
    public static class IOHelper
    {
        public static bool IsDirectory(string path)
        {
            return Directory.Exists(path);
        }


        public static bool DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);

                return true;
            }

            return false;
        }

        public static bool CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                return true;
            }

            return false;
        }


        public static bool DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);

                return true;
            }

            return false;
        }
    }
}