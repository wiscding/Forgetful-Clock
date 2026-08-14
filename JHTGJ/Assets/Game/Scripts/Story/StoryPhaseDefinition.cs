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

        [Header("按按钮阶段")]
        [SerializeField] string choicePrompt = "请选择";

        [Header("阶段立绘")]
        [SerializeField] Sprite phaseProtagonistPortrait;
        [SerializeField] Sprite phaseWifePortrait;

        [Header("角色在场")]
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
