﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper;

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

        public static void Generate(Texture2DArrayConfig config)
        {
            var texs = config.Textures;
            if (texs == null || texs.Count == 0)
            {
                return;
            }

            var firstTex = texs[0];
            var texArray = new Texture2DArray(firstTex.width, firstTex.height, texs.Count, config.Format,
                firstTex.mipmapCount, config.IsLinear);

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

            if (config.TexArray)
            {
                EditorUtility.CopySerialized(texArray, config.TexArray);
            }
            else
            {
                config.TexArray = texArray;
                AssetDatabase.AddObjectToAsset(config);
            }
        }
    }
}