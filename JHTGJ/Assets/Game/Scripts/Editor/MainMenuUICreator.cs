#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JHTGJ.EditorTools
{
    public static class MainMenuUICreator
    {
        [MenuItem("JHTGJ/Create Main Menu UI (Current Scene)")]
        public static void Create()
        {
            BuildInCurrentScene();
        }

        [MenuItem("JHTGJ/Fix Main Menu UI (Legacy Text)")]
        public static void FixLegacyTextFromMenu()
        {
            BuildInCurrentScene();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[JHTGJ] Main Menu UI rebuilt with Legacy Text.");
        }

        public static GameObject BuildInCurrentScene()
        {
            EnsureMainCamera();
            EnsureEventSystem();
            var canvas = CreateCanvas();
            EnsureSettingsManager();

            var mainPanel = CreatePanel(canvas.transform, "MainMenuPanel", new Color(0f, 0f, 0f, 0.65f));
            var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", new Color(0f, 0f, 0f, 0.75f));
            settingsPanel.SetActive(false);

            LegacyMenuUiFactory.CreateTitle(mainPanel.transform, "遗忘时钟");
            var newGame = LegacyMenuUiFactory.CreateMenuButton(mainPanel.transform, "NewGameButton", "新游戏", new Vector2(0f, 80f));
            var loadGame = LegacyMenuUiFactory.CreateMenuButton(mainPanel.transform, "LoadGameButton", "载入游戏", new Vector2(0f, 20f));
            var settings = LegacyMenuUiFactory.CreateMenuButton(mainPanel.transform, "SettingsButton", "设置", new Vector2(0f, -40f));
            var quit = LegacyMenuUiFactory.CreateMenuButton(mainPanel.transform, "QuitButton", "退出游戏", new Vector2(0f, -100f));

            LegacyMenuUiFactory.CreateTitle(settingsPanel.transform, "设置");
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "MasterVolumeLabel", "总音量", new Vector2(-180f, 100f));
            var masterSlider = CreateSlider(settingsPanel.transform, "MasterVolumeSlider", new Vector2(80f, 100f));
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "SfxVolumeLabel", "音效", new Vector2(-180f, 30f));
            var sfxSlider = CreateSlider(settingsPanel.transform, "SfxVolumeSlider", new Vector2(80f, 30f));
            LegacyMenuUiFactory.CreateLabel(settingsPanel.transform, "ResolutionLabel", "窗口大小", new Vector2(-180f, -40f));
            var resolutionDropdown = LegacyMenuUiFactory.CreateResolutionDropdown(settingsPanel.transform, "ResolutionDropdown", new Vector2(80f, -40f));
            var backButton = LegacyMenuUiFactory.CreateMenuButton(settingsPanel.transform, "BackButton", "返回", new Vector2(0f, -120f));

            var mainMenu = canvas.gameObject.AddComponent<MainMenuUI>();
            var settingsUi = canvas.gameObject.AddComponent<SettingsPanelUI>();

            var mainSo = new SerializedObject(mainMenu);
            mainSo.FindProperty("mainMenuPanel").objectReferenceValue = mainPanel;
            mainSo.FindProperty("settingsPanel").objectReferenceValue = settingsUi;
            mainSo.FindProperty("newGameButton").objectReferenceValue = newGame;
            mainSo.FindProperty("loadGameButton").objectReferenceValue = loadGame;
            mainSo.FindProperty("settingsButton").objectReferenceValue = settings;
            mainSo.FindProperty("quitButton").objectReferenceValue = quit;
            mainSo.ApplyModifiedPropertiesWithoutUndo();

            var settingsSo = new SerializedObject(settingsUi);
            settingsSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            settingsSo.FindProperty("mainMenu").objectReferenceValue = mainMenu;
            settingsSo.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider;
            settingsSo.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
            settingsSo.FindProperty("resolutionDropdown").objectReferenceValue = resolutionDropdown;
            settingsSo.FindProperty("backButton").objectReferenceValue = backButton;
            settingsSo.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeObject = canvas;
            EditorGUIUtility.PingObject(canvas);
            Debug.Log("[JHTGJ] Main Menu UI created in current scene.");
            return canvas;
        }

        public static GameSettingsManager EnsureSettingsManager()
        {
            var existing = Object.FindObjectOfType<GameSettingsManager>();
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AudioLibraryPopulator.AssetPath);
            if (library != null)
            {
                AudioLibraryPopulator.EnsureAudioManagerInProject(library);
                AudioLibraryPopulator.EnsureResourcesAudioLibrary(library);
            }

            if (existing != null)
                return existing;

            var go = new GameObject("GameSettings");
            return go.AddComponent<GameSettingsManager>();
        }

        [MenuItem("JHTGJ/Fix MainMenu Camera")]
        public static void FixCamera()
        {
            EnsureMainCamera();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[JHTGJ] Main Camera added to current scene.");
        }

        public static void EnsureMainCamera()
        {
            var go = Camera.main != null
                ? Camera.main.gameObject
                : GameObject.Find("Main Camera");

            if (go == null)
            {
                go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                var cam = go.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                go.transform.position = new Vector3(0f, 0f, -10f);
                go.AddComponent<AudioListener>();
            }
            else
            {
                go.tag = "MainCamera";
                if (go.GetComponent<Camera>() == null)
                    go.AddComponent<Camera>();
                if (go.GetComponent<AudioListener>() == null)
                    go.AddComponent<AudioListener>();

                var cam = go.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                if (cam.orthographicSize <= 0f)
                    cam.orthographicSize = 5f;
            }
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        static GameObject CreateCanvas()
        {
            var existing = GameObject.Find("MainMenuCanvas");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var canvasGo = new GameObject("MainMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        public static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos)
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
#endif
