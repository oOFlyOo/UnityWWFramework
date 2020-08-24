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
    
        [SerializeField] 
        private Camera _camera;
        private Transform _cameraTrans;
        private Matrix4x4 _curCameraMatrix;
        private Vector3 _lastCameraPos;

        [SerializeField]
        private Vector3 _defaultRotation;
        [SerializeField] 
        private float _cameraDistance = 5;

        [SerializeField] 
        private float _shadowBias = 0.002f;
        [SerializeField] 
        private float _shadowStrength = 1f;
        [SerializeField] 
        private float _shadowMapScale = 1;
        private RenderTexture _shadowMap;

        // private void OnEnable()
        // {
        //     UpdateData();
        // }

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
        }

        private void UpdateShadowParams()
        {
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowBias), _shadowBias);
            Shader.SetGlobalFloat(ShaderHelper.PropertyToID(ShaderHelper.ShadowIntensity), _shadowStrength);
        
            Shader.SetGlobalTexture(ShaderHelper.PropertyToID(ShaderHelper.ShadowMap), _shadowMap);
        }

        private void CreateCamera()
        {
            if (_camera == null)
            {
                var camGo = new GameObject("ShadowMap Camera");
                _camera = camGo.AddComponent<Camera>();
                _cameraTrans = _camera.transform;
            
                // 如果改成专用的阴影的话，DC的耗时应该能降低，相对应的RT格式也得更改
                _camera.SetReplacementShader(Shader.Find("DepthMap"), "");
            }
        
            _camera.backgroundColor = Color.white;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.orthographic = true;

            _camera.targetTexture = _shadowMap;

            // 没必要额外生成阴影贴图
            // _camera.depthTextureMode = DepthTextureMode.Depth;
            _camera.depthTextureMode = DepthTextureMode.None;
        
            // 暂时写死这些参数
            _camera.orthographicSize = 2;
            _camera.nearClipPlane = 0;
            _camera.farClipPlane = 10;
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
                Shader.SetGlobalMatrix(ShaderHelper.PropertyToID(ShaderHelper.ShadowMatrix), _curCameraMatrix);
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

            _lastCameraPos = _camera.transform.position;
            _curCameraMatrix = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true) * _camera.worldToCameraMatrix;

            return true;
        }

        private void CreateShadowMap()
        {
            if (_shadowMap)
            {
                // ReleaseShadowMap();

                return;
            }

            // var rtFormat = RenderTextureFormat.Shadowmap;
            // if (!SystemInfo.SupportsRenderTextureFormat(rtFormat))
            // {
            //     rtFormat = RenderTextureFormat.Depth;
            // }
            var rtFormat = RenderTextureFormat.ARGB32;
        
            _shadowMap = RenderTexture.GetTemporary((int)(Screen.width * _shadowMapScale), (int)(Screen.height * _shadowMapScale), 24, rtFormat, RenderTextureReadWrite.Default, 1);
            // _shadowMap.useMipMap = false;
            // _shadowMap.filterMode = FilterMode.Bilinear;

            _image.texture = _shadowMap;
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