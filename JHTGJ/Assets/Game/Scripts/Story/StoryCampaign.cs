using System.Collections.Generic;
using UnityEngine;

namespace JHTGJ.Story
{
    [CreateAssetMenu(fileName = "StoryCampaign", menuName = "JHTGJ/Story Campaign")]
    public class StoryCampaign : ScriptableObject
    {
        [SerializeField] List<DayStorySchedule> days = new List<DayStorySchedule>();
        [SerializeField] bool loopAfterLastDay = true;

        public IReadOnlyList<DayStorySchedule> Days => days;
        public bool LoopAfterLastDay => loopAfterLastDay;

        public DayStorySchedule GetScheduleForDay(int day)
        {
            if (days == null || days.Count == 0)
                return null;

            var index = day - 1;
            if (index < 0)
                index = 0;

            if (index >= days.Count)
            {
                if (!loopAfterLastDay)
                    return days[days.Count - 1];

                index %= days.Count;
            }

            return days[index];
        }

        public int ConfiguredDayCount => days != null ? days.Count : 0;
    }
}
