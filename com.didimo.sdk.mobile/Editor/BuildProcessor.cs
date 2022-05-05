using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#if UNITY_IOS
using Didimo.Mobile.Communication;
using UnityEditor.iOS.Xcode;

#endif

public class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_IOS
        UnityInterfaceGenerator.GenerateIOSUnityInterface();
#endif
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            OnPostprocessBuildIOS(pathToBuiltProject);
        }
    }

    private static void OnPostprocessBuildIOS(string pathToBuiltProject)
    {
#if UNITY_IOS
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string targetGuid = proj.GetUnityFrameworkTargetGuid();
        string dataFolderGuid = proj.FindFileGuidByProjectPath("Data");
        proj.AddFileToBuild(targetGuid, dataFolderGuid);
        proj.RemoveFileFromBuild(proj.GetUnityMainTargetGuid(), dataFolderGuid);
        proj.WriteToFile(projPath);
#endif
    }
}