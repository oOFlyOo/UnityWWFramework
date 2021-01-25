using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Scene.Editor
{
    [Serializable]
    public class ScenePrefab
    {
        public GameObject Prefab;
        public int Weight = 1;
        public float MinScale = 1;
        public float MaxScale = 1;
    }
}