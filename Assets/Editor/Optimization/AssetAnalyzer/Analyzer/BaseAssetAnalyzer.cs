
using UnityEngine;

namespace WWFramework.Optimazation.Editor
{
    public abstract class BaseAssetAnalyzer
    {
        public abstract void Analyse(Object[] assets);
        public virtual void ShowResult()
        {
            
        }
    }
}