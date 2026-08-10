using JHTGJ.Story;

namespace JHTGJ.Scene
{
    public static class SpecialNightEventResolver
    {
        public static bool TryGetConfig(int day, StoryPhaseType phaseType, out SpecialNightEventConfig config)
        {
            config = default;

            if (day == 1 && phaseType == StoryPhaseType.NightEvent)
                return TryGetDay1NightEventConfig(out config);

            if (day == 2 && phaseType == StoryPhaseType.NightEvent)
                return TryGetDay2NightEventConfig(out config);

            if (day == 4 && phaseType == StoryPhaseType.Dusk)
                return TryGetDay4DuskConfig(out config);

            return false;
        }

        static bool TryGetDay1NightEventConfig(out SpecialNightEventConfig config)
        {
            config = default;
            var background = Day1NightEventLibrary.Instance?.BedroomBackground;
            if (background == null)
                return false;

            config = new SpecialNightEventConfig
            {
                Room = RoomType.Bedroom,
                Background = background,
                WifeLocalX = Day1NightEventInteractLayout.WifeLocalX,
                StandXOffset = Day1NightEventInteractLayout.StandXOffset,
                WifeFacing = Day1NightEventInteractLayout.WifeFacing
            };
            return true;
        }

        static bool TryGetDay2NightEventConfig(out SpecialNightEventConfig config)
        {
            config = default;
            var background = Day2NightEventLibrary.Instance?.KitchenBackground;
            if (background == null)
                return false;

            config = new SpecialNightEventConfig
            {
                Room = RoomType.Kitchen,
                Background = background,
                WifeLocalX = Day2NightEventInteractLayout.WifeLocalX,
                StandXOffset = Day2NightEventInteractLayout.StandXOffset,
                WifeFacing = Day2NightEventInteractLayout.WifeFacing
            };
            return true;
        }

        static bool TryGetDay4DuskConfig(out SpecialNightEventConfig config)
        {
            config = default;
            var background = Day4DuskEventLibrary.Instance?.RooftopBackground;
            if (background == null)
                return false;

            config = new SpecialNightEventConfig
            {
                Room = RoomType.Rooftop,
                Background = background,
                WifeLocalX = Day4DuskEventInteractLayout.WifeLocalX,
                StandXOffset = Day4DuskEventInteractLayout.StandXOffset,
                WifeFacing = Day4DuskEventInteractLayout.WifeFacing
            };
            return true;
        }
    }
}
