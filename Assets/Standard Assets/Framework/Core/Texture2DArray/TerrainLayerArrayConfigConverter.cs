using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WWFramework.Core;

public class TerrainLayerArrayConfigConverter : MonoBehaviour
{
    public Material DefaultMat;
    public Material ArrayMat;

    public TerrainLayerArrayConfig Config;

    [ContextMenu("Convert")]
    private void Convert()
    {
        var terrain = GetComponent<Terrain>();
        terrain.materialTemplate = ArrayMat;

        var arrayConverter = GetComponent<TerrainLayerArrayConverter>();
        arrayConverter.InitConverter(terrain, Config);
        arrayConverter.Convert();
    }

    [ContextMenu("Restore")]
    private void Restore()
    {
        var terrain = GetComponent<Terrain>();
        terrain.materialTemplate = DefaultMat;
    }
}
