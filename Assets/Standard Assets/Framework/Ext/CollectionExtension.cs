using System;
using System.Collections.Generic;

namespace WWFramework.Extension
{
    public static class CollectionExtension
    {
        #region 弃用
        [Obsolete("只是一个模板，必须单独每个写", true)]
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
        public static void ForEach<TKey>(this List<TKey> list, Action<TKey> callback)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                callback(list[i]);
            }
        }


        [Obsolete("Unity 5.6 开始不再有 GC")]
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> callback)
        {
            if (dict != null)
            {
                var enumerator = dict.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    callback(enumerator.Current.Key, enumerator.Current.Value);
                }
                enumerator.Dispose();
            }
        }
        #endregion

        #region IEnumerable
        public static void ForEach<T>(this IEnumerable<T> dataset, Action<T> callback)
        {
            if (dataset == null || callback == null)
            {
                return;
            }

            var enumerator = dataset.GetEnumerator();
            while (enumerator.MoveNext())
            {
                callback(enumerator.Current);
            }
            enumerator.Dispose();
        }

        public static void ForEach<T>(this IEnumerable<T> dataset, Action<int, T> callback)
        {
            if (dataset == null || callback == null)
            {
                return;
            }

            var i = 0;
            var enumerator = dataset.GetEnumerator();
            while (enumerator.MoveNext())
            {
                callback(i, enumerator.Current);
                i++;
            }
            enumerator.Dispose();
        }
        #endregion

        #region List or Array
        public static bool AddIfNotExist<T>(this List<T> list, T item)
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

        public static T SafeGetValue<T>(this List<T> list, int index)
        {
            if (list != null && list.Count > index)
            {
                return list[index];
            }

            return default(T);
        }
        #endregion


        #region Dict
        public static V SafeGetValue<K, V>(this Dictionary<K, V> dict, K key)
        {
            if (dict != null)
            {
                V value;
                dict.TryGetValue(key, out value);
                return value;
            }

            return default(V);
        }

        #endregion
    }
}