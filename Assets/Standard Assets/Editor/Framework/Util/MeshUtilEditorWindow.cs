using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using WWFramework.Core;
using WWFramework.Extension;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class MeshUtilEditorWindow : BaseEditorWindow
    {
        private Material _mat;
        private Material _addMat;
        private int _lod;
        private int _splitCount;
        private IndexFormat _indexFormat = IndexFormat.UInt16;
        
        [MenuItem("WWFramework/MeshUtil/Window")]
        private static MeshUtilEditorWindow GetWindow()
        {
            return GetWindowExt<MeshUtilEditorWindow>();
        }
        
        protected override void CustomOnGUI()
        {
            _mat = EditorUIHelper.ObjectField<Material>(_mat, "替换材质球");
            _addMat = EditorUIHelper.ObjectField<Material>(_addMat, "替换材质球");
            _lod = EditorUIHelper.IntSlider("LOD", _lod, 0 , 5);
            _splitCount = EditorUIHelper.IntSlider("分割块数，4^n", _splitCount, 0, 5);
            _indexFormat = EditorUIHelper.EnumPopup<IndexFormat>(_indexFormat);
            
            EditorUIHelper.Space();
            EditorUIHelper.LabelField("使用 UInt16，1025的高度图设置，得分割3次才能满足顶点数限制");
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("ExportSplatmaps", ExportSplatmaps);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("Terrain2Mesh", ConvertTerrain2Mesh);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("SaveMeshes", SaveMeshes);
        }

        private void ConvertTerrain2Mesh()
        {
            var trans = Selection.activeTransform;
            if (trans == null)
            {
                return;
            }

            var terrains = trans.GetComponentsInChildren<Terrain>();
            foreach (var terrain in terrains)
            {
                ConvertTerrain2Mesh(terrain);
            }
        }

        private void ConvertTerrain2Mesh(Terrain terrain)
        {
            var meshes = MeshUtil.ConvertTerrain2Mesh(terrain, _indexFormat, _splitCount, _lod);

            var rootGo = new GameObject($"{terrain.name}_{_lod}");
            rootGo.CopyTransform(terrain.gameObject);

            var tileGridNum = (int) Mathf.Pow(2, _splitCount);
            var tileSize = terrain.terrainData.size.x / tileGridNum;

            for (int i = 0; i < meshes.Count; i++)
            {
                var mesh = meshes[i];

                // 从下往上，再从左往右
                var widthIndex = i / tileGridNum;
                var heightIndex = i % tileGridNum;
                
                var go = new GameObject($"{terrain.name}_{_lod}_mesh_{widthIndex}_{heightIndex}");
                go.transform.SetParent(rootGo.transform);
                go.transform.localPosition = new Vector3(widthIndex * tileSize, 0, heightIndex * tileSize);
                var filer = go.AddComponent<MeshFilter>();
                filer.mesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                // renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat");
                renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                InitMeshRenderer(terrain, renderer, tileGridNum, widthIndex, heightIndex);
            }
        }

        private void EnableKeyword(Material mat)
        {
            mat.EnableKeyword("_NORMALMAP");
        }

        private void InitMeshRenderer(Terrain terrain, MeshRenderer renderer, int tileGridNum, int widthIndex, int heightIndex)
        {
            var splatCounts = terrain.terrainData.alphamapTextureCount;
            var mats = new List<Material>();
            mats.Add(_mat);
            EnableKeyword(_mat);
            if (_addMat != null)
            {
                EnableKeyword(_addMat);

                for (int i = 0; i < splatCounts - 1; i++)
                {
                    mats.Add(_addMat);
                }
            }
            
            renderer.sharedMaterials = mats.ToArray();
            var controls = new List<Texture2D>();
            for (var index = 0; index < terrain.terrainData.alphamapTextures.Length; index++)
            {
                var path = GetSplatmapPath(terrain, index);
                controls.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            }
            
            var converter = renderer.gameObject.AddComponent<TerrainMeshConverter>();
            converter.InitConverter(terrain, controls, tileGridNum, widthIndex, heightIndex);
        }

        private string GetScenePath()
        {
            return EditorSceneManager.GetActiveScene().path;
        }

        private string GetTerrainDataPath()
        {
            var scenePath = GetScenePath();

            var terrainDataPath = $"{Path.GetDirectoryName(scenePath)}/Terrain";

            return terrainDataPath;
        }

        private string GetSplatmapPath(Terrain terrain, int index, bool containExtension = true)
        {
            var terrainDataPath = GetTerrainDataPath();
            var extension = containExtension ? ".tga" : "";
            
            return $"{terrainDataPath}/{terrain.name}_splatmap_{index}{extension}";
        }

        private void ExportSplatmaps()
        {
            var trans = Selection.activeTransform;
            if (trans == null)
            {
                return;
            }

            var terrains = trans.GetComponentsInChildren<Terrain>();
            foreach (var terrain in terrains)
            {
                ExportSplatmaps(terrain);
            }
        }

        private void CheckTerrainDataPath()
        {
            var terrainDataPath = GetTerrainDataPath();
            if (!Directory.Exists(terrainDataPath))
            {
                Directory.CreateDirectory(terrainDataPath);
            }
        }

        private void ExportSplatmaps(Terrain terrain)
        {
            CheckTerrainDataPath();
            
            var data = terrain.terrainData;
            for (var index = 0; index < data.alphamapTextures.Length; index++)
            {
                var tex = data.alphamapTextures[index];
                var bytes = tex.EncodeToTGA();
                File.WriteAllBytes(GetSplatmapPath(terrain, index), bytes);
            }
            
            AssetDatabase.Refresh();
            
            for (var index = 0; index < data.alphamapTextures.Length; index++)
            {
                var path = GetSplatmapPath(terrain, index);
                var importer = (TextureImporter) AssetImporter.GetAtPath(path);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            
            AssetDatabase.SaveAssets();
        }

        private string GetMeshPath(MeshFilter filter)
        {
            var terrainDataPath = GetTerrainDataPath();

            return $"{terrainDataPath}/{filter.name}.asset";
        }

        private void SaveMeshes()
        {
            var trans = Selection.activeTransform;
            if (trans == null)
            {
                return;
            }
            
            CheckTerrainDataPath();

            var filters = trans.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(filter.sharedMesh)))
                {
                    continue;
                }
                
                AssetDatabase.CreateAsset(filter.sharedMesh, GetMeshPath(filter));
            }
            
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}