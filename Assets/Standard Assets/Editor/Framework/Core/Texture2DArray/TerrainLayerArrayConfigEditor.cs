using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WWFramework.Core.Editor
{
    [CustomEditor(typeof(TerrainLayerArrayConfig))]
    public class TerrainLayerArrayConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("生成"))
            {
                Generate((TerrainLayerArrayConfigEditor)target);
            }
        }

        public static void Generate(TerrainLayerArrayConfigEditor config)
        {
            
        }
    }
}