using System;
using UnityEditor;
using UnityEngine;

namespace WWFramework.ShaderGUI.Editor
{
    public abstract class CusomBaseShaderGUI : BaseShaderGUI
    {
        protected class Styles
        {
            public static readonly GUIContent AdditionalLabel = new GUIContent("Additional",
                "扩展");
        }

        bool m_AdditionalFoldout;

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            m_AdditionalFoldout = true;

            base.OnOpenGUI(material, materialEditor);
        }

        public override void DrawAdditionalFoldouts(Material material)
        {
            m_AdditionalFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_AdditionalFoldout, Styles.AdditionalLabel);
            if (m_AdditionalFoldout)
            {
                DrawAdditionalFoldoutsEx(material);
                base.DrawAdditionalFoldouts(material);

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual void DrawAdditionalFoldoutsEx(Material material)
        {

        }
    }
}
