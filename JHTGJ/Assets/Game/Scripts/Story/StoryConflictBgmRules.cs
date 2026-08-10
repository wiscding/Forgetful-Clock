namespace JHTGJ.Story
{
    public static class StoryConflictBgmRules
    {
        public static bool ShouldPlayConflictBgm(int day, StoryPhaseType phaseType)
        {
            switch (day)
            {
                case 1:
                    return phaseType == StoryPhaseType.Dinner
                        || phaseType == StoryPhaseType.Evening
                        || phaseType == StoryPhaseType.BeforeSleep;
                case 2:
                    return phaseType == StoryPhaseType.Dinner
                        || phaseType == StoryPhaseType.Evening;
                case 3:
                    return phaseType == StoryPhaseType.Cooking
                        || phaseType == StoryPhaseType.Morning
                        || phaseType == StoryPhaseType.LunchTime
                        || phaseType == StoryPhaseType.Afternoon
                        || phaseType == StoryPhaseType.Dinner
                        || phaseType == StoryPhaseType.Evening;
                default:
                    return false;
            }
        }
    }
}
