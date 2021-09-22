using UnityEngine;

namespace Didimo.Inspector
{
    public class InfoBoxAttribute : PropertyAttribute
    {
        // We can't use UnityEditor.MessageType, because UnityEditor doesn't go into project builds.
        public enum BoxMessageType
        {
            None,
            Info,
            Warning,
            Error
        }

        public string         Title;
        public BoxMessageType MessageType;

        public InfoBoxAttribute(string title, BoxMessageType messageType = BoxMessageType.Info)
        {
            Title = title;
            MessageType = messageType;
        }
    }
}