using UnityEditor;

namespace Didimo.Core.Editor
{
    public class CheckUrpInstalled : IProjectSettingIssue
    {
        private const string REASON =
            "didimos can only be rendered with URP.\n" +
            "Please install the URP package, and restart Unity";

        public bool CheckOk()
        {
#if USING_UNITY_URP
            return true;
#else
            EditorGUILayout.HelpBox(ResolveText(), MessageType.Error);
            return false;
#endif
        }

        public void Resolve()
        {
            var installPackager = new InstallUnityPackage();
            installPackager.AddPackage("com.unity.render-pipelines.universal");
        }

        public string ResolveText()
        {
            return REASON;
        }
    }
}