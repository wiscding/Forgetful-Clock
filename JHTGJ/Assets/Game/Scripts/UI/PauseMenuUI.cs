using JHTGJ.Core;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] GameObject pausePanel;
        [SerializeField] SettingsPanelUI settingsPanel;
        [SerializeField] VillaSceneManager villaSceneManager;
        [SerializeField] Button continueButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] StoryEventTreeManager storyManager;
        [SerializeField] int currentDay = 1;

        bool isOpen;
        bool buttonsWired;

        public bool IsOpen => isOpen;

        public void Setup(
            GameObject pausePanelRef,
            SettingsPanelUI settingsPanelRef,
            Button resume,
            Button save,
            Button settings,
            Button mainMenu,
            VillaSceneManager manager = null)
        {
            pausePanel = pausePanelRef;
            settingsPanel = settingsPanelRef;
            continueButton = resume;
            saveButton = save;
            settingsButton = settings;
            mainMenuButton = mainMenu;
            if (manager != null)
                villaSceneManager = manager;

            WireButtons();
            buttonsWired = true;
            Hide();
        }

        void Awake()
        {
            if (settingsPanel == null)
                settingsPanel = GetComponent<SettingsPanelUI>();

            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            if (storyManager == null)
                storyManager = FindObjectOfType<StoryEventTreeManager>();

            Hide();
        }

        void Start()
        {
            if (!buttonsWired)
                WireButtons();

            if (storyManager != null)
                currentDay = storyManager.CurrentDay;
        }

        void WireButtons()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinue);
                continueButton.onClick.AddListener(OnContinue);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(OnSave);
                saveButton.onClick.AddListener(OnSave);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettings);
                settingsButton.onClick.AddListener(OnSettings);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnReturnMainMenu);
                mainMenuButton.onClick.AddListener(OnReturnMainMenu);
            }
        }

        void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinue);
            if (saveButton != null)
                saveButton.onClick.RemoveListener(OnSave);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettings);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnReturnMainMenu);

            if (isOpen)
                Time.timeScale = 1f;
        }

        void Update()
        {
            if (IsStoryBlockingPause())
                return;

            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (settingsPanel != null && settingsPanel.IsVisible)
            {
                settingsPanel.Hide();
                ShowPausePanel();
                return;
            }

            if (isOpen)
                Hide();
            else
                ShowPausePanel();
        }

        public void ShowPausePanel()
        {
            isOpen = true;
            if (pausePanel != null)
                pausePanel.SetActive(true);

            if (settingsPanel != null)
                settingsPanel.Hide();

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            isOpen = false;
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.Hide();

            Time.timeScale = 1f;
        }

        void OnContinue()
        {
            Hide();
        }

        void OnSave()
        {
            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            if (storyManager == null)
                storyManager = FindObjectOfType<StoryEventTreeManager>();

            var room = villaSceneManager != null
                ? villaSceneManager.CurrentRoomType
                : RoomType.FrontHall;

            if (storyManager != null)
            {
                storyManager.SaveProgress(room);
                currentDay = storyManager.CurrentDay;
            }
            else
            {
                Debug.LogWarning("[Save] 未找到 StoryEventTreeManager，存档可能不完整。");
                SaveManager.SaveGame(currentDay, room);
            }

            Debug.Log($"[Save] 已保存：第 {currentDay} 天，房间 {room}");
        }

        void OnSettings()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);

            settingsPanel?.ShowFromPauseMenu();
        }

        void OnReturnMainMenu()
        {
            if (storyManager == null)
                storyManager = FindObjectOfType<StoryEventTreeManager>();

            if (storyManager != null && !storyManager.StoryEnded)
            {
                if (villaSceneManager == null)
                    villaSceneManager = FindObjectOfType<VillaSceneManager>();

                var room = villaSceneManager != null
                    ? villaSceneManager.CurrentRoomType
                    : RoomType.Bedroom;
                storyManager.SaveProgress(room);
            }

            Time.timeScale = 1f;
            SceneLoader.LoadMainMenuScene();
        }

        static bool IsStoryBlockingPause()
        {
            if (FindObjectOfType<DialogueUI>(true) is { IsShowing: true })
                return true;

            if (FindObjectOfType<RoomSelectionUI>(true) is { IsShowing: true })
                return true;

            if (FindObjectOfType<OpeningCgUI>(true) is { IsShowing: true })
                return true;

            if (FindObjectOfType<EndingScrollUI>(true) is { IsShowing: true })
                return true;

            if (FindObjectOfType<StoryChoiceUI>(true) is { IsShowing: true })
                return true;

            return false;
        }
    }
}
