using DigitalSalmon.Resources;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public abstract class FontGroup
    {
        //-----------------------------------------------------------------------------------------
        // Backing Fields:
        //-----------------------------------------------------------------------------------------

        private static FontGroup _default;

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        public static FontGroup Default => _default ?? (_default = new RobotoFontGroup());

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        public abstract Font Standard { get; }
        public abstract Font Light { get; }
        public abstract Font Bold { get; }
    }

    public class RobotoFontGroup : FontGroup
    {
        public override Font Standard => Fonts.RobotoRegular;
        public override Font Light => Fonts.RobotoLight;
        public override Font Bold => Fonts.RobotoBold;
    }
}