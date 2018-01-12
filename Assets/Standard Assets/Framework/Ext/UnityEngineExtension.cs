
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
    }
}