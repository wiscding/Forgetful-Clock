using JHTGJ.Core;
using JHTGJ.Scene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class PauseMenuBuilder
    {
        public static GameObject Build(VillaSceneManager villaManager = null)
        {
            EnsureEventSystem();
            EnsureGameSettings();

            var existing = GameObject.Find("PauseMenuCanvas");
            if (existing != null)
                Object.Destroy(existing);

            var canvasGo = new GameObject("PauseMenuCanvas");
            canvasGo.SetActive(true);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var pausePanel = CreatePanel(canvasGo.transform, "PausePanel", new Color(0f, 0f, 0f, 0.7f));
            pausePanel.SetActive(false);

            var settingsPanel = CreatePanel(canvasGo.transform, "SettingsPanel", new Color(0f, 0f, 0f, 0.75f));
            settingsPanel.SetActive(false);

            LegacyMenuUiFactory.CreateTitle(pausePanel.transform, "暂停");
            var resume = LegacyMenuUiFactory.CreateMenuButton(pausePanel.transform, "ContinueButton", "继续游戏", new Vector2(0f, 80f));
            var save = LegacyMenuUiFactory.CreateMenuButton(pausePanel.transform, "SaveButton", "保存", new Vector2(0f, 20f));
            var settings = LegacyMenuUiFactory.CreateMenuButton(pausePanel.transform, "SettingsButton", "设置", new Vector2(0f, -40f));
            var mainMenu = LegacyMenuUiFactory.CreateMenuButton(pausePanel.transform, "MainMenuButton", "返回主界面", new Vector2(0f, -100f));

            LegacyMenuUiFactory.CreateTitle(settingsPanel.transform, "设置");
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "MasterVolumeLabel", "总音量", new Vector2(-180f, 100f));
            var masterSlider = CreateSlider(settingsPanel.transform, "MasterVolumeSlider", new Vector2(80f, 100f));
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "SfxVolumeLabel", "音效", new Vector2(-180f, 30f));
            var sfxSlider = CreateSlider(settingsPanel.transform, "SfxVolumeSlider", new Vector2(80f, 30f));
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "ResolutionLabel", "窗口大小", new Vector2(-180f, -40f));
            var resolutionDropdown = LegacyMenuUiFactory.CreateResolutionDropdown(settingsPanel.transform, "ResolutionDropdown", new Vector2(80f, -40f));
            var backButton = LegacyMenuUiFactory.CreateMenuButton(settingsPanel.transform, "BackButton", "返回", new Vector2(0f, -120f));

            var pauseUi = canvasGo.AddComponent<PauseMenuUI>();
            var settingsUi = canvasGo.AddComponent<SettingsPanelUI>();

            if (villaManager == null)
                villaManager = Object.FindObjectOfType<VillaSceneManager>();

            pauseUi.Setup(pausePanel, settingsUi, resume, save, settings, mainMenu, villaManager);
            settingsUi.Setup(settingsPanel, null, pauseUi, masterSlider, sfxSlider, resolutionDropdown, backButton);

            return canvasGo;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        static void EnsureGameSettings()
        {
            var settings = Object.FindObjectOfType<GameSettingsManager>();
            if (settings == null)
            {
                var go = new GameObject("GameSettings");
                settings = go.AddComponent<GameSettingsManager>();
            }

            if (settings.GetComponent<GameAudioManager>() == null)
                settings.gameObject.AddComponent<GameAudioManager>();
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.AddComponent<Image>().color = color;
            return panel;
        }

        static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 30f);
            rect.anchoredPosition = anchoredPos;

            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = new Color(0.35f, 0.75f, 0.95f, 1f);

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 20f);
            handle.AddComponent<Image>().color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }
    }
}
