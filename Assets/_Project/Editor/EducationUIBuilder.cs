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
        /// <summary>Alt menü yüksekliği (1080×1920 referans); üstte AR/kamera alanı kalır.</summary>
        public const float CompactSheetHeight = 520f;
        public const float RefScreenHeight = 1920f;
        private const float CompactButtonWidth = 540f;
        private const float CompactTextMarginH = 6f;
        private const float CompactButtonHeight = 64f;
        private const float CompactButtonStep = 76f;

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
            bool compactBottomSheet = true, bool buildScenarioMenu = true)
        {
            var simGo = new GameObject("Simulation");
            simGo.transform.SetParent(canvas.root, false);
            var controller = simGo.AddComponent<SimulationController>();
            SetField(controller, "state", state);

            if (!compactBottomSheet)
            {
                var backdrop = CreatePanel(canvas, "Backdrop", Vector2.zero, Vector2.one,
                    Vector2.zero, Vector2.zero, UITheme.ViewportBackground);
                backdrop.transform.SetAsFirstSibling();
            }

            var disclaimer = BuildDisclaimer(canvas);
            var intro = BuildAnatomyIntro(canvas);

            GameObject educationRoot = null;
            ScenarioHUD hud = null;
            ClinicalDashboard dashboard = null;
            LiverAnatomyInfoPanel info = null;
            if (buildScenarioMenu)
            {
                educationRoot = BuildEducationPanel(canvas, controller, compactBottomSheet,
                    out hud, out dashboard, out info);
            }

            // Giriş / uyarı / eğitim katmanı: modal her zaman en üstte kalsın.
            disclaimer.transform.SetAsLastSibling();
            intro.transform.SetAsLastSibling();
            if (educationRoot != null)
            {
                educationRoot.transform.SetAsLastSibling();
            }

            var flowGo = new GameObject("Education Flow");
            flowGo.transform.SetParent(canvas, false);
            var flow = flowGo.AddComponent<EducationFlowController>();
            SetField(flow, "disclaimerScreen", disclaimer);
            SetField(flow, "anatomyIntro", intro);
            if (educationRoot != null)
            {
                SetField(flow, "educationPanelRoot", educationRoot);
            }

            ARSetupGuide setupGuide = null;
            Text status = null;
            Text safety = null;
            if (includeArStatusStrip)
            {
                setupGuide = canvas.gameObject.GetComponent<ARSetupGuide>()
                               ?? canvas.gameObject.AddComponent<ARSetupGuide>();
                var statusBg = CreatePanel(canvas, "StatusStrip",
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -140f), Vector2.zero,
                    UITheme.Transparent);
                statusBg.GetComponent<Image>().raycastTarget = false;
                status = CreateBarText(statusBg.transform, "StatusText", -8f, 120f,
                    TextAnchor.MiddleCenter, 22, "Cihaz uyumluluğu kontrol ediliyor...", UITheme.TextPrimary,
                    bold: true, overlayShadow: true);
                safety = CreateText(canvas, "SafetyText",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, CompactSheetHeight + 6f),
                    TextAnchor.LowerCenter, 16,
                    "Bu uygulama tanı koymaz; yalnızca eğitim amaçlıdır.", UITheme.TextSecondary);
                safety.rectTransform.anchorMin = new Vector2(0f, 0f);
                safety.rectTransform.anchorMax = new Vector2(1f, 0f);
                safety.rectTransform.sizeDelta = new Vector2(-12f, 40f);
                AddTextShadow(safety);
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

        public static ARPlacementPrompt BuildArPlacementPrompt(Transform canvas, EducationFlowController flow,
            LiverAR.Modules.AR.Runtime.Controllers.ARPlacementController placement)
        {
            var root = CreatePanel(canvas, "ARPlacementPrompt",
                new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.92f),
                Vector2.zero, Vector2.zero, UITheme.Transparent);
            root.GetComponent<Image>().raycastTarget = false;
            CreateBarText(root.transform, "PromptTitle", -8f, 48f, TextAnchor.MiddleCenter, 26,
                "Karaciğeri yerleştir", UITheme.TextPrimary, bold: true, overlayShadow: true);
            CreateBarText(root.transform, "PromptBody", -52f, 96f, TextAnchor.MiddleCenter, 18,
                "Masaya veya zemine bak, sonra üst alana dokun veya butona bas.",
                UITheme.TextSecondary, overlayShadow: true);

            var promptGo = root.AddComponent<ARPlacementPrompt>();
            var placeBtn = CreateButton(root.transform, "BtnPlaceLiver", "Karaciğeri yerleştir",
                new Vector2(0f, -120f), UITheme.Success, null, 420f, 76f);
            UnityEventTools.AddPersistentListener(placeBtn.GetComponent<Button>().onClick,
                promptGo.OnPlaceButtonClicked);

            SetField(promptGo, "placementController", placement);
            SetField(promptGo, "educationFlow", flow);
            SetField(promptGo, "promptRoot", root);

            root.SetActive(false);
            root.transform.SetAsLastSibling();
            return promptGo;
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
                rootRt.offsetMax = new Vector2(0f, CompactSheetHeight);
                var sheetBg = CreatePanel(root.transform, "SheetBackground",
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UITheme.Transparent);
                sheetBg.GetComponent<Image>().raycastTarget = false;
                sheetBg.transform.SetAsFirstSibling();
            }
            else
            {
                Stretch(rootRt);
            }

            LiverAnatomyInfoPanel infoPanel = null;

            // --- Ana menü (4 senaryo + anatomi) ---
            var mainMenu = compactBottomSheet
                ? CreateOverlayPanel(root.transform, "MainMenuPanel")
                : CreateGlassPanel(root.transform, "MainMenuPanel",
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (!compactBottomSheet)
            {
                CreateAccentBar(mainMenu.transform, 6f, UITheme.Primary);
            }

            var menuTitleSize = compactBottomSheet ? 26 : 30;
            var menuTitleH = compactBottomSheet ? 40f : 52f;
            CreateBarText(mainMenu.transform, "MenuTitle", compactBottomSheet ? -8f : -12f, menuTitleH,
                TextAnchor.MiddleCenter, menuTitleSize,
                "Post-transplantAR", UITheme.TextPrimary, bold: true, overlayShadow: compactBottomSheet);
            CreateBarText(mainMenu.transform, "MenuSubtitle", compactBottomSheet ? -48f : -78f,
                compactBottomSheet ? 44f : 44f, TextAnchor.MiddleCenter, compactBottomSheet ? 18 : 20,
                "İyileşme veya ilaç uyumu senaryosu seçin.", UITheme.TextSecondary,
                overlayShadow: compactBottomSheet);

            // Kontrolcü kökte kalmalı; MainMenuPanel kapanınca inactive olursa «Ana menü» onClick çalışmaz.
            var menuGo = new GameObject("MenuController");
            menuGo.transform.SetParent(root.transform, false);
            var menu = menuGo.AddComponent<EducationMenuController>();
            var nav = menuGo.AddComponent<MenuNavigationActions>();
            SetField(menu, "controller", controller);
            SetField(nav, "menu", menu);

            float y;
            float step;
            float bw;
            float bh;
            if (compactBottomSheet)
            {
                y = 388f;
                step = CompactButtonStep;
                bw = CompactButtonWidth;
                bh = CompactButtonHeight;
            }
            else
            {
                y = 420f;
                step = 100f;
                bw = 520f;
                bh = 82f;
            }

            CreateButton(mainMenu.transform, "BtnMenuRecovery", "İyileşme süreci", new Vector2(0f, y),
                UITheme.Primary, menu.OpenRecovery, bw, bh);
            y -= step;
            CreateButton(mainMenu.transform, "BtnMenuMedication", "İlaç uyumu", new Vector2(0f, y),
                UITheme.Primary, menu.OpenMedication, bw, bh);
            y -= step;
            CreateButton(mainMenu.transform, "BtnMenuAnatomy", "Anatomi rehberi", new Vector2(0f, y),
                UITheme.AccentSecondary, menu.ShowAnatomyMenu, bw, bh);

            // --- Senaryo ekranı (yalnızca ilgili aksiyonlar) ---
            var scenario = compactBottomSheet
                ? CreateOverlayPanel(root.transform, "ScenarioPanel")
                : CreateGlassPanel(root.transform, "ScenarioPanel",
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scenario.SetActive(false);
            if (!compactBottomSheet)
            {
                CreateAccentBar(scenario.transform, 6f, UITheme.Primary);
            }

            var headerTop = compactBottomSheet ? -196f : -168f;
            var headerH = compactBottomSheet ? 28f : 44f;
            var headerSize = compactBottomSheet ? 20 : 28;
            var header = CreateBarText(scenario.transform, "HeaderText", headerTop, headerH,
                TextAnchor.MiddleCenter, headerSize, "Senaryo", UITheme.TextPrimary, bold: true,
                overlayShadow: compactBottomSheet);
            var descTop = compactBottomSheet ? -224f : -212f;
            var descH = compactBottomSheet ? 72f : 88f;
            var descSize = compactBottomSheet ? 17 : 20;
            var desc = CreateBarText(scenario.transform, "DescriptionText", descTop, descH,
                TextAnchor.MiddleCenter, descSize, "", UITheme.TextSecondary, overlayShadow: compactBottomSheet);

            // Klinik kart kök panelde kalır; senaryo/anatomi değişince üstte görünür.
            dashboard = BuildDashboard(root.transform, controller, compactBottomSheet);

            var actionBottom = compactBottomSheet ? 16f : 120f;
            var actionH = compactBottomSheet ? 64f : 72f;
            var recoveryActions = CreateActionGroup(scenario.transform, "RecoveryActions", actionBottom, actionH);
            CreateButton(recoveryActions.transform, "BtnNextWeek", "Sonraki haftaya geç", new Vector2(0f, 0f),
                UITheme.PrimaryDark, null, compactBottomSheet ? CompactButtonWidth : 400f, actionH);

            var medicationActions = CreateActionGroup(scenario.transform, "MedicationActions", actionBottom, actionH);
            medicationActions.SetActive(false);
            var medBtnW = compactBottomSheet ? 240f : 220f;
            CreateButton(medicationActions.transform, "BtnTakeMed", "İlacı aldım", new Vector2(-128f, 0f),
                UITheme.Success, null, medBtnW, actionH);
            CreateButton(medicationActions.transform, "BtnSkipMed", "Atladım", new Vector2(128f, 0f),
                UITheme.Danger, null, medBtnW, actionH);

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

            CreateTopBackBar(scenario.transform, nav);

            SetField(menu, "scenarioHud", hud);
            SetField(menu, "mainMenuPanel", mainMenu);
            SetField(menu, "scenarioPanel", scenario);
            SetField(menu, "recoveryActions", recoveryActions);
            SetField(menu, "medicationActions", medicationActions);

            // --- Anatomi alt menüsü (her butonun altında satır içi accordion) ---
            var anatomy = compactBottomSheet
                ? CreateOverlayPanel(root.transform, "AnatomyPanel")
                : CreateGlassPanel(root.transform, "AnatomyPanel",
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            anatomy.SetActive(false);
            var aW = compactBottomSheet ? CompactButtonWidth : 480f;
            var aH = compactBottomSheet ? CompactButtonHeight : 72f;
            var expandH = compactBottomSheet ? 210f : 260f;
            var titleTop = compactBottomSheet ? -200f : -100f;

            CreateBarText(anatomy.transform, "AnatomyTitle", titleTop, 36f, TextAnchor.MiddleCenter, 24,
                "Anatomi rehberi", UITheme.TextPrimary, bold: true, overlayShadow: compactBottomSheet);

            infoPanel = BuildAnatomyAccordionList(anatomy.transform, aW, aH, expandH, compactBottomSheet);

            WireButton(anatomy.transform, "BtnRightLobe", infoPanel.ShowRightLobe);
            WireButton(anatomy.transform, "BtnLeftLobe", infoPanel.ShowLeftLobe);
            WireButton(anatomy.transform, "BtnBile", infoPanel.ShowBileDuct);

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
            lbl.color = UITheme.TextOnPrimary;
            lbl.text = "← Ana menü";

            bar.transform.SetAsLastSibling();
        }

        private static GameObject CreateGlassPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            return CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, UITheme.GlassPanel);
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name)
        {
            var go = CreatePanel(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                UITheme.Transparent);
            go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        private static GameObject CreateActionGroup(Transform parent, string name, float bottom, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, bottom);
            rt.sizeDelta = new Vector2(CompactButtonWidth + 40f, height);
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

        public static ClinicalDashboard BuildDashboard(Transform parent, SimulationController controller,
            bool compact)
        {
            GameObject panel;
            if (compact)
            {
                panel = CreatePanel(parent, "ClinicalDashboard",
                    new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(CompactTextMarginH, -188f), new Vector2(-CompactTextMarginH, -52f),
                    UITheme.Transparent);
            }
            else
            {
                panel = CreatePanel(parent, "ClinicalDashboard",
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -300f), new Vector2(-20f, -640f),
                    UITheme.Card);
            }

            panel.GetComponent<Image>().raycastTarget = false;

            var healthRowH = compact ? 40f : 56f;
            var healthRow = CreatePanel(panel.transform, "HealthRow",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(4f, -healthRowH), new Vector2(-4f, -4f),
                compact ? UITheme.Transparent : UITheme.Panel);
            if (compact)
            {
                healthRow.GetComponent<Image>().raycastTarget = false;
            }

            var fillBg = CreatePanel(healthRow.transform, "HealthBg",
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(8f, -8f), new Vector2(-8f, 8f),
                compact ? UITheme.OverlayBarTrack : new Color(0.16f, 0.18f, 0.24f, 1f));
            fillBg.GetComponent<Image>().raycastTarget = false;
            var fill = CreatePanel(fillBg.transform, "HealthFill",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UITheme.HealthGood);
            fill.GetComponent<Image>().raycastTarget = false;
            var fillImg = fill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;

            var healthLabel = CreateBarText(healthRow.transform, "HealthLabel", -20f, 30f,
                TextAnchor.MiddleLeft, compact ? 17 : 22, "Genel durum: %100", UITheme.TextPrimary,
                overlayShadow: compact);
            healthLabel.rectTransform.offsetMin = new Vector2(10f, healthLabel.rectTransform.offsetMin.y);
            healthLabel.rectTransform.offsetMax = new Vector2(-10f, healthLabel.rectTransform.offsetMax.y);

            var labTop = compact ? -72f : -120f;
            var labH = compact ? 30f : 36f;
            var labSize = compact ? 16 : 20;
            var ast = CreateBarText(panel.transform, "AST", labTop, labH,
                TextAnchor.MiddleLeft, labSize, "AST —", UITheme.LabNormal, overlayShadow: compact);
            ast.rectTransform.offsetMin = new Vector2(8f, ast.rectTransform.offsetMin.y);
            var alt = CreateBarText(panel.transform, "ALT", labTop, labH,
                TextAnchor.MiddleCenter, labSize, "ALT —", UITheme.LabNormal, overlayShadow: compact);
            var bili = CreateBarText(panel.transform, "Bili", labTop, labH,
                TextAnchor.MiddleRight, labSize, "Bilirubin —", UITheme.LabNormal, overlayShadow: compact);
            bili.rectTransform.offsetMax = new Vector2(-8f, bili.rectTransform.offsetMax.y);

            var warning = CreatePanel(panel.transform, "Warning",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4f, 4f), new Vector2(-4f, 44f),
                compact ? UITheme.Transparent : UITheme.Danger);
            warning.SetActive(false);
            if (compact)
            {
                warning.GetComponent<Image>().raycastTarget = false;
            }

            var warnText = CreateBarText(warning.transform, "WarnText", -22f, 38f,
                TextAnchor.MiddleCenter, compact ? 15 : 19, "Uyarı",
                compact ? UITheme.Danger : UITheme.TextOnDanger, bold: compact, overlayShadow: compact);

            var medRoot = CreatePanel(panel.transform, "MedReminder",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4f, 48f), new Vector2(-4f, 92f),
                compact ? UITheme.Transparent : UITheme.PanelAccent);
            medRoot.SetActive(false);
            if (compact)
            {
                medRoot.GetComponent<Image>().raycastTarget = false;
            }

            var medLabel = CreateBarText(medRoot.transform, "MedLabel", -20f, 34f,
                TextAnchor.MiddleLeft, compact ? 15 : 19, "Bugün immünosupresif ilacımı aldım", UITheme.TextPrimary,
                overlayShadow: compact);
            medLabel.rectTransform.offsetMin = new Vector2(10f, medLabel.rectTransform.offsetMin.y);

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
            SetField(dash, "medicationReminderRoot", medRoot);
            SetField(dash, "medicationReminderLabel", medLabel);
            SetField(dash, "medicationTakenToggle", toggleGo.GetComponent<Toggle>());

            return dash;
        }

        /// <summary>Buton → detay → buton → detay … dikey liste; detay ilgili butonun hemen altında açılır.</summary>
        private static LiverAnatomyInfoPanel BuildAnatomyAccordionList(Transform anatomyPanel,
            float buttonWidth, float buttonHeight, float expandedHeight, bool compact)
        {
            var listGo = new GameObject("AnatomyList");
            listGo.transform.SetParent(anatomyPanel, false);
            var listRt = listGo.AddComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.offsetMin = new Vector2(12f, 16f);
            listRt.offsetMax = new Vector2(-12f, compact ? -228f : -120f);

            var layout = listGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(0, 0, 4, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateAnatomyLayoutButton(listGo.transform, "BtnRightLobe", "Sağ lob",
                UITheme.PanelAccent, buttonWidth, buttonHeight);
            var rightDetail = CreateAnatomyDetailSlot(listGo.transform, "DetailRightLobe",
                buttonWidth + 16f, expandedHeight, compact);

            CreateAnatomyLayoutButton(listGo.transform, "BtnLeftLobe", "Sol lob",
                UITheme.PanelAccent, buttonWidth, buttonHeight);
            var leftDetail = CreateAnatomyDetailSlot(listGo.transform, "DetailLeftLobe",
                buttonWidth + 16f, expandedHeight, compact);

            CreateAnatomyLayoutButton(listGo.transform, "BtnBile", "Safra yolları",
                UITheme.PanelAccent, buttonWidth, buttonHeight);
            var bileDetail = CreateAnatomyDetailSlot(listGo.transform, "DetailBileDuct",
                buttonWidth + 16f, expandedHeight, compact);

            var info = listGo.AddComponent<LiverAnatomyInfoPanel>();
            SetAnatomySlot(info, "rightLobeSlot", rightDetail);
            SetAnatomySlot(info, "leftLobeSlot", leftDetail);
            SetAnatomySlot(info, "bileDuctSlot", bileDetail);
            SetPropertyFloat(info, "expandedHeight", expandedHeight);
            SetPropertyFloat(info, "expandSpeed", 720f);

            return info;
        }

        private sealed class AnatomyDetailBuilt
        {
            public GameObject Root;
            public LayoutElement Layout;
            public Text Title;
            public Text Body;
        }

        private static void CreateAnatomyLayoutButton(Transform parent, string name, string label,
            Color color, float width, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.preferredWidth = width;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -3f);

            var img = go.AddComponent<Image>();
            img.color = color;
            go.AddComponent<Button>();

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            Stretch(lblGo.AddComponent<RectTransform>());
            var lbl = lblGo.AddComponent<Text>();
            lbl.font = GetFont();
            lbl.fontSize = Mathf.Max(18, Mathf.RoundToInt(height * 0.3f));
            lbl.fontStyle = FontStyle.Bold;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = UITheme.TextOnPrimary;
            lbl.text = label;
        }

        private static AnatomyDetailBuilt CreateAnatomyDetailSlot(Transform parent, string name,
            float width, float expandedHeight, bool compact)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 0f;
            le.flexibleHeight = 0f;

            var img = go.AddComponent<Image>();
            img.color = compact ? UITheme.Transparent : UITheme.Card;
            if (compact)
            {
                img.raycastTarget = false;
            }
            else
            {
                go.AddComponent<RectMask2D>();
                CreateAccentBar(go.transform, 3f, UITheme.Primary);
            }

            var titleSize = compact ? 18 : 22;
            var bodySize = compact ? 14 : 17;
            var titleH = 30f;
            var bodyH = expandedHeight - titleH - 16f;

            var title = CreateBarText(go.transform, "DetailTitle", -8f, titleH,
                TextAnchor.MiddleLeft, titleSize, "", UITheme.TextPrimary, bold: true, overlayShadow: compact);
            title.alignment = TextAnchor.MiddleLeft;
            title.rectTransform.offsetMin = new Vector2(12f, title.rectTransform.offsetMin.y);
            title.rectTransform.offsetMax = new Vector2(-12f, title.rectTransform.offsetMax.y);

            var body = CreateBarText(go.transform, "DetailBody", -titleH - 6f, bodyH,
                TextAnchor.UpperLeft, bodySize, "", UITheme.TextSecondary, overlayShadow: compact);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.rectTransform.offsetMin = new Vector2(12f, body.rectTransform.offsetMin.y);
            body.rectTransform.offsetMax = new Vector2(-12f, body.rectTransform.offsetMax.y);

            return new AnatomyDetailBuilt
            {
                Root = go,
                Layout = le,
                Title = title,
                Body = body
            };
        }

        private static void SetAnatomySlot(LiverAnatomyInfoPanel info, string slotProperty,
            AnatomyDetailBuilt built)
        {
            var so = new SerializedObject(info);
            var slot = so.FindProperty(slotProperty);
            if (slot == null)
            {
                Debug.LogWarning($"[EducationUIBuilder] Slot yok: {slotProperty}");
                return;
            }

            slot.FindPropertyRelative("detailRoot").objectReferenceValue = built.Root;
            slot.FindPropertyRelative("layoutElement").objectReferenceValue = built.Layout;
            slot.FindPropertyRelative("titleText").objectReferenceValue = built.Title;
            slot.FindPropertyRelative("bodyText").objectReferenceValue = built.Body;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateTopAnchoredPanel(Transform parent, string name, float topY,
            float width, float height, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topY);
            rt.sizeDelta = new Vector2(width, height);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateTopButton(Transform parent, string name, string label,
            float topY, Color color, UnityAction action, float w, float h)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topY);
            rt.sizeDelta = new Vector2(w, h);

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
            lbl.color = UITheme.TextOnPrimary;
            lbl.text = label;
            return go;
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
            lbl.color = UITheme.TextOnPrimary;
            lbl.text = label;
            return go;
        }

        /// <summary>Üst şerit metni — AR üzerinde gölge ile okunaklı.</summary>
        private static Text CreateBarText(Transform parent, string name, float topOffset, float height,
            TextAnchor align, int size, string content, Color color, bool bold = false, bool overlayShadow = false)
        {
            var margin = overlayShadow ? CompactTextMarginH : 12f;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topOffset);
            rt.sizeDelta = new Vector2(-margin * 2f, height);
            rt.offsetMin = new Vector2(margin, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-margin, rt.offsetMax.y);
            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            if (overlayShadow || bold)
            {
                AddTextShadow(text, overlayShadow);
            }

            return text;
        }

        private static void AddTextShadow(Text text, bool strong = false)
        {
            if (text.GetComponent<Shadow>() != null)
            {
                return;
            }

            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = strong ? new Color(0f, 0f, 0f, 0.82f) : new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = strong ? new Vector2(2f, -2f) : new Vector2(1f, -1f);
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

        private static void SetPropertyFloat(Object target, string property, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null)
            {
                return;
            }

            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
