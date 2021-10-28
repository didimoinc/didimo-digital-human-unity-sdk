using UnityEngine;

namespace DigitalSalmon.Resources
{
    public class Icons
    {
        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        private const string DIRECTORY = "Icons/";

        //-----------------------------------------------------------------------------------------
        // Backing Fields:
        //-----------------------------------------------------------------------------------------

        private static Texture2D _tick;
        private static Texture2D _refresh;

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        // Generic
        public static Texture2D Tick => Resource.LocateResource(ref _tick, DIRECTORY + "Tick");
        public static Texture2D Refresh => Resource.LocateResource(ref _refresh, DIRECTORY + "Refresh");
    }
}