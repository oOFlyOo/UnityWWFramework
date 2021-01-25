
using UnityEditor;

namespace WWFramework.Optimization.Editor
{
    public class AssetImporterSettingPostprocessor//: AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                AssetImporterConfig.ImportedAsset(asset);
            }
        }
    }
}