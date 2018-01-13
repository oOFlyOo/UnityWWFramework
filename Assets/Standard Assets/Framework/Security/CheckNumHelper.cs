
using System;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;

namespace WWFramework.Security
{
    public static class CheckNumHelper
    {
        #region 常用编码操作
        public static string ConvertBytesToBase64(byte[] datas)
        {
            return Convert.ToBase64String(datas);
        }

        public static byte[] ConvertBase64ToBytes(string str)
        {
            return Convert.FromBase64String(str);
        }

        public static string ConvertBytesToUTF8(byte[] datas)
        {
            return Encoding.UTF8.GetString(datas);
        }

        public static byte[] ConvertUTF8ToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ConvertBytesToDefault(byte[] datas)
        {
            return Encoding.Default.GetString(datas);
        }

        public static byte[] ConvertDefaultToBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        #endregion

        #region CRC
        private static Crc32 _crc;

        public static Crc32 Crc
        {
            get
            {
                if (_crc == null)
                {
                    _crc = new Crc32();
                }
                return _crc;
            }
        }

        public static long ConvertBytesToCrc(byte[] datas)
        {
            Crc.Reset();
            Crc.Update(datas);

            return Crc.Value;
        }
        #endregion


        #region Md5

        private static MD5 _md5;

        public static MD5 Md5
        {
            get
            {
                if (_md5 == null)
                {
                    _md5 = MD5.Create();
                }
                return _md5;
            }
        }


        public static string ConvertBytesToMd5(byte[] datas)
        {
            byte[] hash = Md5.ComputeHash(datas);
            var result = System.BitConverter.ToString(hash);

            return result.Replace("-", "");
        }
        #endregion
    }
}