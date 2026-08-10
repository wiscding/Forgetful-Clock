using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class OpeningCgUIBuilder
    {
        public const float TextPanelAnchorMinY = 0.22f;
        public const float TextPanelAnchorMaxY = 0.36f;
        public const float CgImageAnchorMinY = 0.36f;

        public static OpeningCgUI Build()
        {
            EnsureEventSystem();

            var existing = GameObject.Find("OpeningCgCanvas");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing);
                else
#endif
                    Object.Destroy(existing);
            }

            var canvasGo = new GameObject("OpeningCgCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("OpeningCgRoot");
            root.transform.SetParent(canvasGo.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateFullScreenImage(root.transform, "Background", Color.black);

            var cgGo = new GameObject("CgImage");
            cgGo.transform.SetParent(root.transform, false);
            var cgRect = cgGo.AddComponent<RectTransform>();
            var cgImage = cgGo.AddComponent<Image>();
            cgImage.color = Color.white;
            cgImage.preserveAspect = true;
            cgImage.raycastTarget = false;

            var textPanel = new GameObject("TextPanel");
            textPanel.transform.SetParent(root.transform, false);
            var textPanelRect = textPanel.AddComponent<RectTransform>();
            var textPanelImage = textPanel.AddComponent<Image>();
            textPanelImage.color = new Color(0f, 0f, 0f, 0.82f);

            ApplyLayout(cgRect, textPanelRect);

            var labelGo = new GameObject("BodyText");
            labelGo.transform.SetParent(textPanel.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.06f, 0.12f);
            labelRect.anchorMax = new Vector2(0.94f, 0.88f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.text = string.Empty;
            ApplyBodyTextStyle(label);

            var clickCatcher = CreateFullScreenImage(root.transform, "ClickCatcher", new Color(0f, 0f, 0f, 0f));
            clickCatcher.raycastTarget = true;

            var ui = canvasGo.AddComponent<OpeningCgUI>();
            ui.Setup(root, cgImage, label, textPanelImage);

            root.SetActive(false);
            return ui;
        }

        public static void ApplyBodyTextStyle(Text label)
        {
            if (label == null)
                return;

            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 30;
            label.lineSpacing = 1.1f;
            GameUIFontUtility.ConfigureDialogueLabel(label, multiline: true);
        }

        public static void ApplyLayout(RectTransform cgRect, RectTransform textPanelRect)
        {
            if (textPanelRect != null)
            {
                textPanelRect.anchorMin = new Vector2(0f, TextPanelAnchorMinY);
                textPanelRect.anchorMax = new Vector2(1f, TextPanelAnchorMaxY);
                textPanelRect.pivot = new Vector2(0.5f, 0.5f);
                textPanelRect.offsetMin = Vector2.zero;
                textPanelRect.offsetMax = Vector2.zero;
                textPanelRect.anchoredPosition = Vector2.zero;
            }

            if (cgRect != null)
            {
                cgRect.anchorMin = new Vector2(0f, CgImageAnchorMinY);
                cgRect.anchorMax = Vector2.one;
                cgRect.offsetMin = Vector2.zero;
                cgRect.offsetMax = Vector2.zero;
                cgRect.anchoredPosition = Vector2.zero;
            }
        }

        static Image CreateFullScreenImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
