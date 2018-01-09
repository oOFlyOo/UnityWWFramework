
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WWFramework.Editor.Helper
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

        public RecompileScript(MethodInfo method, object param = null)
        {
            TypeName = method.DeclaringType.FullName;
            MethodName = method.Name;
            if (param != null)
            {
                Parameter = param.GetType().FullName + "|" + param;
            }
        }

        public void Execute()
        {
            var type = Type.GetType(TypeName);
            object parameter = null;
            if (!string.IsNullOrEmpty(Parameter))
            {
                var strs = Parameter.Split('|');
                var paramType = Type.GetType(strs[0]);
                parameter = Convert.ChangeType(strs[1], paramType);
            }
            var param = parameter == null ? null : new[] { parameter, };
            var paramTypes = parameter == null ? new Type[0] : new[] { parameter.GetType(), };
            var method = type.GetMethod(MethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            method.Invoke(null, param);
        }
    }
}
