using UnityEngine;

namespace WWFramework.Helper
{
    public static class WebCamTextureHelper
    {
        /// <summary>
        /// 摄像机设备，这玩意据说消耗大，所以获取后缓存下来
        /// </summary>
        private static WebCamDevice[] _webCamDevices;
        public static WebCamDevice[] WebCamDevices
        {
            get
            {
                if (_webCamDevices == null)
                {
                    _webCamDevices = WebCamTexture.devices;
                }
                return _webCamDevices;
            }
        }


        /// <summary>
        /// 返回摄像机Texture，返回的时候是未打开的，所有设置都得在Play之前设置好
        /// </summary>
        /// <param name="onlyBackCamera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fps"></param>
        /// <returns></returns>
        public static WebCamTexture GetNewWebCamTexture(int width = 0, int height = 0, bool onlyBackCamera = true, int fps = 0)
        {
            WebCamTexture texture = null;
            for (int i = 0; i < WebCamDevices.Length; i++)
            {
                var device = WebCamDevices[i];
                if (!onlyBackCamera || !device.isFrontFacing)
                {
                    texture = new WebCamTexture(device.name);
                    if (width > 0)
                    {
                        texture.requestedWidth = width;
                    }
                    if (height > 0)
                    {
                        texture.requestedHeight = height;
                    }
                    if (fps > 0)
                    {
                        texture.requestedFPS = fps;
                    }
                    else
                    {
                        texture.requestedFPS = 30;
                    }
                    break;
                }
            }
            return texture;
        }


        public static void WebCamTextureUpdate(WebCamTexture texture, out Vector3 scale, out Quaternion rotation)
        {
            scale = new Vector3(1, texture.videoVerticallyMirrored ? -1f : 1f, 1);
            rotation = Quaternion.AngleAxis(texture.videoRotationAngle, Vector3.forward);
        }
    }
}