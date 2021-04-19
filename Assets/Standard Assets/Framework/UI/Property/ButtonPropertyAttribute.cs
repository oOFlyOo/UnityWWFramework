using System;
using UnityEngine;

namespace WWFramework.UI
{
    public class ButtonPropertyAttribute: PropertyAttribute
    {
        public string MethodName;

        public ButtonPropertyAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}