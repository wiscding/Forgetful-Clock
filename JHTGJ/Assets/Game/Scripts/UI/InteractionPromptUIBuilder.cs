using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class InteractionPromptUIBuilder
    {
        public static InteractionPromptUI Build()
        {
            EnsureEventSystem();

            var existing = GameObject.Find("InteractionPromptCanvas");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing);
                else
#endif
                    Object.Destroy(existing);
            }

            var canvasGo = new GameObject("InteractionPromptCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("PromptRoot");
            root.transform.SetParent(canvasGo.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(180f, 40f);
            rootRect.pivot = new Vector2(0.5f, 0f);

            var background = root.AddComponent<Image>();
            background.color = InteractionPromptUI.BackgroundColor;
            background.raycastTarget = false;

            var labelGo = new GameObject("PromptText");
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);

            var label = labelGo.AddComponent<Text>();
            label.text = InteractionPromptUI.DefaultPrompt;
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = InteractionPromptUI.TextColor;
            GameUIFontUtility.ConfigureButtonLabel(label);

            var ui = canvasGo.AddComponent<InteractionPromptUI>();
            ui.Setup(rootRect, label);
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
    }
}
