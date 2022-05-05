using System;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Didimo.Mobile
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CinematicManager : ASingletonBehaviour<CinematicManager>
    {
        private static readonly string DIDIMO_ROOT = "didimo_root";
#if UNITY_EDITOR
        [InitializeOnLoad]
        internal static class EditorSceneManagerSceneSaving
        {
            static EditorSceneManagerSceneSaving() { EditorSceneManager.sceneSaving += OnSceneSaving; }

            private static void OnSceneSaving(Scene scene, string path)
            {
                CinematicManager[] cinematicManagers = Object.FindObjectsOfType<CinematicManager>();
                foreach (CinematicManager cinematicManager in cinematicManagers)
                {
                    cinematicManager.CacheBindings();
                    EditorUtility.SetDirty(cinematicManager);
                }
            }
        }
#endif

        [Serializable]
        private class TimelineBindingCache
        {
            public List<BindingCache> bindingsCache = new List<BindingCache>();
            public TimelineAsset      timelineAsset;
        }

        [Serializable]
        private class BindingCache
        {
            public string gameObjectName;
            public string componentType;

            public BindingCache(string gameObjectName, Type componentType)
            {
                this.gameObjectName = gameObjectName;
                this.componentType = componentType.AssemblyQualifiedName;
            }

            public Type GetComponentType() { return Type.GetType(componentType); }
        }

        [SerializeField]
        private List<TimelineBindingCache> timelineBindingCaches;

        public DidimoComponents didimo;

        public List<TimelineAsset> timelines;

        private PlayableDirector _playableDirector;

        private PlayableDirector playableDirector
        {
            get
            {
                if (_playableDirector == null) _playableDirector = GetComponent<PlayableDirector>();
                return _playableDirector;
            }
        }

        private Action<float> progressCallback;
        private double        previousAnimationTime = -1;

        public void StopCinematic()
        {
            progressCallback = null;
            playableDirector.Stop();
        }

        public void PlayCinematic(string cinematicName, Action<float> progress)
        {
            previousAnimationTime = -1;
            playableDirector.playableAsset = timelines.First(t => t.name == cinematicName);
            playableDirector.Play();
            progressCallback = progress;
            playableDirector.stopped += PlayableDirectorStopped;
        }

        private void PlayableDirectorStopped(PlayableDirector director)
        {
            if (progressCallback != null)
            {
                progressCallback(1);
                progressCallback = null;
            }

            director.stopped -= PlayableDirectorStopped;
        }

        private void Update()
        {
            if (progressCallback == null) return;

            if (previousAnimationTime > playableDirector.time)
            {
                progressCallback(1);
                progressCallback = null;
                playableDirector.stopped -= PlayableDirectorStopped;
                return;
            }

            progressCallback((float) (playableDirector.time / playableDirector.duration));
            previousAnimationTime = playableDirector.time;
        }

        [Button]
        public void CacheBindings()
        {
            if (didimo == null)
            {
                Debug.LogWarning("Didimo is null in the CinematicManager. Director bindings cache will be skipped.");
                return;
            }

            timelineBindingCaches = new List<TimelineBindingCache>();
            if (timelines == null) return;

            foreach (TimelineAsset timelineAsset in timelines)
            {
                TimelineBindingCache timelineBindingCache = new TimelineBindingCache();
                timelineBindingCache.timelineAsset = timelineAsset;

                if (timelineAsset == null) return;

                IEnumerable<TrackAsset> tracks = timelineAsset.GetOutputTracks();
                foreach (TrackAsset track in tracks)
                {
                    Object bindingObject = playableDirector.GetGenericBinding(track);
                    Component componentToReplace = null;
                    // Root game objects may have different names
                    bool isRoot = false;
                    if (bindingObject.name == didimo.name)
                    {
                        componentToReplace = didimo.GetComponent(bindingObject.GetType());
                        isRoot = true;
                    }
                    else // Other objects are supposed to have the same names on all didimos
                    {
                        Component[] components = didimo.GetComponentsInChildren(bindingObject.GetType());
                        componentToReplace = components.FirstOrDefault(c => c.name == bindingObject.name);
                    }

                    if (componentToReplace == null)
                    {
                        timelineBindingCache.bindingsCache.Add(new BindingCache("", null));
                    }
                    else
                    {
                        timelineBindingCache.bindingsCache.Add(new BindingCache(isRoot ? DIDIMO_ROOT : bindingObject.name, bindingObject.GetType()));
                    }
                }

                timelineBindingCaches.Add(timelineBindingCache);
            }
        }

        /// <summary>
        /// Replace references to one didimo (any of its components in the full hierarchy), into another didimo, on the currently active playable asset of playableDirector.
        /// Does NOT use cache. If the original didimo being referenced no longer exists, this method will fail. 
        /// </summary>
        /// <param name="originalDidimoComponents">The didimo that is currently referenced in the active playable asset.</param>
        /// <param name="newDidimoComponents">The new didimo we want to reference.</param>
        public void RemapTimeline(DidimoComponents originalDidimoComponents, DidimoComponents newDidimoComponents)
        {
            TimelineAsset playableAsset = playableDirector.playableAsset as TimelineAsset;
            if (playableAsset == null) return;

            IEnumerable<TrackAsset> tracks = playableAsset.GetOutputTracks();
            foreach (TrackAsset track in tracks)
            {
                Object bindingObject = playableDirector.GetGenericBinding(track);
                Component componentToReplace = null;
                // Root game objects may have different names
                if (bindingObject.name == originalDidimoComponents.name)
                {
                    componentToReplace = newDidimoComponents.GetComponent(bindingObject.GetType());
                }
                else // Other objects are supposed to have the same names on all didimos
                {
                    Component[] components = newDidimoComponents.GetComponentsInChildren(bindingObject.GetType());
                    componentToReplace = components.FirstOrDefault(c => c.name == bindingObject.name);
                }

                if (componentToReplace == null) continue;

                playableDirector.ClearGenericBinding(track);
                playableDirector.SetGenericBinding(track, componentToReplace);
            }

            double time = playableDirector.time;
            playableDirector.RebuildGraph();
            playableDirector.time = time;
        }

        /// <summary>
        /// Replace references to one didimo (any of its components in the full hierarchy), into another didimo, on the currently active playable asset of playableDirector.
        /// Does NOT use cache. If the original didimo being referenced no longer exists, this method will fail. 
        /// </summary>
        /// <param name="originalDidimoKey">The key of the didimo that is currently referenced in the active playable asset.</param>
        /// <param name="newDidimoKey">The key of the new didimo we want to reference.</param>
        public void RemapTimeline(string originalDidimoKey, string newDidimoKey)
        {
            DidimoCache.TryFindDidimo(originalDidimoKey, out DidimoComponents originalDidimoComponents);
            DidimoCache.TryFindDidimo(newDidimoKey, out DidimoComponents newDidimoComponents);

            RemapTimeline(originalDidimoComponents, newDidimoComponents);
        }

        /// <summary>
        /// Replace references to one didimo (any of its components in the full hierarchy), into another didimo, on the currently active playable asset of playableDirector.
        /// Uses our cache to determine which components to update the reference to. This cache is generated automatically when saving the scene.
        /// </summary>
        /// <param name="newDidimoComponents">The didimo we want to reference.</param>
        public void RemapTimeline(DidimoComponents newDidimoComponents)
        {
            TimelineAsset playableAsset = playableDirector.playableAsset as TimelineAsset;

            if (playableAsset == null) return;

            TimelineBindingCache timelineBindingCache = timelineBindingCaches.FirstOrDefault(t => t.timelineAsset == playableAsset);

            if (timelineBindingCache == null)
            {
                throw new Exception($"Couldn't find timeline cache for timeline {playableAsset.name}.");
            }


            if (timelineBindingCache.bindingsCache.Count != playableAsset.outputTrackCount)
            {
                throw new Exception($"Playable asset tracks number ({playableAsset.outputTrackCount}) different from cache bindings number ({timelineBindingCache.bindingsCache.Count}).");
            }

            for (int i = 0; i < playableAsset.outputTrackCount; i++)
            {
                BindingCache bindingCache = timelineBindingCache.bindingsCache[i];

                if (string.IsNullOrEmpty(bindingCache.gameObjectName)) continue;

                TrackAsset track = playableAsset.GetOutputTrack(i);

                // Root game objects may have different names
                Transform didimoTransform = null;
                if (bindingCache.gameObjectName == DIDIMO_ROOT)
                {
                    didimoTransform = newDidimoComponents.transform;
                }
                else
                {
                    newDidimoComponents.transform.TryFindRecursive(bindingCache.gameObjectName, out didimoTransform);
                }

                Component componentToReplace = didimoTransform.GetComponent(bindingCache.GetComponentType());
                if (componentToReplace == null) continue;

                playableDirector.ClearGenericBinding(track);
                playableDirector.SetGenericBinding(track, componentToReplace);
            }

            double time = playableDirector.time;
            playableDirector.RebuildGraph();
            playableDirector.time = time;
        }

        /// <summary>
        /// Replace references to one didimo (any of its components in the full hierarchy), into another didimo, on the currently active playable asset of playableDirector.
        /// Uses our cache to determine which components to update the reference to. This cache is generated automatically when saving the scene.
        /// </summary>
        /// <param name="newDidimoKey">The key of the didimo we want to reference.</param>
        public void RemapTimeline(string newDidimoKey)
        {
            DidimoCache.TryFindDidimo(newDidimoKey, out DidimoComponents newDidimoComponents);
            RemapTimeline(newDidimoComponents);
        }
    }
}