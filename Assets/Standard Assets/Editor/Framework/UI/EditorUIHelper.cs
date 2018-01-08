
using System;
using UnityEditor;
using UnityEngine;

namespace WWFramework.Editor.UI
{
    public static class EditorUIHelper
    {
        #region GUIStyle
        private const int NormalFontSize = 20;

        private static GUIStyle _leftButtonStyle;
        public static GUIStyle LeftButtonStyle
        {
            get
            {
                if (_leftButtonStyle == null)
                {
                    _leftButtonStyle = new GUIStyle(GUI.skin.button);
                    _leftButtonStyle.alignment = TextAnchor.MiddleLeft;
                }
                return _leftButtonStyle;
            }
        }

        private static GUIStyle _normalButtonStyle;
        public static GUIStyle NormalButtonStyle
        {
            get
            {
                if (_normalButtonStyle == null)
                {
                    _normalButtonStyle = new GUIStyle(GUI.skin.button);
                    _normalButtonStyle.fontSize = NormalFontSize;
                }
                return _normalButtonStyle;
            }
        }

        private static GUIStyle _searchTextField;
        public static GUIStyle SearchTextField
        {
            get
            {
                if (_searchTextField == null)
                {
                    _searchTextField = new GUIStyle("SearchTextField");
                }
                return _searchTextField;
            }
        }

        private static GUIStyle _redLabelStyle;
        public static GUIStyle RedLabelStyle
        {
            get
            {
                if (_redLabelStyle == null)
                {
                    _redLabelStyle = new GUIStyle(GUI.skin.label);
                    _redLabelStyle.normal.textColor = Color.red;
                }
                return _redLabelStyle;
            }
        }

        private static GUIStyle _yellowLabelStyle;
        public static GUIStyle YellowLabelStyle
        {
            get
            {
                if (_yellowLabelStyle == null)
                {
                    _yellowLabelStyle = new GUIStyle(GUI.skin.label);
                    _yellowLabelStyle.normal.textColor = Color.yellow;
                }
                return _yellowLabelStyle;
            }
        }
        #endregion

        #region 常用封装
        public static void Space()
        {
            EditorGUILayout.Space();
        }

        public static void DrawLine()
        {
            LabelField("", GUI.skin.horizontalSlider);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }


        public static Rect BeginVertical(params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginVertical(options);
        }


        public static void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }


        public static Rect BeginHorizontal(params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginHorizontal(options);
        }


        public static void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }


        public static Vector2 BeginScrollView(Vector2 pos, params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginScrollView(pos, options);
        }


        public static void EndScrollView()
        {
            EditorGUILayout.EndScrollView();
        }


        public static void TitleField(string title, string label = null)
        {
            EditorGUILayout.LabelField(title, label);
        }


        public static void LabelField(string label, GUIStyle style = null)
        {
            style = style ?? GUI.skin.label;
            EditorGUILayout.LabelField(label, style);
        }


        public static string TextField(string label, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.textField;

            return EditorGUILayout.TextField(label, text, style, options);
        }


        public static int IntSlider(string title, int value, int lValue, int rValue)
        {
            return EditorGUILayout.IntSlider(title, value, lValue, rValue);
        }


        private const int LimitedButtonClickTime = 100;
        private static long _lastButtonClickTime;

        public static void Button(string text, Action callback = null, GUIStyle style = null)
        {
            style = style ?? EditorUIHelper.NormalButtonStyle;
            if (GUILayout.Button(text, style))
            {
                if (DateTime.UtcNow.ToFileTimeUtc() - _lastButtonClickTime < LimitedButtonClickTime)
                {
                    return;
                }

                if (callback != null)
                {
                    callback();
                }

                _lastButtonClickTime = DateTime.UtcNow.ToFileTimeUtc();
            }
        }


        public static T EnumPopup<T>(string title, Enum selectedEnum, GUIStyle style = null)
        {
            style = style ?? EditorStyles.popup;
            return (T)Convert.ChangeType(EditorGUILayout.EnumPopup(title, selectedEnum, style), typeof(T));
        }


        public static UnityEngine.Object ObjectField(string title, UnityEngine.Object obj, Type type = null, bool allowSceneObjects = false)
        {
            return EditorGUILayout.ObjectField(title, obj, type, allowSceneObjects);
        }


        public static bool Toggle(string label, bool value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.toggle;
            return EditorGUILayout.Toggle(label, value, style, options);
        }


        public static bool ToggleLeft(string label, bool value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.toggle;
            return EditorGUILayout.ToggleLeft(label, value, style, options);
        }

        #endregion
    }
}