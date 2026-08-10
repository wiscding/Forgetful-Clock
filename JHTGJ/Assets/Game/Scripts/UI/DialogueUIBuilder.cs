using JHTGJ.Story;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class DialogueUIBuilder
    {
        public const float DefaultBackgroundDimAlpha = 0.58f;

        public static DialogueUI Build(DayStorySchedule schedule = null)
        {
            EnsureEventSystem();

            var existing = GameObject.Find("DialogueCanvas");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing);
                else
#endif
                    Object.Destroy(existing);
            }

            var canvasGo = new GameObject("DialogueCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = CreatePanel(
                canvasGo.transform,
                "DialogueRoot",
                new Color(0f, 0f, 0f, DefaultBackgroundDimAlpha));
            root.SetActive(false);

            var protagonistImage = CreatePortrait(root.transform, "ProtagonistPortrait", new Vector2(-420f, -40f), new Vector2(520f, 780f));
            var wifeImage = CreatePortrait(root.transform, "WifePortrait", new Vector2(420f, -40f), new Vector2(520f, 780f));

            var dialoguePanel = CreatePanel(root.transform, "DialoguePanel", new Color(0.08f, 0.08f, 0.1f, 0.88f));
            var dialogueRect = dialoguePanel.GetComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0.08f, 0.04f);
            dialogueRect.anchorMax = new Vector2(0.92f, 0.28f);
            dialogueRect.offsetMin = Vector2.zero;
            dialogueRect.offsetMax = Vector2.zero;

            var speakerLabel = CreateLabel(
                dialoguePanel.transform,
                "SpeakerName",
                string.Empty,
                30,
                TextAnchor.UpperLeft,
                stretchTop: true);
            var dialogueLabel = CreateLabel(
                dialoguePanel.transform,
                "DialogueText",
                string.Empty,
                28,
                TextAnchor.UpperLeft,
                stretchBody: true);

            var continueButton = CreateContinueButton(dialoguePanel.transform);

            var dialogueUi = canvasGo.AddComponent<DialogueUI>();
            dialogueUi.Setup(
                root,
                protagonistImage,
                wifeImage,
                speakerLabel,
                dialogueLabel,
                continueButton,
                schedule != null ? schedule.DefaultProtagonistPortrait : null,
                schedule != null ? schedule.DefaultWifePortrait : null);

            return dialogueUi;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
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

        static Image CreatePortrait(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
            var image = go.AddComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            return image;
        }

        static Text CreateLabel(
            Transform parent,
            string name,
            string text,
            int fontSize,
            TextAnchor alignment,
            bool stretchTop = false,
            bool stretchBody = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();

            if (stretchBody)
            {
                GameUIFontUtility.StretchDialogueTextRect(rect);
            }
            else if (stretchTop)
            {
                rect.anchorMin = new Vector2(0.04f, 0.82f);
                rect.anchorMax = new Vector2(0.96f, 0.96f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.sizeDelta = new Vector2(1500f, 60f);
            }

            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            GameUIFontUtility.ConfigureDialogueLabel(label, multiline: stretchBody);
            return label;
        }

        static Button CreateContinueButton(Transform parent)
        {
            var go = new GameObject("ContinueButton");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(180f, 44f);
            rect.anchoredPosition = new Vector2(-20f, 20f);

            go.AddComponent<Image>().color = new Color(0.75f, 0.65f, 0.45f, 1f);
            var button = go.AddComponent<Button>();

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.text = "继续";
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            GameUIFontUtility.ConfigureButtonLabel(label);

            go.AddComponent<UISfxButton>();
            return button;
        }
    }
}
