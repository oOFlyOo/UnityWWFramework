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

        private List<MaterialPropertyBlock> _blocks;
        

        private void Start()
        {
            Convert();
        }
        
        public void Convert()
        {
            if (Controls.Count <= 0)
            {
                return;
            }

            _blocks = new List<MaterialPropertyBlock>(Controls.Count);

            for (int bI = 0; bI < Controls.Count; bI++)
            {
                var block = new MaterialPropertyBlock();
                _blocks.Add(block);
                
                block.SetTexture("_Control", Controls[bI]);
                block.SetVector("_Control_TexelSize", ControlTexelSizes[bI]);
                // block.SetVector("_ControlOffset", ControlOffset);

                var startI = bI * 4;
                var endI = Mathf.Min(startI + 4, Diffuses.Count);
                for (int i = startI; i < endI; i++)
                {
                    block.SetTexture($"_Splat{i - startI}", Diffuses[i]);
                    block.SetVector($"_Splat{i - startI}_ST", SplatTillings[i]);
                    block.SetTexture($"_Normal{i - startI}", Normals[i] ? Normals[i] : Texture2D.normalTexture);
                    block.SetFloat($"_NormalScale{i - startI}", NormalScales[i]);
                }

                var renderers = GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    renderer.SetPropertyBlock(block, bI);
                }
                // renderer.sharedMaterial.EnableKeyword("_NORMALMAP");
            }
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
                Diffuses.Add(layer.diffuseTexture);
                var tilling = new Vector4(data.size.x / layer.tileSize.x, data.size.z / layer.tileSize.y, layer.tileOffset.x / layer.tileSize.x, layer.tileOffset.y/ layer.tileSize.y);
                SplatTillings.Add(tilling);
                Normals.Add(layer.normalMapTexture);
                NormalScales.Add(layer.normalScale);
            }
        }
    }
}