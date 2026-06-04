#if UNITY_EDITOR
using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime.Screens;
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LiverAR.EditorTools
{
    /// <summary>AR ve önizleme sahneleri için ortak eğitim arayüzü kurulumu.</summary>
    public static class EducationUIBuilder
    {
        public sealed class BuiltUI
        {
            public SafetyDisclaimerScreen Disclaimer;
            public AnatomyIntroScreen AnatomyIntro;
            public EducationFlowController Flow;
            public SimulationController Simulation;
            public ScenarioHUD Hud;
            public ClinicalDashboard Dashboard;
            public LiverAnatomyInfoPanel InfoPanel;
            public ARSetupGuide SetupGuide;
            public Text StatusText;
            public Text SafetyText;
        }

        public static BuiltUI Build(Transform canvas, SimulationState state, bool includeArStatusStrip,
            bool compactBottomSheet = true)
        {
            var simGo = new GameObject("Simulation");
            simGo.transform.SetParent(canvas.root, false);
            var controller = simGo.AddComponent<SimulationController>();
            SetField(controller, "state", state);

            if (!compactBottomSheet)
            {
                var backdrop = CreatePanel(canvas, "Backdrop", Vector2.zero, Vector2.one,
                    Vector2.zero, Vector2.zero, new Color(0.03f, 0.05f, 0.09f, 0.97f));
                backdrop.transform.SetAsFirstSibling();
                CreateViewportHint(canvas,
                    "Karaciğer modeli bu üst alanda görünür (Editor önizleme).");
            }

            var disclaimer = BuildDisclaimer(canvas);
            var intro = BuildAnatomyIntro(canvas);
            var educationRoot = BuildEducationPanel(canvas, controller, compactBottomSheet,
                out var hud, out var dashboard, out var info);

            if (compactBottomSheet)
            {
                CreateViewportHint(canvas,
                    "AR kamera görüntüsü — üst alan boş bırakıldı (bilerek).");
            }

            // Giriş / uyarı / eğitim katmanı: modal her zaman en üstte kalsın.
            disclaimer.transform.SetAsLastSibling();
            intro.transform.SetAsLastSibling();
            educationRoot.transform.SetAsLastSibling();

            var flowGo = new GameObject("Education Flow");
            flowGo.transform.SetParent(canvas, false);
            var flow = flowGo.AddComponent<EducationFlowController>();
            SetField(flow, "disclaimerScreen", disclaimer);
            SetField(flow, "anatomyIntro", intro);
            SetField(flow, "educationPanelRoot", educationRoot);

            ARSetupGuide setupGuide = null;
            Text status = null;
            Text safety = null;
            if (includeArStatusStrip)
            {
                setupGuide = canvas.gameObject.GetComponent<ARSetupGuide>()
                               ?? canvas.gameObject.AddComponent<ARSetupGuide>();
                var statusBg = CreatePanel(canvas, "StatusStrip",
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), Vector2.zero,
                    UITheme.BackgroundDark);
                CreateAccentBar(statusBg.transform, 6f);
                status = CreateBarText(statusBg.transform, "StatusText", -36f, 72f,
                    TextAnchor.MiddleCenter, 24, "Cihaz uyumluluğu kontrol ediliyor...", UITheme.TextPrimary);
                safety = CreateText(canvas, "SafetyText",
                    new Vector2(0f, 0f), new Vector2(0f, 28f), TextAnchor.LowerCenter, 18,
                    "Bu uygulama tanı koymaz; yalnızca eğitim amaçlıdır.", UITheme.TextMuted);
                safety.rectTransform.anchorMin = new Vector2(0f, 0f);
                safety.rectTransform.anchorMax = new Vector2(1f, 0f);
                safety.rectTransform.sizeDelta = new Vector2(0f, 48f);
                SetField(setupGuide, "statusText", status);
                SetField(setupGuide, "safetyText", safety);
                statusBg.transform.SetAsLastSibling();
            }

            return new BuiltUI
            {
                Disclaimer = disclaimer,
                AnatomyIntro = intro,
                Flow = flow,
                Simulation = controller,
                Hud = hud,
                Dashboard = dashboard,
                InfoPanel = info,
                SetupGuide = setupGuide,
                StatusText = status,
                SafetyText = safety
            };
        }

        private static GameObject BuildEducationPanel(Transform canvas, SimulationController controller,
            bool compactBottomSheet, out ScenarioHUD hud, out ClinicalDashboard dashboard,
            out LiverAnatomyInfoPanel info)
        {
            var root = new GameObject("EducationPanel");
            root.transform.SetParent(canvas, false);
            var rootRt = root.AddComponent<RectTransform>();
            if (compactBottomSheet)
            {
                rootRt.anchorMin = new Vector2(0f, 0f);
                rootRt.anchorMax = new Vector2(1f, 0f);
                rootRt.pivot = new Vector2(0.5f, 0f);
                rootRt.offsetMin = Vector2.zero;
                rootRt.offsetMax = new Vector2(0f, 920f);
            }
            else
            {
                Stretch(rootRt);
            }

            LiverAnatomyInfoPanel infoPanel = null;

            // --- Ana menü (4 senaryo + anatomi) ---
            var mainMenu = CreateGlassPanel(root.transform, "MainMenuPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateAccentBar(mainMenu.transform, 6f, UITheme.Primary);
            CreateBarText(mainMenu.transform, "MenuTitle", -28f, 52f, TextAnchor.MiddleCenter, 30,
                "Post-transplantAR", UITheme.TextPrimary);
            CreateBarText(mainMenu.transform, "MenuSubtitle", -78f, 44f, TextAnchor.MiddleCenter, 20,
                "Bir senaryo seçin veya anatomi rehberine gidin.", UITheme.TextMuted);

            // Kontrolcü kökte kalmalı; MainMenuPanel kapanınca inactive olursa «Ana menü» onClick çalışmaz.
            var menuGo = new GameObject("MenuController");
            menuGo.transform.SetParent(root.transform, false);
            var menu = menuGo.AddComponent<EducationMenuController>();
            var nav = menuGo.AddComponent<MenuNavigationActions>();
            SetField(menu, "controller", controller);
            SetField(nav, "menu", menu);

            float y = 520f;
            const float step = 100f;
            const float bw = 520f;
            const float bh = 82f;
            CreateButton(mainMenu.transform, "BtnMenuRecovery", "Onarım süreci", new Vector2(0f, y),
                UITheme.Primary, menu.OpenRecovery, bw, bh);
            y -= step;
            CreateButton(mainMenu.transform, "BtnMenuMedication", "İlaç uyumu", new Vector2(0f, y),
                UITheme.Primary, menu.OpenMedication, bw, bh);
            y -= step;
            CreateButton(mainMenu.transform, "BtnMenuRejection", "Red / rejeksiyon", new Vector2(0f, y),
                UITheme.Warning, menu.OpenRejection, bw, bh);
            y -= step;
            CreateButton(mainMenu.transform, "BtnMenuLifestyle", "Yaşam tarzı", new Vector2(0f, y),
                UITheme.Success, menu.OpenLifestyle, bw, bh);
            y -= step + 12f;
            CreateButton(mainMenu.transform, "BtnMenuAnatomy", "Anatomi rehberi", new Vector2(0f, y),
                UITheme.PanelAccent, menu.ShowAnatomyMenu, bw, 72f);

            // --- Senaryo ekranı (yalnızca ilgili aksiyonlar) ---
            var scenario = CreateGlassPanel(root.transform, "ScenarioPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scenario.SetActive(false);
            CreateAccentBar(scenario.transform, 6f, UITheme.Primary);

            var header = CreateBarText(scenario.transform, "HeaderText", -168f, 44f,
                TextAnchor.MiddleCenter, 28, "Senaryo", UITheme.TextPrimary);
            var desc = CreateBarText(scenario.transform, "DescriptionText", -212f, 88f,
                TextAnchor.MiddleCenter, 20, "", UITheme.TextMuted);

            dashboard = BuildDashboard(scenario.transform, controller);

            var recoveryActions = CreateActionGroup(scenario.transform, "RecoveryActions", 120f);
            CreateButton(recoveryActions.transform, "BtnNextWeek", "Sonraki hafta", new Vector2(0f, 0f),
                UITheme.PanelAccent, null, 400f, 72f);

            var medicationActions = CreateActionGroup(scenario.transform, "MedicationActions", 120f);
            medicationActions.SetActive(false);
            CreateButton(medicationActions.transform, "BtnTakeMed", "İlacı al", new Vector2(-110f, 0f),
                UITheme.Success, null, 220f, 72f);
            CreateButton(medicationActions.transform, "BtnSkipMed", "İlacı atla", new Vector2(110f, 0f),
                UITheme.Danger, null, 220f, 72f);

            var rejectionActions = CreateActionGroup(scenario.transform, "RejectionActions", 120f);
            rejectionActions.SetActive(false);
            CreateButton(rejectionActions.transform, "BtnAdvanceRej", "Reddi ilerlet", new Vector2(0f, 0f),
                UITheme.Warning, null, 400f, 72f);

            var lifestyleActions = CreateActionGroup(scenario.transform, "LifestyleActions", 120f);
            lifestyleActions.SetActive(false);
            CreateButton(lifestyleActions.transform, "BtnHealthy", "Sağlıklı yaşam", new Vector2(-110f, 0f),
                UITheme.Success, null, 220f, 72f);
            CreateButton(lifestyleActions.transform, "BtnFatty", "Yağlı diyet", new Vector2(110f, 0f),
                UITheme.Danger, null, 220f, 72f);

            var scenarioHudGo = new GameObject("ScenarioHUD");
            scenarioHudGo.transform.SetParent(scenario.transform, false);
            hud = scenarioHudGo.AddComponent<ScenarioHUD>();
            SetField(hud, "controller", controller);
            SetField(hud, "headerText", header);
            SetField(hud, "descriptionText", desc);
            SetField(hud, "clinicalDashboard", dashboard);

            // Aksiyon butonlarını HUD metotlarına bağla (menü zaten senaryoyu başlatır)
            WireButton(scenario.transform, "BtnNextWeek", hud.OnNextWeek);
            WireButton(scenario.transform, "BtnTakeMed", hud.OnTakeMedication);
            WireButton(scenario.transform, "BtnSkipMed", hud.OnSkipMedication);
            WireButton(scenario.transform, "BtnAdvanceRej", hud.OnAdvanceRejection);
            WireButton(scenario.transform, "BtnHealthy", hud.OnHealthyLifestyle);
            WireButton(scenario.transform, "BtnFatty", hud.OnFattyDiet);

            CreateTopBackBar(scenario.transform, nav);

            SetField(menu, "scenarioHud", hud);
            SetField(menu, "mainMenuPanel", mainMenu);
            SetField(menu, "scenarioPanel", scenario);
            SetField(menu, "recoveryActions", recoveryActions);
            SetField(menu, "medicationActions", medicationActions);
            SetField(menu, "rejectionActions", rejectionActions);
            SetField(menu, "lifestyleActions", lifestyleActions);

            // --- Anatomi alt menüsü ---
            var anatomy = CreateGlassPanel(root.transform, "AnatomyPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            anatomy.SetActive(false);
            CreateBarText(anatomy.transform, "AnatomyTitle", -100f, 48f, TextAnchor.MiddleCenter, 26,
                "Anatomi rehberi", UITheme.TextPrimary);
            infoPanel = BuildInfoOverlay(root.transform);

            CreateButton(anatomy.transform, "BtnRightLobe", "Sağ lob", new Vector2(0f, 520f),
                UITheme.PanelAccent, infoPanel.ShowRightLobe, 480f, 72f);
            CreateButton(anatomy.transform, "BtnLeftLobe", "Sol lob", new Vector2(0f, 420f),
                UITheme.PanelAccent, infoPanel.ShowLeftLobe, 480f, 72f);
            CreateButton(anatomy.transform, "BtnBile", "Safra yolları", new Vector2(0f, 320f),
                UITheme.PanelAccent, infoPanel.ShowBileDuct, 480f, 72f);
            CreateButton(anatomy.transform, "BtnCloseInfo", "Kartı kapat", new Vector2(0f, 200f),
                UITheme.PrimaryDark, infoPanel.Hide, 320f, 64f);

            CreateTopBackBar(anatomy.transform, nav);

            SetField(menu, "anatomyPanel", anatomy);
            SetField(menu, "anatomyInfo", infoPanel);

            info = infoPanel;
            return root;
        }

        /// <summary>Üst şerit: geri butonu her zaman tıklanabilir (dashboard altında kalmaz).</summary>
        private static void CreateTopBackBar(Transform panel, MenuNavigationActions nav)
        {
            var bar = new GameObject("TopNavigationBar");
            bar.transform.SetParent(panel, false);
            var barRt = bar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0f, 1f);
            barRt.anchorMax = new Vector2(1f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.anchoredPosition = Vector2.zero;
            barRt.sizeDelta = new Vector2(0f, 72f);

            var back = new GameObject("BtnBackMain");
            back.transform.SetParent(bar.transform, false);
            var rt = back.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -8f);
            rt.sizeDelta = new Vector2(220f, 56f);

            var img = back.AddComponent<Image>();
            img.color = UITheme.PrimaryDark;
            var button = back.AddComponent<Button>();
            back.AddComponent<MenuBackButton>();
            if (nav != null)
            {
                UnityEventTools.AddVoidPersistentListener(button.onClick, nav.BackToMainMenu);
            }

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(back.transform, false);
            Stretch(lblGo.AddComponent<RectTransform>());
            var lbl = lblGo.AddComponent<Text>();
            lbl.font = GetFont();
            lbl.fontSize = 22;
            lbl.fontStyle = FontStyle.Bold;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = UITheme.TextPrimary;
            lbl.text = "← Ana menü";

            bar.transform.SetAsLastSibling();
        }

        private static void CreateViewportHint(Transform canvas, string message)
        {
            var hint = CreateBarText(canvas, "ViewportHint", -200f, 56f,
                TextAnchor.MiddleCenter, 20, message,
                new Color(0.55f, 0.62f, 0.72f, 0.55f));
            hint.raycastTarget = false;
        }

        private static GameObject CreateGlassPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            return CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, UITheme.GlassPanel);
        }

        private static GameObject CreateActionGroup(Transform parent, string name, float bottom)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, bottom);
            rt.sizeDelta = new Vector2(600f, 80f);
            return go;
        }

        private static void WireButton(Transform root, string buttonName, UnityAction action)
        {
            if (action == null)
            {
                return;
            }

            foreach (var btn in root.GetComponentsInChildren<Button>(true))
            {
                if (btn.gameObject.name == buttonName)
                {
                    UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
                    return;
                }
            }
        }

        private static void CreateAccentBar(Transform parent, float height, Color? color = null)
        {
            var bar = CreatePanel(parent, "AccentBar",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -height), Vector2.zero,
                color ?? UITheme.Primary);
        }

        private static ClinicalDashboard BuildDashboard(Transform parent, SimulationController controller)
        {
            var panel = CreatePanel(parent, "ClinicalDashboard",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -300f), new Vector2(-20f, -640f),
                new Color(0.09f, 0.12f, 0.18f, 0.98f));
            panel.GetComponent<Image>().raycastTarget = true;

            var healthRow = CreatePanel(panel.transform, "HealthRow",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -72f), new Vector2(-16f, -16f),
                new Color(0.06f, 0.08f, 0.12f, 1f));

            var fillBg = CreatePanel(healthRow.transform, "HealthBg",
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(12f, -12f), new Vector2(-12f, 12f),
                new Color(0.18f, 0.2f, 0.26f, 1f));
            var fill = CreatePanel(fillBg.transform, "HealthFill",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UITheme.HealthGood);
            var fillImg = fill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;

            var healthLabel = CreateBarText(healthRow.transform, "HealthLabel", -28f, 40f,
                TextAnchor.MiddleLeft, 22, "Organ sağlığı %100", UITheme.TextPrimary);
            healthLabel.rectTransform.offsetMin = new Vector2(16f, healthLabel.rectTransform.offsetMin.y);
            healthLabel.rectTransform.offsetMax = new Vector2(-16f, healthLabel.rectTransform.offsetMax.y);

            var ast = CreateBarText(panel.transform, "AST", -120f, 36f,
                TextAnchor.MiddleLeft, 20, "AST —", UITheme.TextMuted);
            ast.rectTransform.offsetMin = new Vector2(20f, ast.rectTransform.offsetMin.y);
            var alt = CreateBarText(panel.transform, "ALT", -120f, 36f,
                TextAnchor.MiddleCenter, 20, "ALT —", UITheme.TextMuted);
            var bili = CreateBarText(panel.transform, "Bili", -120f, 36f,
                TextAnchor.MiddleRight, 20, "Bilirubin —", UITheme.TextMuted);
            bili.rectTransform.offsetMax = new Vector2(-20f, bili.rectTransform.offsetMax.y);

            var warning = CreatePanel(panel.transform, "Warning",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 56f),
                UITheme.Danger);
            warning.SetActive(false);
            var warnText = CreateBarText(warning.transform, "WarnText", -28f, 44f,
                TextAnchor.MiddleCenter, 19, "Uyarı", UITheme.TextPrimary);

            var rej = CreatePanel(panel.transform, "RejectionDetail",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 64f), new Vector2(-12f, 220f),
                new Color(0.14f, 0.1f, 0.12f, 1f));
            rej.SetActive(false);
            var sym = CreateBarText(rej.transform, "Symptom", -8f, 52f, TextAnchor.UpperLeft, 17,
                "Semptom:", UITheme.TextMuted);
            sym.rectTransform.offsetMin = new Vector2(14f, sym.rectTransform.offsetMin.y);
            var clin = CreateBarText(rej.transform, "Clinical", -60f, 52f, TextAnchor.UpperLeft, 17,
                "Klinik:", UITheme.TextMuted);
            clin.rectTransform.offsetMin = new Vector2(14f, clin.rectTransform.offsetMin.y);
            var act = CreateBarText(rej.transform, "Action", -112f, 56f, TextAnchor.UpperLeft, 17,
                "Eylem:", UITheme.Warning);
            act.rectTransform.offsetMin = new Vector2(14f, act.rectTransform.offsetMin.y);

            var medRoot = CreatePanel(panel.transform, "MedReminder",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 230f), new Vector2(-12f, 278f),
                UITheme.Panel);
            medRoot.SetActive(false);
            var medLabel = CreateBarText(medRoot.transform, "MedLabel", -24f, 40f,
                TextAnchor.MiddleLeft, 19, "Bugün immünosupresif ilacını aldım", UITheme.TextPrimary);
            medLabel.rectTransform.offsetMin = new Vector2(16f, medLabel.rectTransform.offsetMin.y);

            var toggleGo = new GameObject("MedToggle");
            toggleGo.transform.SetParent(medRoot.transform, false);
            var tRt = toggleGo.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(1f, 0.5f);
            tRt.anchorMax = new Vector2(1f, 0.5f);
            tRt.sizeDelta = new Vector2(44f, 44f);
            tRt.anchoredPosition = new Vector2(-28f, 0f);
            toggleGo.AddComponent<Toggle>();

            var dash = panel.AddComponent<ClinicalDashboard>();
            SetField(dash, "controller", controller);
            SetField(dash, "healthFill", fillImg);
            SetField(dash, "healthLabel", healthLabel);
            SetField(dash, "astText", ast);
            SetField(dash, "altText", alt);
            SetField(dash, "bilirubinText", bili);
            SetField(dash, "warningRoot", warning);
            SetField(dash, "warningText", warnText);
            SetField(dash, "rejectionDetailRoot", rej);
            SetField(dash, "symptomText", sym);
            SetField(dash, "clinicalDetailText", clin);
            SetField(dash, "actionText", act);
            SetField(dash, "medicationReminderRoot", medRoot);
            SetField(dash, "medicationReminderLabel", medLabel);
            SetField(dash, "medicationTakenToggle", toggleGo.GetComponent<Toggle>());

            return dash;
        }

        /// <summary>Tam ekran karartma + kart; tüm panellerden sonra eklenir (üstte çizilir).</summary>
        private static LiverAnatomyInfoPanel BuildInfoOverlay(Transform educationRoot)
        {
            var overlay = CreatePanel(educationRoot, "InfoOverlayLayer",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.62f));
            overlay.SetActive(false);
            var dimmerImg = overlay.GetComponent<Image>();
            dimmerImg.raycastTarget = true;

            var card = CreatePanel(overlay.transform, "InfoCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-360f, -240f), new Vector2(360f, 240f),
                UITheme.BackgroundDark);
            var cardImg = card.GetComponent<Image>();
            cardImg.raycastTarget = true;
            CreateAccentBar(card.transform, 6f, UITheme.Primary);

            var title = CreateBarText(card.transform, "Title", -24f, 52f,
                TextAnchor.MiddleCenter, 30, "Bilgi", UITheme.TextPrimary);
            var body = CreateBarText(card.transform, "Body", -300f, 200f,
                TextAnchor.UpperCenter, 22, "", UITheme.TextMuted);

            var closeBtn = CreateButton(card.transform, "BtnCardClose", "Kapat",
                new Vector2(0f, 24f), UITheme.Primary, null, 220f, 56f);

            var infoGo = new GameObject("AnatomyInfo");
            infoGo.transform.SetParent(educationRoot, false);
            var info = infoGo.AddComponent<LiverAnatomyInfoPanel>();
            SetField(info, "overlayRoot", overlay);
            SetField(info, "cardRoot", card);
            SetField(info, "titleText", title);
            SetField(info, "bodyText", body);
            SetField(info, "closeButton", closeBtn.GetComponent<Button>());
            UnityEventTools.AddVoidPersistentListener(closeBtn.GetComponent<Button>().onClick, info.Hide);

            overlay.transform.SetAsLastSibling();
            return info;
        }

        private static SafetyDisclaimerScreen BuildDisclaimer(Transform canvas)
        {
            var panel = CreatePanel(canvas, "SafetyDisclaimerPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.04f, 0.08f, 0.98f));
            CreateAccentBar(panel.transform, 8f, UITheme.Danger);
            var msg = CreateBarText(panel.transform, "DisclaimerText", -200f, 700f,
                TextAnchor.MiddleCenter, 26, "", UITheme.TextPrimary);
            var btn = CreateButton(panel.transform, "AcknowledgeButton", "Anladım, Devam Et",
                new Vector2(0f, 180f), UITheme.Primary, null, 500f, 96f);
            var disclaimer = panel.AddComponent<SafetyDisclaimerScreen>();
            SetField(disclaimer, "panelRoot", panel);
            SetField(disclaimer, "messageText", msg);
            SetField(disclaimer, "acknowledgeButton", btn.GetComponent<Button>());
            return disclaimer;
        }

        private static AnatomyIntroScreen BuildAnatomyIntro(Transform canvas)
        {
            var panel = CreatePanel(canvas, "AnatomyIntroPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.04f, 0.08f, 0.14f, 0.98f));
            panel.SetActive(false);
            CreateAccentBar(panel.transform, 8f, UITheme.Success);
            var msg = CreateBarText(panel.transform, "IntroText", -180f, 760f,
                TextAnchor.MiddleCenter, 25, "", UITheme.TextPrimary);
            var intro = panel.AddComponent<AnatomyIntroScreen>();
            SetField(intro, "panelRoot", panel);
            SetField(intro, "messageText", msg);
            var btnGo = CreateButton(panel.transform, "IntroContinue", "Eğitime Başla",
                new Vector2(0f, 160f), UITheme.Success, null, 460f, 92f);
            SetField(intro, "continueButton", btnGo.GetComponent<Button>());
            return intro;
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 pos, Color color, UnityAction action, float w = 240f, float h = 88f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -3f);

            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            cb.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            cb.selectedColor = color;
            btn.colors = cb;

            if (action != null)
            {
                UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
            }

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            Stretch(lblGo.AddComponent<RectTransform>());
            var lbl = lblGo.AddComponent<Text>();
            lbl.font = GetFont();
            lbl.fontSize = Mathf.Max(18, Mathf.RoundToInt(h * 0.3f));
            lbl.fontStyle = FontStyle.Bold;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = UITheme.TextPrimary;
            lbl.text = label;
            return go;
        }

        /// <summary>Üst şerit metni — ortadaki beyaz çizgi artefaktını önler.</summary>
        private static Text CreateBarText(Transform parent, string name, float topOffset, float height,
            TextAnchor align, int size, string content, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topOffset);
            rt.sizeDelta = new Vector2(0f, height);
            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = size;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            return text;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchor,
            Vector2 pos, TextAnchor align, int size, string content, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(600f, 80f);
            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = size;
            text.alignment = align;
            text.color = color;
            text.text = content;
            return text;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[EducationUIBuilder] Alan yok: {field} ({target.GetType().Name})");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
