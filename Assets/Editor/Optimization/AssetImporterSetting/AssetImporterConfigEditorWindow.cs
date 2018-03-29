
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class AssetImporterConfigEditorWindow : BaseEditorWindow
    {
        private Type[] _assetImporterTypes;
        private Type[] AssetImporterTypes
        {
            get
            {
                if (_assetImporterTypes == null)
                {
                    var assetImporterTypes = AssetImporterConfig.OperatorDict.Keys;
                    var index = 0;
                    _assetImporterTypes = new Type[assetImporterTypes.Count + 1];
                    _assetImporterTypes[index] = null;

                    foreach (var importerType in assetImporterTypes)
                    {
                        index++;
                        _assetImporterTypes[index] = importerType;
                    }
                }

                return _assetImporterTypes;
            }
        }
        private string[] _assetImporterTypeStrs;
        private string[] AssetImporterTypeStrs
        {
            get
            {
                if (_assetImporterTypeStrs == null)
                {
                    var assetImporterTypes = AssetImporterTypes;
                    var index = 0;
                    _assetImporterTypeStrs = new string[assetImporterTypes.Length];
                    _assetImporterTypeStrs[index] = "None";

                    for (int i = index + 1; i < assetImporterTypes.Length; i++)
                    {
                        _assetImporterTypeStrs[i] = assetImporterTypes[i].Name.Replace("Importer", "");
                    }
                }

                return _assetImporterTypeStrs;
            }
        }
        private int _selectedIndex;

        private string[] _showTypes =
        {
            "配置",
            "文件",
        };
        private int _showTypeIndex = 0;

        private Type SelectedAssetImporterType
        {
            get { return AssetImporterTypes[_selectedIndex]; }
        }

        private AssetImporterOperatorInterface Operator
        {
            get
            {
                var type = SelectedAssetImporterType;
                if (type != null)
                {
                    return AssetImporterConfig.OperatorDict[type];
                }

                return null;
            }
        }

        private Vector2 _showTypeScroll;
        private string _searchText;
        private List<string> _assetPaths;
        private int _selectedObjIndex;

        [MenuItem("WWFramework/AssetImporterConfig/Window")]
        private static AssetImporterConfigEditorWindow GetWindow()
        {
            return GetWindowExt<AssetImporterConfigEditorWindow>();
        }


        protected override void CustomOnGUI()
        {
            EditorUIHelper.BeginChangeCheck();
            {
                _selectedIndex = EditorUIHelper.Popup("类型：", _selectedIndex, AssetImporterTypeStrs);
            }
            if (EditorUIHelper.EndChangeCheck())
            {
                ClearData();
            }

            var op = Operator;
            if (op != null)
            {
                EditorUIHelper.Space();
                EditorUIHelper.BeginHorizontal();
                {
                    _showTypeIndex = EditorUIHelper.Toolbar(_showTypeIndex, _showTypes);
                }
                EditorUIHelper.EndHorizontal();

                EditorUIHelper.Space();
                EditorUIHelper.BeginChangeCheck();
                {
                    _searchText = EditorUIHelper.SearchCancelTextField(_searchText);
                }
                if (EditorUIHelper.EndChangeCheck())
                {
                    ClearData();
                }

                EditorUIHelper.Space();
                _showTypeScroll = EditorUIHelper.BeginScrollView(_showTypeScroll);
                {
                    ShowTypePanel();
                }
                EditorUIHelper.EndScrollView();
            }
        }

        private void ShowTypePanel()
        {
            switch (_showTypeIndex)
            {
                case 0:
                    {
                        ShowSettings();
                        break;
                    }
                case 1:
                    {
                        ShowAssets();
                        break;
                    }
            }
        }

        private void ShowSettings()
        {
        }

        private void ShowAssets()
        {
            if (_assetPaths == null)
            {
                _assetPaths = Operator.GetAssetPaths();
                if (!string.IsNullOrEmpty(_searchText))
                {
                    _assetPaths = _assetPaths.Where(s => s.Contains(_searchText)).ToList();
                }
            }

            var count = _assetPaths.Count;
            for (int i = 0; i < count; i++)
            {
                EditorUIHelper.Space(0.1f);
                EditorUIHelper.BeginSelectdColor(_selectedObjIndex == i);
                {
                    EditorUIHelper.BeginHorizontal(EditorUIHelper.TextAreaStyle);
                    {
                        var path = _assetPaths[i];

                        var i1 = i;
                        EditorUIHelper.Button(path, () =>
                        {
                            EditorAssetHelper.SelectObject(AssetDatabase.LoadMainAssetAtPath(path));
                            _selectedObjIndex = i1;
                        }, EditorUIHelper.TextFieldStyle);
                    }
                    EditorUIHelper.EndHorizontal();
                }
                EditorUIHelper.EndSelectedColor();

            }

            if (_selectedObjIndex >= 0)
            {
                ShowSettingPanel(Operator.GetImporterSetting(_assetPaths[_selectedObjIndex]));
            }
        }

        private void ShowSettingPanel(BaseImporterSetting setting)
        {
            if (setting != null)
            {
                EditorUIHelper.Space();
                Operator.ShowSettingPanel(setting);
            }
        }

        private void ClearData()
        {
            _assetPaths = null;
            _selectedObjIndex = -1;
        }
    }
}