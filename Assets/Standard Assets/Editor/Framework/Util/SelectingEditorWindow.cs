﻿
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Editor.Helper;
using WWFramework.Editor.UI;

namespace WWFramework.Editor.Util
{
    public class SelectingEditorWindow: BaseEditorWindow
    {
        private List<Object> _goList;
        private string _msg;

        private Vector2 _ListScroll;

        public static void Show(List<Object> goList, string msg = null)
        {
            if (goList != null && goList.Count > 0)
            {
                Open(goList, msg);
            }
        }

        private static void Open(List<Object> goList, string msg)
        {
            var ew = GetWindow<SelectingEditorWindow>();
            ew.SetData(goList, msg);
        }

        private void SetData(List<Object> goList, string msg)
        {
            _goList = goList;
            _msg = msg;
        }

        protected override void CustomOnGUI()
        {
            if (_msg != null)
            {
                EditorUIHelper.TitleField(_msg);
            }

            EditorUIHelper.Space();
            _ListScroll = EditorUIHelper.BeginScrollView(_ListScroll);
            {
                var count = _goList.Count;
                for (int i = 0; i < count; i++)
                {
                    var go = _goList[i];
                    EditorUIHelper.DrawLine();
                    EditorUIHelper.BeginHorizontal();
                    {
                        EditorUIHelper.ObjectField(string.Empty, go, null, true);
                        EditorUIHelper.Button("选中", () => AssetHelper.SelectObject(go));
                        var index = i;
                        EditorUIHelper.Button("移除", () => RemoveGo(index));
                    }
                    EditorUIHelper.EndHorizontal();
                }
            }
            EditorUIHelper.EndScrollView();
        }

        private void RemoveGo(int index)
        {
            EditorApplication.delayCall += () => _goList.RemoveAt(index);
        }
    }
}