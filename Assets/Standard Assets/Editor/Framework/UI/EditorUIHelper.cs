
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WWFramework.UI.Editor
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

        private static GUIStyle _rightButtonStyle;
        public static GUIStyle RightButtonStyle
        {
            get
            {
                if (_rightButtonStyle == null)
                {
                    _rightButtonStyle = new GUIStyle(GUI.skin.button);
                    _rightButtonStyle.alignment = TextAnchor.MiddleRight;
                }
                return _rightButtonStyle;
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

        private static GUIStyle _buttonStyle;
        public static GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(GUI.skin.button);
                }
                return _buttonStyle;
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

        private static GUIStyle _searchCancelButton;
        public static GUIStyle SearchCancelButton
        {
            get
            {
                if (_searchCancelButton == null)
                {
                    _searchCancelButton = new GUIStyle("SearchCancelButton");
                }
                return _searchCancelButton;
            }
        }

        private static GUIStyle _searchCancelButtonEmpty;
        public static GUIStyle SearchCancelButtonEmpty
        {
            get
            {
                if (_searchCancelButtonEmpty == null)
                {
                    _searchCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
                }
                return _searchCancelButtonEmpty;
            }
        }

        private static GUIStyle _buttonLeft;
        public static GUIStyle ButtonLeft
        {
            get
            {
                if (_buttonLeft == null)
                {
                    _buttonLeft = new GUIStyle("ButtonLeft");
                }
                return _buttonLeft;
            }
        }

        private static GUIStyle _buttonMid;
        public static GUIStyle ButtonMid
        {
            get
            {
                if (_buttonMid == null)
                {
                    _buttonMid = new GUIStyle("ButtonMid");
                }
                return _buttonMid;
            }
        }

        private static GUIStyle _buttonRight;
        public static GUIStyle ButtonRight
        {
            get
            {
                if (_buttonRight == null)
                {
                    _buttonRight = new GUIStyle("ButtonRight");
                }
                return _buttonRight;
            }
        }

        private static GUIStyle _textFieldStyle;
        public static GUIStyle TextFieldStyle
        {
            get
            {
                if (_textFieldStyle == null)
                {
                    _textFieldStyle = new GUIStyle("TextField");
                }
                return _textFieldStyle;
            }
        }

        private static GUIStyle _textAreaStyle;
        public static GUIStyle TextAreaStyle
        {
            get
            {
                if (_textAreaStyle == null)
                {
                    _textAreaStyle = new GUIStyle("TextArea");
                }
                return _textAreaStyle;
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

        public static bool Changed
        {
            get { return GUI.changed; }
            set { GUI.changed = value; }
        }

        public static void Space()
        {
            EditorGUILayout.Space();
        }

        public static void Space(float pixel)
        {
            GUILayout.Space(pixel);
        }

        public static void DrawLine()
        {
            LabelField("", GUI.skin.horizontalSlider);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }


        public static Rect BeginVertical(GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUIStyle.none;
            return EditorGUILayout.BeginVertical(style, options);
        }


        public static void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }


        public static Rect BeginHorizontal(GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUIStyle.none;
            return EditorGUILayout.BeginHorizontal(style, options);
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


        public static void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }


        public static bool EndChangeCheck()
        {
            return EditorGUI.EndChangeCheck();
        }


        public static void BeginSelectdColor(bool selected)
        {
            GUI.backgroundColor = selected ? Color.yellow : Color.white;
        }

        public static void EndSelectedColor()
        {
            GUI.backgroundColor = Color.white;
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

        public static string SearchCancelTextField(string text)
        {
            BeginHorizontal();
            {
                text = TextField(string.Empty, text, SearchTextField);

                var flag = !string.IsNullOrEmpty(text);
                Action action = null;
                if (flag)
                {
                    action = () =>
                    {
                        text = string.Empty;
                        GUIUtility.keyboardControl = 0;
                    };
                }

                Button(string.Empty, action, flag ? SearchCancelButton : SearchCancelButtonEmpty);
            }
            EndHorizontal();

            return text;
        }

        public static int IntSlider(string title, int value, int lValue, int rValue)
        {
            return EditorGUILayout.IntSlider(title, value, lValue, rValue);
        }


        private const int LimitedButtonClickTime = 60;
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


        public static T EnumPopup<T>(Enum selectedEnum, string title = "", GUIStyle style = null)
        {
            style = style ?? EditorStyles.popup;
            return (T)Convert.ChangeType(EditorGUILayout.EnumPopup(title, selectedEnum, style), typeof(T));
        }

        public static int Popup(string title, int index, string[] displayedOptions, GUIStyle style = null)
        {
            style = style ?? EditorStyles.popup;
            return EditorGUILayout.Popup(title, index, displayedOptions, style);
        }


        public static Object ObjectField(Object obj, Type type = null, string title = "", bool allowSceneObjects = false)
        {
            return EditorGUILayout.ObjectField(title, obj, type, allowSceneObjects);
        }


        public static bool Toggle(string label, bool value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.toggle;
            return EditorGUILayout.Toggle(label, value, style, options);
        }

        public static bool ToggleInner(string label, bool value)
        {
            return GUILayout.Toggle(value, label, TextFieldStyle);
        }

        public static int Toolbar(int index, string[] labels)
        {
            return GUILayout.Toolbar(index, labels);
        }


        public static bool ToggleLeft(string label, bool value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style = style ?? GUI.skin.toggle;
            return EditorGUILayout.ToggleLeft(label, value, style, options);
        }

        #endregion
    }
}