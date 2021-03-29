using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Development
{
    public class DynamicSettings : MonoBehaviour
    {
        [SerializeField] private Vector2 _offset = new Vector2(20, 10);
        [SerializeField] private Vector2 _defaultScreenResolution = new Vector2(1334, 750);
        [SerializeField] private Vector2 _defaultBtnSize = new Vector2(160, 50);
        [SerializeField] private float _space = 10;
        
        private List<Tuple<Func<string>, Action>> _btnFuncs = new List<Tuple<Func<string>, Action>>()
        {
            Tuple.Create<Func<string>, Action>( () => $"Quality {QualitySettings.GetQualityLevel()} -", () => QualitySettings.DecreaseLevel(true)),
            Tuple.Create<Func<string>, Action>( () => $"Quality {QualitySettings.GetQualityLevel()} +", () => QualitySettings.IncreaseLevel(true)),
        };

        private Vector2 _dynamicBtnSize;
        private float _spaceHeight;
        
        private void Start()
        {
            var dynamicHeight = Screen.height;
            var scale = dynamicHeight / _defaultScreenResolution.y;
            _dynamicBtnSize = _defaultBtnSize * scale;

            _spaceHeight = _space * scale + _dynamicBtnSize.y;
        }
    
        private void OnGUI()
        {
            var startPos = new Vector2(0, _spaceHeight) + _offset;
            foreach (var btnFunc in _btnFuncs)
            {
                if (GUI.Button(new Rect(startPos, _dynamicBtnSize), btnFunc.Item1()))
                {
                    btnFunc.Item2();
                }
                
                startPos += Vector2.up * _spaceHeight;
            }
        }
    }
}