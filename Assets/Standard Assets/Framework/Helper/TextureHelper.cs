
using System;
using UnityEngine;

namespace WWFramework.Helper
{
    public static class TextureHelper
    {
        public static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (source == null)
            {
                return null;
            }

            var result = new Texture2D(targetWidth, targetHeight, source.format, false);
            try
            {
                var incX = 1f / targetWidth;
                var incY = 1f / targetHeight;
                var newColors = new Color[targetWidth * targetHeight];
                for (int i = 0; i < result.height; i++)
                {
                    for (int j = 0; j < result.width; j++)
                    {
                        newColors[i * result.width + j] = source.GetPixelBilinear(incX * j, incY * i);
                    }
                }

                result.SetPixels(newColors);
                result.Apply();
            }
            catch (Exception e)
            {
                result = source;
            }

            return result;
        }


        public static Texture2D LimitingTextureSize(Texture2D tex, int limitedWidth, int limitedHeight)
        {
            if (tex.width > limitedWidth || tex.height > limitedHeight)
            {
                var widthScale = 1f * limitedWidth / tex.width;
                var heightScale = 1f * limitedHeight / tex.height;
                var scale = Mathf.Min(widthScale, heightScale);
                return ResizeTexture(tex, (int)(tex.width * scale), (int)(tex.height * scale));
            }
            else
            {
                return tex;
            }
        }
    }
}