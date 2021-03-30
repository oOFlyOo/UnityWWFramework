
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public class MaterialAnalyzer: BaseAssetAnalyzer<Material>
    {
        protected override List<Material> GetFilterObjects(Object[] assets)
        {
            return assets.ToList().ConvertAll(input => input as Material);
        }

        protected override List<Material> GetProjectObjects()
        {
            return EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Material)
                    .ConvertAll(input => AssetDatabase.LoadAssetAtPath<Material>(input));
        }

        protected override bool IsSickAsset(Material obj, bool needCorrect = false, bool needSave = true)
        {
            var correct = false;

            var matInfo = new SerializedObject(obj);
            var propArray = matInfo.FindProperty("m_SavedProperties");
            propArray.Next(true);
            do
            {
                if (propArray.isArray)
                {
                    for (int i = propArray.arraySize - 1; i >= 0; --i)
                    {
                        var prop =
                            propArray.GetArrayElementAtIndex(i);
                        if (!obj.HasProperty(prop.displayName))
                        {
                            if (needCorrect)
                            {
                                correct = true;
                                propArray.DeleteArrayElementAtIndex(i);
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            } while (propArray.Next(false));
            
            if (correct)
            {
                matInfo.ApplyModifiedProperties();
                matInfo.UpdateIfRequiredOrScript();
                if (needSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            return false;
        }
    }
}