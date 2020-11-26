
using System;
using UnityEngine;

namespace WWFramework.Extension
{
    public static class UnityEngineExtension
    {
        public static T GetMissingComponent<T>(this GameObject go) where T : Component
        {
            var com = go.GetComponent<T>();
            if (com == null)
            {
                com = go.AddComponent<T>();
            }

            return com;
        }

        public static string GetHierarchyPath(this Transform trans, Transform root = null, bool includeRoot = true)
        {
            if (trans == null || (trans == root && !includeRoot))
            {
                return String.Empty;
            }

            if (trans != root && trans.parent != null)
            {
                var hierarchy = trans.parent.GetHierarchyPath(root, includeRoot);
                if (string.IsNullOrEmpty(hierarchy))
                {
                    return trans.name;
                }
                else
                {
                    return $"{hierarchy}/{trans.name}";
                }
            }

            return trans.name;
        }

        public static bool IsParentOrSelf(this Transform root, Transform trans)
        {
            while (trans != null)
            {
                if (root == trans)
                {
                    return true;
                }

                trans = trans.parent;
            }

            return false;
        }
    }
}
