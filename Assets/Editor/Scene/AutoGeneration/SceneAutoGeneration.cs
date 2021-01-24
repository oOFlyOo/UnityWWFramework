using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.UI.Editor;

namespace WWFramework.Scene.Editor
{
    public class SceneAutoGeneration : BaseEditorWindow
    {
        /// <summary>
        /// 间隔判断
        /// </summary>
        private const int TextureEdgeSize = 2;
    
        private const int MinHitNums = 1;
        private const int MaxHitNums = 6;

        private const int MinRange = -1;
        private const int MaxRange = 36;

        private const int MinSpace = 1;
        private const int MaxSpace = 100;
    
        private int _maxHits = 1;
        private int _checkLayerMask = -1;
        private int _generateLayerMask = 1 << 0;

        private SceneInfoCalculation.CalculateType _calculateType;
        private int _minRange = -1;
        private int _maxRange = -1;

        private SceneRaycastInfo _sceneRaycastInfo = new SceneRaycastInfo();
        private Texture2D _previewTexture;

        private Vector2 _scenePrefabScrollPos;
        private List<ScenePrefab> _scenePrefabs = new List<ScenePrefab>();

        private Transform _parent;

        [MenuItem("WWFramework/SceneAutoGeneration/Window")]
        private static SceneAutoGeneration GetWindow()
        {
            return GetWindowExt<SceneAutoGeneration>();
        }


        protected override void CustomOnGUI()
        {
            _maxHits = EditorUIHelper.IntSlider("射线最多碰撞次数", _maxHits, MinHitNums, MaxHitNums);
            _checkLayerMask = EditorUIHelper.LayerMask(_checkLayerMask, "检测层（要参与检测的都得勾上）");
            _generateLayerMask = EditorUIHelper.LayerMask(_generateLayerMask, "生成层（参与生成规则的层）");
        
            EditorUIHelper.Space();
            _calculateType = EditorUIHelper.EnumPopup<SceneInfoCalculation.CalculateType>(_calculateType, "筛选条件");
            _minRange = EditorUIHelper.IntSlider("边界最近距离（-1代表全部）", _minRange, MinRange, MaxRange);
            _maxRange = EditorUIHelper.IntSlider("影响最远距离（-1代表全部）", _maxRange, MinRange, MaxRange);
        
            EditorUIHelper.Space();
            EditorUIHelper.Button("刷新", Refresh);
        
            EditorUIHelper.Space();
            EditorUIHelper.ObjectField<Texture>(_previewTexture, "预览");
        
            EditorUIHelper.Space();
            EditorUIHelper.BeginScrollView(_scenePrefabScrollPos);

            var deletePrefabIndex = -1;
            for (int i = 0; i < _scenePrefabs.Count; i++)
            {
                var scenePrefab = _scenePrefabs[i];
                EditorUIHelper.BeginHorizontal();
                scenePrefab.Prefab = EditorUIHelper.ObjectField<GameObject>(scenePrefab.Prefab);
                scenePrefab.Space = EditorUIHelper.IntSlider("", scenePrefab.Space, MinSpace, MaxSpace);
                var i1 = i;
                EditorUIHelper.Button("删除", () =>
                {
                    deletePrefabIndex = i1;
                });
                EditorUIHelper.EndHorizontal();
            }
            if (deletePrefabIndex >= 0)
            {
                _scenePrefabs.RemoveAt(deletePrefabIndex);
            }
            
            EditorUIHelper.Button("添加", () => _scenePrefabs.Add(new ScenePrefab()));
            
            EditorUIHelper.EndScrollView();
            
            EditorUIHelper.Space();
            _parent = EditorUIHelper.ObjectField<Transform>(_parent, "父节点", true);
        
            EditorUIHelper.Space();
            EditorUIHelper.Button("生成", Generate);
        }

        private void Refresh()
        {
            var trans = Selection.activeTransform;
            if (trans == null)
            {
                return;
            }
        
            var bounds = GenerateBounds(trans);
            var arrayWidth = Mathf.FloorToInt(bounds.size.x) + 1 + TextureEdgeSize * 2;
            var arrayHeight = Mathf.FloorToInt(bounds.size.z) + 1 + TextureEdgeSize * 2;

            var hits = SceneJobUtility.RaycastGrid(bounds.center, arrayWidth, arrayHeight, _checkLayerMask, _maxHits);
            DebugRaycast(hits);
        
            _sceneRaycastInfo.UpdateData(arrayWidth, arrayHeight, hits, _maxHits, bounds.center);
            _previewTexture =
                SceneInfoCalculation.GetTexture(_sceneRaycastInfo, _calculateType, _minRange, _maxRange, _generateLayerMask);
        }

        private void Generate()
        {
            foreach (var prefab in _scenePrefabs)
            {
                SceneInfoCalculation.GenerateBySceneInfo(prefab, _parent, _previewTexture, _sceneRaycastInfo);
            }
        }

        private void DebugRaycast(RaycastHit[] hits)
        {
            return;
        
            var newHists = hits.Where(hit => hit.collider != null);
            foreach (var hit in newHists)
            {
                Debug.Log($"Collider:{hit.collider} Point:{hit.point}");
            }
        }

        private Bounds GenerateBounds(Transform trans)
        {
            var bounds = new Bounds();
            var renderers = trans.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                return bounds;
            }

            bounds = renderers[0].bounds;
            if (renderers.Length == 1)
            {
                return bounds;
            }

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}