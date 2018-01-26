
using System.Collections.Generic;
using UnityEditor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class TextureImporterOperator: BaseAssetImporterOperator<TextureImporter, TextureImporterConfig, TextureImporterSetting>
    {
        protected override void ApplyImportedSetting(TextureImporter importer, TextureImporterSetting setting)
        {
            base.ApplyImportedSetting(importer, setting);

            importer.mipmapEnabled = setting.mipmapEnabled;
        }

        protected override void ShowSettingPanel(TextureImporterSetting setting)
        {
            base.ShowSettingPanel(setting);

            EditorUIHelper.Space();
            setting.mipmapEnabled = EditorUIHelper.Toggle("mipmapEnabled", setting.mipmapEnabled);
        }

        public override List<string> GetAssetPaths(string path = null)
        {
            return EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Texture, path);
        }
    }
}