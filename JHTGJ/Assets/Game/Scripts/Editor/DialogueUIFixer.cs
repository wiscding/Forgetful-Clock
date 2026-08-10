#if UNITY_EDITOR
using JHTGJ.Story;
using JHTGJ.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JHTGJ.EditorTools
{
    public static class DialogueUIFixer
    {
        [MenuItem("JHTGJ/Fix Dialogue UI (Layout, Font, Dim)")]
        public static void FixFromMenu()
        {
            var canvas = GameObject.Find("DialogueCanvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("未找到 DialogueCanvas", "请先运行 JHTGJ/Create Dialogue UI (Game Scene)。", "OK");
                return;
            }

            FixExisting(canvas);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[JHTGJ] Dialogue UI 已修复：Legacy Text + 思源宋体。");
        }

        [MenuItem("JHTGJ/Convert Dialogue UI To Legacy Text")]
        public static void ConvertFromMenu() => ConvertInternal(showDialog: true);

        public static void ConvertFromCommandLine() => ConvertInternal(showDialog: false);

        static void ConvertInternal(bool showDialog)
        {
            var canvas = GameObject.Find("DialogueCanvas");
            if (canvas == null)
            {
                var message = "未找到 DialogueCanvas，请先运行 JHTGJ/Create Dialogue UI (Game Scene)。";
                if (showDialog)
                    EditorUtility.DisplayDialog("未找到 DialogueCanvas", message, "OK");
                else
                    Debug.LogError("[JHTGJ] " + message);

                if (!showDialog)
                    EditorApplication.Exit(1);

                return;
            }

            FixExisting(canvas);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (showDialog)
                EditorUtility.DisplayDialog("完成", "对话 UI 已切换为 Unity 普通 Text（思源宋体 TTF）。", "OK");
            else
                Debug.Log("[JHTGJ] Dialogue UI converted to Legacy Text.");

            if (!showDialog)
                EditorApplication.Exit(0);
        }

        public static void FixExisting(GameObject dialogueCanvas)
        {
            if (dialogueCanvas == null)
                return;

            ConvertTmpLabelsToLegacyText(dialogueCanvas);

            var root = dialogueCanvas.transform.Find("DialogueRoot");
            if (root != null)
            {
                var dim = root.GetComponent<Image>();
                if (dim != null)
                    dim.color = new Color(0f, 0f, 0f, DialogueUIBuilder.DefaultBackgroundDimAlpha);
            }

            Text speakerLabel = null;
            Text dialogueLabel = null;
            Button continueButton = null;

            foreach (var label in dialogueCanvas.GetComponentsInChildren<Text>(true))
            {
                if (label.gameObject.name == "DialogueText")
                {
                    dialogueLabel = label;
                    GameUIFontUtility.ConfigureDialogueLabel(label, multiline: true);
                    GameUIFontUtility.StretchDialogueTextRect(label.rectTransform);
                }
                else if (label.gameObject.name == "SpeakerName")
                {
                    speakerLabel = label;
                    label.gameObject.SetActive(false);
                }
                else
                {
                    GameUIFontUtility.ConfigureButtonLabel(label);
                }

                EditorUtility.SetDirty(label);
            }

            continueButton = dialogueCanvas.GetComponentInChildren<Button>(true);

            var dialogueUi = dialogueCanvas.GetComponent<DialogueUI>();
            if (dialogueUi != null)
            {
                var so = new SerializedObject(dialogueUi);
                so.FindProperty("speakerNameLabel").objectReferenceValue = speakerLabel;
                so.FindProperty("dialogueTextLabel").objectReferenceValue = dialogueLabel;
                so.FindProperty("continueButton").objectReferenceValue = continueButton;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dialogueUi);
            }
        }

        static void ConvertTmpLabelsToLegacyText(GameObject dialogueCanvas)
        {
            var tmpLabels = dialogueCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmpLabels)
                ConvertSingleLabel(tmp);
        }

        static Text ConvertSingleLabel(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return null;

            var go = tmp.gameObject;
            var content = tmp.text;
            var fontSize = Mathf.RoundToInt(tmp.fontSize);
            var color = tmp.color;
            var alignment = MapAlignment(tmp.alignment);
            var richText = tmp.richText;

            Object.DestroyImmediate(tmp);

            var legacy = go.GetComponent<Text>();
            if (legacy == null)
                legacy = go.AddComponent<Text>();

            legacy.text = content;
            legacy.fontSize = fontSize;
            legacy.color = color;
            legacy.alignment = alignment;
            legacy.supportRichText = richText;
            legacy.font = GameUIFontUtility.DefaultLegacyFont;
            legacy.raycastTarget = false;

            return legacy;
        }

        static TextAnchor MapAlignment(TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.TopRight:
                    return alignment == TextAlignmentOptions.Top
                        ? TextAnchor.UpperCenter
                        : alignment == TextAlignmentOptions.TopRight
                            ? TextAnchor.UpperRight
                            : TextAnchor.UpperLeft;

                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.Center:
                case TextAlignmentOptions.Right:
                    return alignment == TextAlignmentOptions.Left
                        ? TextAnchor.MiddleLeft
                        : alignment == TextAlignmentOptions.Right
                            ? TextAnchor.MiddleRight
                            : TextAnchor.MiddleCenter;

                default:
                    return TextAnchor.UpperLeft;
            }
        }
    }
}
#endif
