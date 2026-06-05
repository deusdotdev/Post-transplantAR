#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LiverAR.EditorTools
{
    public static class MobileBuildPipeline
    {
        private const string IosOutputFolder = "Builds/iOS";
        private const string AndroidApkPath = "Builds/Android/PostTransplantAR.apk";

        [MenuItem("Post-transplantAR/Build/iOS (Xcode Project)", false, 100)]
        public static void BuildIosMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play modunu kapat", "Build için önce Play'den çık.", "Tamam");
                return;
            }

            try
            {
                BuildIos(showDialog: true);
            }
            catch (Exception ex)
            {
                Debug.LogError("[MobileBuild] " + ex.Message);
                EditorUtility.DisplayDialog("iOS build hatası", ex.Message, "Tamam");
            }
        }

        [MenuItem("Post-transplantAR/Build/Android (APK)", false, 101)]
        public static void BuildAndroidMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play modunu kapat", "Build için önce Play'den çık.", "Tamam");
                return;
            }

            try
            {
                BuildAndroid(showDialog: true);
            }
            catch (Exception ex)
            {
                Debug.LogError("[MobileBuild] " + ex.Message);
                EditorUtility.DisplayDialog("Android build hatası", ex.Message, "Tamam");
            }
        }

        private static void BuildIos(bool showDialog)
        {
            Directory.CreateDirectory(IosOutputFolder);
            EnsureArSceneInBuild();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }

            EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

            var scenes = GetScenes();
            var report = BuildPipeline.BuildPlayer(scenes, IosOutputFolder, BuildTarget.iOS, BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Build başarısız: " + report.summary.result);
            }

            if (showDialog)
            {
                EditorUtility.DisplayDialog("iOS hazır",
                    Path.GetFullPath(IosOutputFolder) +
                    "\n\nXcode'da Unity-iPhone.xcodeproj aç → Team seç → iPhone'a Run.",
                    "Tamam");
                EditorUtility.RevealInFinder(IosOutputFolder);
            }
        }

        private static void BuildAndroid(bool showDialog)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AndroidApkPath)!);
            EnsureArSceneInBuild();
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.deusex.posttransplantar");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            EditorUserBuildSettings.buildAppBundle = false;

            var scenes = GetScenes();
            var report = BuildPipeline.BuildPlayer(scenes, AndroidApkPath, BuildTarget.Android, BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Build başarısız: " + report.summary.result);
            }

            if (showDialog)
            {
                EditorUtility.DisplayDialog("APK hazır", Path.GetFullPath(AndroidApkPath), "Tamam");
                EditorUtility.RevealInFinder(Path.GetDirectoryName(AndroidApkPath));
            }
        }

        private static void EnsureArSceneInBuild()
        {
            const string scene = "Assets/_Project/Scenes/ARMain.unity";
            if (!File.Exists(scene))
            {
                throw new FileNotFoundException("ARMain yok — önce Build AR Scene çalıştır.");
            }

            if (EditorBuildSettings.scenes.All(s => s.path != scene))
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scene, true) };
            }
        }

        private static string[] GetScenes()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        }
    }
}
#endif
