#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// Eski sahne dosyalarından kaldırılan senaryo UI parçalarını temizler.
    /// </summary>
    public static class EducationScenePatcher
    {
        private static readonly string[] LegacyObjectNames =
        {
            "BtnMenuRejection",
            "BtnMenuLifestyle",
            "RejectionActions",
            "LifestyleActions",
            "RejectionDetail"
        };

        [MenuItem("Post-transplantAR/Remove Legacy Scenario UI (Active Scene)", false, 20)]
        public static void RemoveLegacyScenarioUi()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play modunu kapat", "Bu işlem Play modunda çalışmaz.", "Tamam");
                return;
            }

            var removed = 0;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (t == null)
                {
                    continue;
                }

                foreach (var legacyName in LegacyObjectNames)
                {
                    if (t.name != legacyName)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(t.gameObject);
                    removed++;
                    break;
                }
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            EditorUtility.DisplayDialog("UI temizliği",
                removed > 0
                    ? $"{removed} eski UI öğesi silindi.\nSahneyi kaydet (Ctrl+S)."
                    : "Silinecek eski buton bulunamadı.\nMenü hâlâ eskiyse: Build AR Scene çalıştır.",
                "Tamam");
            Debug.Log($"[EducationScenePatcher] Removed {removed} legacy UI object(s).");
        }

        [MenuItem("Post-transplantAR/Remove Legacy Scenario UI (Active Scene)", true)]
        private static bool RemoveLegacyScenarioUiValidate()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }
}
#endif
