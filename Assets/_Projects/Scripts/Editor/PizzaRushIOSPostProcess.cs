#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class PizzaRushIOSPostProcess
{
    [PostProcessBuild(1000)]
    public static void AddRequiredSystemFrameworks(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);

        project.AddFrameworkToProject(
            project.GetUnityFrameworkTargetGuid(),
            "AppTrackingTransparency.framework",
            true);

        project.WriteToFile(projectPath);
    }
}
#endif
