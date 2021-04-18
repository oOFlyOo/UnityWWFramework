using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWFramework.Core.Editor
{
    [CustomEditor(typeof(TerrainLayerArrayConfig))]
    public class TerrainLayerArrayConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(1);

            if (GUILayout.Button("生成"))
            {
                Generate((TerrainLayerArrayConfig)target);
            }
        }

        public static string GetDiffuseConfigPath(TerrainLayerArrayConfig config)
        {
            var path = AssetDatabase.GetAssetPath(config);

            return $"{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)}_diffuse.asset";
        }
        
        public static string GetNormalConfigPath(TerrainLayerArrayConfig config)
        {
            var path = AssetDatabase.GetAssetPath(config);

            return $"{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)}_normal.asset";
        }

        private static void FindOrCreateTextureConfigIfMissing(ref Texture2DArrayConfig config, string path)
        {
            if (config != null)
            {
                return;
            }

            config = AssetDatabase.LoadAssetAtPath<Texture2DArrayConfig>(path);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<Texture2DArrayConfig>();
                AssetDatabase.CreateAsset(config, path);
            }
        }

        private static void AddTextureToConfig(Texture2DArrayConfig config, Texture2D tex)
        {
            if (tex == null)
            {
                return;
            }

            if (config.Textures.Contains(tex))
            {
                return;
            }
            
            config.Textures.Add(tex);
        }

        public static void Generate(TerrainLayerArrayConfig config)
        {
            FindOrCreateTextureConfigIfMissing(ref config.DiffuseArrayConfig, GetDiffuseConfigPath(config));
            FindOrCreateTextureConfigIfMissing(ref config.NormalArrayConfig, GetNormalConfigPath(config));

            foreach (var layer in config.Layers)
            {
                AddTextureToConfig(config.DiffuseArrayConfig, layer.diffuseTexture);
                AddTextureToConfig(config.NormalArrayConfig, layer.normalMapTexture);
            }

            config.DiffuseArrayConfig.UseFirstTextureSettings = config.UseFirstTextureSettings;
            config.DiffuseArrayConfig.IsLinear = false;
            config.NormalArrayConfig.UseFirstTextureSettings = config.UseFirstTextureSettings;
            config.NormalArrayConfig.IsLinear = true;
            
            Texture2DArrayConfigEditor.Generate(config.DiffuseArrayConfig);
            Texture2DArrayConfigEditor.Generate(config.NormalArrayConfig);
            
            AssetDatabase.SaveAssets();
        }
    }
}