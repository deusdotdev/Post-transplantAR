using System;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Her anatomi butonunun hemen altında açılan satır içi bilgi kutuları (accordion).
    /// </summary>
    public sealed class LiverAnatomyInfoPanel : MonoBehaviour
    {
        [Serializable]
        public sealed class AnatomyDetailSlot
        {
            public GameObject detailRoot;
            public LayoutElement layoutElement;
            public Text titleText;
            public Text bodyText;

            [NonSerialized] public float currentHeight;
            [NonSerialized] public float targetHeight;
        }

        [SerializeField] private AnatomyDetailSlot rightLobeSlot;
        [SerializeField] private AnatomyDetailSlot leftLobeSlot;
        [SerializeField] private AnatomyDetailSlot bileDuctSlot;

        [SerializeField] private float expandedHeight = 210f;
        [SerializeField] private float expandSpeed = 900f;

        private AnatomyDetailSlot[] _slots;
        private int _activeIndex = -1;

        private void Awake()
        {
            _slots = new[] { rightLobeSlot, leftLobeSlot, bileDuctSlot };
            CollapseAllImmediate();
        }

        private void Update()
        {
            if (_slots == null)
            {
                return;
            }

            foreach (var slot in _slots)
            {
                AnimateSlot(slot);
            }
        }

        public void ShowRightLobe()
        {
            ShowSlot(0, "Sağ lob",
                "Karaciğerin yaklaşık dörtte üçünü oluşturur. Kanı süzer, zararlı maddeleri parçalar, " +
                "protein ve enerji depolar.\n\n" +
                "Nakil sonrası: Bu bölgenin iyi kanlanması çok önemlidir. Doku yavaş yavaş iyileşirken " +
                "sağ lobun yeniden çalışması, genel toparlanmanın göstergesidir.\n\n" +
                "Dikkat: Karın ağrısı, ateş veya idrar/kakı renginde belirgin değişiklik olursa ekibinize bildirin.");
        }

        public void ShowLeftLobe()
        {
            ShowSlot(1, "Sol lob",
                "Sağ loba göre daha küçüktür; karaciğerin sol üst bölümünü oluşturur.\n\n" +
                "Nakil sonrası: Çoğu nakilde greft (nakledilen parça) sağ lobdan alınır. Gövdede kalan sol lob " +
                "ve sağ lobun kenarı zamanla yenilenerek karaciğer işlevini sürdürebilir.\n\n" +
                "Bu bölge de kan testleri (AST, ALT, bilirubin) ile birlikte takip edilir; tek başına " +
                "küçük olması normaldir, önemli olan işlevin korunmasıdır.");
        }

        public void ShowBileDuct()
        {
            ShowSlot(2, "Safra yolları",
                "Safra, karaciğerde üretilir ve ince kanallarla safra kesesine, oradan bağırsağa taşınır. " +
                "Yağların sindirilmesine yardımcı olur.\n\n" +
                "Nakil sonrası: Bağlantı yerlerinde darlık, sızıntı veya tıkanıklık gelişebilir. " +
                "Buna bağlı sarılık (cilt ve gözlerde sararma), koyu idrar veya açık renkli dışkı görülebilir.\n\n" +
                "Bu belirtilerden biri olursa gecikmeden transplant ekibinizle iletişime geçin; erken müdahale " +
                "çoğu sorunu daha kolay yönetilebilir kılar.");
        }

        public void Hide()
        {
            _activeIndex = -1;
            if (_slots == null)
            {
                return;
            }

            foreach (var slot in _slots)
            {
                if (slot != null)
                {
                    slot.targetHeight = 0f;
                }
            }
        }

        private void ShowSlot(int index, string title, string body)
        {
            if (_slots == null || index < 0 || index >= _slots.Length)
            {
                return;
            }

            if (_activeIndex == index && _slots[index].targetHeight > 0f)
            {
                Hide();
                return;
            }

            _activeIndex = index;

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null)
                {
                    continue;
                }

                if (i == index)
                {
                    if (slot.titleText != null)
                    {
                        slot.titleText.text = title;
                    }

                    if (slot.bodyText != null)
                    {
                        slot.bodyText.text = body;
                    }

                    if (slot.detailRoot != null)
                    {
                        slot.detailRoot.SetActive(true);
                    }

                    slot.targetHeight = expandedHeight;
                }
                else
                {
                    slot.targetHeight = 0f;
                }
            }
        }

        private void AnimateSlot(AnatomyDetailSlot slot)
        {
            if (slot?.layoutElement == null)
            {
                return;
            }

            slot.currentHeight = Mathf.MoveTowards(slot.currentHeight, slot.targetHeight,
                expandSpeed * Time.deltaTime);
            slot.layoutElement.preferredHeight = slot.currentHeight;

            if (slot.targetHeight <= 0f && slot.currentHeight <= 0.5f)
            {
                slot.currentHeight = 0f;
                slot.layoutElement.preferredHeight = 0f;

                if (slot.detailRoot != null)
                {
                    slot.detailRoot.SetActive(false);
                }
            }
        }

        private void CollapseAllImmediate()
        {
            if (_slots == null)
            {
                return;
            }

            foreach (var slot in _slots)
            {
                if (slot == null)
                {
                    continue;
                }

                slot.currentHeight = 0f;
                slot.targetHeight = 0f;

                if (slot.layoutElement != null)
                {
                    slot.layoutElement.preferredHeight = 0f;
                }

                if (slot.detailRoot != null)
                {
                    slot.detailRoot.SetActive(false);
                }
            }
        }
    }
}
