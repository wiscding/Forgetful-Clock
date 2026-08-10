#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JHTGJ.EditorTools
{
    public static class PauseMenuUIFixer
    {
        [MenuItem("JHTGJ/Fix Pause Menu UI (Legacy Text)")]
        public static void FixFromMenu()
        {
            FixActiveScene(showDialog: true);
        }

        public static void FixFromCommandLine()
        {
            FixActiveScene(showDialog: false);
        }

        public static void FixActiveScene(bool showDialog)
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != SceneLoader.GameSceneName)
            {
                if (!EditorUtility.DisplayDialog(
                        "需要在游戏场景",
                        "暂停菜单应放在 SampleScene（游戏场景）。\n\n是否打开 SampleScene 并继续？",
                        "打开 SampleScene",
                        "取消"))
                    return;

                EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            }

            var canvas = GameObject.Find("PauseMenuCanvas");
            if (canvas == null)
            {
                PauseMenuUICreator.CreateInternal(forceRecreate: true);
                return;
            }

            FixExisting(canvas);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (showDialog)
                EditorUtility.DisplayDialog("完成", "暂停菜单已切换为 Unity 普通 Text（思源宋体）。", "OK");
            else
                Debug.Log("[JHTGJ] Pause Menu UI converted to Legacy Text.");
        }

        public static void FixExisting(GameObject pauseMenuCanvas)
        {
            if (pauseMenuCanvas == null)
                return;

            if (!pauseMenuCanvas.activeSelf)
                pauseMenuCanvas.SetActive(true);

            PauseMenuLegacyTextUtility.ConvertCanvas(pauseMenuCanvas);
            EnsureLegacyResolutionDropdown(pauseMenuCanvas.transform.Find("SettingsPanel"));
            RewireSettingsPanel(pauseMenuCanvas);

            EditorUtility.SetDirty(pauseMenuCanvas);
        }

        static void RewireSettingsPanel(GameObject pauseMenuCanvas)
        {
            var settingsUi = pauseMenuCanvas.GetComponent<SettingsPanelUI>();
            if (settingsUi == null)
                return;

            var settingsPanel = pauseMenuCanvas.transform.Find("SettingsPanel");
            if (settingsPanel == null)
                return;

            var so = new SerializedObject(settingsUi);
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel.gameObject;
            so.FindProperty("pauseMenu").objectReferenceValue = pauseMenuCanvas.GetComponent<PauseMenuUI>();

            var masterSlider = settingsPanel.Find("MasterVolumeSlider")?.GetComponent<Slider>();
            var sfxSlider = settingsPanel.Find("SfxVolumeSlider")?.GetComponent<Slider>();
            var resolutionDropdownTransform = settingsPanel.Find("ResolutionDropdown");
            var resolutionDropdown = resolutionDropdownTransform != null &&
                                     LegacyMenuUiFactory.TryGetLegacyDropdown(
                                         resolutionDropdownTransform.gameObject,
                                         out var legacyDropdown)
                ? legacyDropdown
                : null;
            var backButton = settingsPanel.Find("BackButton")?.GetComponent<Button>();

            if (masterSlider != null)
                so.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider;
            if (sfxSlider != null)
                so.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
            if (resolutionDropdown != null)
                so.FindProperty("resolutionDropdown").objectReferenceValue = resolutionDropdown;
            if (backButton != null)
                so.FindProperty("backButton").objectReferenceValue = backButton;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settingsUi);
        }

        static void EnsureLegacyResolutionDropdown(Transform settingsPanel)
        {
            if (settingsPanel == null)
                return;

            var dropdownTransform = settingsPanel.Find("ResolutionDropdown");
            if (dropdownTransform != null &&
                LegacyMenuUiFactory.TryGetLegacyDropdown(dropdownTransform.gameObject, out _))
                return;

            if (dropdownTransform != null)
                Object.DestroyImmediate(dropdownTransform.gameObject);

            LegacyMenuUiFactory.CreateResolutionDropdown(
                settingsPanel,
                "ResolutionDropdown",
                new Vector2(80f, -40f));
        }
    }
}
#endif
