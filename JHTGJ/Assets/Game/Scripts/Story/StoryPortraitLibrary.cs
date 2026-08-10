using UnityEngine;

namespace JHTGJ.Story
{
    [CreateAssetMenu(fileName = "StoryPortraitLibrary", menuName = "JHTGJ/Story Portrait Library")]
    public class StoryPortraitLibrary : ScriptableObject
    {
        [SerializeField] Sprite protagonistCasualHalf;
        [SerializeField] Sprite wifeCasualHalf;
        [SerializeField] Sprite protagonistPajamaHalf;
        [SerializeField] Sprite wifePajamaHalf;

        static StoryPortraitLibrary cached;

        public static StoryPortraitLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<StoryPortraitLibrary>("StoryPortraitLibrary");
                return cached;
            }
        }

        public static bool UsesPajamaPortrait(StoryPhaseType phaseType) =>
            phaseType == StoryPhaseType.BeforeSleep;

        public Sprite GetProtagonistPortrait(StoryPhaseType phaseType) =>
            UsesPajamaPortrait(phaseType) ? protagonistPajamaHalf : protagonistCasualHalf;

        public Sprite GetWifePortrait(StoryPhaseType phaseType) =>
            UsesPajamaPortrait(phaseType) ? wifePajamaHalf : wifeCasualHalf;
    }
}
