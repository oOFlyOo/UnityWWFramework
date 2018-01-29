
using System;
using System.Reflection;

namespace WWFramework.Reflection
{
    public static class ReflectionExtension
    {
        public const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        public static Type GetSameAssemblyType(this Type type, string name)
        {
            return type.Assembly.GetType(name);
        }


        public static object InvokeStaticMethod(this Type type, string name, BindingFlags flags = DefaultFlags, params object[] parameters)
        {
            return type.GetMethod(name, flags).Invoke(null, parameters);
        }


        /// <summary>
        /// 虽然可以使用 obj.GetType，但是不想扩展 object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <param name="flags"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object InvokeMethod(this Type type, string name, object obj, BindingFlags flags = DefaultFlags, params object[] parameters)
        {
            return type.GetMethod(name, flags).Invoke(obj, parameters);
        }

        public static object GetStaticPropertyValue(this Type type, string name, BindingFlags flags = DefaultFlags)
        {
            return type.GetProperty(name, flags).GetValue(null, null);
        }

        public static void SetPropertyValue(this Type type, string name, object obj, object value, BindingFlags flags = DefaultFlags)
        {
            type.GetProperty(name, flags).SetValue(obj, value, null);
        }

        public static object GetPropertyValue(this Type type, string name, object obj, BindingFlags flags = DefaultFlags)
        {
            return type.GetProperty(name, flags).GetValue(obj, null);
        }

        public static object GetFieldValue(this Type type, string name, object obj, BindingFlags flags = DefaultFlags)
        {
            return type.GetField(name, flags).GetValue(obj);
        }
    }
}