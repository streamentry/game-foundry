#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Evergrove.UnityClient.Editor
{
    /// <summary>
    /// I275: link Apple's UserNotifications framework for the garden-notes plugin
    /// (Assets/Plugins/iOS/MosseloraNotifications.mm). The plugin's .meta framework
    /// dependency was not applied to the UnityFramework target in b292 (undefined
    /// UN* symbols at link), so it is added here, explicitly and weakly linked.
    /// </summary>
    internal static class MosseloraNotificationsLink
    {
        [PostProcessBuild(101)]
        public static void OnPostprocessBuild(BuildTarget target, string exportPath)
        {
            if (target != BuildTarget.iOS) return;
            string projectPath = PBXProject.GetPBXProjectPath(exportPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            string framework = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(framework, "UserNotifications.framework", true);
            project.WriteToFile(projectPath);
        }
    }
}
#endif
