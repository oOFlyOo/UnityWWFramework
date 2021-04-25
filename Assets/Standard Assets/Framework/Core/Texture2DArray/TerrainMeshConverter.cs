using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WWFramework.Core
{
    [ExecuteInEditMode]
    public class TerrainMeshConverter: MonoBehaviour
    {
        public List<Texture2D> Controls = new List<Texture2D>();
        public List<Vector4> ControlTexelSizes = new List<Vector4>();
        public Vector4 ControlOffset;
        public List<Texture2D> Diffuses = new List<Texture2D>();
        public List<Vector4> SplatTillings = new List<Vector4>();
        public List<Texture2D> Normals= new List<Texture2D>();
        public List<float> NormalScales = new List<float>();

        private MaterialPropertyBlock _block;
        
        private void InitParams()
        {
            _block = _block ?? new MaterialPropertyBlock();
        }

        private void Start()
        {
            InitParams();
            
            Convert();
        }
        
        public void Convert()
        {
            if (Controls.Count <= 0)
            {
                return;
            }
            
            _block.Clear();
            
            _block.SetTexture("_Control", Controls.FirstOrDefault());
            _block.SetVector("_Control_TexelSize", ControlTexelSizes.FirstOrDefault());
            _block.SetVector("_ControlOffset", ControlOffset);

            for (int i = 0; i < Mathf.Min(Diffuses.Count, 4); i++)
            {
                _block.SetTexture($"_Splat{i}", Diffuses[i]);
                _block.SetVector($"_Splat{i}_ST", SplatTillings[i]);
                _block.SetTexture($"_Normal{i}", Normals[i]);
                _block.SetFloat($"_NormalScale{i}", NormalScales[i]);
            }
            
            var renderer = GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(_block);
            renderer.sharedMaterial.EnableKeyword("_NORMALMAP");
        }

        public void InitConverter(Terrain terrain, List<Texture2D> controls, int tileGridNum, int widthIndex, int heightIndex)
        {
            var data = terrain.terrainData;

            Controls.Clear();
            Controls.AddRange(controls);

            ControlTexelSizes.Clear();
            foreach (var tex in controls)
            {
                ControlTexelSizes.Add(new Vector4(1f / tex.width, 1f / tex.height, tex.width, tex.height));
            }

            var revertGridNum = 1f / tileGridNum;
            ControlOffset = new Vector4(revertGridNum, revertGridNum, widthIndex * revertGridNum, heightIndex * revertGridNum);

            Diffuses.Clear();
            SplatTillings.Clear();
            Normals.Clear();
            NormalScales.Clear();
            foreach (var layer in data.terrainLayers)
            {
                Diffuses.Add(layer.diffuseTexture ? layer.diffuseTexture : Texture2D.grayTexture);
                var tilling = new Vector4(data.size.x / layer.tileSize.x, data.size.z / layer.tileSize.y, layer.tileOffset.x / layer.tileSize.x, layer.tileOffset.y/ layer.tileSize.y);
                SplatTillings.Add(tilling);
                Normals.Add(layer.normalMapTexture ? layer.normalMapTexture : Texture2D.normalTexture);
                NormalScales.Add(layer.normalScale);
            }
        }
    }
}