using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using WWFramework.Uitl.UI;

namespace WWFramework.UI.Editor
{
    [CustomEditor(typeof(ScaleScrollRect), true)]
    [CanEditMultipleObjects]
    public class ScaleScrollRectEditor : ScrollRectEditor
    {
        private SerializedProperty _debugMode;
        private SerializedProperty _scaleContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            _debugMode = serializedObject.FindProperty("_debugMode");
            _scaleContent = serializedObject.FindProperty("_scaleContent");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_debugMode);
            EditorGUILayout.PropertyField(_scaleContent);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}