using UnityEditor;
using UnityEngine;

namespace WWFramework.UnityReflect.Inspector.Editor
{
    [CustomEditor(typeof(MeshRenderer))]
    internal class CustomMeshRendererEditor : MeshRendererEditor
    {
        private MeshRenderer _renderer;
        private int _sortingOrder;

        public override void OnEnable()
        {
            base.OnEnable();
        
            _renderer = serializedObject.targetObject as MeshRenderer;
            _sortingOrder = _renderer.sortingOrder;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _sortingOrder = EditorGUILayout.IntField("SortingOrder", _sortingOrder);
            _renderer.sortingOrder = _sortingOrder;
        }
    }
}