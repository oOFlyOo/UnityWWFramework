
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.Optimization.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Optimaztion.Editor
{
    public class MaterialAnalyzer: BaseAssetAnalyzer
    {
        private class SickMaterial
        {
            public Material Mat;
            public string SickName;
        }

        private List<SickMaterial> _sickMaterials = new List<SickMaterial>();

        public override void Analyse(Object[] assets)
        {
            var mats = GetObjects<Material>(assets);
            _sickMaterials.Clear();

            foreach (var mat in mats)
            {
                var text = File.ReadAllText(AssetDatabase.GetAssetPath(mat));
                var shader = mat.shader;
                var texNames = new HashSet<string>();
                foreach (Match match in Regex.Matches(text, @"(name:|-) (\w+).+?m_Texture", RegexOptions.Singleline))
                {
                    texNames.Add(match.Groups[2].Value);
                }
                for (int i = ShaderUtil.GetPropertyCount(shader) - 1; i >= 0; i--)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        var name = ShaderUtil.GetPropertyName(shader, i);
                        texNames.Remove(name);
                    }
                }
                foreach (var texName in texNames)
                {
                    _sickMaterials.Add(new SickMaterial()
                    {
                        Mat = mat,
                        SickName = texName,
                    });
                }
            }
        }

        public override void ShowResult()
        {
            base.ShowResult();

            foreach (var sickMaterial in _sickMaterials)
            {
                EditorUIHelper.BeginHorizontal();
                {
                    EditorUIHelper.ObjectField(string.Empty, sickMaterial.Mat);
                    EditorUIHelper.LabelField(sickMaterial.SickName);
                }
                EditorUIHelper.EndHorizontal();
            }
        }

        protected override List<Object> GetFilterObjects(Object[] assets)
        {
            return assets.Where(o => o as Material).ToList();
        }

        protected override List<Object> GetProjectObjects()
        {
            return EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Material)
                    .ConvertAll(AssetDatabase.LoadMainAssetAtPath);
        }
    }
}