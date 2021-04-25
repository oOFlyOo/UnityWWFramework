using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WWFramework.Core;
using WWFramework.Extension;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class MeshUtilEditorWindow : BaseEditorWindow
    {
        private Material _mat;
        private int _lod;
        private int _splitCount;
        
        [MenuItem("WWFramework/MeshUtil/Window")]
        private static MeshUtilEditorWindow GetWindow()
        {
            return GetWindowExt<MeshUtilEditorWindow>();
        }
        
        protected override void CustomOnGUI()
        {
            _mat = EditorUIHelper.ObjectField<Material>(_mat, "替换材质球");
            _lod = EditorUIHelper.IntSlider("LOD", _lod, 0 , 5);
            _splitCount = EditorUIHelper.IntSlider("分割块数，4^n", _splitCount, 0, 3);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("ExportSplatmaps", ExportSplatmaps);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("Terrain2Mesh", ConvertTerrain2Mesh);
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
            var meshes = MeshUtil.ConvertTerrain2Mesh(terrain, _splitCount, _lod);

            var rootGo = new GameObject(terrain.name);
            rootGo.CopyTransform(terrain.gameObject);

            var tileGridNum = (int) Mathf.Pow(2, _splitCount);
            var tileSize = terrain.terrainData.size.x / tileGridNum;

            for (int i = 0; i < meshes.Count; i++)
            {
                var mesh = meshes[i];

                // 从下往上，再从左往右
                var widthIndex = i / tileGridNum;
                var heightIndex = i % tileGridNum;
                
                var go = new GameObject($"{terrain.name}_{widthIndex}_{heightIndex}");
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

        private void InitMeshRenderer(Terrain terrain, MeshRenderer renderer, int tileGridNum, int widthIndex, int heightIndex)
        {
            renderer.sharedMaterial = _mat;
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
            
            return $"{terrainDataPath}/{terrain.name}_{index}{extension}";
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

        private void ExportSplatmaps(Terrain terrain)
        {
            var terrainDataPath = GetTerrainDataPath();
            if (!Directory.Exists(terrainDataPath))
            {
                Directory.CreateDirectory(terrainDataPath);
            }
            
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
                importer.sRGBTexture = false;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            
            AssetDatabase.SaveAssets();
        }
    }
}