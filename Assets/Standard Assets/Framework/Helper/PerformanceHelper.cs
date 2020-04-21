

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR || UNITY_IPHONE
using UnityEngine.iOS;
#endif

namespace WWFramework.Helper
{
    /// <summary>
    /// 用于计算客户端性能
    /// </summary>
    public static class PerformanceHelper
    {
        #region Application
        public static NetworkReachability Network
        {
            get
            {
                return Application.internetReachability;
            }
        }
        #endregion

        #region SystemInfo
        public static float BatteryLevel
        {
            get { return SystemInfo.batteryLevel; }
        }

        public static BatteryStatus BatteryStatus
        {
            get { return SystemInfo.batteryStatus; }
        }

        public static int SystemMemorySize
        {
            get { return SystemInfo.systemMemorySize; }
        }

        public static int GraphicsMemorySize
        {
            get { return SystemInfo.graphicsMemorySize; }
        }

        public static GraphicsDeviceType GraphicsDeviceType
        {
            get { return SystemInfo.graphicsDeviceType; }
        }

        public static int GraphicsShaderLevel
        {
            get { return SystemInfo.graphicsShaderLevel; }
        }
        #endregion

        public enum PerformanceType
        {
            Unknow,
            Low,
            Middle,
            High,
        }
        private static PerformanceType _curPerformanceType = PerformanceType.Unknow;
        public static PerformanceType CurPerformanceType
        {
            get
            {
                if (_curPerformanceType == PerformanceType.Unknow)
                {
                    _curPerformanceType = CalculatePerformanceType();
                }

                return _curPerformanceType;
            }
        }

        private static Dictionary<Func<int>, float> ScoreDict = new Dictionary<Func<int>, float>()
        {
            {GetMemorySizeScore, 0.4f},
            {GetShaderLevelScore, 0.2f},
            {GetProcessorFrequencyScore, 0.4f},
        };

        private static PerformanceType CalculatePerformanceType()
        {
            var score = 0f;
            foreach (var keyValuePair in ScoreDict)
            {
                score += keyValuePair.Key() * keyValuePair.Value;
            }

            if (score > 80)
            {
                return PerformanceType.High;
            }
            else if (score > 30)
            {
                return PerformanceType.Middle;
            }

            return PerformanceType.Low;
        }

        private static int GetMemorySizeScore()
        {
            var size = SystemMemorySize;
            if (size >= 4086)
            {
                return 100;
            }
            else if (size >= 3072)
            {
                return 80;
            }
            else if (size >= 2048)
            {
                return 60;
            }
            else if (size >= 1024)
            {
                return 30;
            }

            return 0;
        }

        private static int GetShaderLevelScore()
        {
            var level = GraphicsShaderLevel;
            if (level >= 50)
            {
                return 100;
            }
            else if (level >= 40)
            {
                return 80;
            }
            else if (level >= 30)
            {
                return 60;
            }
            else if (level >= 20)
            {
                return 30;
            }

            return 0;
        }

        private static int GetProcessorFrequencyScore()
        {
#if UNITY_EDITOR || UNITY_IPHONE
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var generation = Device.generation;
                if (generation >= DeviceGeneration.iPhone8)
                {
                    return 100;
                }
                else if (generation >= DeviceGeneration.iPhone6S)
                {
                    return 80;
                }
                else if (generation >= DeviceGeneration.iPhone5C)
                {
                    return 60;
                }

                return 30;
            }
            else
#endif
            {
                var frequency = SystemInfo.processorFrequency;
                frequency = frequency / 1000;
                if (frequency >= 2.8)
                {
                    return 100;
                }
                else if (frequency >= 2.6)
                {
                    return 80;
                }
                else if (frequency >= 2.4)
                {
                    return 60;
                }
                else if (frequency >= 2.2)
                {
                    return 30;
                }

                return 0;
            }
        }
    }
}