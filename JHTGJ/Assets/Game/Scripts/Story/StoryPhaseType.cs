using UnityEngine;

namespace JHTGJ.Story
{
    public enum StoryPhaseType
    {
        [InspectorName("上午")]
        Morning,

        [InspectorName("下午")]
        Afternoon,

        [InspectorName("晚上")]
        Evening,

        [InspectorName("晚饭时间")]
        Dinner,

        [InspectorName("睡觉前")]
        BeforeSleep,

        [InspectorName("按按钮")]
        ButtonChoice,

        [InspectorName("醒来")]
        WakeUp,

        [InspectorName("做饭")]
        Cooking,

        [InspectorName("午饭时间")]
        LunchTime,

        [InspectorName("夜晚事件")]
        NightEvent,

        [InspectorName("傍晚")]
        Dusk
    }
}
