using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WWFramework.Uitl.UI
{
    public class ScaleDragPanel: MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        private const float ScrollFactor = 1.5f;

        [SerializeField] 
        private RectTransform _content;
        [SerializeField] 
        private bool _debugMode = false;
        [SerializeField]
        private bool _scaleContent = true;
        [SerializeField]
        private bool _dragContent = true;

        private RectTransform _cachePanel;

        /// <summary>
        /// 得到的是缩放的比例以及缩放的中心点
        /// </summary>
        public Action<float, Vector2> OnZoom;

        /// <summary>
        /// 根据UI的位置来拖动位置
        /// </summary>
        public Action<Vector2> OnUIDrag;

        private int _touchCount;
        private float _preDistance;
        private Vector2 _beginZoomPosition1;
        private Vector2 _beginZoomPosition2;

        private Vector2 _lastDragPosition;
        private bool _dragging;

        private void Start()
        {
            _cachePanel = (RectTransform)transform;
        }

        public void OnScroll(PointerEventData data)
        {
            if (!isActiveAndEnabled)
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

        private bool IsDragButton(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left;
        }

        private Vector2 GetDragPoint(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachePanel, eventData.position, eventData.pressEventCamera, out Vector2 point);

            return point;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled)
                return;

            if (Input.touchCount > 1)
            {
                return;
            }
            
            if (!IsDragButton(eventData))
                return;

            _dragging = true;

            _lastDragPosition = GetDragPoint(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled)
                return;

            if (Input.touchCount > 1)
            {
                _touchCount = Input.touchCount;
                return;
            }
            else if (Input.touchCount == 1 && _touchCount > 1)
            {
                _touchCount = Input.touchCount;
                OnBeginDrag(eventData);
                return;
            }

            if (!_dragging)
            {
                return;
            }

            if (!IsDragButton(eventData))
            {
                return;
            }

            var pos = GetDragPoint(eventData);
            var deltaPos = pos - _lastDragPosition;
            _lastDragPosition = pos;
            
            UIDrag(deltaPos);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            // if (!isActiveAndEnabled)
            // {
            //     return;
            // }
            
            if (!IsDragButton(eventData))
                return;

            _dragging = false;
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

        private void OnDestroy()
        {
            OnZoom = null;
            OnUIDrag = null;
        }

        private void UIDrag(Vector2 offset)
        {
            if (_dragContent)
            {
                _content.anchoredPosition += offset;
            }
            
            OnUIDrag?.Invoke(offset);
        }

        private void Zoom(float scale, Vector2 pos)
        {
            if (_debugMode)
            {
                Debug.Log($"Scale:{scale} Pos:{pos}");
            }
            
            if (_scaleContent)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_content, pos, _content.GetComponentInParent<Canvas>().worldCamera, out Vector2 contentPoint))
                {
                    var originScale = _content.localScale;
                    var targetScale = originScale * scale;
                    
                    var deltaPos = contentPoint * originScale - contentPoint * targetScale;
                    
                    _content.anchoredPosition = _content.anchoredPosition + deltaPos;
                    _content.localScale = targetScale;
                }
            }
            
            OnZoom?.Invoke(scale, pos);
        }
    }
}