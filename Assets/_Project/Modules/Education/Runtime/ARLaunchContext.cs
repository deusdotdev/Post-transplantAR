using UnityEngine;

namespace LiverAR.Modules.Education.Runtime
{
    /// <summary>
    /// Ana ekran ile AR sahnesi arasında "hangi modda açılacağı" bilgisini taşır.
    /// ScriptableObject sahne yüklemeleri arasında bellekte kalır; SimulationState ile aynı idiom.
    /// </summary>
    [CreateAssetMenu(fileName = "ARLaunchContext", menuName = "LiverAR/AR Launch Context")]
    public sealed class ARLaunchContext : ScriptableObject
    {
        public enum Mode
        {
            ExploreAnatomy,
            DrugRegion
        }

        [Tooltip("AR sahnesi açılırken kullanılacak mod.")]
        public Mode CurrentMode = Mode.ExploreAnatomy;
    }
}
