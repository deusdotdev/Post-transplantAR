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
            EnsureScenesInBuild();

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
            EnsureScenesInBuild();
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

        private static void EnsureScenesInBuild()
        {
            const string hubScene = "Assets/_Project/Scenes/EducationHub.unity";
            const string arScene = "Assets/_Project/Scenes/ARMain.unity";

            if (!File.Exists(arScene))
            {
                throw new FileNotFoundException("ARMain yok — önce Post-transplantAR > Build AR Scene çalıştır.");
            }

            var ordered = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            // Ana ekran varsa her zaman index 0 olmalı (uygulama hub'dan açılır).
            if (File.Exists(hubScene))
            {
                ordered.Add(new EditorBuildSettingsScene(hubScene, true));
            }

            ordered.Add(new EditorBuildSettingsScene(arScene, true));

            // Build ayarlarındaki diğer mevcut sahneleri koru (varsa), kopyaları atla.
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s == null || string.IsNullOrEmpty(s.path))
                {
                    continue;
                }

                if (s.path == hubScene || s.path == arScene)
                {
                    continue;
                }

                ordered.Add(s);
            }

            EditorBuildSettings.scenes = ordered.ToArray();
        }

        private static string[] GetScenes()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        }
    }
}
#endif
