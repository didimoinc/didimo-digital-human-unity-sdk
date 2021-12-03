using UnityEngine;

namespace Didimo.Resources
{
    public class Icons
    {
        private const string DIRECTORY = "Icons/";

        private static Texture2D _tick;
        private static Texture2D _refresh;

        public static Texture2D Tick => Resource.LocateResource(ref _tick, DIRECTORY + "Tick");
        public static Texture2D Refresh => Resource.LocateResource(ref _refresh, DIRECTORY + "Refresh");
    }
}