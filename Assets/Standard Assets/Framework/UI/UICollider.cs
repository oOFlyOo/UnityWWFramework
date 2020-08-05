using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WWFramework.Uitl.UI
{
    public class UICollider : Graphic, ICanvasRaycastFilter
    {
        protected override void Awake() {
            base.Awake();
        
            color = Color.clear;
        }
    
        /// <summary>
        /// 这个非必要，如果需要填充颜色的话；不过需要填充颜色的话Image可能更合适
        /// </summary>
        /// <param name="toFill"></param>
        protected override void OnPopulateMesh(VertexHelper toFill) {
            toFill.Clear();
        }
    
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return true;
        }
    }
}