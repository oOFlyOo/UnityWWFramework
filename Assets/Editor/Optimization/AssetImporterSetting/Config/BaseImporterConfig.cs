using System.Collections.Generic;
using UnityEngine;

namespace WWFramework.Optimization.Editor
{
    public enum MatchType
    {
        File,
        Folder,
        Root,
    }

    public abstract class BaseImporterConfig<T>: ScriptableObject where T: BaseImporterSetting
    {
        public List<T> Settings = new List<T>();
    }

    [System.Serializable]
    public abstract class BaseImporterSetting
    {
        public MatchType Match;
        public string Path;
    }
}