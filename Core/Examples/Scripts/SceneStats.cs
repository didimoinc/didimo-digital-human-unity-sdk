using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Stats
{
    /// <summary>
    /// Example component that provides FPS stats for a given scene.
    /// Visit the DidimoInspector scene to see it in action or look at the <c>DidimoStatsExample</c> script.
    /// </summary>
    public static class SceneStats
    {
        /// <summary>
        /// Struct to track the FPS values for a scene.
        /// Should be created using the public <c>GetFPSData</c> method.
        /// </summary>
        public struct FPSData
        {
            public float Max => fpsTracker.Max();
            public float Min => fpsTracker.Min();
            public float Average => GetAverageFPS();

            public readonly int  NumberOfFramesToAverage;
            public          bool excludeExtremes;

            private readonly List<float> fpsTracker;
            private IReadOnlyList<float> FPSTracker => fpsTracker;

            public FPSData(int numberOfFramesToAverage, bool excludeExtremes)
            {
                NumberOfFramesToAverage = numberOfFramesToAverage;
                this.excludeExtremes = excludeExtremes;

                fpsTracker = new List<float>(numberOfFramesToAverage);
            }

            public void AddFrame(float deltaTime)
            {
                if (deltaTime <= 0f)
                {
                    Debug.LogWarning($"Invalid deltaTime received {deltaTime}, so discarding sample.");
                    return;
                }

                if (fpsTracker.Count >= NumberOfFramesToAverage) fpsTracker.RemoveAt(0);
                float fps = 1 / deltaTime;
                fpsTracker.Add(fps);
            }

            private float GetAverageFPS()
            {
                if (!excludeExtremes) return fpsTracker.Average();

                List<float> fpsValues = new List<float>(fpsTracker);
                fpsValues.Remove(fpsValues.Max());
                fpsValues.Remove(fpsValues.Min());
                return fpsValues.Average();
            }
        }

        /// <summary>
        /// Build a <c>FPSData</c> object that contains the FPS information of the current scene.
        /// </summary>
        /// <param name="numberOfFramesToAverage">Number of frames to use to get smoother FPS results.
        /// Default 10</param>
        /// <param name="excludeExtremes">Exclude the highest and lowest latest FPS values. Default true</param>
        /// <returns><c>FPSData</c> struct with information of the scene FPS stats</returns>
        public static FPSData GetFPSData(int numberOfFramesToAverage = 10, bool excludeExtremes = true)
            => new FPSData(numberOfFramesToAverage, excludeExtremes);
    }
}