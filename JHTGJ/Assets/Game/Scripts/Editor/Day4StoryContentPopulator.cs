#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day4StoryContentPopulator
    {
        public const string Day4AssetPath = "Assets/Game/Data/Day4StorySchedule.asset";

        const string StorageCleanBackgroundPath = StorageRoomBackgroundPaths.CleanBackground;

        [MenuItem("JHTGJ/Populate Day 4 Story Content")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day4AssetPath);
            if (schedule == null)
            {
                schedule = ScriptableObject.CreateInstance<DayStorySchedule>();
                AssetDatabase.CreateAsset(schedule, Day4AssetPath);
            }

            Populate(schedule);
            EditorUtility.SetDirty(schedule);
            AssetDatabase.SaveAssets();

            DayStoryScheduleCreator.EnsureCampaignIncludesAllDays();

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log("[JHTGJ] Day 4 story content populated.");
        }

        public static void Populate(DayStorySchedule schedule)
        {
            var protagonistCasualHalf = LoadSprite(StoryPortraitPaths.ProtagonistCasualHalf);
            var wifeCasualHalf = LoadSprite(StoryPortraitPaths.WifeCasualHalf);
            var protagonistPajamaHalf = LoadSprite(StoryPortraitPaths.ProtagonistPajamaHalf);
            var wifePajamaHalf = LoadSprite(StoryPortraitPaths.WifePajamaHalf);
            var wifeCasualFull = LoadSprite(StoryPortraitPaths.WifeCasualFull);
            var wifePajamaFull = LoadSprite(StoryPortraitPaths.WifePajamaFull);
            var storageCleanBackground = LoadSprite(StorageCleanBackgroundPath);

            var so = new SerializedObject(schedule);
            so.FindProperty("defaultProtagonistPortrait").objectReferenceValue = protagonistCasualHalf;
            so.FindProperty("defaultWifePortrait").objectReferenceValue = wifeCasualHalf;
            so.FindProperty("storageCleanBackground").objectReferenceValue = storageCleanBackground;
            so.FindProperty("includeNightEvent").boolValue = false;

            var phases = so.FindProperty("phases");
            phases.ClearArray();

            AddWakeUpPhase(phases);
            AddCookingPhase(phases);
            AddMorningPhase(phases, wifeCasualFull);
            AddLunchPhase(phases);
            AddAfternoonPhase(phases, wifeCasualFull);
            AddDinnerPhase(phases);
            AddDuskPhase(phases, wifeCasualFull);
            AddEveningPhase(phases);
            AddBeforeSleepPhase(phases, protagonistPajamaHalf, wifePajamaHalf, wifePajamaFull);

            SetEnding(so.FindProperty("endingEvent"));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddWakeUpPhase(SerializedProperty phases)
        {
            AddPhase(phases, "醒来", StoryPhaseType.WakeUp,
                Ev("Auto_WakeUp", "醒来", InteractionKind.Sleep,
                    ("许薇", "早上好啊，阿述！"),
                    N("妻子的声音与笑容都是那样接近，以至于我有点想哭出来。"),
                    ("阿述", "早上好，薇薇。"),
                    N("我回给她一个笑容，立刻着衣起床。"),
                    N("回头看见无法起床的妻子的瞬间，我突然意识到一切已经不一样了。车祸后下肢瘫痪加上器官衰竭的她，现在难以独自行动。"),
                    ("阿述", "来，我来帮你。"),
                    ("许薇", "怎么不叫我公主呢？"),
                    N("我愣了一下，这句话上次出现似乎已经是我俩二十出头的时候了。"),
                    ("阿述", "来，爱撒娇的公主殿下，你的王子来接你了。"),
                    ("许薇", "好土啊！"),
                    N("她笑得很开心。"),
                    ("阿述", "你自己要求的！"),
                    N("我感觉自己的脸有点发热，还是先帮她起床吧。"),
                    ("阿述", "想吃点什么呢，薇薇？"),
                    ("许薇", "还是吃面包吧。"),
                    ("阿述", "好的，我这就去准备。"),
                    ("许薇", "阿述，那我就等你。")));
        }

        static void AddCookingPhase(SerializedProperty phases)
        {
            AddPhase(phases, "做饭", StoryPhaseType.Cooking,
                Ev("Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                    N("一切按计划进行。冰箱里有面包店买的面包，虽然没有刚出炉时的腾腾热气，但味道想必还是一如既往的好。"),
                    ("许薇", "怎么吃也吃不腻呢，这家的面包。"),
                    ("阿述", "这话说得怎么跟老婆婆似的呢？"),
                    ("许薇", "你这个嘴倔的老爷爷。"),
                    N("她做出一个鬼脸，我不禁笑了起来。一切都是那么的和谐。")));
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

            AddEvent(eventsProp, "Interact_Storage", "收拾储物间", InteractionKind.Search,
                N("虽然理论上进入循环后，之前所有设置好的人和物件都会被重置，但既然已经不会出去了，就做点什么权当消遣吧。"),
                N("家里的东西虽然已经都搬过来了，但有不少都堆在了这里，反正有时间，就收拾一下吧。"),
                N("（一段时间后）我正在翻阅着许薇大学里写的日记本。"),
                ("阿述", "好怀念啊……"),
                N("虽然是第一次见，却有种莫名的熟悉感。"),
                ("许薇", "这就是你偷看别人日记的借口吗？"),
                ("阿述", "收拾旧物的时候，很自然地就会看里面的内容吧。"),
                ("许薇", "我看你就是爱收拾。"),
                ("阿述", "不管如何，内容还是要确认的。"),
                ("许薇", "杠精。"),
                N("她做出一个鬼脸。"),
                ("阿述", "好啦好啦。"),
                N("我把日记本合上，起身来摸了摸她的头。"),
                ("许薇", "原谅你了。"),
                N("她嬉笑着，眼睛眯成了两弯。"),
                ("阿述", "好~好~"),
                N("一切都是那么的快乐。"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                ("阿述", "早餐这家面包店的味道一直都很好呢！"),
                ("许薇", "连锁店却可以每家店味道都一样。"),
                ("阿述", "还记得第一次吃就是你带我去的。"),
                ("许薇", "你第一次去还在我抱怨什么买个面包够你在食堂吃一顿了。"),
                ("阿述", "节约是一种美德，我钱都是留给你花的。"),
                ("许薇", "就只有嘴上功夫厉害。"),
                N("我笨拙地装傻，把她看得笑个不停。"),
                ("阿述", "总之现在你最喜欢的是红豆沙馅的，我就全买红豆沙馅的。"),
                ("许薇", "那肯定会吃腻吧。"),
                ("阿述", "不是说怎么吃也吃不腻吗？"),
                ("许薇", "好好，看来不是你嘴硬是我三生有幸。"),
                ("阿述", "投降投降。"),
                N("我举起双手，又把她逗笑了。"),
                N("我们又聊了很多过去的事。一切都是那么的快乐。"));

            AddEvent(eventsProp, "Interact_Flowerpot", "给花浇水", InteractionKind.Reconcile,
                N("我哼着歌浇着水，听到背后电动轮椅的声音。"),
                ("许薇", "斑叶铃兰、重瓣铃兰、君影草……"),
                ("阿述", "前两个我倒是听出来是铃兰了，最后一个倒是个神奇名字。"),
                ("阿述", "不过它确实就是铃兰，话说你怎么知道这是铃兰？"),
                ("许薇", "真是小看我！我最喜欢的花我还不知道长什么样吗？"),
                N("她嘴上说着，脸上却露出笑容。"),
                ("阿述", "对呀，你最喜欢的，我就种满了。"),
                ("许薇", "爱你。"),
                N("她比了个心，还冲我眨了眨眼。"),
                ("阿述", "好土。"),
                N("我这是在报复她早上对我射出的那支冷箭。"),
                N("她笑着掩饰自己的害羞，让我心中一颤。一切都是那么的快乐。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
            AddCharacterPresence(phase, "Interact_Flowerpot", RoomType.BackGarden, -2f, FacingDirection.Right);
        }

        static void AddLunchPhase(SerializedProperty phases)
        {
            AddPhase(phases, "午饭", StoryPhaseType.LunchTime,
                Ev("Interact_Stove", "灶台/做午饭", InteractionKind.CookLunch,
                    ("许薇", "哇——"),
                    ("阿述", "怎么样？还不错吧？"),
                    ("许薇", "上得了厅堂、下得了厨房的好阿述～"),
                    N("她唱得不着调，把我逗得笑了起来。"),
                    N("我们吃得很愉快，恍惚间我感觉仿佛回到了我们热恋的时候。"),
                    N("一切都是那么的熟悉，有种既视感，有种按部就班的感觉。")));
        }

        static void AddAfternoonPhase(SerializedProperty phases, Sprite wifeCasual)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = "下午";
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)StoryPhaseType.Afternoon;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            AddEvent(eventsProp, "Interact_Chair", "天台躺着", InteractionKind.WatchSunset,
                N("我躺在躺椅上，看着天上的白云。太阳藏在云后，但光芒仍然难以掩盖。"),
                N("我听到轮椅转动的声音。"),
                ("许薇", "阿述，晒太阳不叫我？"),
                ("阿述", "你不是一向不喜欢大太阳吗？"),
                ("许薇", "笨蛋，都这个时候了还担心晒黑？"),
                ("阿述", "把一只小粉猪晒成小黑猪。"),
                ("许薇", "嗯？"),
                N("她虽然在笑，但我感觉其中并没有真正的快乐。"),
                ("许薇", "下次再这样，我就罚你唱《车车车车车》。"),
                ("阿述", "车车车车车车车……"),
                N("她忍俊不禁，也跟着唱了起来。"),
                ("许薇", "车车车车车车车……"),
                N("我们就像两个傻子，就好像回到了我们一起看麦兜的时候。一切都是那么的单纯。"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                N("我们又聊了很多过去的事。"),
                ("阿述", "话说你从多久开始写日记的？"),
                ("许薇", "从小时候吧。"),
                ("阿述", "那你现在为什么不写日记了呢？"),
                N("她顿了顿，仿佛在思考着什么。"),
                ("许薇", "写日记是为了留住美好的回忆。有你在的每一天都很美好，所以懒得写了。"),
                ("阿述", "这思维也太抽象了一点吧。"),
                ("许薇", "你以前比我还土味呢。"),
                ("阿述", "我实打实地不解道。"),
                ("许薇", "你以前比现在还落伍。"),
                N("我装出失落的样子。"),
                ("许薇", "摸摸。"),
                N("她过来摸了摸我的头。我感觉有点不自在，她笑而不语。"));

            AddEvent(eventsProp, "Interact_Sofa", "在客厅看书", InteractionKind.ReadDiary,
                N("我一直很喜欢看书，从很久以前便喜欢。每当我阅读的时候，现实时的烦恼就暂时忘怀了。"),
                N("而遇见她后我更喜欢了，就比如此刻一样。我每次抬头，就会看见一双偷瞄的眼睛。"),
                N("我对上她的目光，她就比了个胜利的手势。我们一起笑了起来。一切都是那么的单纯。"),
                ("阿述", "看书怎么这么不认真？"),
                N("她把偷笑藏在书本后面。"),
                ("阿述", "说不过你说不过你。"),
                N("她露出了得逞的表情。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
        }

        static void AddDuskPhase(SerializedProperty phases, Sprite wifeCasual)
        {
            phases.InsertArrayElementAtIndex(phases.arraySize);
            var phase = phases.GetArrayElementAtIndex(phases.arraySize - 1);
            phase.FindPropertyRelative("displayName").stringValue = "傍晚";
            phase.FindPropertyRelative("phaseType").enumValueIndex = (int)StoryPhaseType.Dusk;
            ClearPhaseCharacterState(phase);

            var eventsProp = phase.FindPropertyRelative("events");
            eventsProp.ClearArray();

            AddEvent(eventsProp, "Interact_Partner", "与妻子看日落", InteractionKind.TalkToPartner,
                ("许薇", "怎么，想到天台上来是有什么计划吗？"),
                ("阿述", "我想和你一起看日落。"),
                N("我的心随着她纯净的笑容而绽放。"),
                ("许薇", "哇~好浪漫的想法。"),
                ("阿述", "这是你的想法哦。"),
                ("许薇", "我的？"),
                N("我或许曾经与她有过约定，虽然我似乎是忘记了，但我确实记得我们没有一起看过日落。"),
                ("阿述", "你说我们要一起去山顶看日出，去海边看日落。"),
                ("许薇", "那看来是我年轻的时候说的了。"),
                N("她笑而不语，我则是有些疑惑。"),
                N("我们一同在天台望向那日落的方向，记忆中我们一起看过的电影一部一部划过。"),
                N("《大话西游》《红高粱》《泰坦尼克号》《怦然心动》《爱乐之城》《乱世佳人》《白日梦想家》《闰年》《小王子》……"),
                N("一个个日落的镜头划过。"),
                N("我仿佛看了四十四次日落，又爱了她三千遍，如果硬要加个期限，我希望是五百年。"),
                N("当我沉醉于自己的想象中时，她拉了拉我的衣角。"),
                ("许薇", "蹲下，蹲下。"),
                N("我依言蹲下，她吻上了我的脸颊。熟悉又陌生。"),
                ("许薇", "我爱你，程述。"),
                N("她带着告别的温柔说出这句话，像一杯维斯珀，又像一颗射进心里的子弹。"),
                N("我搂住她的肩膀，也轻声说道。"),
                ("阿述", "我也爱你，许薇。"),
                N("我回吻了她的脸颊。"),
                N("这是一个珍贵而难忘的傍晚，但明天一切都会重来。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.Rooftop, 2f, FacingDirection.Left, wifeCasual);
        }

        static void AddDinnerPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚饭", StoryPhaseType.Dinner,
                Ev("Interact_Stove", "灶台/做晚饭", InteractionKind.CookDinner,
                    N("晚餐时间到了。"),
                    ("许薇", "勤劳勇敢阿述，为团队做关键晚餐~"),
                    ("阿述", "你在唱什么？"),
                    ("许薇", "不知道哪里听来的。"),
                    N("她笑着看向我。"),
                    ("阿述", "味道还好吗？"),
                    ("许薇", "还是熟悉的味道。你很久没做饭了，还这么自信。"),
                    ("阿述", "肌肉记忆，不可能忘的。这是我自创的谚语。"),
                    ("许薇", "你绝对不适合写书。"),
                    ("阿述", "那是。"),
                    ("许薇", "还挺自豪。"),
                    N("一句一句闲话中我们吃完了晚饭。")));
        }

        static void AddEveningPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚上", StoryPhaseType.Evening,
                Ev("Interact_Chair", "一起看夜空", InteractionKind.WatchSunset,
                    N("今晚应该陪陪她。今天意外地顺利，我为什么会感到意外呢？"),
                    N("我和她来到天台，这里的夜空是那么清澈，群星捧月清晰可见。"),
                    N("我把她抱起来，小心地放在躺椅上，然后自己再躺上另一个躺椅。"),
                    ("阿述", "这里的夜空很美。"),
                    ("许薇", "确实很美呢。"),
                    N("夜空闪烁得让人沉醉，云间一点微光摇曳，像身旁的她。"),
                    N("夜空的星光在闪烁，就像我们的今天。一切都是那么顺利。")),
                Ev("Interact_Desk", "一起吃甜点", InteractionKind.EatTogether,
                    N("我们来到餐厅。我从冰箱里取出冷藏了一天的芝士蛋糕，切好一片，端到餐桌对面坐下。"),
                    ("阿述", "这是你最喜欢的芝士蛋糕。"),
                    N("她笑得很开心，我也为她的开心而开心。"),
                    N("我们身处循环，但我希望每一天都能像今天一样快乐。"),
                    N("蛋糕盘子上的水珠在闪烁，就像我们的今天。一切都是那么顺利。")),
                Ev("Interact_Sofa", "一起看相册", InteractionKind.TalkToPartner,
                    N("我们来到客厅。柜子里有一本相册，记录着我们一路走来的点滴。"),
                    N("我把她抱起来，小心地放在沙发上，在她身旁坐下。"),
                    ("阿述", "每次大日子我们都会翻相册。"),
                    N("我笑了笑，某种意义上，循环往复的这一天也是大日子。"),
                    N("看着照片里一个个生日、游乐园、旅行，我感叹生命的短暂。"),
                    N("她在照片里闪耀，就像今天一样。她沉浸在回忆里，我不知道她在想什么。"),
                    N("相册塑封上的反光在闪烁，就像我们的今天。一切都是那么顺利。")));
        }

        static void AddBeforeSleepPhase(
            SerializedProperty phases,
            Sprite protagonistPajamaHalf,
            Sprite wifePajamaHalf,
            Sprite wifePajamaPresence)
        {
            var phaseIndex = phases.arraySize;
            AddPhase(phases, "睡觉前", StoryPhaseType.BeforeSleep,
                Ev("Interact_Bed", "床/睡觉", InteractionKind.Sleep,
                    N("洗漱过后，我和她依偎在床上，有的没的一句一句聊着。"),
                    ("许薇", "你会想要更进一步吗？"),
                    ("阿述", "你为什么会这么问？"),
                    ("许薇", "就问问呗。"),
                    N("我露出不知道怎么办是好的样子，但实际上我们彼此什么都明白。"),
                    ("阿述", "Lady first."),
                    ("许薇", "软蛋！"),
                    ("阿述", "骗你的。"),
                    N("我吻了上去，不过这一次不是在脸颊，而且也不轻。"),
                    ("阿述", "真的可以吗？"),
                    ("许薇", "像往常一样，我才能暂时忘记现在身体上的狼狈。"),
                    N("一切结束后，我们都没有说话。这种事情大概不会每个循环都发生。"),
                    N("我一边沉浸于今天的幸福中，一边意识逐渐模糊。"),
                    ("许薇", "再见，程述。"),
                    N("而此时，"),
                    N("时间，"),
                    N("重置了……")));

            var phase = phases.GetArrayElementAtIndex(phaseIndex);
            phase.FindPropertyRelative("phaseProtagonistPortrait").objectReferenceValue = protagonistPajamaHalf;
            phase.FindPropertyRelative("phaseWifePortrait").objectReferenceValue = wifePajamaHalf;
            AddCharacterPresence(phase, "Interact_Partner", RoomType.Bedroom, 2f, FacingDirection.Left, wifePajamaPresence);
        }

        static void SetEnding(SerializedProperty ending)
        {
            SetEvent(ending, "Interact_EmergencyStop", "紧急停止", InteractionKind.EmergencyStop,
                N("要按按钮吗？"),
                N("真的要按按钮吗？"));
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
