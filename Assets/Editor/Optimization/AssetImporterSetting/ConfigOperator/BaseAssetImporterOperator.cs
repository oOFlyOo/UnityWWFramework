
using System;
using System.Collections.Generic;
using UnityEditor;
using WWFramework.Extension.Editor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public interface AssetImporterOperatorInterface
    {
        Type AssetImporterType { get; }

        BaseImporterSetting GetImporterSetting(string assetPath);
        void AddImporterSetting(BaseImporterSetting setting);
        void RemoveImporterSetting(BaseImporterSetting setting);
        void ImportedAsset(AssetImporter importer);
        void ShowSettingPanel(BaseImporterSetting setting);

        List<string> GetAssetPaths(string path = null);
    }

    public abstract class BaseAssetImporterOperator<T, U, V>: AssetImporterOperatorInterface where T : AssetImporter where U : BaseImporterConfig<V> where V: BaseImporterSetting
    {
        protected U _importerConfig;
        protected U ImporterConfig
        {
            get
            {
                if (_importerConfig == null)
                {
                    var monoScript = EditorAssetHelper.FindScriptableObject(typeof (U));
                    var path = monoScript.GetScriptableObjectPathByMonoScript();
                    _importerConfig = AssetDatabase.LoadAssetAtPath<U>(path);

                    if (_importerConfig == null)
                    {
                        _importerConfig = EditorAssetHelper.CreateScriptableObjectAsset<U>(path);
                    }
                }

                return _importerConfig;
            }
        }

        protected List<V> ImporterSettings
        {
            get { return ImporterConfig.Settings; }
        }

        public Type AssetImporterType
        {
            get { return typeof(T); }
        }

        public BaseImporterSetting GetImporterSetting(string assetPath)
        {
            foreach (var setting in ImporterConfig.Settings)
            {
                if (assetPath.Contains(setting.Path))
                {
                    return setting;
                }
            }

            return null;
        }

        public void AddImporterSetting(BaseImporterSetting setting)
        {
            var count = ImporterSettings.Count;
            for (int i = 0; i < count; i++)
            {
                if (ImporterSettings[i].Match == setting.Match)
                {
                    ImporterSettings.Insert(i, setting as V);
                }
            }

            if (count == ImporterSettings.Count)
            {
                ImporterSettings.Add(setting as V);
            }
        }

        public void RemoveImporterSetting(BaseImporterSetting setting)
        {
            ImporterSettings.Remove(setting as V);
        }

        public void ImportedAsset(AssetImporter importer)
        {
            var setting = GetImporterSetting(importer.assetPath);
            if (setting != null)
            {
                ApplyImportedSetting(importer as T, setting as V);
            }
        }

        protected virtual void ApplyImportedSetting(T importer, V setting)
        {
            
        }

        public void ShowSettingPanel(BaseImporterSetting setting)
        {
            ShowSettingPanel(setting as V);
        }

        protected virtual void ShowSettingPanel(V setting)
        {
            setting.Match = EditorUIHelper.EnumPopup<MatchType>(setting.Match, "Match");
            setting.Path = EditorUIHelper.TextField("Path", setting.Path);
        }

        public abstract List<string> GetAssetPaths(string path = null);
    }
}