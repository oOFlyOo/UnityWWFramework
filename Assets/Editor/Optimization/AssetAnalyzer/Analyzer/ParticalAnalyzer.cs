

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;

namespace WWFramework.Optimization.Editor
{
    public class ParticalAnalyzer:BaseAssetAnalyzer<GameObject>
    {
        public override void Analyse(Object[] assets)
        {
            throw new System.NotImplementedException();
        }

        public override void CorrectAll()
        {
            throw new System.NotImplementedException();
        }

        protected override List<GameObject> GetFilterObjects(Object[] assets)
        {
            throw new System.NotImplementedException();
        }

        protected override List<GameObject> GetProjectObjects()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsSickAsset(GameObject obj, bool needCorrect = false, bool needSave = true)
        {
            throw new System.NotImplementedException();
        }
    }
}