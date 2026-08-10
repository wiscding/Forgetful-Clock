using JHTGJ.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] SettingsPanelUI settingsPanel;
        [SerializeField] Button newGameButton;
        [SerializeField] Button loadGameButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;

        void Awake()
        {
            if (settingsPanel == null)
                settingsPanel = FindObjectOfType<SettingsPanelUI>(true);

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGame);
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);

            RefreshLoadButton();
            ShowMainMenu();
        }

        void OnEnable()
        {
            GameAudioManager.Instance?.PlayBgm(BgmTrack.Menu);
        }

        void Start()
        {
            GameAudioManager.Instance?.PlayBgm(BgmTrack.Menu);
        }

        void OnDestroy()
        {
            if (newGameButton != null)
                newGameButton.onClick.RemoveListener(OnNewGame);
            if (loadGameButton != null)
                loadGameButton.onClick.RemoveListener(OnLoadGame);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettings);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuit);
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (settingsPanel != null)
                settingsPanel.Hide();

            RefreshLoadButton();
        }

        void RefreshLoadButton()
        {
            if (loadGameButton != null)
                loadGameButton.interactable = SaveManager.HasContinuableSave();
        }

        void OnNewGame()
        {
            GameSession.RequestNewGame();
            SceneLoader.LoadGameScene();
        }

        void OnLoadGame()
        {
            if (!GameSession.RequestContinue())
            {
                Debug.LogWarning("没有可载入的存档。");
                RefreshLoadButton();
                return;
            }

            SceneLoader.LoadGameScene();
        }

        void OnSettings()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            settingsPanel?.Show();
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
