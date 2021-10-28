using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Stats
{
    public static class SceneStats
    {
        public struct FPSData
        {
            public float Max => _fpsTracker.Max();
            public float Min => _fpsTracker.Min();
            public float Average => GetAverageFPS();

            public readonly int  NumberOfFramesToAverage;
            public          bool excludeExtremes;

            private readonly List<float> _fpsTracker;
            private IReadOnlyList<float> FPSTracker => _fpsTracker;

            public FPSData(int numberOfFramesToAverage, bool excludeExtremes)
            {
                NumberOfFramesToAverage = numberOfFramesToAverage;
                this.excludeExtremes = excludeExtremes;

                _fpsTracker = new List<float>(numberOfFramesToAverage);
            }

            public void AddFrame(float deltaTime)
            {
                if (deltaTime <= 0f)
                {
                    Debug.LogWarning($"Invalid deltaTime received {deltaTime}, so discarding sample.");
                    return;
                }

                if (_fpsTracker.Count >= NumberOfFramesToAverage) _fpsTracker.RemoveAt(0);
                float fps = 1 / deltaTime;
                _fpsTracker.Add(fps);
            }

            private float GetAverageFPS()
            {
                if (!excludeExtremes) return _fpsTracker.Average();

                List<float> fpsValues = new List<float>(_fpsTracker);
                fpsValues.Remove(fpsValues.Max());
                fpsValues.Remove(fpsValues.Min());
                return fpsValues.Average();
            }
        }

        public static FPSData GetFPSData(int numberOfFramesToAverage = 10, bool excludeExtremes = true) => new FPSData(numberOfFramesToAverage, excludeExtremes);
    }
}