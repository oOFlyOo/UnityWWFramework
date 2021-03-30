
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.UI.Editor;

namespace WWFramework.Optimization.Editor
{
    public interface IAssetAnalyzer
    {
        void Analyse(Object[] assets);
        void ShowResult();
        void CorrectAll();
    }

    public abstract class BaseAssetAnalyzer<T>: IAssetAnalyzer where T:  Object
    {
        protected List<T> _sickAssets = new List<T>();

        public virtual void Analyse(Object[] assets)
        {
            var objs = GetObjects(assets);

            _sickAssets = objs.Where(obj => IsSickAsset(obj)).ToList();
        }


        public virtual void ShowResult()
        {
            foreach (var sickAsset in _sickAssets)
            {
                EditorUIHelper.BeginHorizontal();
                {
                    EditorUIHelper.ObjectField(sickAsset);
                    EditorUIHelper.Space();
                    EditorUIHelper.Button("修正", () => 
                    {
                        IsSickAsset(sickAsset, true);
                    });
                }
                EditorUIHelper.EndHorizontal();
            }
        }

        public virtual void CorrectAll()
        {
            foreach (var sickAsset in _sickAssets)
            {
                IsSickAsset(sickAsset, true, false);
            }
            AssetDatabase.SaveAssets();
        }

        protected List<T> GetObjects(Object[] assets)
        {
            var objs = assets == null ? GetProjectObjects() : GetFilterObjects(assets);

            return objs.Where(arg1 => arg1 != null).ToList();
        }

        protected abstract List<T> GetFilterObjects(Object[] assets);

        protected abstract List<T> GetProjectObjects();

        protected abstract bool IsSickAsset(T obj, bool needCorrect = false, bool needSave = true);
    }
}