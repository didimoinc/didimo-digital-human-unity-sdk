using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public static class BuildPostProcessor
{
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