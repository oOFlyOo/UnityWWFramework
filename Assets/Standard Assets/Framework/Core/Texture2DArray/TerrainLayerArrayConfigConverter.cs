using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using WWFramework.UI;


namespace WWFramework.Core
{
    public class TerrainLayerArrayConfigConverter : MonoBehaviour
    {
        public Material DefaultMat;
        public Material ArrayMat;
        public TerrainLayerArrayConfig Config;

        public void Convert()
        {
            var terrains = GetComponentsInChildren<Terrain>(true);

            foreach (var terrain in terrains)
            {
                terrain.materialTemplate = ArrayMat;
            
                var arrayConverter = terrain.GetComponent<TerrainLayerArrayConverter>();
                if (arrayConverter == null)
                {
                    arrayConverter = terrain.gameObject.AddComponent<TerrainLayerArrayConverter>();
                }
                arrayConverter.InitConverter(Config);
                arrayConverter.Convert(); 
            }
        }
    
        public void Restore()
        {
            var terrains = GetComponentsInChildren<Terrain>(true);

            foreach (var terrain in terrains)
            {
                terrain.materialTemplate = DefaultMat;
            }
        }
    
#if UNITY_EDITOR
        [ButtonProperty("Convert")]
        public string ConvertTerrain = "生成Array地形";
    
        [ButtonProperty("Restore")]
        public string RestoreTerrain = "还原地形";

        [ButtonProperty("Generate")]
        public string GenerateConfig = "生成地形配置";
    
        public void Generate()
        {
            if (Config == null)
            {
                var scenePath = SceneManager.GetActiveScene().path;
                var path = EditorUtility.SaveFilePanel("Save Config", Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath), "asset");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
            
                Config = ScriptableObject.CreateInstance<TerrainLayerArrayConfig>();
                path = path.Substring(path.IndexOf("Assets/"));
                AssetDatabase.CreateAsset(Config, path);
            }

            Config.Layers.Clear();
            var terrains = GetComponentsInChildren<Terrain>();
            foreach (var terrain in terrains)
            {
                foreach (var layer in terrain.terrainData.terrainLayers)
                {
                    if (!Config.Layers.Contains(layer))
                    {
                        Config.Layers.Add(layer);
                    }
                }
            }
            Config.Generate();
        }
#endif
    }
}
