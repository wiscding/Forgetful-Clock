using System;
using System.Collections;
using System.Collections.Generic;
using JHTGJ.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class RoomSelectionUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] CanvasGroup fadeOverlay;
        [SerializeField] CanvasGroup panelGroup;
        [SerializeField] RectTransform panelRect;
        [SerializeField] Text titleLabel;
        [SerializeField] Text hintLabel;
        [SerializeField] Transform buttonContainer;
        [SerializeField] Button optionButtonTemplate;
        [SerializeField] Button cancelButton;
        [SerializeField] float fadeDuration = 0.28f;

        readonly List<Button> spawnedButtons = new List<Button>();
        float previousTimeScale = 1f;
        Action<RoomDestinationEntry> onSelected;
        Action onClosed;
        Coroutine fadeRoutine;
        bool isShowing;

        bool cancelWired;

        public bool IsShowing => isShowing;

        void Awake()
        {
            if (PauseMenuLegacyTextUtility.NeedsLegacyConversion(gameObject))
                PauseMenuLegacyTextUtility.ConvertCanvas(gameObject);
            else
                PauseMenuLegacyTextUtility.FixCanvasScale(transform);

            ResolveLabelReferences();
            WireCancelButton();
        }

        void ResolveLabelReferences()
        {
            var panel = FindSelectionPanel(transform);
            if (panel == null)
                return;

            if (root == null)
            {
                var rootTransform = transform.Find("RoomSelectionRoot");
                if (rootTransform != null)
                    root = rootTransform.gameObject;
            }

            if (fadeOverlay == null)
                fadeOverlay = transform.GetComponentInChildren<CanvasGroup>(true);

            if (panelRect == null)
                panelRect = panel.GetComponent<RectTransform>();

            if (panelGroup == null)
                panelGroup = panel.GetComponent<CanvasGroup>();

            titleLabel = FindPanelText(panel, "Title") ?? titleLabel;
            hintLabel = FindPanelText(panel, "Hint") ?? hintLabel;

            if (buttonContainer == null)
            {
                var container = panel.Find("ButtonContainer");
                if (container != null)
                    buttonContainer = container;
            }

            if (optionButtonTemplate == null)
            {
                var template = panel.Find("OptionTemplate");
                if (template != null)
                    optionButtonTemplate = template.GetComponent<Button>();
            }

            if (cancelButton == null)
            {
                var cancel = panel.Find("CancelButton");
                if (cancel != null)
                    cancelButton = cancel.GetComponent<Button>();
            }
        }

        static Transform FindSelectionPanel(Transform canvasTransform)
        {
            if (canvasTransform == null)
                return null;

            var root = canvasTransform.Find("RoomSelectionRoot");
            if (root != null)
            {
                var panel = root.Find("SelectionPanel");
                if (panel != null)
                    return panel;
            }

            return canvasTransform.Find("RoomSelectionRoot/SelectionPanel");
        }

        static Text FindPanelText(Transform panel, string objectName)
        {
            var transform = panel.Find(objectName);
            return transform != null ? transform.GetComponent<Text>() : null;
        }

        void WireCancelButton()
        {
            if (cancelButton == null)
            {
                foreach (var button in transform.GetComponentsInChildren<Button>(true))
                {
                    if (button.name == "CancelButton")
                    {
                        cancelButton = button;
                        break;
                    }
                }
            }

            if (cancelButton == null || cancelWired)
                return;

            cancelButton.onClick.RemoveListener(OnCancelClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);
            cancelWired = true;
        }

        public void Setup(
            GameObject rootPanel,
            CanvasGroup overlay,
            CanvasGroup panel,
            RectTransform panelTransform,
            Text title,
            Text hint,
            Transform buttonsRoot,
            Button optionTemplate,
            Button cancel)
        {
            root = rootPanel;
            fadeOverlay = overlay;
            panelGroup = panel;
            panelRect = panelTransform;
            titleLabel = title;
            hintLabel = hint;
            buttonContainer = buttonsRoot;
            optionButtonTemplate = optionTemplate;
            cancelButton = cancel;

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelClicked);
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (optionButtonTemplate != null)
                optionButtonTemplate.gameObject.SetActive(false);

            WireCancelButton();
            HideImmediate();
        }

        void OnDestroy()
        {
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(
            string title,
            IReadOnlyList<RoomDestinationEntry> destinations,
            Action<RoomDestinationEntry> selected,
            Action closed = null)
        {
            if (destinations == null || destinations.Count == 0)
            {
                closed?.Invoke();
                return;
            }

            ResolveLabelReferences();
            onSelected = selected;
            onClosed = closed;

            if (titleLabel != null)
            {
                titleLabel.text = title;
                GameUIFontUtility.ConfigureDialogueLabel(titleLabel, multiline: false);
            }

            if (hintLabel != null)
            {
                hintLabel.text = "选择要前往的房间";
                GameUIFontUtility.ConfigureDialogueLabel(hintLabel, multiline: false);
            }

            RebuildButtons(destinations);

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isShowing = true;

            if (root != null)
                root.SetActive(true);

            if (fadeOverlay != null)
                fadeOverlay.blocksRaycasts = false;

            if (optionButtonTemplate != null)
                optionButtonTemplate.gameObject.SetActive(false);

            if (fadeOverlay != null)
                fadeOverlay.alpha = 0f;
            if (panelGroup != null)
                panelGroup.alpha = 0f;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeIn());
        }

        void RebuildButtons(IReadOnlyList<RoomDestinationEntry> destinations)
        {
            ClearSpawnedButtons();

            if (optionButtonTemplate == null || buttonContainer == null)
                return;

            optionButtonTemplate.gameObject.SetActive(false);

            foreach (var destination in destinations)
            {
                if (destination == null)
                    continue;

                var button = Instantiate(optionButtonTemplate, buttonContainer);
                button.gameObject.SetActive(true);
                button.name = $"GoTo_{destination.TargetRoom}";

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = destination.Label;
                    GameUIFontUtility.ConfigureButtonLabel(label);
                }

                var captured = destination;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnDestinationClicked(captured));
                spawnedButtons.Add(button);
            }
        }

        void OnDestinationClicked(RoomDestinationEntry destination)
        {
            var callback = onSelected;
            Close(() => callback?.Invoke(destination));
        }

        void OnCancelClicked()
        {
            Close(skipFade: true);
        }

        void Update()
        {
            if (!isShowing)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                Close(skipFade: true);
        }

        void Close(Action beforeHide = null, bool skipFade = false)
        {
            if (!isShowing)
                return;

            beforeHide?.Invoke();

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            if (skipFade)
            {
                var closed = onClosed;
                HideImmediate();
                closed?.Invoke();
                return;
            }

            fadeRoutine = StartCoroutine(FadeOut());
        }

        IEnumerator FadeIn()
        {
            yield return FadeGroup(fadeOverlay, 0f, 0.82f);
            yield return FadeGroup(panelGroup, 0f, 1f);
            fadeRoutine = null;
        }

        IEnumerator FadeOut()
        {
            yield return FadeGroup(panelGroup, panelGroup != null ? panelGroup.alpha : 1f, 0f);
            yield return FadeGroup(fadeOverlay, fadeOverlay != null ? fadeOverlay.alpha : 0.82f, 0f);

            var closed = onClosed;
            HideImmediate();
            closed?.Invoke();
            fadeRoutine = null;
        }

        IEnumerator FadeGroup(CanvasGroup group, float from, float to)
        {
            if (group == null)
                yield break;

            var duration = Mathf.Max(0.01f, fadeDuration);
            var elapsed = 0f;
            group.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            group.alpha = to;
        }

        void ClearSpawnedButtons()
        {
            foreach (var button in spawnedButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }

            spawnedButtons.Clear();
        }

        public void HideImmediate()
        {
            isShowing = false;
            onSelected = null;
            onClosed = null;
            ClearSpawnedButtons();

            if (root != null)
                root.SetActive(false);

            Time.timeScale = previousTimeScale;
        }
    }
}
