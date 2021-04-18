using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}