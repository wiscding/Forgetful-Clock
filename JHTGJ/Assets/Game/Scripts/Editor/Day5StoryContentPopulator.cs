#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day5StoryContentPopulator
    {
        public const string Day5AssetPath = "Assets/Game/Data/Day5StorySchedule.asset";

        const string StorageCleanBackgroundPath = StorageRoomBackgroundPaths.CleanBackground;

        [MenuItem("JHTGJ/Populate Day 5 Story Content")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day5AssetPath);
            if (schedule == null)
            {
                schedule = ScriptableObject.CreateInstance<DayStorySchedule>();
                AssetDatabase.CreateAsset(schedule, Day5AssetPath);
            }

            Populate(schedule);
            EditorUtility.SetDirty(schedule);
            AssetDatabase.SaveAssets();

            DayStoryScheduleCreator.EnsureCampaignIncludesAllDays();

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log("[JHTGJ] Day 5 story content populated.");
        }

        public static void Populate(DayStorySchedule schedule)
        {
            var protagonistCasualHalf = LoadSprite(StoryPortraitPaths.ProtagonistCasualHalf);
            var wifeCasualHalf = LoadSprite(StoryPortraitPaths.WifeCasualHalf);
            var wifeCasualFull = LoadSprite(StoryPortraitPaths.WifeCasualFull);
            var storageCleanBackground = LoadSprite(StorageCleanBackgroundPath);

            var so = new SerializedObject(schedule);
            so.FindProperty("defaultProtagonistPortrait").objectReferenceValue = protagonistCasualHalf;
            so.FindProperty("defaultWifePortrait").objectReferenceValue = wifeCasualHalf;
            so.FindProperty("storageCleanBackground").objectReferenceValue = storageCleanBackground;
            so.FindProperty("includeNightEvent").boolValue = false;

            var phases = so.FindProperty("phases");
            phases.ClearArray();

            AddWakeUpPhase(phases);
            AddCookingPhase(phases, wifeCasualFull);
            AddMorningPhase(phases, wifeCasualFull);
            AddFinalPhase(phases, wifeCasualFull);

            SetEnding(so.FindProperty("endingEvent"));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddWakeUpPhase(SerializedProperty phases)
        {
            AddPhase(phases, "醒来", StoryPhaseType.WakeUp,
                Ev("Auto_WakeUp", "醒来", InteractionKind.Sleep,
                    N("清晨的窗外传来了雨声，我从睡梦中醒来。"),
                    N("她正怀着笑意地凝望着我，我不禁困惑。"),
                    ("阿述", "怎么不叫醒我呢？"),
                    N("她就这样在我的身边，微微地摇了摇头。"),
                    ("许薇", "我还想多看看你的睡脸。"),
                    N("我笑了。"),
                    ("许薇", "先起来吃早饭吧。"),
                    N("她的声音具有穿透力，仿佛揭开一切的阴霾，竟然让我隐隐地感到不安。"),
                    ("阿述", "没问题。"),
                    N("也许是为了逃避这种不安，我决定立刻起身准备早饭，好让自己转移一下注意力。")));
        }

        static void AddCookingPhase(SerializedProperty phases, Sprite wifeCasual)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = "做饭";
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)StoryPhaseType.Cooking;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            AddEvent(eventsProp, "Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                N("我准备好了早餐，那是她最喜欢的面包。"),
                N("她吃得很开心，但同时又时刻注意着我。"),
                N("一旦我想对上视线，她又会有些犹豫地撇开。"),
                ("阿述", "怎么了吗？"),
                N("我问出了从醒来以后一直存在的疑问，她则是深吸了一口气，像是要一下道出什么惊天新闻。"),
                ("许薇", "阿述，你做好心理准备了吗？"),
                N("她用一种近乎担忧的语气问道。"),
                ("阿述", "怎么忧心忡忡的会是你呢？"),
                N("我用玩笑般的语气笑着回应，但实际上我一点也笑不出来。"),
                ("许薇", "阿述……不对，程述……"),
                ("许薇", "结束循环吧。"),
                N("她的声音有些颤抖，一只手用力地握住了我的手，以至于现在我分不清，究竟是她在颤抖，还是我在颤抖。"),
                ("阿述", "你……"),
                ("阿述", "你怎么会知道循环的事情？"),
                N("我选择了用问题来回答问题。"),
                ("许薇", "我不知道为什么。"),
                ("许薇", "在我第一次触摸到我的日记本之后，我的记忆就不再重置了。"),
                ("阿述", "什……什么日记本？"),
                ("许薇", "阿述，自从我手术之后我重新开始写起了日记。"),
                ("许薇", "或者说——遗书。"),
                N("我的脑中还是一团浆糊。"),
                ("许薇", "我也不知道为什么，我的这本日记同样没有被循环。"),
                ("许薇", "就像……就像一个bug一样。"),
                N("我的思绪逐渐理出了一条线条。"),
                ("许薇", "就是到这里的第二天，或者说第一次循环时，我触摸到我的日记……"),
                ("阿述", "你恢复了记忆，而且记忆也不再重置了。"),
                ("许薇", "是的。"),
                N("我很震惊，但一条清晰的链条出现在我的脑中。"),
                N("许薇的日记根本就没有记录在时间的循环里，而一个月的时间在无测试情况下启动的循环机器，出现了意外的bug。"),
                N("循环的区域里，有没有被列入循环的物件，甚至是承载记忆的物件，让许薇也成为了半个bug。"),
                N("身体虽然还在循环，但记忆已经脱离了循环。"),
                ("阿述", "今天是第几天了？"),
                ("许薇", "嗯——"),
                N("她思考了一会儿。"),
                ("许薇", "第三十二天，循环了三十一次。"),
                N("我感觉巨量的信息涌入大脑，或许我现在也成为了类似于bug的存在，但我分不清究竟是循环记忆的涌入还是大量信息的冲击。"),
                ("许薇", "先歇一下吧，阿述。"),
                N("我回过神来，注意到自己已经被汗水浸湿。"),
                ("阿述", "好……好的。"),
                N("我已经无法品尝出嘴中的任何滋味。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
        }

        static void AddMorningPhase(SerializedProperty phases, Sprite wifeCasual)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = "上午";
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)StoryPhaseType.Morning;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                N("继续跟她交流一下吧。"),
                ("阿述", "可以给我看一下日记吗？"),
                ("许薇", "抱歉，阿述，我不能给你。"),
                ("阿述", "为什么？"),
                N("我十分的不解，但出于对她的信任我控制住了任何负面的情绪。"),
                ("许薇", "我在触摸到了日记的时候，脑子感觉到了一阵剧痛。"),
                ("许薇", "这一阵剧痛一直持续到我回忆起第一天的所有记忆。"),
                ("许薇", "而之后每过一天，我的都会感受到隐隐的头痛。"),
                ("许薇", "循环的天数越多，这种头痛越强烈。"),
                ("许薇", "所以我担心，你触碰到了日记之后承受更强烈的痛苦。"),
                ("许薇", "所以别着急，阿述，等结束循环，我再把日记交给你。"),
                N("我在听她陈述的时候就已经感觉到了难以控制的悲伤，在我在一个个循环中毫不知情的同时，她却在承受如此的痛苦。"),
                N("而当我知晓这一切的时候，不是因为她倾诉自己的痛苦，而是她担心我承受痛苦。"),
                N("我不禁握紧了自己的拳头，为自己的无能为力而自责。"),
                ("许薇", "阿述，结束循环之后，你答应我。"),
                ("阿述", "答应你什么？"),
                ("许薇", "你先答应我。"),
                ("阿述", "我答应你，什么我都答应你。"),
                ("许薇", "那你一定要回归自己正常的生活。"),
                ("阿述", "好的……"),
                ("许薇", "你要照顾好我们的爸爸妈妈。"),
                ("阿述", "好的……"),
                ("许薇", "如果一个人太累了你就多请几个保姆。"),
                ("阿述", "好的……"),
                ("许薇", "你这么优秀说不定还会有新的人喜欢上你。"),
                ("阿述", "……"),
                ("许薇", "你一定要看清楚了找一个爱你的人。"),
                ("阿述", "你在说什么啊？"),
                ("许薇", "关于这里的事，你就跟外面说我让你是带我去临终旅行。"),
                ("许薇", "这个机械该销毁就销毁了，做这种事是不对的你是清楚的。"),
                ("阿述", "好的……"),
                ("许薇", "然后……"),
                ("许薇", "在我死之前好好陪陪我吧，不要再像之前一样消失了。"),
                ("阿述", "……"),
                ("阿述", "好的……"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
        }

        static void AddFinalPhase(SerializedProperty phases, Sprite wifeCasual)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = "最后";
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)StoryPhaseType.Afternoon;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            AddCharacterPresence(phase, "Interact_Partner", RoomType.Basement, 2f, FacingDirection.Left, wifeCasual);
        }

        static void SetEnding(SerializedProperty ending)
        {
            SetEvent(ending, "Interact_EmergencyStop", "紧急停止", InteractionKind.EmergencyStop,
                N("要按下按钮吗？"));
        }

        static Sprite LoadSprite(string path) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(path);

        static (string speaker, string text) N(string text) => ("", text);

        static (string eventId, string summary, InteractionKind kind, (string speaker, string text)[] lines) Ev(
            string eventId,
            string summary,
            InteractionKind kind,
            params (string speaker, string text)[] lines) =>
            (eventId, summary, kind, lines);

        static void AddPhase(
            SerializedProperty phases,
            string displayName,
            StoryPhaseType phaseType,
            params (string eventId, string summary, InteractionKind kind, (string speaker, string text)[] lines)[] events)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = displayName;
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)phaseType;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            foreach (var evt in events)
                AddEvent(eventsProp, evt.eventId, evt.summary, evt.kind, evt.lines);
        }

        static void ClearPhaseCharacterState(SerializedProperty phase)
        {
            phase.FindPropertyRelative("characterPresences").ClearArray();
            phase.FindPropertyRelative("phaseProtagonistPortrait").objectReferenceValue = null;
            phase.FindPropertyRelative("phaseWifePortrait").objectReferenceValue = null;
        }

        static void AddCharacterPresence(
            SerializedProperty phase,
            string interactId,
            RoomType room,
            float localX,
            FacingDirection facing,
            Sprite idleSprite = null)
        {
            var list = phase.FindPropertyRelative("characterPresences");
            list.InsertArrayElementAtIndex(list.arraySize);
            var entry = list.GetArrayElementAtIndex(list.arraySize - 1);
            entry.FindPropertyRelative("interactId").stringValue = interactId;
            entry.FindPropertyRelative("room").enumValueIndex = (int)room;
            entry.FindPropertyRelative("localX").floatValue = localX;
            entry.FindPropertyRelative("useScenePosition").boolValue = false;
            entry.FindPropertyRelative("idleSprite").objectReferenceValue = idleSprite;
            SetFacing(entry.FindPropertyRelative("facing"), facing);
        }

        static void SetFacing(SerializedProperty property, FacingDirection facing)
        {
            for (var i = 0; i < property.enumNames.Length; i++)
            {
                if (property.enumNames[i] != facing.ToString())
                    continue;

                property.enumValueIndex = i;
                return;
            }
        }

        static void AddEvent(
            SerializedProperty events,
            string eventId,
            string summary,
            InteractionKind kind,
            params (string speaker, string text)[] lines)
        {
            events.InsertArrayElementAtIndex(events.arraySize);
            SetEvent(events.GetArrayElementAtIndex(events.arraySize - 1), eventId, summary, kind, lines);
        }

        static void SetEvent(
            SerializedProperty evt,
            string eventId,
            string summary,
            InteractionKind kind,
            params (string speaker, string text)[] lines)
        {
            evt.FindPropertyRelative("eventId").stringValue = eventId;
            evt.FindPropertyRelative("summary").stringValue = summary;
            evt.FindPropertyRelative("interactKind").enumValueIndex = (int)kind;

            var linesProp = evt.FindPropertyRelative("lines");
            linesProp.ClearArray();
            foreach (var (speaker, text) in lines)
            {
                linesProp.InsertArrayElementAtIndex(linesProp.arraySize);
                var line = linesProp.GetArrayElementAtIndex(linesProp.arraySize - 1);
                line.FindPropertyRelative("speakerName").stringValue = speaker;
                line.FindPropertyRelative("text").stringValue = text;
            }
        }
    }
}
#endif
