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

        private void Awake()
        {
            _sceneView = SceneView.lastActiveSceneView;
        }

        private void LateUpdate()
        {
            if (_sceneView != null)
            {
                _sceneView.LookAt(transform.position, transform.rotation);
            }
        }

        private void OnDestroy()
        {
            if (_sceneView != null)
            {
                _sceneView.LookAt(transform.position, transform.rotation, 5f);

                _sceneView = null;
            }
        }
#endif
    }
}