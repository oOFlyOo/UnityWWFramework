

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;

namespace WWFramework.Optimization.Editor
{
    public class ParticalAnalyzer:BaseAssetAnalyzer
    {
        private class SickPartical
        {
            public ParticleSystem Partical;
        }


        public override void Analyse(Object[] assets)
        {
        }


        public override void ShowResult()
        {
            base.ShowResult();
        }

        protected override List<Object> GetFilterObjects(Object[] assets)
        {
            throw new System.NotImplementedException();
        }

        protected override List<Object> GetProjectObjects()
        {
            return EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Prefab)
                                .ConvertAll(AssetDatabase.LoadMainAssetAtPath);
        }
    }
}