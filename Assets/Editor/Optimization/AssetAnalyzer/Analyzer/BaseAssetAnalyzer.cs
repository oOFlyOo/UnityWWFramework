
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WWFramework.Optimization.Editor
{
    public abstract class BaseAssetAnalyzer
    {
        public abstract void Analyse(Object[] assets);

        public virtual void ShowResult()
        {
        }


        protected List<T> GetObjects<T>(Object[] assets) where T: Object
        {
            if (assets != null && assets.Length > 1)
            {
                return GetFilterObjects(assets).Select(o => o as T).Where(arg1 => arg1 != null).ToList();
            }
            else
            {
                return GetProjectObjects().ConvertAll(input => (T) input);
            }
        }

        protected abstract List<Object> GetFilterObjects(Object[] assets);

        protected abstract List<Object> GetProjectObjects();
    }
}