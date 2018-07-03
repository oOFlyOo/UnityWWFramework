using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class ModelOptimizeEditorWindow : BaseEditorWindow
    {
        private enum ModelType
        {
            Selection,
            Fashion,
            Pet,
            Weapon,
            Soul,
            Other,
        }
        private ModelType _curModelType = ModelType.Selection;

        private List<ModelImporter> _modelImporters = new List<ModelImporter>();
        private List<Object> _models = new List<Object>();
        private Vector2 _scroll;
        private string _search;


        [MenuItem("WWFramework/CrossDomainDelegate/Window")]
        private static CrossDomainDelegateEditorWindow GetWindow()
        {
            return GetWindowExt<CrossDomainDelegateEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.BeginHorizontal();
            {
                _curModelType = EditorUIHelper.EnumPopup<ModelType>(_curModelType);
                if (GUI.changed)
                {
                    Refresh();
                }

                EditorUIHelper.Button("刷新", Refresh);
            }
            EditorUIHelper.EndHorizontal();

            EditorUIHelper.Space();
            _search = EditorUIHelper.SearchCancelTextField(_search);

            EditorUIHelper.Space();
            _scroll = EditorUIHelper.BeginScrollView(_scroll);
            {
                for (int i = 0; i < _models.Count; i++)
                {
                    var importer = _modelImporters[i];
                    var model = _models[i];

                    if (!string.IsNullOrEmpty(_search))
                    {
                        if (!model.name.Contains(_search))
                        {
                            continue;
                        }
                    }

                    EditorUIHelper.DrawLine();
                    EditorUIHelper.BeginHorizontal();
                    {
                        EditorUIHelper.ObjectField(model);
                        EditorUIHelper.Toggle("绑点优化状态",
                            importer.animationType == ModelImporterAnimationType.Generic &&
                            importer.optimizeGameObjects);
                        EditorUIHelper.Button("优化绑点", () => EditorHelper.Run(() => ModelOptimizeHelper.OptimizeGameObject(importer)));
                        EditorUIHelper.Button("还原绑点", () => EditorHelper.Run(() => ModelOptimizeHelper.RevertOptimizeGameObject(importer)));
                    }
                    EditorUIHelper.EndHorizontal();

                }
            }
            EditorUIHelper.EndScrollView();
        }

        private void Refresh()
        {
            switch (_curModelType)
            {
                case ModelType.Selection:
                    {
                        UpdateSelectionModels();
                        break;
                    }
                default:
                    {
                        UpdateDefaultModels();
                        break;
                    }
            }

            // 不能优化的玩毛啊
            _modelImporters.RemoveAll(importer => importer.animationType != ModelImporterAnimationType.Generic);
            _modelImporters.Sort((model1, model2) => -File.GetLastWriteTimeUtc(model1.assetPath)
                .CompareTo(File.GetLastWriteTimeUtc(model2.assetPath)));
            _models = _modelImporters.ConvertAll(mode => AssetDatabase.LoadMainAssetAtPath(mode.assetPath));
        }

        private void UpdateSelectionModels()
        {
            _modelImporters.Clear();

            var guids = Selection.assetGUIDs;
            var assetPaths = new List<string>();
            var folderPaths = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    folderPaths.Add(path);
                }
                else
                {
                    assetPaths.Add(path);
                }
            }
            if (folderPaths.Count > 0)
            {
                var modelGuids = AssetDatabase.FindAssets("t:Model", folderPaths.ToArray());
                _modelImporters = modelGuids.Select(guid => (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid))).ToList();
            }

            foreach (var assetPath in assetPaths)
            {
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer != null)
                {
                    _modelImporters.Add(importer);
                }
            }
        }

        private void UpdateDefaultModels()
        {
            string path = null;
            switch (_curModelType)
            {
                case ModelType.Fashion:
                {
                    path = ModelOptimizeHelper.FashionPath;
                    break;
                }
                case ModelType.Pet:
                {
                    path = ModelOptimizeHelper.PetPath;
                    break;
                }
                case ModelType.Weapon:
                {
                    path = ModelOptimizeHelper.WeaponPath;
                    break;
                }

                case ModelType.Soul:
                {
                    path = ModelOptimizeHelper.SoulPath;
                    break;
                }
                case ModelType.Other:
                {
                    path = ModelOptimizeHelper.OtherPath;
                    break;
                }
                default:
                {
                    _modelImporters.Clear();
                    return;
                }
            }

            var modelGuids = AssetDatabase.FindAssets("t:Model", new []{path});
            _modelImporters = modelGuids.Select(guid => (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }
    }
}