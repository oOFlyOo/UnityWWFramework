using UnityEngine;
using UnityEditor;

namespace WWFramework.Editor.UI
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        protected Vector2 _scrollPos;

        protected new static T GetWindow<T>() where T : BaseEditorWindow
        {
            return GetWindow<T>(typeof(T).Name.Replace("EditorWindow", ""));
        }


        protected virtual void OnGUI()
        {
            StartOnGUI();
            EditorUIHelper.Space();
            CustomOnGUI();
            EditorUIHelper.Space();
            EndOnGUI();
        }


        protected virtual void StartOnGUI()
        {
            _scrollPos = EditorUIHelper.BeginScrollView(_scrollPos);
        }

        protected abstract void CustomOnGUI();

        protected virtual void EndOnGUI()
        {
            EditorUIHelper.EndScrollView();
        }
    }
}