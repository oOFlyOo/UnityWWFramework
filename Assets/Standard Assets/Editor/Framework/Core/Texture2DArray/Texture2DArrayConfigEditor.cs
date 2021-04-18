using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper;
using Path = System.IO.Path;

namespace WWFramework.Core.Editor
{
    [CustomEditor(typeof(Texture2DArrayConfig))]
    public class Texture2DArrayConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("生成"))
            {
                Generate((Texture2DArrayConfig)target);
            }
        }

        public static string GetTexture2DArrayPath(Texture2DArrayConfig config)
        {
            var path = AssetDatabase.GetAssetPath(config);

            return $"{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)}_array.asset";
        }

        public static void Generate(Texture2DArrayConfig config)
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
    }
}