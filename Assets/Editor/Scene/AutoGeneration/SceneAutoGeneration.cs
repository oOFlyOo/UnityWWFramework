using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.UI.Editor;
using Object = UnityEngine.Object;

namespace WWFramework.Scene.Editor
{
    public class SceneAutoGeneration : BaseEditorWindow
    {
        /// <summary>
        /// 间隔判断
        /// </summary>
        private const int TextureEdgeSize = 2;

        private static readonly Vector2 WindowSize = new Vector2(800, 600);
    
        private const int MinHitNums = 1;
        private const int MaxHitNums = 6;

        private const int MinRange = -1;
        private const int MaxRange = 36;
        
        private const int MinWeight = 0;
        private const int MaxWeight = 16;

        private const int MinSpace = 1;
        private const int MaxSpace = 100;

        private const float MinScale = 0.1f;
        private const float MaxScale = 10f;
        
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

        private int _space;
        private Transform _parent;
        private bool _clearParent;

        [MenuItem("WWFramework/SceneAutoGeneration/Window")]
        private static SceneAutoGeneration GetWindow()
        {
            var win = GetWindowExt<SceneAutoGeneration>();
            win.minSize = WindowSize;

            return win;
        }


        protected override void CustomOnGUI()
        {
            _maxHits = EditorUIHelper.IntSlider("射线最多碰撞次数（检测叠层的时候才需要）", _maxHits, MinHitNums, MaxHitNums);
            _checkLayerMask = EditorUIHelper.LayerMask(_checkLayerMask, "检测层（涉及层）");
            _generateLayerMask = EditorUIHelper.LayerMask(_generateLayerMask, "生成层（某些层的内外围）");
        
            EditorUIHelper.Space();
            _calculateType = EditorUIHelper.EnumPopup<SceneInfoCalculation.CalculateType>(_calculateType, "筛选条件（如内外围）");
            _minRange = EditorUIHelper.IntSlider("边界最近距离（-1代表全部）", _minRange, MinRange, MaxRange);
            _maxRange = EditorUIHelper.IntSlider("影响最远距离（-1代表全部）", _maxRange, MinRange, MaxRange);
        
            EditorUIHelper.Space();
            EditorUIHelper.Button("刷新", Refresh);
        
            EditorUIHelper.Space();
            EditorUIHelper.ObjectField<Texture>(_previewTexture, "预览");
            
            EditorUIHelper.Space();
            _parent = EditorUIHelper.ObjectField<Transform>(_parent, "父节点", true);
            _clearParent = EditorUIHelper.Toggle("清理父节点", _clearParent);
            _space = EditorUIHelper.IntSlider("间隔（密度）", _space, MinSpace, MaxSpace);
        
            EditorUIHelper.Space();
            EditorUIHelper.BeginScrollView(_scenePrefabScrollPos);

            var deletePrefabIndex = -1;
            for (int i = 0; i < _scenePrefabs.Count; i++)
            {
                var scenePrefab = _scenePrefabs[i];
                
                EditorUIHelper.BeginHorizontal();
                scenePrefab.Prefab = EditorUIHelper.ObjectField<GameObject>(scenePrefab.Prefab);
                scenePrefab.Weight = EditorUIHelper.IntSlider("权重", scenePrefab.Weight, MinWeight, MaxWeight);
                
                var i1 = i;
                EditorUIHelper.Button("删除", () =>
                {
                    deletePrefabIndex = i1;
                });
                EditorUIHelper.EndHorizontal();
                
                EditorUIHelper.BeginHorizontal();
                scenePrefab.MinScale = EditorUIHelper.Slider("缩放随机最小值", scenePrefab.MinScale, MinScale, MaxScale);
                scenePrefab.MaxScale = EditorUIHelper.Slider("缩放随机最大值", scenePrefab.MaxScale, MinScale, MaxScale);
                EditorUIHelper.EndHorizontal();
                
                EditorUIHelper.Space();
            }
            if (deletePrefabIndex >= 0)
            {
                _scenePrefabs.RemoveAt(deletePrefabIndex);
            }
            
            EditorUIHelper.Button("添加", () => _scenePrefabs.Add(new ScenePrefab()));
            
            EditorUIHelper.EndScrollView();

            EditorUIHelper.Space();
            EditorUIHelper.Button("生成", Generate);
        }

        private void Refresh()
        {
            var tranes = GetSelections();
            if (tranes.Length == 0)
            {
                return;
            }
        
            var bounds = GenerateBounds(tranes);
            var arrayWidth = Mathf.FloorToInt(bounds.size.x) + 1 + TextureEdgeSize * 2;
            var arrayHeight = Mathf.FloorToInt(bounds.size.z) + 1 + TextureEdgeSize * 2;

            var hits = SceneJobUtility.RaycastGrid(bounds.center, arrayWidth, arrayHeight, _checkLayerMask, _maxHits);
            DebugRaycast(hits);
        
            _sceneRaycastInfo.UpdateData(arrayWidth, arrayHeight, hits, _maxHits, bounds.center);
            _previewTexture =
                SceneInfoCalculation.GetTexture(_sceneRaycastInfo, _calculateType, _minRange, _maxRange, _generateLayerMask);
        }

        private Transform[] GetSelections()
        {
            return Selection.GetTransforms(SelectionMode.TopLevel);
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

        private Bounds GenerateBounds(Transform[] transes)
        {
            var bounds = new Bounds();
            var renderers = new List<Renderer>();
            foreach (var trans in transes)
            {
                renderers.AddRange(trans.GetComponentsInChildren<Renderer>());
            }
            
            if (renderers.Count == 0)
            {
                return bounds;
            }

            bounds = renderers[0].bounds;
            if (renderers.Count == 1)
            {
                return bounds;
            }

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
        
        
        private void Generate()
        {
            if (_clearParent && _parent != null)
            {
                for (int i = _parent.childCount - 1; i >= 0; i--)
                {
                    var child = _parent.GetChild(i);
                    Object.DestroyImmediate(child.gameObject);
                }
            }
            
            SceneInfoCalculation.GenerateBySceneInfo(_scenePrefabs, _parent, _previewTexture, _sceneRaycastInfo, _space);
        }
    }
}