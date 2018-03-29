
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WWFramework.Optimization.Editor
{
    public interface IAssetAnalyzer
    {
        void Analyse(Object[] assets);
        void ShowResult();
        void CorrectAll();
    }

    public abstract class BaseAssetAnalyzer<T>: IAssetAnalyzer
    {
        public abstract void Analyse(Object[] assets);


        public virtual void ShowResult()
        {
        }

        public abstract void CorrectAll();

        protected List<T> GetObjects(Object[] assets)
        {
            var objs = assets != null ? GetProjectObjects() : GetFilterObjects(assets);

            return objs.Where(arg1 => arg1 != null).ToList();
        }

        protected abstract List<T> GetFilterObjects(Object[] assets);

        protected abstract List<T> GetProjectObjects();

        protected abstract bool IsSickAsset(T obj, bool needCorrect = false, bool needSave = true);
    }
}