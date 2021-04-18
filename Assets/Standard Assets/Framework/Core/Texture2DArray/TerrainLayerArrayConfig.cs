using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Core
{
    [CreateAssetMenu(menuName = "Texture/Terrain Layer Array Config", fileName = "TerrainLayerArrayConfig.asset")]
    public class TerrainLayerArrayConfig : ScriptableObject
    {
        public TextureFormat Format = TextureFormat.ASTC_4x4;
        public bool IsLinear = false;
        public bool UseFirstTextureSettings = false;
    
        public List<TerrainLayer> Layers;
        
        public Texture2DArrayConfig DiffuseArrayConfig;
        public Texture2DArrayConfig NormalArrayConfig;
    }
}