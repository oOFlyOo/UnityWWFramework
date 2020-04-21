

using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace WWFramework.Util
{
    public class SelfShadowing : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;
        private Transform _cameraTransform;
        private Matrix4x4 _curCameraMatrix;
        private Vector3 _lastCameraPos = Vector3.zero;
        /// <summary>
        /// 距离超过0.1则更新
        /// </summary>
        private const float CheckSqrDistance = 0.01f;
        //        [SerializeField] 
        //        private Material _depthMaterial;
        [SerializeField]
        private RenderTexture _depthTexture;
        [SerializeField]
        private float _textureScale = 1.0f;

        [Space]
//#if UNITY_EDITOR
        [SerializeField]
        private List<Material> _showDepthMaterials;
//#endif
        private HashSet<Material> _applyDepthMaterials = new HashSet<Material>();
        private const string DepthMatrixName = "_ShadowMatrix";
        private int _depthMatrixId;
        private const string DepthTextureName = "_ShadowTexture";
        private int _depthTextureId;

        private void Awake()
        {
//            OnInit();
            ResetData();
        }


        private void OnDestroy()
        {
            OnRelease();
        }


//#if UNITY_EDITOR
        [ContextMenu("ResetData")]
        private void ResetData()
        {
            OnInit();

            foreach (var mat in _showDepthMaterials)
            {
                SetMatSelfShadowTexture(mat);
                UpdateMatSelfShadowMatrix(mat);
            }
        }


        [ContextMenu("UseShowMaterials")]
        private void UseShowMaterials()
        {
            ClearMatsSelfShadow();
            foreach (var mat in _showDepthMaterials)
            {
                AddMatSelfShadow(mat);
            }
        }
//#endif


        private void OnInit()
        {
            CreateRenderTexture();

            _cameraTransform = _camera.transform;
            _camera.depthTextureMode = DepthTextureMode.Depth;
            _camera.targetTexture = _depthTexture;
            UpdateCameraMatrix(false);

            _depthMatrixId = Shader.PropertyToID(DepthMatrixName);
            _depthTextureId = Shader.PropertyToID(DepthTextureName);
        }


        private void OnRelease()
        {
            ClearMatsSelfShadow();
            ReleaseRenderTexture();
        }


        private bool UpdateCameraMatrix(bool check)
        {
            var newPos = _cameraTransform.position;
            if (check && Vector3.SqrMagnitude(newPos - _lastCameraPos) < CheckSqrDistance)
            {
                return false;
            }

            _lastCameraPos = _camera.transform.position;
            _curCameraMatrix = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true) * _camera.worldToCameraMatrix;

            return true;
        }


        private void CreateRenderTexture()
        {
            ReleaseRenderTexture();

            _depthTexture = RenderTexture.GetTemporary((int)(Screen.width * _textureScale), (int)(Screen.height * _textureScale), 0, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Default, 1);
        }

        private void ReleaseRenderTexture()
        {
            if (_depthTexture == null)
            {
                return;
            }

            RenderTexture.ReleaseTemporary(_depthTexture);
            _depthTexture = null;
        }


        private void SetMatSelfShadowTexture(Material mat)
        {
            mat.SetTexture(_depthTextureId, _depthTexture);
        }


        /// <summary>
        /// 清掉引用
        /// </summary>
        /// <param name="mat"></param>
        private void ReleaseMatSelfShadowTexture(Material mat)
        {
            mat.SetTexture(_depthTextureId, null);
        }


        /// <summary>
        /// 注意如果Shader重新编译，信息会丢失
        /// 这时候需要重新赋值
        /// </summary>
        /// <param name="mat"></param>
        private void UpdateMatSelfShadowMatrix(Material mat)
        {
            mat.SetMatrix(_depthMatrixId, _curCameraMatrix);
        }

        public void AddMatSelfShadow(Material mat)
        {
            _applyDepthMaterials.Add(mat);
            SetMatSelfShadowTexture(mat);
            UpdateMatSelfShadowMatrix(mat);
        }


        public void RemoveMatSelfShadow(Material mat)
        {
            ReleaseMatSelfShadowTexture(mat);
            _applyDepthMaterials.Remove(mat);
        }


        public void ClearMatsSelfShadow()
        {
            var clearMats = _applyDepthMaterials.ToArray();
            foreach (var mat in clearMats)
            {
                RemoveMatSelfShadow(mat);
            }
        }

        /// <summary>
        /// 每帧检查摄像机位置变化，不考虑其它参数导致的变化
        /// 如果摄像机不动，甚至可以优化掉修改
        /// </summary>
        private void Update()
        {
            if (_applyDepthMaterials.Count == 0)
            {
                return;
            }

            if (UpdateCameraMatrix(true))
            {
                foreach (var mat in _applyDepthMaterials)
                {
                    UpdateMatSelfShadowMatrix(mat);
                }
            }
        }


//        private void OnRenderImage(RenderTexture source, RenderTexture destination)
//        {
//            if (_depthMaterial != null)
//            {
//                Graphics.Blit(source, destination, _depthMaterial);
//            }
//            else
//            {
//                Graphics.Blit(source, destination);
//            }
//        }
    }
}