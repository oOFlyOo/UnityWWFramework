using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;

namespace WWFramework.Util.Editor
{
    public class GameObjectEditorWindow : BaseEditorWindow
    {
        private Transform _srcTrans;
        private Transform _dstTrans;
        
        [MenuItem("WWFramework/GameObject/Window")]
        private static GameObjectEditorWindow GetWindow()
        {
            return GetWindowExt<GameObjectEditorWindow>();
        }

        protected override void CustomOnGUI()
        {
            EditorUIHelper.Space();
            _srcTrans = EditorUIHelper.ObjectField<Transform>(_srcTrans, "来源对象", true);
            _dstTrans = EditorUIHelper.ObjectField<Transform>(_dstTrans, "目标对象", true);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("替换Components", ReplaceComponents);
        }

        private void ReplaceComponents()
        {
            EditorHelper.ReplaceComponents(_srcTrans, _dstTrans);
        }
    }
}