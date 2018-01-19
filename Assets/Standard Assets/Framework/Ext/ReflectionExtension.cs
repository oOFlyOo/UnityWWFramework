
using System;
using System.Reflection;

namespace WWFramework.Reflection
{
    public static class ReflectionExtension
    {
        public static Type GetSameAssemblyType(this Type type, string name)
        {
            return type.Assembly.GetType(name);
        }


        public static object InvokeStaticMethod(this Type type, string name, BindingFlags flags = BindingFlags.Default, params object[] parameters)
        {
            return type.GetMethod(name, flags).Invoke(null, parameters);
        }


        /// <summary>
        /// 虽然可以使用 obj.GetType，但是不想扩展 object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object InvokeMethod(this Type type, object obj, string name, BindingFlags flags = BindingFlags.Default,
            params object[] parameters)
        {
            return type.GetMethod(name, flags).Invoke(obj, parameters);
        }
    }
}