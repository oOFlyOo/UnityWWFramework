
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.Optimization.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimaztion.Editor
{
    public class MaterialAnalyzer: BaseAssetAnalyzer<Material>
    {
        private List<Material> _sickMaterials = new List<Material>();

        public override void Analyse(Object[] assets)
        {
            var mats = GetObjects(assets);

            _sickMaterials = mats.Where(mat => IsSickAsset(mat)).ToList();
        }

        public override void ShowResult()
        {
            base.ShowResult();

            foreach (var sickMaterial in _sickMaterials)
            {
                EditorUIHelper.BeginHorizontal();
                {
                    EditorUIHelper.ObjectField(sickMaterial);
                    EditorUIHelper.Space();
                    EditorUIHelper.Button("修正", () => 
                    {
                        IsSickAsset(sickMaterial, true);
                    });
                }
                EditorUIHelper.EndHorizontal();
            }
        }

        public override void CorrectAll()
        {
            foreach (var mat in _sickMaterials)
            {
                IsSickAsset(mat, true, false);
            }
            AssetDatabase.SaveAssets();
        }

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
            var propArry = matInfo.FindProperty("m_SavedProperties");
            propArry.Next(true);
            do
            {
                if (!propArry.isArray)
                {
                    continue;
                }

                for (int i = propArry.arraySize - 1; i >= 0; --i)
                {
                    var prop =
                        propArry.GetArrayElementAtIndex(i).FindPropertyRelative("first").FindPropertyRelative("name");
                    if (!obj.HasProperty(prop.stringValue))
                    {
                        if (needCorrect)
                        {
                            correct = true;
                            propArry.DeleteArrayElementAtIndex(i);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            } while (propArry.Next(false));

            if (correct)
            {
                matInfo.ApplyModifiedProperties();
                matInfo.UpdateIfDirtyOrScript();
                if (needSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            return false;
        }
    }
}