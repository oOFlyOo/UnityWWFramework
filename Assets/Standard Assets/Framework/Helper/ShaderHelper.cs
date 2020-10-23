using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WWFramework.Helper
{
    public class ShaderHelper
    {
        #region Shader 匹配
        public const string ToonPBR = "Lyra/Char/ToonPBR";
        public const string OutlineToonPBR = "Lyra/Char/OutlineToonPBR";
        public const string StencilWrite = "Lyra/StencilWrite";
        public const string DepthPass = "WWFramework/Base/DepthMap";

        private static Dictionary<string, Shader> _shaderDict;

        public static Shader Find(string name)
        {
            if (_shaderDict == null)
            {
                _shaderDict = new Dictionary<string, Shader>(10);
            }

            Shader shader = null;
            if (!_shaderDict.TryGetValue(name, out shader))
            {
                // shader = Shader.Find(name);
                shader = Shader.Find(name);

                _shaderDict[name] = shader;
            }

            return shader;
        }
        #endregion

        #region Shader Property

        public const string CharLightProperty = "_CharLightDir";
        public const string StencilWriteRefProperty = "_StencilWriteRef";
        public const string ShadowBias = "_ShadowBias";
        public const string ShadowNormalBias = "_ShadowNormalBias";
        public const string ShadowMatrixV = "_ShadowMatrixV";
        public const string ShadowMatrixVP = "_ShadowMatrixVP";
        public const string ShadowFarScale = "_ShadowFarScale";
        public const string ShadowLightDir = "_ShadowLightDir";
        public const string ShadowIntensity = "_ShadowIntensity";
        public const string ShadowMap = "_ShadowMap";
        public const string ShadowMapWidthScale = "_ShadowMapWidthScale";
        public const string ShadowMapHeightScale = "_ShadowMapHeightScale";
    
        private static Dictionary<string, int> _propertyIDDict;

        public static int PropertyToID(string name)
        {
            if (_propertyIDDict == null)
            {
                _propertyIDDict = new Dictionary<string, int>(10);
            }

            int id;
            if (!_propertyIDDict.TryGetValue(name, out id))
            {
                id = Shader.PropertyToID(name);

                _propertyIDDict[name] = id;
            }

            return id;
        }
        #endregion

        #region Shader Keyword
        public const string _USE_CHAR_LIGHT = "_USE_CHAR_LIGHT";
        public const string CHAR_SHADOWMAP_ON = "_CHAR_SHADOWMAP_ON";
    
        #endregion
    #region Shader Tag
    public const string RenderTypeTag = "RenderType";
    #endregion

        #region Shader 常用参数
        public const int TransparentHairQueue = (int) RenderQueue.AlphaTest + 1;

        public const int StencilWriteRef = 7;
    

        #endregion

        #region MaterialPropertyBlock
        public static MaterialPropertyBlock MaterialProperty = new MaterialPropertyBlock();
    

        #endregion
    }
}