using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WWFramework.Helper;

namespace WWFramework.Util
{
    public class ArtisticShadowMapController : MonoBehaviour
    {
        private const int CheckSqrDistance = 1;

        [SerializeField] private RawImage _image;

        [SerializeField] private Camera _camera;
        private Transform _cameraTrans;
        private Matrix4x4 _cameraMatrixVP;
        private Matrix4x4 _cameraMatrixV;
        private Vector3 _lastCameraPos;

        [SerializeField] private Vector3 _defaultRotation = new Vector3(50, 0, 0);
        [SerializeField] private float _cameraDistance = 10;
        [SerializeField] private float _cameraSize = 2;
        [SerializeField] private float _cameraNear = 1;
        [SerializeField] private float _cameraFar = 20;

        [SerializeField, Range(0, 0.1f)] private float _shadowBias = 0.02f;
        [SerializeField, Range(0, 0.1f)] private float _shadowNormalBias = 0.05f;
        [SerializeField, Range(0, 1)] private float _shadowStrength = 1f;

        private enum ShadowMapSizeType
        {
            ResolutionHeight,
            NotEqual,
        }

        [SerializeField] private ShadowMapSizeType _shadowMapSizeType = ShadowMapSizeType.ResolutionHeight;
        [SerializeField] private float _shadowMapScale = 2;
        private RenderTexture _shadowMap;

#if UNITY_EDITOR
        private void OnEnable()
        {
            UpdateData();
        }

        private void Update()
        {
            UpdateData();
        }
#endif

        public void EnableShadowMap(Vector3 rotation, List<Renderer> renders)
        {
            foreach (var render in renders)
            {
                // 传进来的第一个
                render.material.EnableKeyword(ShaderHelper.CHAR_SHADOWMAP_ON);
            }

            _defaultRotation = rotation;

            UpdateData();
        }


        [ContextMenu("UpdateData")]
        private void UpdateData()
        {
            CreateShadowMap();
            CreateCamera();

            UpdateShadowParams();
            UpdateCameraParams();
        }

        private void UpdateShadowParams()
        {
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowBias), _shadowBias);
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowNormalBias), _shadowNormalBias);
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowIntensity), _shadowStrength);

            Shader.SetGlobalTexture(ShaderHelper.PropertyToID(ShaderHelper.ShadowMap), _shadowMap);
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowMapWidthScale),
                1f / GetShadowMapWidth());
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowMapHeightScale),
                1f / GetShadowMapHeight());
        }

        private void UpdateCameraParams()
        {
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowFarScale), 1 / _cameraFar);
            Shader.SetGlobalVector(ShaderHelper.PropertyToID(ShaderHelper.ShadowLightDir),
                (Quaternion.Euler(_cameraTrans.eulerAngles) * Vector3.back).normalized);
        }

        private void CreateCamera()
        {
            if (_camera == null)
            {
                var camGo = new GameObject("ShadowMap Camera");
                _camera = camGo.AddComponent<Camera>();
                _cameraTrans = _camera.transform;

                // 如果改成专用的阴影的话，DC的耗时应该能降低，相对应的RT格式也得更改
                _camera.SetReplacementShader(ShaderHelper.Find(ShaderHelper.DepthPass), ShaderHelper.RenderTypeTag);
            }

            _camera.backgroundColor = Color.white;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.orthographic = true;

            _camera.targetTexture = _shadowMap;

            // 没必要额外生成阴影贴图
            // _camera.depthTextureMode = DepthTextureMode.Depth;
            _camera.depthTextureMode = DepthTextureMode.None;

            _camera.orthographicSize = _cameraSize;
            _camera.nearClipPlane = _cameraNear;
            _camera.farClipPlane = _cameraFar;
            _camera.allowHDR = false;
            // 采用渲染物体的Layer，可能得更改
            _camera.cullingMask = 1 << gameObject.layer;

            UpdateCameraPosition();

            CheckUpdateCameraMatrix(true);
        }

        private void ReleaseCamera()
        {
            if (_camera == null)
            {
                return;
            }

            // 假定Camera必定是动态创建出来的
            Destroy(_camera);
            _camera = null;
            _cameraTrans = null;
        }

        private void UpdateCameraPosition()
        {
            _cameraTrans.eulerAngles = _defaultRotation;
            _cameraTrans.position = transform.position - _cameraTrans.forward * _cameraDistance;
        }

        public void UpdateCameraRotation(Vector3 rotation)
        {
            _defaultRotation = rotation;
            UpdateCameraPosition();

            CheckUpdateCameraMatrix(true);
        }

        private void CheckUpdateCameraMatrix(bool force = false)
        {
            if (UpdateCameraMatrix(force))
            {
                Shader.SetGlobalMatrix(ShaderHelper.PropertyToID(ShaderHelper.ShadowMatrixV), _cameraMatrixV);
                Shader.SetGlobalMatrix(ShaderHelper.PropertyToID(ShaderHelper.ShadowMatrixVP), _cameraMatrixVP);
            }
        }

        private bool UpdateCameraMatrix(bool force)
        {
            if (!_cameraTrans)
            {
                return false;
            }

            var newPos = _cameraTrans.position;
            if (!force && Vector3.SqrMagnitude(newPos - _lastCameraPos) < CheckSqrDistance)
            {
                return false;
            }

            _lastCameraPos = _cameraTrans.position;
            _cameraMatrixV = _camera.worldToCameraMatrix;
            _cameraMatrixVP = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true) * _cameraMatrixV;

            return true;
        }

        private int GetShadowMapWidth()
        {
            if (_shadowMapSizeType == ShadowMapSizeType.ResolutionHeight)
            {
                return GetShadowMapHeight();
            }

            // var width = Screen.width;
            var width = Screen.currentResolution.width;
            return (int) (width * _shadowMapScale);
        }

        private int GetShadowMapHeight()
        {
            // var height = Screen.height;
            var height = Screen.currentResolution.height;
            return (int) (height * _shadowMapScale);
        }

        private void CreateShadowMap()
        {
            if (_shadowMap)
            {
                // ReleaseShadowMap();

                return;
            }

            // var rtFormat = RenderTextureFormat.ARGBHalf;
            var rtFormat = RenderTextureFormat.ARGB32;

            _shadowMap = RenderTexture.GetTemporary(GetShadowMapWidth(), GetShadowMapHeight(), 24, rtFormat,
                RenderTextureReadWrite.Linear, 1);
            // _shadowMap.useMipMap = false;
            _shadowMap.filterMode = FilterMode.Bilinear;

            if (_image)
            {
                _image.texture = _shadowMap;
            }
        }

        private void ReleaseShadowMap()
        {
            if (_shadowMap == null)
            {
                return;
            }

            RenderTexture.ReleaseTemporary(_shadowMap);
            _shadowMap = null;
        }

        private void OnDestroy()
        {
            ReleaseCamera();
            ReleaseShadowMap();
        }

        private void LateUpdate()
        {
            CheckUpdateCameraMatrix();
        }
    }
}