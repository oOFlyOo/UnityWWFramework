using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WWFramework.Helper;
using WWFramework.UI;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace WWFramework.Core
{
    [CreateAssetMenu(menuName = "Texture/Texture Array Config", fileName = "Texture2DArrayConfig.asset")]
    public class Texture2DArrayConfig : ScriptableObject
    {
        public TextureFormat Format = TextureFormat.ASTC_4x4;
        public bool IsLinear = false;
        public bool UseFirstTextureSettings = false;
    
        public List<Texture2D> Textures;
        public Texture2DArray TexArray;
        
        
        #if UNITY_EDITOR
        [ButtonProperty("Generate")]
        public string GenerateMethod = "生成Array";
        
        public void Generate()
        {
            GenerateConfig(this);
        }
        
        public static string GetTexture2DArrayPath(Texture2DArrayConfig config)
        {
            var path = AssetDatabase.GetAssetPath(config);

            return $"{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)}_array.asset";
        }

        public static void GenerateConfig(Texture2DArrayConfig config)
        {
            var texs = config.Textures;
            if (texs == null || texs.Count == 0)
            {
                return;
            }

            var firstTex = texs[0];
            var format = config.UseFirstTextureSettings ? firstTex.format : config.Format;
            var isLinear = config.UseFirstTextureSettings ? TextureHelper.IsLinearFormat(firstTex) : config.IsLinear;
            var texArray = new Texture2DArray(firstTex.width, firstTex.height, texs.Count, format,
                firstTex.mipmapCount, isLinear);

            for (int i = 0; i < texs.Count; i++)
            {
                var tex = texs[i];
                if (tex == null)
                {
                    continue;
                }
                
                TextureHelper.CopyToTexture2DArray(tex, texArray, i);
            }
            
            texArray.Apply(false);

            var configArr = config.TexArray;
            if (configArr == null)
            {
                configArr = AssetDatabase.LoadAssetAtPath<Texture2DArray>(GetTexture2DArrayPath(config));
            }

            if (configArr == null)
            {
                config.TexArray = texArray;
                config.TexArray.name = config.name; 
                AssetDatabase.CreateAsset(texArray, GetTexture2DArrayPath(config));
            }
            else
            {
                config.TexArray = configArr;
                EditorUtility.CopySerialized(texArray, config.TexArray);
            }

            AssetDatabase.SaveAssets();
        }
        #endif
    }
}