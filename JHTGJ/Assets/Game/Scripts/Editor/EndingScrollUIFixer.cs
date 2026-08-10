#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class EndingScrollUIFixer
    {
        [MenuItem("JHTGJ/Fix Ending Scroll UI")]
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
                        "结局滚动 UI 应放在 SampleScene（游戏场景）。\n\n是否打开 SampleScene 并继续？",
                        "打开 SampleScene",
                        "取消"))
                    return;

                EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            }

            var canvas = GameObject.Find("EndingScrollCanvas");
            if (canvas == null)
            {
                EndingScrollUIBuilder.Build();
                canvas = GameObject.Find("EndingScrollCanvas");
            }

            if (canvas != null)
                FixExisting(canvas);

            var storyManager = Object.FindObjectOfType<StoryEventTreeManager>();
            if (storyManager != null)
            {
                var endingUi = canvas != null ? canvas.GetComponent<EndingScrollUI>() : null;
                if (endingUi != null)
                {
                    var so = new SerializedObject(storyManager);
                    so.FindProperty("endingScrollUI").objectReferenceValue = endingUi;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(storyManager);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (showDialog)
                EditorUtility.DisplayDialog("完成", "结局滚动 UI 已修复（Canvas 缩放 + 引用）。", "OK");
            else
                Debug.Log("[JHTGJ] Ending scroll UI fixed.");
        }

        public static void FixExisting(GameObject endingScrollCanvas)
        {
            if (endingScrollCanvas == null)
                return;

            PauseMenuLegacyTextUtility.FixCanvasScale(endingScrollCanvas.transform);

            var canvas = endingScrollCanvas.GetComponent<Canvas>();
            if (canvas != null)
                canvas.sortingOrder = 210;

            var ui = endingScrollCanvas.GetComponent<EndingScrollUI>();
            if (ui == null)
                return;

            var root = endingScrollCanvas.transform.Find("EndingScrollRoot");
            var viewport = endingScrollCanvas.transform.Find("EndingScrollRoot/Viewport");
            var content = endingScrollCanvas.transform.Find("EndingScrollRoot/Viewport/Content");
            var body = endingScrollCanvas.transform.Find("EndingScrollRoot/Viewport/Content/BodyText");

            var so = new SerializedObject(ui);
            if (root != null)
                so.FindProperty("root").objectReferenceValue = root.gameObject;
            if (viewport != null)
                so.FindProperty("viewport").objectReferenceValue = viewport.GetComponent<RectTransform>();
            if (content != null)
                so.FindProperty("content").objectReferenceValue = content.GetComponent<RectTransform>();
            if (body != null)
                so.FindProperty("bodyLabel").objectReferenceValue = body.GetComponent<UnityEngine.UI.Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ui);
        }
    }
}
#endif
