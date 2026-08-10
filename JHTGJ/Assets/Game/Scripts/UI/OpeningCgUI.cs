using System;
using JHTGJ.Story;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class OpeningCgUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image cgImage;
        [SerializeField] Text bodyLabel;
        [SerializeField] Image textPanelBackground;

        Action onComplete;
        Sprite[] slideImages;
        DialogueLine[][] slideLines;
        int slideIndex;
        int lineIndex;
        bool isShowing;
        float previousTimeScale;

        public bool IsShowing => isShowing;

        public void Setup(
            GameObject rootPanel,
            Image image,
            Text label,
            Image panelBackground = null)
        {
            root = rootPanel;
            cgImage = image;
            bodyLabel = label;
            textPanelBackground = panelBackground;
            HideSpeakerLabel();
            ApplyLayout();
            HideImmediate();
        }

        void Awake()
        {
            HideSpeakerLabel();
            ApplyLayout();
        }

        void HideSpeakerLabel()
        {
            if (textPanelBackground == null)
                return;

            var existing = textPanelBackground.transform.Find("SpeakerName");
            if (existing != null)
                existing.gameObject.SetActive(false);
        }

        void ApplyLayout()
        {
            if (cgImage == null && textPanelBackground == null)
                return;

            OpeningCgUIBuilder.ApplyLayout(
                cgImage != null ? cgImage.rectTransform : null,
                textPanelBackground != null ? textPanelBackground.rectTransform : null);

            OpeningCgUIBuilder.ApplyBodyTextStyle(bodyLabel);
            HideSpeakerLabel();

            if (bodyLabel != null)
            {
                var bodyRect = bodyLabel.rectTransform;
                bodyRect.anchorMin = new Vector2(0.06f, 0.12f);
                bodyRect.anchorMax = new Vector2(0.94f, 0.88f);
                bodyRect.offsetMin = Vector2.zero;
                bodyRect.offsetMax = Vector2.zero;
            }
        }

        public void Play(Sprite[] images, DialogueLine[][] lines, Action complete)
        {
            if (images == null || images.Length == 0 || lines == null || lines.Length == 0)
            {
                complete?.Invoke();
                return;
            }

            slideImages = images;
            slideLines = lines;
            slideIndex = 0;
            lineIndex = 0;
            onComplete = complete;

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isShowing = true;

            if (root != null)
                root.SetActive(true);

            ShowCurrentSlide();
            ShowCurrentLine();
        }

        void ShowCurrentSlide()
        {
            if (cgImage == null || slideImages == null || slideIndex < 0 || slideIndex >= slideImages.Length)
                return;

            cgImage.sprite = slideImages[slideIndex];
            cgImage.enabled = slideImages[slideIndex] != null;
            cgImage.preserveAspect = true;
        }

        void ShowCurrentLine()
        {
            if (slideLines == null || slideIndex >= slideLines.Length)
                return;

            var lines = slideLines[slideIndex];
            if (lines == null || lineIndex < 0 || lineIndex >= lines.Length)
            {
                if (bodyLabel != null)
                    bodyLabel.text = string.Empty;
                return;
            }

            var line = lines[lineIndex];
            if (bodyLabel != null)
                bodyLabel.text = StoryCharacterNames.FormatCgDialogue(line.SpeakerName, line.Text);
        }

        void Update()
        {
            if (!isShowing)
                return;

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetMouseButtonDown(0))
                Advance();
        }

        void Advance()
        {
            if (slideLines == null || slideIndex >= slideLines.Length)
            {
                Finish();
                return;
            }

            var lines = slideLines[slideIndex];
            lineIndex++;

            if (lines != null && lineIndex < lines.Length)
            {
                ShowCurrentLine();
                return;
            }

            slideIndex++;
            lineIndex = 0;

            if (slideIndex >= slideImages.Length || slideIndex >= slideLines.Length)
            {
                Finish();
                return;
            }

            ShowCurrentSlide();
            ShowCurrentLine();
        }

        void Finish()
        {
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
            slideImages = null;
            slideLines = null;
            slideIndex = 0;
            lineIndex = 0;

            if (root != null)
                root.SetActive(false);
        }
    }
}
