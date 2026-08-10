using System;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class EndingScrollUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] RectTransform viewport;
        [SerializeField] RectTransform content;
        [SerializeField] Text bodyLabel;
        [SerializeField] float scrollSpeed = 42f;
        [SerializeField] float endPadding = 280f;

        Action onComplete;
        float initialScrollY;
        float scrollTargetY;
        float currentScrollY;
        bool isShowing;
        float previousTimeScale;

        const float TextTopPadding = 40f;

        public bool IsShowing => isShowing;

        void Awake()
        {
            PauseMenuLegacyTextUtility.FixCanvasScale(transform);
            ResolveReferences();
        }

        void ResolveReferences()
        {
            if (root == null)
            {
                var rootTransform = transform.Find("EndingScrollRoot");
                if (rootTransform != null)
                    root = rootTransform.gameObject;
            }

            if (viewport == null)
                viewport = transform.Find("EndingScrollRoot/Viewport")?.GetComponent<RectTransform>();

            if (content == null)
                content = transform.Find("EndingScrollRoot/Viewport/Content")?.GetComponent<RectTransform>();

            if (bodyLabel == null)
                bodyLabel = transform.Find("EndingScrollRoot/Viewport/Content/BodyText")?.GetComponent<Text>();
        }

        public void Setup(GameObject rootPanel, RectTransform viewportRect, RectTransform contentRect, Text label)
        {
            root = rootPanel;
            viewport = viewportRect;
            content = contentRect;
            bodyLabel = label;
            HideImmediate();
        }

        public void Show(string body, Action complete)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                complete?.Invoke();
                return;
            }

            PauseMenuLegacyTextUtility.FixCanvasScale(transform);
            ResolveReferences();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (bodyLabel != null)
                bodyLabel.text = body;

            LayoutContent();
            ResetScrollPosition();

            onComplete = complete;
            isShowing = true;

            if (root != null)
                root.SetActive(true);

            if (scrollTargetY <= initialScrollY + 1f && bodyLabel != null)
                Debug.LogWarning("[EndingScroll] 布局异常，滚动高度过小。");
        }

        void LayoutContent()
        {
            if (bodyLabel == null || content == null || viewport == null)
            {
                initialScrollY = 0f;
                scrollTargetY = 4000f;
                return;
            }

            var viewportWidth = viewport.rect.width > 0f ? viewport.rect.width : 1200f;
            var viewportHeight = viewport.rect.height > 0f ? viewport.rect.height : 900f;
            var textWidth = Mathf.Min(viewportWidth * 0.72f, 860f);
            var textRect = bodyLabel.rectTransform;
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
            GameUIFontUtility.ConfigureDialogueLabel(bodyLabel);

            Canvas.ForceUpdateCanvases();

            var textHeight = bodyLabel.preferredHeight;
            var contentHeight = textHeight + TextTopPadding * 2f;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, -TextTopPadding);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

            var firstLineCenterFromBottom = contentHeight - TextTopPadding - bodyLabel.fontSize * 0.5f;
            initialScrollY = viewportHeight * 0.5f - firstLineCenterFromBottom;
            scrollTargetY = viewportHeight + endPadding;
        }

        void ResetScrollPosition()
        {
            if (content == null)
                return;

            content.anchorMin = new Vector2(0.5f, 0f);
            content.anchorMax = new Vector2(0.5f, 0f);
            content.pivot = new Vector2(0.5f, 0f);
            currentScrollY = initialScrollY;
            content.anchoredPosition = new Vector2(0f, currentScrollY);
        }

        void Update()
        {
            if (!isShowing || content == null)
                return;

            currentScrollY += scrollSpeed * Time.unscaledDeltaTime;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, currentScrollY);

            if (currentScrollY >= scrollTargetY)
                Finish();
        }

        void Finish()
        {
            if (!isShowing)
                return;

            var callback = onComplete;
            Hide();
            callback?.Invoke();
        }

        public void Hide()
        {
            HideImmediate();
            Time.timeScale = previousTimeScale;
        }

        void HideImmediate()
        {
            isShowing = false;
            onComplete = null;

            if (root != null)
                root.SetActive(false);
        }
    }
}
