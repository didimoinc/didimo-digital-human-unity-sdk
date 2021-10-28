using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalSalmon
{
    /// <summary>
    /// A sequence component maintains active delayed actions and sequences of actions, whether method invokes, coroutines, or
    /// tweens,
    /// and provides a simple way to cancel them.
    /// </summary>
    public class Sequence
    {
        //-----------------------------------------------------------------------------------------
        // Classes:
        //-----------------------------------------------------------------------------------------

        private class QueuedAction
        {
            public Action Action { get; }
            public float Delay { get; }
            public bool IsRealtime { get; }

            public QueuedAction(float delay, Action action, bool isRealtime)
            {
                Delay = delay;
                Action = action;
                IsRealtime = isRealtime;
            }
        }

        //-----------------------------------------------------------------------------------------
        // Type Definitions:
        //-----------------------------------------------------------------------------------------

        public enum CancelledStatus
        {
            /// <summary>
            /// The sequence did not need to cancel as it wasn't running.
            /// </summary>
            NotRunning,

            /// <summary>
            /// A full cancellation completed.
            /// </summary>
            Completed
        }

        //-----------------------------------------------------------------------------------------
        // Events:
        //-----------------------------------------------------------------------------------------

        public event Didimo.EventHandler<CancelledStatus> Cancelled;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private MonoBehaviour _monoBehaviour;

        // Unqueued Coroutine Cache
        private readonly HashSet<IEnumerator> coroutines = new HashSet<IEnumerator>();

        // Queue of actions to be performed by actionQueueCoroutine
        private readonly Queue<QueuedAction> actionQueue = new Queue<QueuedAction>();

        // The coroutine which dequeues the action queue and runs its actions.
        private IEnumerator actionQueueCoroutine;

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// The MonoBehaviour this sequence is running on. Changing the MonoBehaviour will necessarily cause the sequence to
        /// cancel.
        /// </summary>
        public MonoBehaviour MonoBehaviour
        {
            get => _monoBehaviour;
            set
            {
                Cancel();
                _monoBehaviour = value;
            }
        }

        /// <summary>
        /// Is the sequence's underlying MonoBehaviour active and enabled and ready to run coroutines.
        /// </summary>
        // ReSharper disable once SimplifyConditionalTernaryExpression
        public bool IsActiveAndEnabled => _monoBehaviour == null ? false : _monoBehaviour.isActiveAndEnabled;

        /// <summary>
        /// Do we have any active running delayed actions, queued actions or coroutines?
        /// </summary>
        public bool IsRunning => actionQueue.Count > 0 || coroutines.Count > 0;

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Instantiates an <c>Sequence</c> using the given <c>MonoBehaviour</c> to run coroutines.
        /// </summary>
        /// <param name="monoBehaviour">The <c>MonoBehaviour</c> on which to run coroutines.</param>
        public Sequence(MonoBehaviour monoBehaviour)
        {
            MonoBehaviour = monoBehaviour;
            if (MonoBehaviour == null)
            {
                Debug.LogWarning("Sequence created with null Monobehaviour.");
            }
        }

        //-----------------------------------------------------------------------------------------
        // Action After Wait:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Invokes an action on the next fixed update.
        /// </summary>
        /// <param name="action">The action to be performed on the next fixed update.</param>
        public Sequence FixedUpdate(Action action)
        {
            if (action == null) return this;
            Coroutine(AfterWaitCoroutine(Wait.FixedUpdate, action));
            return this;
        }

        /// <summary>
        /// Invokes an action on the next frame.
        /// </summary>
        /// <param name="action">The action to be performed on the next frame.</param>
        public Sequence NextFrame(Action action)
        {
            if (action == null) return this;
            Coroutine(AfterFramesCoroutine(1, action));
            return this;
        }

        /// <summary>
        /// Invokes an action after a given number of frames.
        /// </summary>
        /// <param name="numFrames">The number of frames after the present frame in which to execute the action.</param>
        /// <param name="action">The action to be performed in the determined frame.</param>
        public Sequence AfterFrames(int numFrames, Action action)
        {
            if (action == null) return this;
            Coroutine(AfterFramesCoroutine(numFrames, action));
            return this;
        }

        /// <summary>
        /// Invokes an action at the end of the current frame.
        /// </summary>
        /// <param name="action">The action to be performed in the determined frame.</param>
        public Sequence EndOfFrame(Action action)
        {
            if (action == null) return this;
            Coroutine(AfterWaitCoroutine(Wait.EndOfFrame, action));
            return this;
        }

        //-----------------------------------------------------------------------------------------
        // Action Until Condition:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Invokes a function each frame until the function returns false.
        /// </summary>
        /// <param name="func">The function to be executed each frame. When it returns false it will no longer be called.</param>
        public Sequence EachFrame(Func<bool> func)
        {
            if (func == null) return this;
            Coroutine(EachFrameCoroutine(func));
            return this;
        }

        /// <summary>
        /// Invokes a function each end of frame until the function returns false.
        /// </summary>
        /// <param name="func">The function to be executed each end of frame. When it returns false it will no longer be called.</param>
        public Sequence EachEndOfFrame(Func<bool> func)
        {
            if (func == null) return this;
            Coroutine(EachAfterWaitCoroutine(Wait.EndOfFrame, func));
            return this;
        }

        /// <summary>
        /// Invokes a function each fixed update until the function returns false.
        /// </summary>
        /// <param name="func">The function to be executed each fixed update. When it returns false it will no longer be called.</param>
        public Sequence EachFixedUpdate(Func<bool> func)
        {
            if (func == null) return this;
            Coroutine(EachAfterWaitCoroutine(Wait.FixedUpdate, func));
            return this;
        }

        //-----------------------------------------------------------------------------------------
        // UnQueued Actions:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Invokes an action after a delay.
        /// </summary>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <param name="action">The action to invoke.</param>
        public Sequence Do(float delay, Action action)
        {
            if (action == null) return this;

            if (delay <= 0)
            {
                action();
                return this;
            }

            StartCachedCoroutine(delay, action);
            return this;
        }

        /// <summary>
        /// Invokes an action after a realtime delay uninfluenced by time scale.
        /// </summary>
        /// <param name="delay">The realtime delay before invoking the action.</param>
        /// <param name="action">The action to invoke.</param>
        public Sequence DoRealtime(float delay, Action action)
        {
            if (action == null) return this;

            if (delay <= 0)
            {
                action();
                return this;
            }

            StartCachedCoroutine(delay, action, true);
            return this;
        }

        /// <summary>
        /// Starts a coroutine and keeps a reference to it for cancellation later.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        public Sequence Coroutine(IEnumerator coroutine)
        {
            StartCachedCoroutine(coroutine);
            return this;
        }

        //-----------------------------------------------------------------------------------------
        // Queued Actions:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Queues up an action to be invoked after all other queued actions are processed and the given delay has elapsed.
        /// </summary>
        /// <param name="delay">The delay before invoking the action, after the existing queue has been processed.</param>
        /// <param name="action">The action to invoke.</param>
        public Sequence Queue(float delay, Action action)
        {
            EnqueueAction(delay, action);
            return this;
        }

        /// <summary>
        /// Queues up an action to be invoked after all other queued actions are processed and the given realtime delay has
        /// elapsed.
        /// </summary>
        /// <param name="delay">The realtime delay before invoking the action, after the existing queue has been processed.</param>
        /// <param name="action">The action to invoke.</param>
        public Sequence QueueRealtime(float delay, Action action)
        {
            EnqueueAction(delay, action, true);
            return this;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Cancels all actions and queued actions.
        /// </summary>
        public void Cancel()
        {
            // don't cancel if we're not running, invoke event and return.
            // N.B. this event might be useful for cancelling other things in response even if we didn't have to cancel ourselves.
            if (!IsRunning || !IsActiveAndEnabled)
            {
                Cancelled?.Invoke(CancelledStatus.NotRunning);
                return;
            }

            // stop any coroutines and empty the list.
            foreach (IEnumerator coroutine in coroutines)
            {
                if (coroutine == null) continue;
                _monoBehaviour.StopCoroutine(coroutine);
            }

            coroutines.Clear();

            // null the queue coroutine reference, and clear the queue if it exists and has queued actions.
            // N.B. we don't need to stop the queue coroutine, because it exists in the coroutines list which is cancelled above.
            actionQueueCoroutine = null;
            actionQueue.Clear();

            // invoke event that we completed a full cancellation.
            Cancelled?.Invoke(CancelledStatus.Completed);
        }

        public IEnumerator DoInline(params IEnumerator[] enumerators)
        {
            if (enumerators.Length == 0) yield break;

            Coroutine[] yieldList = new Coroutine[enumerators.Length];
            for (int i = 0; i < enumerators.Length; i++)
            {
                yieldList[i] = MonoBehaviour.StartCoroutine(enumerators[i]);
            }

            for (int i = 0; i < yieldList.Length; i++)
            {
                yield return yieldList[i];
            }
        }

        /// <summary>
        /// Coroutine that processes the queue of actions, waiting the specified time for an action then invoking it, then
        /// continuing through the queue.
        /// </summary>
        /// <returns>Enumerator.</returns>
        private IEnumerator QueueCoroutine()
        {
            // while the queue contains actions, dequeue them, wait and then invoke.
            while (actionQueue.Count > 0)
            {
                QueuedAction queuedAction = actionQueue.Dequeue();

                // wait the appropriate time, either realtime or time scale influenced.
                if (queuedAction.IsRealtime)
                {
                    yield return Wait.SecondsRealtime(queuedAction.Delay);
                }
                else
                {
                    yield return Wait.Seconds(queuedAction.Delay);
                }

                queuedAction.Action();
            }

            actionQueueCoroutine = null;
        }

        //-----------------------------------------------------------------------------------------
        // Private Methods:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Starts a coroutine which will be added to the cache.
        /// </summary>
        /// <param name="enumerator">The coroutine enumerator.</param>
        private void StartCachedCoroutine(IEnumerator enumerator)
        {
            if (!IsActiveAndEnabled) return;

            coroutines.Add(enumerator);

            // start the a nested coroutine wrapping the enumerator which removes it on complete.
            MonoBehaviour.StartCoroutine(YieldCoroutineAndRemoveOnComplete(enumerator, coroutines));
        }

        /// <summary>
        /// Starts a coroutine, using a DoCoroutine and with an action, which will be added to the cache.
        /// </summary>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <param name="action"></param>
        /// <param name="isRealtime">Is the delay realtime or time scale influenced?</param>
        private void StartCachedCoroutine(float delay, Action action, bool isRealtime = false)
        {
            if (!IsActiveAndEnabled) return;

            IEnumerator enumerator = DoCoroutine(delay, action, isRealtime);
            coroutines.Add(enumerator);

            // start a coroutine that yields on the enumerator and removes it from the list of coroutines on completion.
            // N.B. this StartCoroutine is "hanging" in the sense that we don't keep track of it; however, because we keep track of the
            // inner enumerator, when the inner one is cancelled, this outer coroutine will also stop yielding over it.
            MonoBehaviour.StartCoroutine(YieldCoroutineAndRemoveOnComplete(enumerator, coroutines));
        }

        /// <summary>
        /// Queues up an action to be invoked after all other queued actions are processed and the given delay (either
        /// time-scale-influenced or realtime).
        /// </summary>
        /// <param name="delay">The delay before invoking the action, after the existing queue has been processed.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="isRealtime">Is the delay realtime or time scale influenced?</param>
        private void EnqueueAction(float delay, Action action, bool isRealtime = false)
        {
            if (action == null) return;

            // if the delay is zero and the queue is null or empty, invoke and return.
            if (delay <= 0 && (actionQueue == null || actionQueue.Count == 0))
            {
                action();
                return;
            }

            // instantiate and queue up a queued action.
            actionQueue.Enqueue(new QueuedAction(delay, action, isRealtime));

            // if the queue coroutine isn't running, restart it.
            if (actionQueueCoroutine != null) return;

            actionQueueCoroutine = QueueCoroutine();
            StartCachedCoroutine(actionQueueCoroutine);
        }

        //-----------------------------------------------------------------------------------------
        // Coroutines:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Coroutine that invokes the action after waiting for the specified time (either time-scale-influenced or realtime).
        /// </summary>
        /// <param name="delay">The delay in seconds before invoking the action.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="isRealtime">Is the delay realtime or time scale influenced?</param>
        /// <returns>Enumerator.</returns>
        private static IEnumerator DoCoroutine(float delay, Action action, bool isRealtime = false)
        {
            // wait the appropriate time, either realtime or time scale influenced.
            if (isRealtime)
            {
                yield return Wait.SecondsRealtime(delay);
            }
            else
            {
                yield return Wait.Seconds(delay);
            }

            action();
        }

        //-----------------------------------------------------------------------------------------
        // Helper Coroutines:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Coroutine that executes the action after waiting for the specified number of frames.
        /// </summary>
        /// <param name="numFrames">The number of frames after the present frame in which to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>Enumerator.</returns>
        private static IEnumerator AfterFramesCoroutine(int numFrames, Action action)
        {
            int frameCount = 0;
            while (frameCount < numFrames)
            {
                ++frameCount;
                yield return null;
            }

            action();
        }

        /// <summary>
        /// Coroutine that executes the action after yielding over a given wait object..
        /// </summary>
        /// <param name="wait">The wait <c>YieldInstruction</c> to yield over before executing the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>Enumerator.</returns>
        private static IEnumerator AfterWaitCoroutine(YieldInstruction wait, Action action)
        {
            yield return wait;
            action();
        }

        /// <summary>
        /// Coroutine that executes a given function each frame until the function returns false.
        /// </summary>
        /// <param name="func">The function to execute. When this returns false the coroutine will stop.</param>
        /// <returns>Enumerator.</returns>
        private static IEnumerator EachFrameCoroutine(Func<bool> func)
        {
            while (true)
            {
                if (!func()) yield break;
                yield return null;
            }
        }

        /// <summary>
        /// Coroutine that executes a given function after yielding over a given wait object until the function returns false.
        /// </summary>
        /// <param name="wait">The wait <c>YieldInstruction</c> to yield over before executing the function.</param>
        /// <param name="func">The function to execute. When this returns false the coroutine will stop.</param>
        /// <returns>Enumerator.</returns>
        private static IEnumerator EachAfterWaitCoroutine(YieldInstruction wait, Func<bool> func)
        {
            while (true)
            {
                yield return wait;
                if (!func()) yield break;
            }
        }

        /// <summary>
        /// Yields a given coroutine then removes that coroutine from a given hashset when complete.
        /// </summary>
        private static IEnumerator YieldCoroutineAndRemoveOnComplete(IEnumerator coroutine, ICollection<IEnumerator> cache)
        {
            yield return coroutine;

            // N.B. we won't make it here if the above nested coroutine is cancelled, but that's fine, as the act of cancelling will
            // itself do what we wanted to do below.

            if (cache.Contains(coroutine)) { cache.Remove(coroutine); }
        }
    }
}