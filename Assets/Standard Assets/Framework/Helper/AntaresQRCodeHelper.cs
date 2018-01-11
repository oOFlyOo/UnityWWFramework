using Antares.QRCode;
using UnityEngine;

namespace WWFramework.Helper
{
    /// <summary>
    /// 之前的 zxing.unity 在 iOS 上有问题，所以改为用这个
    /// 如果后续 zxing.unity 修复了的话，可以考虑改用 ZXing，这玩意更通用
    /// AntaresQRCode 不支持 Utf8
    /// </summary>
    public static class AntaresQRCodeHelper
    {
        /// <summary>
        /// 容错级别
        /// </summary>
        public enum ErrorCorrectionType
        {
            L,
            M,
            Q,
            H,
        }

        private static Texture2D _texture;

        private static Texture2D Texture
        {
            get
            {
                if (_texture == null)
                {
                    _texture = new Texture2D(0, 0, TextureFormat.RGB24, false);
                }
                return _texture;
            }
        }

        private static ErrorCorrectionLevel[] ErrorCorrectionLevelList = new[]
        {
            ErrorCorrectionLevel.L,
            ErrorCorrectionLevel.M,
            ErrorCorrectionLevel.Q,
            ErrorCorrectionLevel.H,
        };

        public static Texture2D Encode(string msg, int size, ErrorCorrectionType type)
        {
            return Encode(msg, size, size, type);
        }

        public static Texture2D Encode(string msg, int width, int height, ErrorCorrectionType type)
        {
            return QRCodeProcessor.Encode(msg, width, height, ErrorCorrectionLevelList[(int)type], null);
        }

        public static string Decode(Color32[] colors, int width, int height)
        {
            Texture.Resize(width, height);
            Texture.SetPixels32(colors);
            Texture.Apply();

            return Decode(Texture);
        }

        public static string Decode(Color32[] oldColors, int oWidth, int oHeight, int cropWidth, int cropHeight)
        {
            var tempColors = new Color32[cropWidth * cropHeight];
            for (int i = 0; i < cropWidth; i++)
            {
                for (int j = 0; j < cropHeight; j++)
                {
                    tempColors[i + j * cropWidth] =
                        oldColors[
                            (i + oWidth / 2 - cropWidth / 2) +
                            (j + oHeight / 2 - cropHeight / 2) * oWidth
                            ];
                }
            }

            return Decode(tempColors, cropWidth, cropHeight);

        }

        public static string Decode(Texture2D tex)
        {
            var result = QRCodeProcessor.Decode(tex);
            if (result != null && result.Text != null)
            {
                return result.Text;
            }

            return null;
        }
    }
}