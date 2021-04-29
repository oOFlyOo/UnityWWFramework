using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WWFramework.Development
{
    public class SyncSceneView: MonoBehaviour
    {
#if UNITY_EDITOR
        private SceneView _sceneView;

        private Camera _mainCamera;

        private void Awake()
        {
            _sceneView = SceneView.lastActiveSceneView;
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_sceneView != null && _mainCamera != null)
            {
                // _sceneView.LookAt(transform.position, transform.rotation);
                
                _sceneView.cameraSettings.nearClip = _mainCamera.nearClipPlane;
                _sceneView.cameraSettings.fieldOfView = _mainCamera.fieldOfView;
                _sceneView.pivot = _mainCamera.transform.position +
                                   _mainCamera.transform.forward * _sceneView.cameraDistance;
                _sceneView.rotation = _mainCamera.transform.rotation;
            }
        }
#endif
    }
}