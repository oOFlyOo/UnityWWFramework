
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.Optimazation.Editor;
using WWFramework.UI.Editor;
using WWFramework.Util;

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
            List<Material> mats = null;
            if (assets != null && assets.Length > 0)
            {
                mats = new List<Material>();
                Material mat;
                foreach (var asset in assets)
                {
                    mat = asset as Material;
                    if (mat != null)
                    {
                        mats.Add(mat);
                    }
                }
            }
            else
            {
                mats = GetProjectMaterials();
            }

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

        private List<Material> GetProjectMaterials()
        {
            return
                EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Material)
                    .ConvertAll(AssetDatabase.LoadAssetAtPath<Material>);
        }
    }
}