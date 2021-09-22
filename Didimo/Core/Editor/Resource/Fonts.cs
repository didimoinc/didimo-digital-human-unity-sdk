using UnityEngine;

namespace DigitalSalmon.Resources
{
    public static class Fonts
    {
        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        private const string DIRECTORY = "Fonts/";

        //-----------------------------------------------------------------------------------------
        // Backing Fields:
        //-----------------------------------------------------------------------------------------

        private static Font _robotoRegular;
        private static Font _robotoLight;
        private static Font _robotoBold;

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        public static Font RobotoRegular
        {
            get
            {
                if (_robotoRegular != null) return _robotoRegular;
                _robotoRegular = Resource.Load<Font>(DIRECTORY + "Roboto-Regular");
                return _robotoRegular;
            }
        }

        public static Font RobotoLight
        {
            get
            {
                if (_robotoLight != null) return _robotoLight;
                _robotoLight = Resource.Load<Font>(DIRECTORY + "Roboto-Light");
                return _robotoLight;
            }
        }

        public static Font RobotoBold
        {
            get
            {
                if (_robotoBold != null) return _robotoBold;
                _robotoBold = Resource.Load<Font>(DIRECTORY + "Roboto-Bold");
                return _robotoBold;
            }
        }
    }
}