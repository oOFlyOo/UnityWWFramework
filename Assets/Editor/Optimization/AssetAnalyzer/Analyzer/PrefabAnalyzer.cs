using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;

namespace WWFramework.Optimization.Editor
{
    public class PrefabAnalyzer: BaseAssetAnalyzer<GameObject>
    {
        protected override List<GameObject> GetFilterObjects(Object[] assets)
        {
            return assets.ToList().ConvertAll(input => input as GameObject);
        }

        protected override List<GameObject> GetProjectObjects()
        {
            return EditorAssetHelper.FindAssetsPaths(EditorAssetHelper.SearchFilter.Prefab)
                .ConvertAll(input => AssetDatabase.LoadAssetAtPath<GameObject>(input));
        }

        protected override bool IsSickAsset(GameObject obj, bool needCorrect = false, bool needSave = true)
        {
            var correct = false;

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) > 0)
            {
                if (needCorrect)
                {
                    correct = true;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                }
                else
                {
                    return true;
                }
            }
            
            if (correct)
            {
                EditorUtility.SetDirty(obj);
                // AssetDatabase.ForceReserializeAssets(new []
                // {
                //     AssetDatabase.GetAssetPath(obj),
                // });
                
                if (needSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            return false;
        }
    }
}