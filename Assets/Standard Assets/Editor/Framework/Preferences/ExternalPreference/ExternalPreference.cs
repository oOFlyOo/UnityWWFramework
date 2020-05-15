
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WWFramework.Extension.Editor;
using WWFramework.Helper.Editor;
using WWFramework.UI.Editor;
using WWFramework.Util;

namespace WWFramework.Preference.Editor
{
    public static class ExternalPreference
    {
        private static ExternalScriptableObject _externalSo;

        private static ExternalScriptableObject ExternalSo
        {
            get
            {
                if (_externalSo == null)
                {
                    _externalSo = LoadScriptableObject();
                }
                return _externalSo;
            }
        }

        private static string ExternalSoPath
        {
            get
            {
                return
                    EditorAssetHelper.FindScriptableObjectQuickly(typeof(ExternalScriptableObject))
                        .GetScriptableObjectPathByMonoScript();
            }
        }

        private static Vector2 _scroll;

        private static ExternalScriptableObject LoadScriptableObject()
        {
            var so = AssetDatabase.LoadAssetAtPath<ExternalScriptableObject>(ExternalSoPath);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<ExternalScriptableObject>();
                so.ToolList = new List<ExternalScriptableObject.ExternalTool>();
            }
            else
            {
                so = Object.Instantiate(so);
            }

            return so;
        }

        private static void SaveScriptableObject()
        {
            AssetDatabase.CreateAsset(Object.Instantiate(ExternalSo), ExternalSoPath);
        }


        [SettingsProvider]
        private static SettingsProvider PreferenceGUI()
        {
            return new SettingsProvider("Preferences/External", SettingsScope.User)
            {
                guiHandler = searchContext => PreferenceOnGUI()
            };
        }


        private static void PreferenceOnGUI()
        {
            EditorUIHelper.BeginHorizontal();
            {
                EditorUIHelper.Button("读取", () =>
                {
                    _externalSo = null;
                });

                EditorUIHelper.Button("保存", SaveScriptableObject);

                EditorUIHelper.Button("增加", () =>
                {
                    ExternalSo.ToolList.Add(new ExternalScriptableObject.ExternalTool());
                });
            }
            EditorUIHelper.EndHorizontal();

            _scroll = EditorUIHelper.BeginScrollView(_scroll);
            {
                foreach (var externalTool in ExternalSo.ToolList)
                {
                    EditorUIHelper.Space();
                    externalTool.ToolPath = EditorUIHelper.TextField("路径：", externalTool.ToolPath);

                    externalTool.Extension = EditorUIHelper.TextField("后缀：", externalTool.Extension);

                    var tool = externalTool;
                    EditorUIHelper.Button("删", () =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            ExternalSo.ToolList.Remove(tool);
                        };
                    });
                }
            }
            EditorUIHelper.EndScrollView();
        }


        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceId);

            foreach (var externalTool in ExternalSo.ToolList)
            {
                if (string.IsNullOrEmpty(externalTool.Extension))
                {
                    continue;
                }

                if (path.EndsWith(externalTool.Extension, true, null))
                {
                    if (!File.Exists(externalTool.ToolPath))
                    {
                        EditorUtility.OpenWithDefaultApp(path);
                    }
                    else
                    {
                        var process = ExecutableProcess.CreateProcess(externalTool.ToolPath, false);
                        process.AppendPath(path);
                        process.Start();
//                        Process.Start(externalTool.ToolPath, path);
                    }
                    return true;
                }
            }

            return false;
        }
    }
}