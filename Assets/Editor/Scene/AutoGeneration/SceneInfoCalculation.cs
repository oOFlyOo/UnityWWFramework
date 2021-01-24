﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace WWFramework.Scene.Editor
{
    public class SceneInfoCalculation
    {
        private static readonly Color32 NoneColor = Color.black;
        private static readonly Color32 HitColor = Color.white;
        private static readonly Color32 GenerateColor = Color.green;

        public enum CalculateType
        {
            Inside,
            Outside,
        }

        public static Texture2D GetTexture(SceneRaycastInfo sceneInfo, CalculateType calType, int minRange = -1, int maxRange = -1,
            int generateLayerMask = -1)
        {
            var width = sceneInfo.Width;
            var height = sceneInfo.Height;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

            var pixels = SceneJobUtility.CalculateGenerateTexture(sceneInfo, calType, minRange, maxRange, generateLayerMask, NoneColor, HitColor, GenerateColor);

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }

        public static Texture2D GetTextureWithoutJob(SceneRaycastInfo sceneInfo, CalculateType calType, int range = -1,
            int generateLayerMask = -1)
        {
            var width = sceneInfo.Width;
            var height = sceneInfo.Height;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            var pixels = new Color32[width * height];

            var outsideColor = calType == CalculateType.Inside ? HitColor : GenerateColor;
            var insideColor = calType == CalculateType.Inside ? GenerateColor : HitColor;

            // 这里的计算可以放进job
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var topHit = sceneInfo.GetTopHit(i, j);
                    Color32 color;
                    if (!sceneInfo.IsValidHit(topHit))
                    {
                        color = NoneColor;
                    }
                    else
                    {
                        color = outsideColor;
                        var isInside = sceneInfo.IsContainsLayer(topHit, generateLayerMask);

                        if (isInside)
                        {
                            color = insideColor;
                        }

                        if (range >= 0)
                        {
                            var isGenerate = false;
                            if ((calType == CalculateType.Inside && isInside) ||
                                (calType == CalculateType.Outside && !isInside))
                            {
                                var checkRange = range + 1;
                                var left = Mathf.Max(0, i - checkRange);
                                var right = Mathf.Min(width, i + checkRange);
                                var top = Mathf.Max(0, j - checkRange);
                                var down = Mathf.Min(height, j + checkRange);

                                // 检查四周
                                for (int k = left; k < right; k++)
                                {
                                    for (int l = top; l < down; l++)
                                    {
                                        if (k == i && j == l)
                                        {
                                            continue;
                                        }

                                        var offset = new Vector2(k - i, l - j);
                                        if (offset.magnitude > checkRange)
                                        {
                                            continue;
                                        }

                                        var checkTopHit = sceneInfo.GetTopHit(k, l);
                                        if (sceneInfo.IsValidHit(checkTopHit))
                                        {
                                            var checkIsInside =
                                                sceneInfo.IsContainsLayer(checkTopHit, generateLayerMask);

                                            if (isInside != checkIsInside)
                                            {
                                                isGenerate = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (isGenerate)
                            {
                                color = GenerateColor;
                            }
                            else
                            {
                                color = HitColor;
                            }
                        }
                    }

                    pixels[i + j * width] = color;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }


        public static void GenerateBySceneInfo(ScenePrefab scenePrefab, Transform parent, Texture2D tex,
            SceneRaycastInfo sceneInfo)
        {
            var hits = sceneInfo.GetTopHits();

            Generate(scenePrefab, parent, tex, hits);
            // GenerateByTexture(scenePrefab, parent, tex, sceneInfo.Center);
        }

        public static void GenerateByTexture(ScenePrefab scenePrefab, Transform parent, Texture2D tex, Vector3 center)
        {
            var hits = SceneJobUtility.RaycastGrid(center, tex.width, tex.height);

            Generate(scenePrefab, parent, tex, hits);
        }

        public static void Generate(ScenePrefab scenePrefab, Transform parent, Texture2D tex, RaycastHit[] hits)
        {
            var prefab = scenePrefab.Prefab;
            var space = scenePrefab.Space;

            var pixels = tex.GetPixels32();
            var generatePoses = new List<RaycastHit>();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!pixels[i].Equals(GenerateColor))
                {
                    continue;
                }

                generatePoses.Add(hits[i]);
            }

            var generateNum = Mathf.CeilToInt(1f * generatePoses.Count / space);
            if (generateNum == 0)
            {
                return;
            }

            for (int i = 0; i < generateNum; i++)
            {
                var pos = Random.Range(0, generatePoses.Count - 1);
                var hit = generatePoses[pos];

                Object.Instantiate(prefab, hit.point, Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up), parent);

                generatePoses.RemoveAt(pos);
            }
        }
    }
}