using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Scene.Editor
{
    public class SceneRaycastInfo
    {
        private int _width;
        public int Width => _width;
        private int _height;
        public int Height => _height;

        private Vector3 _center;
        public Vector3 Center => _center;
        
        private RaycastHit[] _hits;
        private int _maxHitNums;
    
        private List<RaycastHit> _tempHits = new List<RaycastHit>(8);


        public void UpdateData(int width, int height, RaycastHit[] hits, int maxHitNums, Vector3 center)
        {
            _width = width;
            _height = height;
            _hits = hits;
            _maxHitNums = maxHitNums;

            _center = center;
        }
    

        public bool IsContainsLayer(int posX, int posY, int layerMask)
        {
            var hits = CheckRaycastHit(posX, posY);
            foreach (var hit in hits)
            {
                if (IsContainsLayer(hit, layerMask))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsValidHit(RaycastHit hit)
        {
            return hit.collider != null;
        }

        public bool IsContainsLayer(RaycastHit hit, int layerMask)
        {
            if (!IsValidHit(hit))
            {
                return false;
            }
        
            var hitMask = 1 << hit.collider.gameObject.layer;
        
            return (hitMask & layerMask) != 0;
        }

        public RaycastHit[] GetTopHits()
        {
            _tempHits.Clear();
            for (int i = 0; i < _hits.Length; i = i + _maxHitNums)
            {
                _tempHits.Add(_hits[i]);
            }

            return _tempHits.ToArray();
        }

        public RaycastHit GetTopHit(int posX, int posY)
        {
            var index = Pos2StartIndex(posX, posY);
            return _hits[index];
        }

        public RaycastHit GetTopHit(int index)
        {
            return _hits[index * _maxHitNums];
        }

        public List<RaycastHit> CheckRaycastHit(int posX, int posY)
        {
            _tempHits.Clear();
        
            var index = Pos2StartIndex(posX, posY);
            for (int i = index; i < index + _maxHitNums; i++)
            {
                var hit = _hits[i];
                if (hit.collider != null)
                {
                    _tempHits.Add(hit);
                }
            }

            return _tempHits;
        }

        private int Pos2StartIndex(int posX, int posY)
        {
            return (posX + posY * Width) * _maxHitNums;
        }
    }
}