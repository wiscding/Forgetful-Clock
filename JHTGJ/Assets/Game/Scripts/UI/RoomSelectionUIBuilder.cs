using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class RoomSelectionUIBuilder
    {
        public static RoomSelectionUI Build()
        {
            EnsureEventSystem();

            var existing = GameObject.Find("RoomSelectionCanvas");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing);
                else
#endif
                    Object.Destroy(existing);
            }

            var canvasGo = new GameObject("RoomSelectionCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("RoomSelectionRoot");
            root.transform.SetParent(canvasGo.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var overlay = CreateOverlay(root.transform);
            var panel = CreatePanel(root.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            var panelGroup = panel.AddComponent<CanvasGroup>();
            var title = CreateLabel(panel.transform, "Title", "选择目的地", new Vector2(0f, 150f), 34);
            var hint = CreateLabel(panel.transform, "Hint", "选择要前往的房间", new Vector2(0f, 100f), 22);
            var buttonContainer = CreateButtonContainer(panel.transform);
            var optionTemplate = CreateOptionButton(panel.transform, "OptionTemplate", "选项");
            var cancelButton = CreateCancelButton(panel.transform);

            var ui = canvasGo.AddComponent<RoomSelectionUI>();
            ui.Setup(root, overlay, panelGroup, panelRect, title, hint, buttonContainer, optionTemplate, cancelButton);

            root.SetActive(false);
            return ui;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        static CanvasGroup CreateOverlay(Transform parent)
        {
            var go = new GameObject("FadeOverlay");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = Color.black;
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            var group = go.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return group;
        }

        static GameObject CreatePanel(Transform parent)
        {
            var panel = new GameObject("SelectionPanel");
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 420f);
            rect.anchoredPosition = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0.1f, 0.11f, 0.14f, 0.94f);
            return panel;
        }

        static Transform CreateButtonContainer(Transform parent)
        {
            var go = new GameObject("ButtonContainer");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 220f);
            rect.anchoredPosition = new Vector2(0f, -20f);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return go.transform;
        }

        static Button CreateOptionButton(Transform parent, string name, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 56f);
            go.AddComponent<Image>().color = new Color(0.78f, 0.68f, 0.48f, 1f);
            var button = go.AddComponent<Button>();

            var label = CreateButtonText(go.transform, text, 28, Color.black);
            GameUIFontUtility.ConfigureButtonLabel(label);

            go.SetActive(false);
            go.AddComponent<UISfxButton>();
            return button;
        }

        static Button CreateCancelButton(Transform parent)
        {
            var go = new GameObject("CancelButton");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(180f, 44f);
            rect.anchoredPosition = new Vector2(0f, 24f);
            go.AddComponent<Image>().color = new Color(0.35f, 0.36f, 0.4f, 1f);
            var button = go.AddComponent<Button>();

            var label = CreateButtonText(go.transform, "取消", 22, Color.white);
            GameUIFontUtility.ConfigureButtonLabel(label);

            go.AddComponent<UISfxButton>();
            return button;
        }

        static Text CreateLabel(
            Transform parent,
            string name,
            string text,
            Vector2 anchoredPos,
            int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(460f, 50f);
            rect.anchoredPosition = anchoredPos;
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            GameUIFontUtility.ConfigureDialogueLabel(label, multiline: false);
            return label;
        }

        static Text CreateButtonText(Transform parent, string text, int fontSize, Color color)
        {
            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(parent, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }
    }
}
