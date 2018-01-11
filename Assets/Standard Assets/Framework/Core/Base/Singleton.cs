
namespace WWFramework.Core
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        #region 单例
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.OnInit();
                }

                return _instance;
            }
        }


        protected Singleton()
        {

        }

        public static bool DisposeInstance()
        {
            if (_instance != null)
            {
                _instance.OnDispose();
                _instance = null;

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