using System;
using System.Collections.Generic;

namespace WWFramework.Extension
{
    public static class CollectionExtension
    {
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
    }
}