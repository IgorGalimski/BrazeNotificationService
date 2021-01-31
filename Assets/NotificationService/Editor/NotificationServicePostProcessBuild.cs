#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace Appboy.NotificationSerivice.Editor
{
    public class NotificationServicePostProcessBuild
    {
        private static readonly string PathToNotificationService = Path.Combine(Application.dataPath, "NotificationService");

        [PostProcessBuild]
        public static void OnPostprocessBuildHandler(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                var projPath = PBXProject.GetPBXProjectPath(path);
                var proj = new PBXProject ();
                proj.ReadFromFile (projPath);

#if UNITY_2019_3_OR_NEWER
             var targetGUID = proj.GetUnityFrameworkTargetGuid();
 #else
             var targetGUID = proj.TargetGuidByName ("Unity-iPhone");
 #endif
                var notificationServicePlistPath = PathToNotificationService + "/Info.plist";

                var notificationServicePlist = new PlistDocument();
                notificationServicePlist.ReadFromFile (notificationServicePlistPath);
                notificationServicePlist.root.SetString ("CFBundleShortVersionString", PlayerSettings.bundleVersion);
                notificationServicePlist.root.SetString ("CFBundleVersion", PlayerSettings.iOS.buildNumber);
                notificationServicePlist.root.SetString ("CFBundleDisplayName", PlayerSettings.iOS.applicationDisplayName);

                notificationServicePlist.WriteToFile (notificationServicePlistPath);

                var notificationServiceTarget = PBXProjectExtensions.AddAppExtension (proj, targetGUID, "notificationservice", PlayerSettings.GetApplicationIdentifier (BuildTargetGroup.iOS) + ".bknotificationservice", notificationServicePlistPath);
                proj.AddFileToBuild (notificationServiceTarget, proj.AddFile (PathToNotificationService + "/NotificationService.h", "NotificationService/NotificationService.h"));
                proj.AddFileToBuild (notificationServiceTarget, proj.AddFile (PathToNotificationService + "/NotificationService.m", "NotificationService/NotificationService.m"));
                proj.AddFrameworkToProject (notificationServiceTarget, "NotificationCenter.framework", true);
                proj.AddFrameworkToProject (notificationServiceTarget, "UserNotifications.framework", true);
                proj.SetBuildProperty (notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
                proj.SetBuildProperty(notificationServiceTarget, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");
                proj.SetBuildProperty(notificationServiceTarget, "TARGETED_DEVICE_FAMILY", "1,2");

                proj.WriteToFile (projPath);
            }
        }
    }
}

#endif