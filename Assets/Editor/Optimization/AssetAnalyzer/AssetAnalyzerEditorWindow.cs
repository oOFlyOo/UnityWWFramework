
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Extension;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class AssetAnalyzerEditorWindow: BaseEditorWindow
    {
        private List<IAssetAnalyzer> _assetAnalyzers;
        private List<IAssetAnalyzer> AssetAnalyzers
        {
            get
            {
                if (_assetAnalyzers == null)
                {
                    _assetAnalyzers = new List<IAssetAnalyzer>();

                    var analyzerType = typeof (IAssetAnalyzer);
                    foreach (var type in analyzerType.Assembly.GetTypes())
                    {
                        if (analyzerType.IsAssignableFrom(type) && analyzerType != type && !type.IsAbstract)
                        {
                            var ins = (IAssetAnalyzer)Activator.CreateInstance(type);
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
                    AssetAnalyzers.ForEach((i, analyzer) =>
                    {
                        _assetAnalyzerStrs[i] = analyzer.GetType().Name;
                    });
                }

                return _assetAnalyzerStrs;
            }
        }

        private int _curAnalyzerIndex;

        private IAssetAnalyzer CurAnalyzer
        {
            get { return AssetAnalyzers[_curAnalyzerIndex]; }
        }

        private Vector2 _analyzerScroll;


        [MenuItem("WWFramework/AssetAnalyzer/Window")]
        private static AssetAnalyzerEditorWindow GetWindow()
        {
            return GetWindowExt<AssetAnalyzerEditorWindow>();
        }


        protected override void CustomOnGUI()
        {
            _curAnalyzerIndex = EditorUIHelper.Popup("选择分析器：", _curAnalyzerIndex, AssetAnalyzerStrs);

            EditorUIHelper.Space();
            EditorUIHelper.Button("搜索", () =>
            {
                CurAnalyzer.Analyse(Selection.GetFiltered(typeof(UnityEngine.Object),SelectionMode.DeepAssets));
            });

            EditorUIHelper.Space();
            EditorUIHelper.Button("修正全部", () =>
            {
                CurAnalyzer.CorrectAll();
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