
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WWFramework.Helper.Editor
{
    public class RecompileScriptableObject : ScriptableObject
    {
        public List<RecompileScript> ReloadScriptList = new List<RecompileScript>();
    }

    [Serializable]
    public class RecompileScript
    {
        public string TypeName;
        public string MethodName;
        public string Parameter;
        public bool Executing;

    public RecompileScript(MethodInfo method, params object[] args)
    {
        TypeName = method.DeclaringType.FullName;
        MethodName = method.Name;
        if (args.Length > 0)
        {
            var sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.AppendFormat("{0}|{1},", arg.GetType().FullName, arg);
            }
            Parameter = sb.ToString().TrimEnd(',');
        }
    }

    public void Execute()
    {
        if (!Executing)
        {
            var type = Type.GetType(TypeName);
            object[] param = null;
            Type[] paramTypes = null;
            if (!string.IsNullOrEmpty(Parameter))
            {
                var args = Parameter.Split(',');
                param = new object[args.Length];
                paramTypes = new Type[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    var strs = arg.Split('|');

                    var paramType = Type.GetType(strs[0]);
                    var pa = Convert.ChangeType(strs[1], paramType);

                    param[i] = pa;
                    paramTypes[i] = paramType;
                }
            }
            else
            {
                paramTypes = new Type[0];
            }
            var method = type.GetMethod(MethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            method.Invoke(null, param);

            Executing = true;
            }
        }
    }
}