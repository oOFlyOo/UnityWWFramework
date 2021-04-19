using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WWFramework.UI.Editor
{
    [CustomPropertyDrawer(typeof(ButtonPropertyAttribute))]
    public class ButtonPropertyDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var btnRect = new Rect(position);
            if (GUI.Button(btnRect, string.IsNullOrEmpty(property.stringValue) ? L10n.Tr(property.displayName) : property.stringValue))
            {
                var target = property.serializedObject.targetObject;
                var method = target.GetType().GetMethod(((ButtonPropertyAttribute) attribute).MethodName);

                method.Invoke(target, null);
            }
        }
    }
}