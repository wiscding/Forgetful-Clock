using System;
using System.Collections.Generic;
using UnityEngine;

namespace JHTGJ.Story
{
    [Serializable]
    public class StoryPhaseDefinition
    {
        [SerializeField] string displayName = "上午";
        [SerializeField] StoryPhaseType phaseType = StoryPhaseType.Morning;
        [SerializeField] List<StoryEventDefinition> events = new List<StoryEventDefinition>();

        [Header("按按钮阶段（可选）")]
        [Tooltip("Phase Type 为「按按钮」时，作为选择面板标题。")]
        [SerializeField] string choicePrompt = "请选择";

        [Header("阶段立绘（可选）")]
        [Tooltip("留空则使用 DayStorySchedule 的默认立绘。")]
        [SerializeField] Sprite phaseProtagonistPortrait;
        [Tooltip("留空则使用 DayStorySchedule 的默认立绘。")]
        [SerializeField] Sprite phaseWifePortrait;

        [Header("角色在场（可选）")]
        [Tooltip("配置本阶段出现的可互动角色（如妻子）及其位置、形象。")]
        [SerializeField] List<StoryPhaseCharacterPresence> characterPresences = new List<StoryPhaseCharacterPresence>();

        public string DisplayName => displayName;
        public StoryPhaseType PhaseType => phaseType;
        public string ChoicePrompt => choicePrompt;
        public IReadOnlyList<StoryEventDefinition> Events => events;
        public Sprite PhaseProtagonistPortrait => phaseProtagonistPortrait;
        public Sprite PhaseWifePortrait => phaseWifePortrait;
        public IReadOnlyList<StoryPhaseCharacterPresence> CharacterPresences => characterPresences;
    }
}
