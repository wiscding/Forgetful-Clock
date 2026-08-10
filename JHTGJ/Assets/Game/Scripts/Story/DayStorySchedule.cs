using System.Collections.Generic;
using UnityEngine;

namespace JHTGJ.Story
{
    [CreateAssetMenu(fileName = "DayStorySchedule", menuName = "JHTGJ/Day Story Schedule")]
    public class DayStorySchedule : ScriptableObject
    {
        [SerializeField] List<StoryPhaseDefinition> phases = new List<StoryPhaseDefinition>();
        [SerializeField] bool includeNightEvent = true;
        [SerializeField] StoryEventDefinition endingEvent;
        [SerializeField] Sprite defaultProtagonistPortrait;
        [SerializeField] Sprite defaultWifePortrait;
        [SerializeField] Sprite storageCleanBackground;

        public IReadOnlyList<StoryPhaseDefinition> Phases => phases;
        public bool IncludeNightEvent => includeNightEvent;
        public StoryEventDefinition EndingEvent => endingEvent;
        public Sprite DefaultProtagonistPortrait => defaultProtagonistPortrait;
        public Sprite DefaultWifePortrait => defaultWifePortrait;
        public Sprite StorageCleanBackground => storageCleanBackground;
    }
}
