using System.Collections.Generic;
using UnityEngine;

namespace DigitalSalmon
{
    public static class Wait
    {
        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private static readonly Dictionary<float, WaitForSeconds>         waitCache         = new Dictionary<float, WaitForSeconds>();
        private static readonly Dictionary<float, WaitForSecondsRealtime> waitRealtimeCache = new Dictionary<float, WaitForSecondsRealtime>();

        private static WaitForEndOfFrame  endOfFrame;
        private static WaitForFixedUpdate fixedUpdate;

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        public static WaitForEndOfFrame EndOfFrame => endOfFrame ?? (endOfFrame = new WaitForEndOfFrame());
        public static WaitForFixedUpdate FixedUpdate => fixedUpdate ?? (fixedUpdate = new WaitForFixedUpdate());

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a cached <c>WaitForSeconds</c> object corresponding to the given time in seconds, scaled by time scale.
        /// </summary>
        /// <returns>The <c>WaitForSeconds</c> object.</returns>
        public static WaitForSeconds Seconds(float seconds, bool caching = true)
        {
            if (!caching) return new WaitForSeconds(seconds);

            // if the wait key is not cached, cache it.
            if (!waitCache.ContainsKey(seconds))
            {
                waitCache.Add(seconds, new WaitForSeconds(seconds));
            }

            return waitCache[seconds];
        }

        /// <summary>
        /// Gets a cached <c>WaitForSecondsRealtime</c> object corresponding to the given time in realtime seconds.
        /// </summary>
        /// <returns>The <c>WaitForSecondsRealtime</c> object.</returns>
        public static WaitForSecondsRealtime SecondsRealtime(float seconds, bool caching = true)
        {
            if (!caching) return new WaitForSecondsRealtime(seconds);

            // if the wait key is not cached, cache it.
            if (!waitRealtimeCache.ContainsKey(seconds))
            {
                waitRealtimeCache.Add(seconds, new WaitForSecondsRealtime(seconds));
            }

            return waitRealtimeCache[seconds];
        }
    }
}