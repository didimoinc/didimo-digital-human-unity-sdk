using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo
{
    public class ThreadingUtility : ASingletonBehaviour<ThreadingUtility>
    {
        private readonly Queue<Action> actionQueue = new Queue<Action>();

        protected void Update()
        {
            while (actionQueue.Any())
            {
                actionQueue.Dequeue()();
            }
        }

        public static void WhenMainThread(Action action) { Instance.actionQueue.Enqueue(action); }

        [RuntimeInitializeOnLoadMethod]
        // Automatically instantiate on main thread. If we lazy load this singleton behaviour outside the main thread, we would get an error.
        private static void CreateInstance() { WhenMainThread(() => { }); }
    }
}