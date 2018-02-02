
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Extension;
using WWFramework.UI.Editor;

namespace WWFramework.Optimazation.Editor
{
    public class AssetAnalyzerEditorWindow: BaseEditorWindow
    {
        private List<BaseAssetAnalyzer> _assetAnalyzers;
        private List<BaseAssetAnalyzer> AssetAnalyzers
        {
            get
            {
                if (_assetAnalyzers == null)
                {
                    _assetAnalyzers = new List<BaseAssetAnalyzer>();

                    var analyzerType = typeof (BaseAssetAnalyzer);
                    foreach (var type in analyzerType.Assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(analyzerType))
                        {
                            var ins = (BaseAssetAnalyzer)Activator.CreateInstance(type);
                            _assetAnalyzers.Add(ins);
                        }
                    }
                }

                return _assetAnalyzers;
            }
        }

        private string[] _assetAnalyzerStrs;
        private string[] AssetAnalyzerStrs
        {
            get
            {
                if (_assetAnalyzerStrs == null)
                {
                    _assetAnalyzerStrs = new string[AssetAnalyzers.Count];
                    AssetAnalyzers.Foreach((i, analyzer) =>
                    {
                        _assetAnalyzerStrs[i] = analyzer.GetType().Name;
                    });
                }

                return _assetAnalyzerStrs;
            }
        }

        private int _curAnalyzerIndex;

        private BaseAssetAnalyzer CurAnalyzer
        {
            get { return AssetAnalyzers[_curAnalyzerIndex]; }
        }

        private Vector2 _analyzerScroll;


        [MenuItem("WWFramework/AssetAnalyzer/Window")]
        private static AssetAnalyzerEditorWindow GetWindow()
        {
            return GetWindow<AssetAnalyzerEditorWindow>();
        }


        protected override void CustomOnGUI()
        {
            _curAnalyzerIndex = EditorUIHelper.Popup("选择分析器：", _curAnalyzerIndex, AssetAnalyzerStrs);

            EditorUIHelper.Space();
            EditorUIHelper.Button("执行", () =>
            {
                CurAnalyzer.Analyse(Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets));
            });

            EditorUIHelper.Space();
            _analyzerScroll = EditorUIHelper.BeginScrollView(_analyzerScroll);
            {
                CurAnalyzer.ShowResult();
            }
            EditorUIHelper.EndScrollView();
        }
    }
}