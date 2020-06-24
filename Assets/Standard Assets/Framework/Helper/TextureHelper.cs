
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

        public static void Dithering(Texture2D tex)
        {
            var texw = tex.width;
            var texh = tex.height;

            var pixels = tex.GetPixels();
            var offs = 0;

            var k1Per15 = 1.0f / 15.0f;
            var k1Per16 = 1.0f / 16.0f;
            var k3Per16 = 3.0f / 16.0f;
            var k5Per16 = 5.0f / 16.0f;
            var k7Per16 = 7.0f / 16.0f;

            for (var y = 0; y < texh; y++)
            {
                for (var x = 0; x < texw; x++)
                {
                    float a = pixels[offs].a;
                    float r = pixels[offs].r;
                    float g = pixels[offs].g;
                    float b = pixels[offs].b;

                    var a2 = Mathf.Clamp01(Mathf.Floor(a * 16) * k1Per15);
                    var r2 = Mathf.Clamp01(Mathf.Floor(r * 16) * k1Per15);
                    var g2 = Mathf.Clamp01(Mathf.Floor(g * 16) * k1Per15);
                    var b2 = Mathf.Clamp01(Mathf.Floor(b * 16) * k1Per15);

                    var ae = a - a2;
                    var re = r - r2;
                    var ge = g - g2;
                    var be = b - b2;

                    pixels[offs].a = a2;
                    pixels[offs].r = r2;
                    pixels[offs].g = g2;
                    pixels[offs].b = b2;

                    var n1 = offs + 1;
                    var n2 = offs + texw - 1;
                    var n3 = offs + texw;
                    var n4 = offs + texw + 1;

                    if (x < texw - 1)
                    {
                        pixels[n1].a += ae * k7Per16;
                        pixels[n1].r += re * k7Per16;
                        pixels[n1].g += ge * k7Per16;
                        pixels[n1].b += be * k7Per16;
                    }

                    if (y < texh - 1)
                    {
                        pixels[n3].a += ae * k5Per16;
                        pixels[n3].r += re * k5Per16;
                        pixels[n3].g += ge * k5Per16;
                        pixels[n3].b += be * k5Per16;

                        if (x > 0)
                        {
                            pixels[n2].a += ae * k3Per16;
                            pixels[n2].r += re * k3Per16;
                            pixels[n2].g += ge * k3Per16;
                            pixels[n2].b += be * k3Per16;
                        }

                        if (x < texw - 1)
                        {
                            pixels[n4].a += ae * k1Per16;
                            pixels[n4].r += re * k1Per16;
                            pixels[n4].g += ge * k1Per16;
                            pixels[n4].b += be * k1Per16;
                        }
                    }

                    offs++;
                }
            }

            tex.SetPixels(pixels);
        }


        public static Texture GetMiMapTexture(Texture2D source, int mipLevel)
        {
            var mipMapTex = new Texture2D(source.width, source.height, source.format, true);
            mipMapTex.SetPixels(source.GetPixels());
            mipMapTex.Apply();
            var pixels = mipMapTex.GetPixels(mipLevel);

            var result = new Texture2D(source.width / (int)Mathf.Pow(2, mipLevel), source.height / (int)Mathf.Pow(2, mipLevel), source.format, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }


        public static Texture2D ConvertCubemap2Texture2D(Cubemap cubemap)
        {
            var size = cubemap.width;
            var texWith = size * 4;
            var texHeight = size * 3;
            var tex = new Texture2D(texWith, texHeight, TextureFormat.RGBAHalf, false, false);
            
            tex.SetPixels(size, 0, size, size, GetPixels(cubemap, CubemapFace.NegativeY, size, size));
            tex.SetPixels(0, size, size, size, GetPixels(cubemap, CubemapFace.NegativeX, size, size));
            tex.SetPixels(size, size, size, size, GetPixels(cubemap, CubemapFace.PositiveZ, size, size));
            tex.SetPixels(size * 2, size, size, size, GetPixels(cubemap, CubemapFace.PositiveX, size, size));
            tex.SetPixels(size * 3, size, size, size, GetPixels(cubemap, CubemapFace.NegativeZ, size, size));
            tex.SetPixels(size, size * 2, size, size, GetPixels(cubemap, CubemapFace.PositiveY, size, size));
            tex.Apply();

            return tex;
        }
        
        
        /// <summary>
        /// 翻转的
        /// </summary>
        /// <param name="cubemap"></param>
        /// <param name="face"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Color[] GetPixels(Cubemap cubemap, CubemapFace face, int width, int height)
        {
            var colors = new Color[width*height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    colors[i*height+j] = cubemap.GetPixel(face,j, height - i);
                }
            }
            return colors;
        }
    }
}