using System;
using System.Collections.Generic;
using JHTGJ.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class StoryChoiceUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI hintLabel;
        [SerializeField] Transform buttonContainer;
        [SerializeField] Button optionButtonTemplate;

        readonly List<Button> spawnedButtons = new List<Button>();
        Action<StoryEventDefinition> onSelected;

        public bool IsShowing => root != null && root.activeSelf;

        public void Setup(
            GameObject rootPanel,
            TextMeshProUGUI title,
            TextMeshProUGUI hint,
            Transform buttonsRoot,
            Button optionTemplate)
        {
            root = rootPanel;
            titleLabel = title;
            hintLabel = hint;
            buttonContainer = buttonsRoot;
            optionButtonTemplate = optionTemplate;

            if (optionButtonTemplate != null)
                optionButtonTemplate.gameObject.SetActive(false);

            HideImmediate();
        }

        public void Show(
            string phaseTitle,
            string prompt,
            IReadOnlyList<StoryEventDefinition> events,
            Action<StoryEventDefinition> selected)
        {
            if (events == null || events.Count == 0)
            {
                HideImmediate();
                return;
            }

            onSelected = selected;

            if (titleLabel != null)
                titleLabel.text = string.IsNullOrWhiteSpace(phaseTitle) ? "请选择" : phaseTitle;
            if (hintLabel != null)
                hintLabel.text = string.IsNullOrWhiteSpace(prompt) ? "选择一个选项" : prompt;

            RebuildButtons(events);

            if (root != null)
                root.SetActive(true);
        }

        public void Hide() => HideImmediate();

        void HideImmediate()
        {
            onSelected = null;
            ClearSpawnedButtons();

            if (root != null)
                root.SetActive(false);
        }

        void RebuildButtons(IReadOnlyList<StoryEventDefinition> events)
        {
            ClearSpawnedButtons();

            if (optionButtonTemplate == null || buttonContainer == null)
                return;

            optionButtonTemplate.gameObject.SetActive(false);

            foreach (var storyEvent in events)
            {
                if (storyEvent == null || storyEvent.Lines == null || storyEvent.Lines.Count == 0)
                    continue;

                var button = Instantiate(optionButtonTemplate, buttonContainer);
                button.gameObject.SetActive(true);
                button.name = $"Choice_{storyEvent.EventId}";

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = storyEvent.ButtonLabel;

                var captured = storyEvent;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onSelected?.Invoke(captured));
                spawnedButtons.Add(button);
            }
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
    }
}
