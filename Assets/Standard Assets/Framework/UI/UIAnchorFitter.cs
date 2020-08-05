using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WWFramework.Uitl.UI
{
    public class UIAnchorFitter : UIBehaviour, ILayoutSelfController
    {
        private enum Side
        {
            Left,
            Right,
            Top,
            Bottom,
        }
        
        [SerializeField]
        private RectTransform _anchorLeft;
        [SerializeField]
        private Side _leftSide = Side.Right;
        
        [SerializeField]
        private RectTransform _anchorRight;
        [SerializeField]
        private Side _rightSide = Side.Left;
        
        [SerializeField]
        private RectTransform _anchorTop;
        [SerializeField]
        private Side _topSide = Side.Bottom;
        
        [SerializeField]
        private RectTransform _anchorBottom;
        [SerializeField]
        private Side _bottomSize = Side.Top;

        private RectTransform _cacheRect;
        private RectTransform rectTransform
        {
            get
            {
                if (!_cacheRect)
                {
                    _cacheRect = GetComponent<RectTransform>();
                }

                return _cacheRect;
            }
        }
        
        private Vector3[] _tempCorners = new Vector3[4];
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }
    
        public void SetLayoutHorizontal()
        {
            UpdateAnchors();
        }

        public void SetLayoutVertical()
        {
            UpdateAnchors();
        }

        [ContextMenu("UpdateAnchors")]
        public void UpdateAnchors()
        {
            _anchorLeft.GetWorldCorners(_tempCorners);
            var right = _tempCorners[2];
            ((RectTransform) rectTransform.parent).GetWorldCorners(_tempCorners);
            var parentLeft = _tempCorners[0];
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, right.x - parentLeft.x, 160);
            // Vector2 leftPoint;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rectTransform.parent, left, null, out leftPoint);
            // rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, );
        }
        
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
    }
}