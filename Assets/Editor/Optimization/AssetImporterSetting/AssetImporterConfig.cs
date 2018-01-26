
using System;
using System.Collections.Generic;
using UnityEditor;

namespace WWFramework.Optimization.Editor
{
    public static class AssetImporterConfig
    {
        private static Dictionary<Type, AssetImporterOperatorInterface> _operatorDict;

        public static Dictionary<Type, AssetImporterOperatorInterface> OperatorDict
        {
            get
            {
                if (_operatorDict == null)
                {
                    _operatorDict = new Dictionary<Type, AssetImporterOperatorInterface>();
                    var baseType = typeof (AssetImporterOperatorInterface);
                    foreach (var type in baseType.Assembly.GetTypes())
                    {
                        if (baseType.IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            var op = Activator.CreateInstance(type) as AssetImporterOperatorInterface;
                            _operatorDict[op.AssetImporterType] = op;
                        }
                    }
                }
                return _operatorDict;
            }
        }

        public static void ImportedAsset(string path)
        {
            ImportedAsset(AssetImporter.GetAtPath(path));
        }

        public static void ImportedAsset(AssetImporter importer)
        {
            AssetImporterOperatorInterface importerOperator = null;
            if (OperatorDict.TryGetValue(importer.GetType(), out importerOperator))
            {
                importerOperator.ImportedAsset(importer);
            }
        }
    }
}