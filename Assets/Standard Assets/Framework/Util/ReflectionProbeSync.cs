using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WWFramework.Util
{
    public class ReflectionProbeSync : MonoBehaviour
    {
        [SerializeField] private Transform _probeTrans;
        [SerializeField] private Transform _cameraTrans;
        [SerializeField] private Transform _planeTrans;

        public void SetCameraTrans(Transform cameraTrans)
        {
            _cameraTrans = cameraTrans;
        }

        [ContextMenu("快速匹配Trans")]
        private void TryCheckTransform()
        {
            if (_probeTrans == null)
            {
                _probeTrans = transform;
            }

            if (_cameraTrans == null)
            {
                _cameraTrans = Camera.main.transform;
            }
        }

        [ContextMenu("更新Probe位置")]
        private void UpdateTransform()
        {
            if (_cameraTrans == null || _probeTrans == null || _planeTrans == null)
            {
                return;
            }

            var cameraPos = _cameraTrans.position;
            _probeTrans.transform.position =
                new Vector3(cameraPos.x, _planeTrans.position.y * 2 - cameraPos.y, cameraPos.z);
        }

        private void LateUpdate()
        {
            UpdateTransform();
        }
    }
}