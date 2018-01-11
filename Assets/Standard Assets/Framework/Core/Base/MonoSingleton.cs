
using UnityEngine;

namespace WWFramework.Core
{
    public abstract class MonoSingleton<T>: MonoBehaviour where T:MonoSingleton<T>
    {
        #region 单例
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<T>();
                    _instance.OnInit();
                }

                return _instance;
            }
        }


        public static bool DisposeInstance()
        {
            if (_instance != null)
            {
                _instance.OnDispose();
                Destroy(_instance.gameObject);

                return true;
            }

            return false;
        }

        #endregion

        #region 辅助

        protected virtual void OnInit()
        {
        }

        protected virtual void OnDispose()
        {

        }
        #endregion
    }
}