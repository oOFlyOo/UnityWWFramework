using System;
using System.Collections;
using System.Collections.Generic;

namespace WWFramework.Extension
{
    public static class CollectionExtension
    {
        #region 弃用
        [Obsolete("只是一个模板，必须单独每个写")]
        public sealed class EnumComparer<T> : IEqualityComparer<T> where T : IComparable, IConvertible
        {
            private static EnumComparer<T> _instance;

            public static EnumComparer<T> Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new EnumComparer<T>();
                    }
                    return _instance;
                }
            }

            private EnumComparer()
            {

            }

            public bool Equals(T x, T y)
            {
                return x.CompareTo(y) == 0;
            }

            public int GetHashCode(T obj)
            {
                return obj.ToInt32(null);
            }
        }

        [Obsolete("Unity 5.6 开始不再有 GC")]
        public static void Foreach<TKey>(this List<TKey> list, Action<TKey> callback)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                callback(list[i]);
            }
        }


        [Obsolete("Unity 5.6 开始不再有 GC")]
        public static void Foreach<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> callback)
        {
            if (dict != null)
            {
                var enumerator = dict.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    callback(enumerator.Current.Key, enumerator.Current.Value);
                }
            }
        }
        #endregion

        #region list
        public static bool AddIfWithout<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);

                return true;
            }

            return false;
        }

        public static T[] ParamsFixing<T>(this T[] array)
        {
            if (array != null && array.Length == 1 && array[0] == null)
            {
                array = null;
            }

            return array;
        }
        #endregion
    }
}