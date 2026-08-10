using System;
using System.Collections.Generic;
using JHTGJ.Character;
using JHTGJ.Story;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image backgroundDim;
        [SerializeField] Image protagonistPortrait;
        [SerializeField] Image wifePortrait;
        [SerializeField] Text speakerNameLabel;
        [SerializeField] Text dialogueTextLabel;
        [SerializeField] Button continueButton;
        [SerializeField] Sprite defaultProtagonistPortrait;
        [SerializeField] Sprite defaultWifePortrait;
        [SerializeField] SideViewCharacterController gameplayCharacter;
        [SerializeField] float backgroundDimAlpha = 0.58f;

        readonly List<DialogueLine> activeLines = new List<DialogueLine>();
        int lineIndex;
        Action onComplete;
        float previousTimeScale = 1f;
        Sprite eventProtagonistPortrait;
        Sprite eventWifePortrait;
        bool showWifePortrait = true;

        public bool IsShowing => root != null && root.activeSelf;

        void Awake()
        {
            ResolveGameplayCharacter();
            ResolveBackgroundDim();
            ApplyLabelSettings();
        }

        public void Setup(
            GameObject rootPanel,
            Image protagonistImage,
            Image wifeImage,
            Text speakerLabel,
            Text dialogueLabel,
            Button continueBtn,
            Sprite protagonistDefault,
            Sprite wifeDefault)
        {
            root = rootPanel;
            protagonistPortrait = protagonistImage;
            wifePortrait = wifeImage;
            speakerNameLabel = speakerLabel;
            dialogueTextLabel = dialogueLabel;
            continueButton = continueBtn;
            defaultProtagonistPortrait = protagonistDefault;
            defaultWifePortrait = wifeDefault;

            ResolveBackgroundDim();
            ApplyLabelSettings();

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
                continueButton.onClick.AddListener(OnContinueClicked);

                var buttonLabel = continueButton.GetComponentInChildren<Text>();
                GameUIFontUtility.ConfigureButtonLabel(buttonLabel);
            }

            HideImmediate();
        }

        void ResolveGameplayCharacter()
        {
            if (gameplayCharacter != null)
                return;

            var protagonist = GameObject.Find("Protagonist");
            if (protagonist != null)
                gameplayCharacter = protagonist.GetComponent<SideViewCharacterController>();
        }

        void ResolveBackgroundDim()
        {
            if (backgroundDim != null || root == null)
                return;

            backgroundDim = root.GetComponent<Image>();
        }

        void ApplyLabelSettings()
        {
            if (speakerNameLabel != null)
                speakerNameLabel.gameObject.SetActive(false);

            GameUIFontUtility.ConfigureDialogueLabel(dialogueTextLabel, multiline: true);
            GameUIFontUtility.StretchDialogueTextRect(dialogueTextLabel != null
                ? dialogueTextLabel.rectTransform
                : null);
        }

        void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        public void ShowStory(
            StoryEventDefinition storyEvent,
            Sprite protagonistDefault,
            Sprite wifeDefault,
            Action complete,
            bool hideWifePortrait = false)
        {
            if (storyEvent == null || storyEvent.Lines == null || storyEvent.Lines.Count == 0)
            {
                complete?.Invoke();
                return;
            }

            ResolveGameplayCharacter();
            ApplyLabelSettings();

            showWifePortrait = !hideWifePortrait;
            eventProtagonistPortrait = storyEvent.ProtagonistPortrait != null
                ? storyEvent.ProtagonistPortrait
                : protagonistDefault;
            eventWifePortrait = showWifePortrait && storyEvent.WifePortrait != null
                ? storyEvent.WifePortrait
                : showWifePortrait
                    ? wifeDefault
                    : null;

            activeLines.Clear();
            activeLines.AddRange(storyEvent.Lines);
            lineIndex = 0;
            onComplete = complete;

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            SetGameplayPresentation(true);

            if (root != null)
                root.SetActive(true);

            ShowCurrentLine();
        }

        void SetGameplayPresentation(bool dialogueActive)
        {
            if (backgroundDim != null)
                backgroundDim.color = new Color(0f, 0f, 0f, dialogueActive ? backgroundDimAlpha : 0f);

            ResolveGameplayCharacter();
            if (gameplayCharacter == null)
                return;

            if (dialogueActive)
            {
                gameplayCharacter.SetGameplayVisible(false);
                return;
            }

            gameplayCharacter.SetGameplayVisible(true);
        }

        void ShowCurrentLine()
        {
            if (lineIndex < 0 || lineIndex >= activeLines.Count)
                return;

            var line = activeLines[lineIndex];
            if (speakerNameLabel != null)
                speakerNameLabel.gameObject.SetActive(false);

            if (dialogueTextLabel != null)
                dialogueTextLabel.text = StoryCharacterNames.FormatStoryDialogue(line.SpeakerName, line.Text);

            if (protagonistPortrait != null)
            {
                protagonistPortrait.sprite = line.ProtagonistPortraitOverride != null
                    ? line.ProtagonistPortraitOverride
                    : eventProtagonistPortrait != null
                        ? eventProtagonistPortrait
                        : defaultProtagonistPortrait;
                protagonistPortrait.enabled = protagonistPortrait.sprite != null;
            }

            if (wifePortrait != null)
            {
                if (!showWifePortrait)
                {
                    wifePortrait.enabled = false;
                }
                else
                {
                    wifePortrait.sprite = line.WifePortraitOverride != null
                        ? line.WifePortraitOverride
                        : eventWifePortrait != null
                            ? eventWifePortrait
                            : defaultWifePortrait;
                    wifePortrait.enabled = wifePortrait.sprite != null;
                }
            }
        }

        void OnContinueClicked() => Advance();

        void Update()
        {
            if (!IsShowing)
                return;

            if (FindObjectOfType<EndingScrollUI>(true) is { IsShowing: true })
                return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                Advance();
        }

        void Advance()
        {
            lineIndex++;
            if (lineIndex >= activeLines.Count)
            {
                var callback = onComplete;
                Hide();
                callback?.Invoke();
                return;
            }

            ShowCurrentLine();
        }

        public void Hide()
        {
            HideImmediate();
            Time.timeScale = previousTimeScale;
        }

        void HideImmediate()
        {
            SetGameplayPresentation(false);

            onComplete = null;
            activeLines.Clear();
            lineIndex = 0;

            if (root != null)
                root.SetActive(false);
        }
    }
}
