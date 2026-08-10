using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class StoryChoiceUIBuilder
    {
        public static StoryChoiceUI Build()
        {
            EnsureEventSystem();

            var existing = GameObject.Find("StoryChoiceCanvas");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing);
                else
#endif
                    Object.Destroy(existing);
            }

            var canvasGo = new GameObject("StoryChoiceCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 115;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("StoryChoiceRoot");
            root.transform.SetParent(canvasGo.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var overlay = CreateOverlay(root.transform);
            var panel = CreatePanel(root.transform);
            var title = CreateLabel(panel.transform, "Title", "请选择", new Vector2(0f, 150f), 34f);
            var hint = CreateLabel(panel.transform, "Hint", "选择一个选项", new Vector2(0f, 100f), 22f);
            var buttonContainer = CreateButtonContainer(panel.transform);
            var optionTemplate = CreateOptionButton(panel.transform, "OptionTemplate", "选项");

            var ui = canvasGo.AddComponent<StoryChoiceUI>();
            ui.Setup(root, title, hint, buttonContainer, optionTemplate);

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

        static GameObject CreateOverlay(Transform parent)
        {
            var go = new GameObject("FadeOverlay");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.45f);
            image.raycastTarget = false;
            return go;
        }

        static GameObject CreatePanel(Transform parent)
        {
            var panel = new GameObject("ChoicePanel");
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
            rect.sizeDelta = new Vector2(420f, 240f);
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

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 28;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.black;
            GameUIFontUtility.ApplyDefaultFont(label);

            go.SetActive(false);
            go.AddComponent<UISfxButton>();
            return button;
        }

        static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            string text,
            Vector2 anchoredPos,
            float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(460f, 50f);
            rect.anchoredPosition = anchoredPos;
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            GameUIFontUtility.ApplyDefaultFont(label);
            return label;
        }
    }
}
