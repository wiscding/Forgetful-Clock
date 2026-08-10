using System.Linq;
using JHTGJ.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] GameObject settingsPanel;
        [SerializeField] MainMenuUI mainMenu;
        [SerializeField] PauseMenuUI pauseMenu;
        [SerializeField] Slider masterVolumeSlider;
        [SerializeField] Slider sfxVolumeSlider;
        [SerializeField] Dropdown resolutionDropdown;
        [SerializeField] Button backButton;

        ResolutionOption[] resolutionOptions = System.Array.Empty<ResolutionOption>();

        struct ResolutionOption
        {
            public int index;
            public string label;
        }

        bool openedFromPauseMenu;
        bool controlsWired;

        public bool IsVisible =>
            settingsPanel != null && settingsPanel.activeSelf;

        public void Setup(
            GameObject settingsPanelRef,
            MainMenuUI mainMenuRef,
            PauseMenuUI pauseMenuRef,
            Slider masterSlider,
            Slider sfxSlider,
            Dropdown resolutionDropdownRef,
            Button back)
        {
            settingsPanel = settingsPanelRef;
            mainMenu = mainMenuRef;
            pauseMenu = pauseMenuRef;
            masterVolumeSlider = masterSlider;
            sfxVolumeSlider = sfxSlider;
            resolutionDropdown = resolutionDropdownRef;
            backButton = back;

            WireControls();
            controlsWired = true;
            Hide();
        }

        void Awake()
        {
            if (mainMenu == null)
                mainMenu = FindObjectOfType<MainMenuUI>();

            if (pauseMenu == null)
                pauseMenu = GetComponent<PauseMenuUI>();
            if (pauseMenu == null)
                pauseMenu = FindObjectOfType<PauseMenuUI>(true);

            EnsureControlsBound();
            Hide();
        }

        void Start()
        {
            EnsureControlsBound();

            if (!controlsWired)
                WireControls();

            BuildResolutionDropdown();
            SyncFromSettings();
        }

        void EnsureControlsBound()
        {
            if (settingsPanel == null)
            {
                var panelTransform = transform.Find("SettingsPanel");
                if (panelTransform != null)
                    settingsPanel = panelTransform.gameObject;
            }

            if (settingsPanel == null)
                return;

            var panel = settingsPanel.transform;
            if (masterVolumeSlider == null)
                masterVolumeSlider = panel.Find("MasterVolumeSlider")?.GetComponent<Slider>();
            if (sfxVolumeSlider == null)
                sfxVolumeSlider = panel.Find("SfxVolumeSlider")?.GetComponent<Slider>();
            if (backButton == null)
                backButton = panel.Find("BackButton")?.GetComponent<Button>();

            EnsureLegacyResolutionDropdown();
        }

        void EnsureLegacyResolutionDropdown()
        {
            if (resolutionDropdown != null)
                return;

            if (settingsPanel == null)
                return;

            var panel = settingsPanel.transform;
            var dropdownTransform = panel.Find("ResolutionDropdown");
            var defaultPos = new Vector2(80f, -40f);

            if (dropdownTransform != null)
            {
                if (LegacyMenuUiFactory.TryGetLegacyDropdown(dropdownTransform.gameObject, out resolutionDropdown))
                    return;

                defaultPos = dropdownTransform.GetComponent<RectTransform>().anchoredPosition;
                Object.DestroyImmediate(dropdownTransform.gameObject);
            }

            resolutionDropdown = LegacyMenuUiFactory.CreateResolutionDropdown(
                panel,
                "ResolutionDropdown",
                defaultPos);
            controlsWired = false;
        }

        void WireControls()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBack);
                backButton.onClick.AddListener(OnBack);
            }
        }

        void OnDestroy()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBack);
        }

        public void Show()
        {
            openedFromPauseMenu = false;
            ShowInternal();
        }

        public void ShowFromPauseMenu()
        {
            openedFromPauseMenu = true;
            ShowInternal();
        }

        void ShowInternal()
        {
            EnsureControlsBound();

            if (!controlsWired)
                WireControls();

            if (settingsPanel != null)
                settingsPanel.SetActive(true);

            BuildResolutionDropdown();
            SyncFromSettings();
        }

        public void Hide()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        void SyncFromSettings()
        {
            var settings = GameSettingsManager.Instance;
            if (settings == null)
                return;

            if (masterVolumeSlider != null)
                masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.SetValueWithoutNotify(settings.SfxVolume);

            SelectResolution(settings.ResolutionIndex);
        }

        void BuildResolutionDropdown()
        {
            if (resolutionDropdown == null)
                return;

            resolutionOptions = Screen.resolutions
                .Select((r, i) => new ResolutionOption
                {
                    index = i,
                    label = $"{r.width} x {r.height}"
                })
                .GroupBy(o => o.label)
                .Select(g => g.Last())
                .ToArray();

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionOptions.Select(o => o.label).ToList());
        }

        void SelectResolution(int resolutionIndex)
        {
            if (resolutionDropdown == null || resolutionOptions.Length == 0)
                return;

            for (var i = 0; i < resolutionOptions.Length; i++)
            {
                if (resolutionOptions[i].index != resolutionIndex)
                    continue;

                resolutionDropdown.SetValueWithoutNotify(i);
                return;
            }
        }

        void OnMasterVolumeChanged(float value)
        {
            GameSettingsManager.Instance?.SetMasterVolume(value);
        }

        void OnSfxVolumeChanged(float value)
        {
            GameSettingsManager.Instance?.SetSfxVolume(value);
        }

        void OnResolutionChanged(int dropdownIndex)
        {
            if (dropdownIndex < 0 || dropdownIndex >= resolutionOptions.Length)
                return;

            GameSettingsManager.Instance?.SetResolutionIndex(resolutionOptions[dropdownIndex].index);
        }

        void OnBack()
        {
            GameSettingsManager.Instance?.SaveSettings();
            Hide();

            if (openedFromPauseMenu)
                pauseMenu?.ShowPausePanel();
            else
                mainMenu?.ShowMainMenu();
        }
    }
}
