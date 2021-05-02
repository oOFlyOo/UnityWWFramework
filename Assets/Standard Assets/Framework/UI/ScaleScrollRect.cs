using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WWFramework.Uitl.UI
{
    public class ScaleScrollRect : ScrollRect
    {
        private const float ScaleFactor = 10;
        private const float ScrollFactor = 1.5f;

        [SerializeField] 
        private bool _debugMode = false;
        [SerializeField]
        private bool _scaleContent = true;

        /// <summary>
        /// 得到的是缩放的比例以及缩放的中心点
        /// </summary>
        public Action<float, Vector2> OnZoom;

        private int _touchCount;
        private float _preDistance;
        private Vector2 _beginZoomPosition1;
        private Vector2 _beginZoomPosition2;

        public override void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            // y值为1向上滚，否则向下滚
            var delta = data.scrollDelta.y;

            if (delta >= 0)
            {
                Zoom(Mathf.Pow(ScrollFactor, delta), data.position);
            }
            else
            {
                Zoom(Mathf.Pow(1 / ScrollFactor, -delta), data.position);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsActive())
                return;

            if (Input.touchCount > 1)
            {
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsActive())
                return;

            if (Input.touchCount > 1)
            {
                _touchCount = Input.touchCount;
                return;
            }
            else if (Input.touchCount == 1 && _touchCount > 1)
            {
                _touchCount = Input.touchCount;
                base.OnBeginDrag(eventData);
                return;
            }

            base.OnDrag(eventData);
        }

        private void Update()
        {
            if (Input.touchCount != 2)
            {
                return;
            }

            var t1 = Input.GetTouch(0);
            var t2 = Input.GetTouch(1);

            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                _beginZoomPosition1 = t1.position;
                _beginZoomPosition2 = t2.position;
                _preDistance = Vector2.Distance(_beginZoomPosition1, _beginZoomPosition2);
            }
            else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
            {
                var pos1 = t1.position;
                var pos2 = t2.position;
                var dis = Vector2.Distance(pos1, pos2);
                var scale = dis / _preDistance;
                _preDistance = dis;

                Zoom(scale, (_beginZoomPosition1 + _beginZoomPosition2) * 0.5f);
            }
        }

        protected override void OnDestroy()
        {
            OnZoom = null;

            base.OnDestroy();
        }

        private void Zoom(float scale, Vector2 pos)
        {
            if (_debugMode)
            {
                Debug.Log($"Scale:{scale} Pos:{pos}");
            }
            
            if (_scaleContent)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pos, content.GetComponentInParent<Canvas>().worldCamera, out Vector2 contentPoint))
                {
                    var originScale = content.localScale;
                    var targetScale = originScale * scale;
                    
                    var deltaPos = contentPoint * originScale - contentPoint * targetScale;
                    
                    content.anchoredPosition = content.anchoredPosition + deltaPos;
                    content.localScale = targetScale;
                }
            }
            
            OnZoom?.Invoke(scale, pos);
        }
    }
}