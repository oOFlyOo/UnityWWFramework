

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;

namespace WWFramework.Optimization.Editor
{
    public class ParticalAnalyzer:BaseAssetAnalyzer<GameObject>
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

            var particles = obj.GetComponentsInChildren<ParticleSystemRenderer>(true);
            if (particles.Length > 0)
            {
                foreach (var particle in particles)
                {
                    if (particle.renderMode != ParticleSystemRenderMode.Mesh && particle.mesh != null)
                    {
                        if (needCorrect)
                        {
                            correct = true;
                            particle.mesh = null;
                        }
                        else
                        {
                            return true;
                        }
                    }
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