#if UNITY_EDITOR
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    [CustomEditor(typeof(DayStorySchedule))]
    public class DayStoryScheduleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "一天顺序：醒来 → 做饭 → 上午 → 午饭 → 下午 → 晚饭 → 晚上 → 夜晚事件 → 睡觉前。\n" +
                "· 醒来：进入阶段后自动播放 Events 第一条对话\n" +
                "· 做饭 / 午饭：需到场景互动点按 E 触发\n" +
                "· 做饭阶段结束后（如冰箱对话完成）进入下一阶段，Character Presences 才会刷新（如妻子到餐厅）\n" +
                "· Include Night Event：关闭则跳过「夜晚事件」，晚上结束后直接进睡觉前\n" +
                "· 上午/下午/晚上：完成当前阶段任意一个交互后进入下一阶段\n" +
                "Phase Type 选「按按钮」时，会自动弹出选项面板，点击按钮进入对话。\n" +
                "地下室「紧急停止」：第 1–2 天播放 Ending Event 后继续当天；第 3–4 天播放后进入结局一（黑幕滚动）；\n" +
                "第 5 天「最后」阶段播放 Ending Event 后进入结局二（黑幕滚动）。\n\n" +
                "在 Phases 列表中用 + / - 添加或删除阶段；每个阶段内的 Events 同样可增删。\n" +
                "Event Id 需与场景中 InteractPoint 的 Interact Id 一致（如 Interact_Fridge）。\n\n" +
                "立绘优先级（从高到低）：\n" +
                "· 单句 Lines → Protagonist/Wife Portrait Override\n" +
                "· 单个 Event → Protagonist/Wife Portrait\n" +
                "· 当前阶段 Phase → Phase Protagonist/Wife Portrait（如睡前睡衣立绘）\n" +
                "· 日程默认 → Default Protagonist/Wife Portrait\n\n" +
                "角色在场：在 Phase → Character Presences 配置妻子等角色的房间、位置、形象；\n" +
                "场景中需有 StoryCharacter_Wife（菜单 JHTGJ/Setup Story Character Wife）。\n" +
                "Interact Id 与 Event Id 一致，靠近角色按 E 进入对话。\n\n" +
                "按按钮阶段：Phase Type = 按按钮；Events 每项填 Lines 与 Button Label（按钮文字）；\n" +
                "不需要场景互动点，进入阶段后自动弹出选择面板。\n\n" +
                "可选：勾选「Change Room Background After Dialogue」并指定 Sprite，对话结束后会更换房间 Background 子物体的背景图。",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
}
#endif
