
using System.IO;

namespace WWFramework.Helper
{
    public static class IOHelper
    {
        #region 路径操作
        private static string _currentDirectory;
        public static string CurrentDirectory
        {
            get
            {
                if (_currentDirectory == null)
                {
                    _currentDirectory = Directory.GetCurrentDirectory().Replace("\\", "/");
                    _currentDirectory.TrimEnd('/');
                }

                return _currentDirectory;
            }
        }


        /// <summary>
        /// 输入进来的 fullPath 的保证是 / 而不是 \
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string fullPath, string rootPath = null)
        {
            rootPath = rootPath ?? CurrentDirectory;

            return fullPath.Remove(0, rootPath.Length + 1);
        }
        #endregion

        #region 文件，文件夹操作
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
        #endregion
    }
}