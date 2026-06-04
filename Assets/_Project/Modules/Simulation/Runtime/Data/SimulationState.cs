using UnityEngine;

namespace LiverAR.Modules.Simulation.Runtime.Data
{
    public enum ScenarioType
    {
        None,
        Recovery,
        Medication,
        Rejection,
        Lifestyle
    }

    /// <summary>
    /// Simülasyonun tek doğruluk kaynağı (single source of truth).
    /// Davranış script'leri (controller, görsel, UI) bu veriyi okur/yazar.
    /// ScriptableObject olduğu için sahneden ve mantıktan bağımsızdır.
    /// </summary>
    [CreateAssetMenu(fileName = "SimulationState", menuName = "LiverAR/Simulation State")]
    public sealed class SimulationState : ScriptableObject
    {
        [Header("Akış")]
        public ScenarioType CurrentScenario = ScenarioType.None;
        public int SimulationWeek = 1;

        [Header("Rejenerasyon")]
        [Range(0.3f, 1f)] public float GrowthPercentage = 0.3f;

        [Header("İlaç & Bağışıklık")]
        public bool IsAdherent = true;
        public bool ImmuneAttack = false;
        [Range(0f, 100f)] public float HealthPoints = 100f;

        [Header("Klinik Değerler")]
        public float AST = 25f;        // Normal: 10-40 U/L
        public float ALT = 30f;        // Normal: 7-56 U/L
        public float Bilirubin = 0.8f; // Normal: 0.1-1.2 mg/dL

        [Header("Red / Rejeksiyon")]
        public int RejectionStage = 0;              // 0 yok, 1 erken, 2 akut, 3 kronik
        [Range(0f, 1f)] public float VascularOcclusion = 0f; // damar tıkanıklığı
        [Range(0f, 1f)] public float FibrosisFactor = 0f;    // fibrozis derecesi
        public bool IsRejecting = false;

        [Header("Yaşam Tarzı")]
        public float NutritionMultiplier = 1f;
        public float ExerciseMultiplier = 1f;
        public bool IsFattyDiet = false;

        /// <summary>
        /// ScriptableObject değerleri Editor oturumları arasında kalıcıdır;
        /// senaryo başında bilinen bir başlangıca döndürmek için sıfırlanır.
        /// </summary>
        public void ResetToDefault()
        {
            SimulationWeek = 1;
            GrowthPercentage = 0.3f;
            IsAdherent = true;
            ImmuneAttack = false;
            HealthPoints = 100f;
            AST = 25f;
            ALT = 30f;
            Bilirubin = 0.8f;

            RejectionStage = 0;
            VascularOcclusion = 0f;
            FibrosisFactor = 0f;
            IsRejecting = false;

            NutritionMultiplier = 1f;
            ExerciseMultiplier = 1f;
            IsFattyDiet = false;
        }
    }
}
