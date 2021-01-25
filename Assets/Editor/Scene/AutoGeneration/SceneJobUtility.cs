using System.Collections;
using System.Collections.Generic;
using WWFramework.DOTS;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WWFramework.Scene.Editor
{
    public class SceneJobUtility
    {
        /// <summary>
        /// 该接口要达到一定量级性能才会更优
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static RaycastHit[] RaycastGrid(Vector3 center, int width, int height, int layerMask = -1,
            int maxHists = 1, int maxHeight = 1000)
        {
            var results = new NativeArray<RaycastHit>(width * height * maxHists, Allocator.TempJob);
            var commands = new NativeArray<RaycastCommand>(width * height, Allocator.TempJob);
            var halfWidth = (width - 1) * 0.5f;
            var halfHeight = (height - 1) * 0.5f;
            var offsetHeight = Vector3.up * maxHeight;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var rayCenter = center - new Vector3(halfWidth, center.y, halfHeight) + new Vector3(i, 0, j);
                    commands[i + j * width] =
                        new RaycastCommand(rayCenter + offsetHeight, Vector3.down, layerMask: layerMask);
                }
            }

            if (maxHists == 1)
            {
                var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
                handle.Complete();
            }
            else
            {
                var handle = new RaycastAllCommand(commands, results, maxHists);
                handle.Schedule(default).Complete();
                handle.Dispose();
            }

            var hits = results.ToArray();
            results.Dispose();
            commands.Dispose();

            return hits;
        }


        [BurstCompile]
        private struct PixelJob : IJobParallelFor
        {
            [ReadOnly] public int Width;
            [ReadOnly] public int Height;

            [ReadOnly] public int MinRange;
            [ReadOnly] public int MaxRange;

            [ReadOnly] public int GenerateLayerMask;

            [ReadOnly] public Color32 NoneColor;
            [ReadOnly] public Color32 HitColor;
            [ReadOnly] public Color32 GenerateColor;

            [ReadOnly] public NativeArray<int> HitLayerMasks;
            [WriteOnly] public NativeArray<Color32> Pixels;

            [ReadOnly] public SceneInfoCalculation.CalculateType CalculateType;

            private bool IsValidHit(int index)
            {
                return HitLayerMasks[index] != 0;
            }

            private bool IsContainsLayer(int index, int layerMask)
            {
                if (!IsValidHit(index))
                {
                    return false;
                }

                var hitMask = HitLayerMasks[index];

                return (hitMask & layerMask) != 0;
            }

            public void Execute(int index)
            {
                var outsideColor = CalculateType == SceneInfoCalculation.CalculateType.Inside
                    ? HitColor
                    : GenerateColor;
                var insideColor = CalculateType == SceneInfoCalculation.CalculateType.Inside ? GenerateColor : HitColor;

                Color32 color;
                if (!IsValidHit(index))
                {
                    color = NoneColor;
                }
                else
                {
                    color = outsideColor;
                    var isInside = IsContainsLayer(index, GenerateLayerMask);

                    if (isInside)
                    {
                        color = insideColor;
                    }

                    // 非同边，必定不生成
                    if ((CalculateType == SceneInfoCalculation.CalculateType.Inside && isInside) ||
                        (CalculateType == SceneInfoCalculation.CalculateType.Outside && !isInside))
                    {
                        // 符合范围为 [MinRange, MaxRange]
                        var isMinGenerate = true;
                        if (MinRange > 0)
                        {
                            var i = index % Width;
                            var j = index / Width;
                            
                            var left = Mathf.Max(0, i - MinRange);
                            var right = Mathf.Min(Width, i + MinRange);
                            var top = Mathf.Max(0, j - MinRange);
                            var down = Mathf.Min(Height, j + MinRange);
                            
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
                                    if (offset.magnitude > MinRange)
                                    {
                                        continue;
                                    }

                                    var checkIndex = k + l * Width;
                                    if (IsValidHit(checkIndex))
                                    {
                                        var checkIsInside =
                                            IsContainsLayer(checkIndex, GenerateLayerMask);

                                        if (isInside != checkIsInside)
                                        {
                                            isMinGenerate = false;

                                            goto CheckMaxRange;
                                        }
                                    }
                                }
                            }
                        }
                        
                        CheckMaxRange:
                        var isMaxGenerate = true;
                        if (isMinGenerate && MaxRange >= 0)
                        {
                            isMaxGenerate = false;
                            var i = index % Width;
                            var j = index / Width;

                            var checkMaxRange = MaxRange + 1;
                            var left = Mathf.Max(0, i - checkMaxRange);
                            var right = Mathf.Min(Width, i + checkMaxRange);
                            var top = Mathf.Max(0, j - checkMaxRange);
                            var down = Mathf.Min(Height, j + checkMaxRange);

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
                                    if (offset.magnitude > checkMaxRange)
                                    {
                                        continue;
                                    }

                                    var checkIndex = k + l * Width;
                                    if (IsValidHit(checkIndex))
                                    {
                                        var checkIsInside =
                                            IsContainsLayer(checkIndex, GenerateLayerMask);

                                        if (isInside != checkIsInside)
                                        {
                                            isMaxGenerate = true;
                                            goto Generate;
                                        }
                                    }
                                }
                            }
                        }

                        Generate:
                        if (isMinGenerate && isMaxGenerate)
                        {
                            color = GenerateColor;
                        }
                        else
                        {
                            color = HitColor;
                        }
                    }
                }

                Pixels[index] = color;
            }
        }

        private static int GetRaycastLayerMask(RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return 0;
            }
            
            return 1 << hit.collider.gameObject.layer;
        }

        public static Color32[] CalculateGenerateTexture(SceneRaycastInfo sceneInfo, SceneInfoCalculation.CalculateType calType, int minRange, int maxRange, int generateLayerMask, Color32 noneColor, Color32 hitColor, Color32 generateColor)
        {
            var hits = sceneInfo.GetTopHits();
            var hitLayerMasks = new NativeArray<int>(hits.Length, Allocator.TempJob);
            for (int i = 0; i < hits.Length; i++)
            {
                hitLayerMasks[i] = GetRaycastLayerMask(hits[i]);
            }
            
            var pixels = new NativeArray<Color32>(hits.Length, Allocator.TempJob);
            
            var job = new PixelJob()
            {
                Width = sceneInfo.Width,
                Height = sceneInfo.Height,
                MinRange = minRange,
                MaxRange = maxRange,
                GenerateLayerMask = generateLayerMask,
                NoneColor = noneColor,
                HitColor = hitColor,
                GenerateColor = generateColor,
                HitLayerMasks = hitLayerMasks,
                Pixels = pixels,
                CalculateType = calType,
            };
            job.Schedule(hits.Length, 16).Complete();

            var texPixels = pixels.ToArray();

            hitLayerMasks.Dispose();
            pixels.Dispose();

            return texPixels;
        }
    }
}