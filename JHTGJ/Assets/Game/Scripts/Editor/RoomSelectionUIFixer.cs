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
    public static class RoomSelectionUIFixer
    {
        [MenuItem("JHTGJ/Fix Room Selection UI (Legacy Text)")]
        public static void FixFromMenu()
        {
            FixActiveScene(showDialog: true);
        }

        public static void FixActiveScene(bool showDialog)
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != SceneLoader.GameSceneName)
            {
                if (!EditorUtility.DisplayDialog(
                        "需要在游戏场景",
                        "房间选择 UI 应放在 SampleScene（游戏场景）。\n\n是否打开 SampleScene 并继续？",
                        "打开 SampleScene",
                        "取消"))
                    return;

                EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            }

            var canvas = GameObject.Find("RoomSelectionCanvas");
            if (canvas == null)
            {
                RoomSelectionUIBuilder.Build();
                canvas = GameObject.Find("RoomSelectionCanvas");
            }

            if (canvas != null)
                FixExisting(canvas);

            RoomSelectorSetup.FixOptionTemplatePlacement();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (showDialog)
                EditorUtility.DisplayDialog("完成", "房间选择 UI 已切换为 Unity 普通 Text（思源宋体）。", "OK");
            else
                Debug.Log("[JHTGJ] Room selection UI converted to Legacy Text.");
        }

        public static void FixExisting(GameObject roomSelectionCanvas)
        {
            if (roomSelectionCanvas == null)
                return;

            if (!roomSelectionCanvas.activeSelf)
                roomSelectionCanvas.SetActive(true);

            PauseMenuLegacyTextUtility.ConvertCanvas(roomSelectionCanvas);
            RewireRoomSelectionUi(roomSelectionCanvas);
            EditorUtility.SetDirty(roomSelectionCanvas);
        }

        static void RewireRoomSelectionUi(GameObject roomSelectionCanvas)
        {
            var ui = roomSelectionCanvas.GetComponent<RoomSelectionUI>();
            if (ui == null)
                return;

            var root = roomSelectionCanvas.transform.Find("RoomSelectionRoot");
            var panel = root != null ? root.Find("SelectionPanel") : roomSelectionCanvas.transform.Find("SelectionPanel");
            if (panel == null)
                return;

            var so = new SerializedObject(ui);
            if (root != null)
                so.FindProperty("root").objectReferenceValue = root.gameObject;

            so.FindProperty("fadeOverlay").objectReferenceValue =
                roomSelectionCanvas.GetComponentInChildren<CanvasGroup>(true);
            so.FindProperty("panelRect").objectReferenceValue = panel.GetComponent<RectTransform>();
            so.FindProperty("panelGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
            so.FindProperty("titleLabel").objectReferenceValue = panel.Find("Title")?.GetComponent<Text>();
            so.FindProperty("hintLabel").objectReferenceValue = panel.Find("Hint")?.GetComponent<Text>();
            so.FindProperty("buttonContainer").objectReferenceValue = panel.Find("ButtonContainer");
            so.FindProperty("optionButtonTemplate").objectReferenceValue =
                panel.Find("OptionTemplate")?.GetComponent<Button>();
            so.FindProperty("cancelButton").objectReferenceValue =
                panel.Find("CancelButton")?.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ui);
        }
    }
}
#endif
