using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Core
{
    [ExecuteInEditMode]
    public class TerrainLayerArrayConverter : MonoBehaviour
    {
        public List<Texture2D> ControlArray = new List<Texture2D>();

        public Texture2DArray DiffuseArray;
        public Texture2DArray NormalArray;
        public List<float> DiffuseIndexes = new List<float>();
        public List<float> NormalIndexes = new List<float>();
        public List<Vector4> Tillings = new List<Vector4>();

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
            if (ControlArray.Count <= 0)
            {
                return;
            }
            
            _block.Clear();
            
            for (int i = 0; i < ControlArray.Count; i++)
            {
                var control = ControlArray[i];
                _block.SetTexture($"_Control_{i}", control);
            }
            
            _block.SetTexture("_DiffuseArray", DiffuseArray);
            _block.SetTexture("_NormalArray", NormalArray);

            _block.SetFloatArray("_DiffuseIndexes", DiffuseIndexes);
            _block.SetFloatArray("_NoralMalIndexes", NormalIndexes);
            _block.SetVectorArray("_Splats_ST", Tillings);

            var terrain = GetComponent<Terrain>();
            terrain.SetSplatMaterialPropertyBlock(_block);
        }

        public void InitConverter(TerrainLayerArrayConfig config)
        {
            InitParams();
            var terrain = GetComponent<Terrain>();

            ControlArray.Clear();
            ControlArray.AddRange(terrain.terrainData.alphamapTextures);

            DiffuseArray = config.DiffuseArrayConfig.TexArray;
            NormalArray = config.NormalArrayConfig.TexArray;
            
            DiffuseIndexes.Clear();
            NormalIndexes.Clear();
            Tillings.Clear();
            for (int i = 0; i < terrain.terrainData.terrainLayers.Length; i++)
            {
                var layer = terrain.terrainData.terrainLayers[i];
                DiffuseIndexes.Add(config.DiffuseArrayConfig.IndexOfTexture(layer.diffuseTexture));
                NormalIndexes.Add(config.NormalArrayConfig.IndexOfTexture(layer.normalMapTexture));
                var tilling = new Vector4(terrain.terrainData.size.x / layer.tileSize.x, terrain.terrainData.size.z / layer.tileSize.y, layer.tileOffset.x / layer.tileSize.x, layer.tileOffset.y/ layer.tileSize.y);
                Tillings.Add(tilling);
            }
        }
    }
}