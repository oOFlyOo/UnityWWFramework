using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WWFramework.Helper;
using WWFramework.UI.Editor;


namespace WWFramework.Util.Editor
{
    public class CubemapEditorWindow : BaseEditorWindow
    {
        private Camera _camera;
        private Cubemap _cubemap;
        private ReflectionProbe _probe;
        
        [MenuItem("WWFramework/Cubemap/Window")]
        private static CubemapEditorWindow GetWindow()
        {
            return GetWindowExt<CubemapEditorWindow>();
        }
        
        protected override void CustomOnGUI()
        {
            EditorUIHelper.TextArea("还有一种方式就是创建一个Reflection Probe，然后利用Lighting Bake出来一个Cubemap");            
            
            EditorUIHelper.Space();
            _camera = EditorUIHelper.ObjectField<Camera>(_camera, "摄像机", true);
            _cubemap = EditorUIHelper.ObjectField<Cubemap>(_cubemap, "Cubemap", true);
            _probe = EditorUIHelper.ObjectField<ReflectionProbe>(_probe, "ReflectionProbe", true);
            
            EditorUIHelper.Space();
            EditorUIHelper.Button("生成Legacy", GenerateLegacyCubemap);
            EditorUIHelper.Button("生成", GenerateCubemap);
        }

        private void GenerateLegacyCubemap()
        {
            var camera = _camera ? _camera : SceneView.lastActiveSceneView.camera;
            var cubemap = _cubemap ? _cubemap : new Cubemap(128, TextureFormat.RGBAHalf, false);
            camera.RenderToCubemap(cubemap);
            
            if (!_cubemap)
                AssetDatabase.CreateAsset(cubemap, EditorUtility.SaveFilePanelInProject("保存 Cubemap", "Cubemap.cubemap", "cubemap", ""));
        }
        
        private void GenerateCubemap()
        {
            var camera = _camera ? _camera : SceneView.lastActiveSceneView.camera;
            var cubemap = _cubemap ? _cubemap : new Cubemap(128, TextureFormat.RGBAHalf, false);
            camera.RenderToCubemap(cubemap);

            var tex = TextureHelper.ConvertCubemap2Texture2D(cubemap);
            // File.WriteAllBytes(EditorUtility.SaveFilePanelInProject("保存 Cubemap", "Cubemap.exr", "exr", ""), tex.EncodeToEXR());
            File.WriteAllBytes(EditorUtility.SaveFilePanelInProject("保存 Cubemap", "Cubemap.png", "png", ""), tex.EncodeToPNG());
        }
    }
}

