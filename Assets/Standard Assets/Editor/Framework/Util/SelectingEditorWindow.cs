
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class SelectingEditorWindow: BaseEditorWindow
    {
        private List<Object> _goList;
        private string _msg;
        private string _search;
        private EditorAssetHelper.SearchFilter _searchFilter;

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
            var ew = GetWindowExt<SelectingEditorWindow>();
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
                EditorUIHelper.Space();
            }

            _search = EditorUIHelper.SearchCancelTextField(_search);
            _searchFilter = EditorUIHelper.EnumPopup<EditorAssetHelper.SearchFilter>(_searchFilter, "SearchFilter");

            EditorUIHelper.Space();
            _ListScroll = EditorUIHelper.BeginScrollView(_ListScroll);
            {
                var count = _goList != null ? _goList.Count : 0;
                var needSearch = !string.IsNullOrEmpty(_search);
                for (int i = 0; i < count; i++)
                {
                    var go = _goList[i];
                    if (needSearch && !go.name.Contains(_search))
                    {
                        continue;
                    }

                    if (!EditorAssetHelper.IsMatch(go, _searchFilter))
                    {
                        continue;
                    }

                    EditorUIHelper.DrawLine();
                    EditorUIHelper.BeginHorizontal();
                    {
                        EditorUIHelper.ObjectField(go, null, string.Empty, true);
                        EditorUIHelper.Button("选中", () => EditorAssetHelper.SelectObject(go));
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