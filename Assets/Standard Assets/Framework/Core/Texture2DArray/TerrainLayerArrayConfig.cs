using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WWFramework.UI;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;

#endif

namespace WWFramework.Core
{
    [CreateAssetMenu(menuName = "Texture/Terrain Layer Array Config", fileName = "TerrainLayerArrayConfig.asset")]
    public class TerrainLayerArrayConfig : ScriptableObject
    {
        public bool UseFirstTextureSettings = false;

        public List<TerrainLayer> Layers;

        public Texture2DArrayConfig DiffuseArrayConfig;
        public Texture2DArrayConfig NormalArrayConfig;

#if UNITY_EDITOR
        [ButtonProperty("Generate")]
        public string GenerateMethod = "生成配置";
        
        public void Generate()
        {
            GenerateConfig(this);
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

        public static void GenerateConfig(TerrainLayerArrayConfig config)
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

            config.DiffuseArrayConfig.Generate();
            config.NormalArrayConfig.Generate();

            AssetDatabase.SaveAssets();
        }
#endif
    }
}