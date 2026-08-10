namespace JHTGJ.Story
{
    public static class StoryPhaseTimeOfDay
    {
        public static bool UsesNightRoomBackground(StoryPhaseType phaseType)
        {
            switch (phaseType)
            {
                case StoryPhaseType.WakeUp:
                case StoryPhaseType.Cooking:
                case StoryPhaseType.Morning:
                case StoryPhaseType.LunchTime:
                case StoryPhaseType.Afternoon:
                    return false;
                default:
                    return true;
            }
        }
    }
}
