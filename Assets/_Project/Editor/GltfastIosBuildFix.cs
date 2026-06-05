#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// glTFast 6.12 Animation modülündeki yanlış NUnit referansını iOS IL2CPP build öncesi düzeltir.
    /// </summary>
    public static class GltfastIosBuildFix
    {
        private const string BrokenUsing = "using NUnit.Framework;";
        private const string FixedUsing = "using UnityEngine.Assertions;";

        [MenuItem("Post-transplantAR/Fix glTFast iOS Linker Error", false, 15)]
        public static void FixFromMenu()
        {
            if (TryPatchAnimationComponent(out var message))
            {
                EditorUtility.DisplayDialog("glTFast düzeltildi", message + "\n\nŞimdi iOS Build tekrar dene.", "Tamam");
            }
            else
            {
                EditorUtility.DisplayDialog("glTFast", message, "Tamam");
            }
        }

        [InitializeOnLoadMethod]
        private static void AutoPatchOnLoad()
        {
            TryPatchAnimationComponent(out _);
        }

        internal static bool TryPatchAnimationComponent(out string message)
        {
            var file = FindAnimationComponentFile();
            if (file == null)
            {
                message = "glTFast paketi bulunamadı (Package Manager'da com.unity.cloud.gltfast yüklü mü?).";
                return false;
            }

            var text = File.ReadAllText(file);
            if (!text.Contains(BrokenUsing))
            {
                message = "glTFast Animation dosyası zaten düzeltilmiş.";
                return true;
            }

            text = text.Replace(BrokenUsing, FixedUsing);
            File.WriteAllText(file, text);
            AssetDatabase.Refresh();
            message = "NUnit → UnityEngine.Assertions düzeltmesi uygulandı:\n" + file;
            return true;
        }

        private static string FindAnimationComponentFile()
        {
            var cacheRoot = Path.Combine(Directory.GetCurrentDirectory(), "Library/PackageCache");
            if (!Directory.Exists(cacheRoot))
            {
                return null;
            }

            return Directory.GetDirectories(cacheRoot, "com.unity.cloud.gltfast@*")
                .Select(dir => Path.Combine(dir, "Runtime/Scripts/Animation/AnimationPlayableComponent.cs"))
                .FirstOrDefault(File.Exists);
        }
    }

    public sealed class GltfastIosBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            GltfastIosBuildFix.TryPatchAnimationComponent(out var message);
            Debug.Log("[GltfastIosBuildFix] " + message);
        }
    }
}
#endif
