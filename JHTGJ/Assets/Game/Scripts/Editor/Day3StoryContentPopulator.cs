#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day3StoryContentPopulator
    {
        public const string Day3AssetPath = "Assets/Game/Data/Day3StorySchedule.asset";

        const string StorageCleanBackgroundPath = StorageRoomBackgroundPaths.CleanBackground;

        [MenuItem("JHTGJ/Populate Day 3 Story Content")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day3AssetPath);
            if (schedule == null)
            {
                schedule = ScriptableObject.CreateInstance<DayStorySchedule>();
                AssetDatabase.CreateAsset(schedule, Day3AssetPath);
            }

            Populate(schedule);
            EditorUtility.SetDirty(schedule);
            AssetDatabase.SaveAssets();

            DayStoryScheduleCreator.EnsureCampaignIncludesAllDays();

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log("[JHTGJ] Day 3 story content populated.");
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
            AddEveningPhase(phases);
            AddBeforeSleepPhase(phases, protagonistPajamaHalf, wifePajamaHalf, wifePajamaFull);

            SetEnding(so.FindProperty("endingEvent"));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddWakeUpPhase(SerializedProperty phases)
        {
            AddPhase(phases, "醒来", StoryPhaseType.WakeUp,
                Ev("Auto_WakeUp", "醒来", InteractionKind.Sleep,
                    N("清晨的窗外传来了鸟鸣，我从睡梦中醒来。"),
                    N("坐起身来，看着她迷蒙的睡脸，我不禁微笑。"),
                    N("她就这样在我的身边，我们永远都不会分开……"),
                    N("我突然感到刚刚的想法十分病态。没有任何告知便自顾自地把她困在了循环里，她真的愿意吗？"),
                    N("我感到一阵不安。没事，没事，这都是为了救她……"),
                    N("我不敢继续细想下去，像是不敢窥探自己内心深处的黑暗。"),
                    N("也许也是为了逃避这种不安，我决定让她继续睡着，自己起来准备早餐。")));
        }

        static void AddCookingPhase(SerializedProperty phases)
        {
            AddPhase(phases, "做饭", StoryPhaseType.Cooking,
                Ev("Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                    N("我为她准备她最喜欢的面包当早餐。"),
                    N("突然，楼下传来声响，我连忙跑过去。"),
                    N("她在床上，神情有些不安。看到我，她哭了出来。"),
                    ("许薇", "你怎么突然不见了？"),
                    N("她的声音颤抖着，像是差点失去什么一般。"),
                    N("我抱住她，轻轻拍着她的背。"),
                    ("阿述", "我在，我在。我只是去准备早餐。"),
                    N("我用与平时不符的温柔语气说道。"),
                    ("许薇", "不要再这样突然不见了……"),
                    N("她语气微弱，似乎不想显得像在责备。"),
                    ("阿述", "我不会了，我不会了……"),
                    N("我心中一颤。我想起一个月前，她刚做完手术连坐起来都做不到时，我也曾离开过她。那时她也是这样哭。"),
                    N("她还在抽泣。"),
                    N("我们分开拥抱。"),
                    ("阿述", "先吃早饭吧。"),
                    N("她点了点头。"),
                    N("她收拾好后，我们一起回到餐厅。"),
                    N("吐司已经凉了，没有了香气。我们沉默地吃着。")));
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
                N("「好怀念啊……」"),
                N("「不准看！我的大学日记！」"),
                N("「啊啊啊 o((>ω< ))o，今天程述给我买了一朵铃兰……」"),
                N("「明明我没有给他说过这是我最喜欢的花的……」"),
                N("「还在念，还在念！」"),
                N("「该不会是我最好的朋友给他说的吧？好朋友噢好朋友……」"),
                N("「我要生气了！」"),
                N("我看着刚收拾好的架子上摆着的高达模型。"),
                N("她当年问遍了我的朋友，打听我最想要哪一款。那时我买不起，她却买来，想亲手拼好当生日礼物送我。第一次拼，拼到一半就给了我，还跟我道歉。"),
                N("那天我吻了她，她说我「好突然」。也是那天，我决定要和她永远在一起。"),
                ("阿述", "好啦好啦。"),
                N("我把笔记本合上，光是回忆起就忍不住笑出来。"),
                N("她怎么没有出现？我不知道为什么脑中闪过一阵违和感。"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                ("阿述", "早餐这家面包店的味道一直都很好呢！"),
                N("她没有说话。"),
                ("阿述", "你说连锁店怎么做到每家店味道都一样的呢？"),
                N("我感觉到了点违和感。"),
                ("阿述", "大概是因为统一采购统一配方吧。"),
                N("她还是没有开口。"),
                ("阿述", "话说你还记得第一次吃就是你带我去的……"),
                ("许薇", "阿述，抱歉，让我静一静吧……"),
                N("我感受到一阵心绞般的难受。"),
                N("她向来很照顾我的情绪，可今天有什么不一样。"),
                N("或许我从一开始就应该陪着她，无论是今天早上还是一个月前。"),
                N("我怎么什么都没有做好……"));

            AddEvent(eventsProp, "Interact_Flowerpot", "给花浇水", InteractionKind.Reconcile,
                N("我哼着歌浇着水。"),
                ("阿述", "斑叶铃兰、重瓣铃兰、君影草……"),
                N("我如同程序一般念着这些名字。"),
                ("阿述", "前两个我倒是听出来是铃兰了，最后一个倒是个神奇名字。"),
                ("阿述", "不过它确实就是铃兰。"),
                N("这些是薇薇最喜欢的，所以我种满了。"),
                N("我自言自语，因为没有人会回应。"),
                N("没有对话的对象，让我有些害羞于这毫无意义的自说自话。"),
                N("我感受到一种强烈的违和感。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
            AddCharacterPresence(phase, "Interact_Flowerpot", RoomType.BackGarden, -2f, FacingDirection.Right);
        }

        static void AddLunchPhase(SerializedProperty phases)
        {
            AddPhase(phases, "午饭", StoryPhaseType.LunchTime,
                Ev("Interact_Stove", "灶台/做午饭", InteractionKind.CookLunch,
                    N("该吃午饭了，还是没能和她说上话。"),
                    N("在饭桌上，她吃得有些木讷。"),
                    ("阿述", "许薇，我做错了什么？"),
                    N("她愣了一下。"),
                    ("阿述", "我们约定过彼此之间一定要坦诚。"),
                    N("我深吸了一口气。"),
                    ("阿述", "我真的很担心你。"),
                    N("我或许是成人后第一次如此直率地言说自己的情绪。"),
                    N("她回过神来，我们凝视着彼此的眼睛，突然她的眼中又一次湿润。"),
                    ("许薇", "对不起，阿述……"),
                    N("她的声音让人心疼。"),
                    ("许薇", "我只是发现了自己是个自私的人。"),
                    N("我完全无法理解她的言语，她也许也看出了我的疑惑。"),
                    ("许薇", "我发现了自己想要不顾一切地占有你。"),
                    N("她说出了我一直在逃避的想法，只不过要主宾互换一下。"),
                    N("而我在听了这句话之后更加难过，她会因为我抛下她而自我内耗。"),
                    N("我却只是因为害怕而一直在逃避。"),
                    ("阿述", "先吃饭吧。"),
                    N("这是我最后的缓冲时间。"),
                    ("阿述", "我也有些想要道歉的，下午好好聊聊吧。"),
                    N("她把话咽了回去，点了点头。")));
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

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                N("和她好好聊聊吧。"),
                N("我们看着彼此，却不知道怎么开口。"),
                ("阿述", "我对不起你，我不应该离开你。"),
                N("长久的沉默我率先打破，似乎是要直面我内心最深的伤口。她的肩膀微微颤抖了一下。"),
                ("许薇", "你不要这样说。"),
                ("阿述", "我想听听你的想法。"),
                N("我已经决定直面她对我的看法，即使再负面我也一定会接受。"),
                ("许薇", "我……"),
                ("许薇", "我不想和你分开。"),
                N("她的言语是那样朴素，即使是这种情况她还是这样温柔。"),
                ("许薇", "从我知晓自己的身体情况开始……"),
                ("许薇", "每天醒来，我都在害怕见不到你。"),
                ("阿述", "我……"),
                ("许薇", "所以当我今天早晨醒来，身边的你消失不见时……"),
                N("她的声音变得不稳。"),
                ("许薇", "我真的，我真的好害怕。"),
                N("万千思绪涌入我的内心，我当时的缺失的陪伴究竟伤害了她多少？"),
                ("许薇", "但我同时意识到我究竟有多么想占有你。"),
                N("我不知她怎么会如此卑微，如此短暂的陪伴都会让她感到失位。"),
                ("许薇", "即使我知道这会让你愈发远离正常的生活。"),
                ("阿述", "不要这样看轻你自己。"),
                N("我忍不住抓住她的肩膀，强行让她停下这种自残般的发言。"),
                ("阿述", "我求你了，不要再贬低你自己了。"),
                N("我发自内心地恳求道。"),
                ("阿述", "你是不是不会被打倒的……"),
                ("阿述", "你是那样的坚强……"),
                ("阿述", "你的自尊、自爱都是那样的闪耀……"),
                N("我的言语毫无逻辑地倾泻而出，所有的话都是直觉般地冒出来。"),
                ("阿述", "所以多依靠一下我一点也可以啊。"),
                N("我想通了，或许是我需要她此时的依靠，才能让自己对于当初抛下她一事赎罪。"),
                N("所以我渴望为她付出，就是一个如此自私的理由。"),
                ("阿述", "你已经保持那么久完美的自己了……"),
                ("阿述", "现在就休息、休息一下吧。"),
                N("我抱紧她，生怕稍微松手就会失去她。"),
                ("许薇", "谢谢你，程述。"),
                N("良久沉默的相拥之后，她淡淡地说道。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.LivingRoom, 2f, FacingDirection.Left, wifeCasual);
        }

        static void AddDinnerPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚饭", StoryPhaseType.Dinner,
                Ev("Interact_Stove", "灶台/做晚饭", InteractionKind.CookDinner,
                    N("晚餐时间到了。"),
                    N("她一边吃着晚饭，一边微笑着偷偷看着我。"),
                    ("阿述", "怎么了，薇薇？怎么还偷偷看我？"),
                    ("许薇", "阿述，谢谢你。"),
                    N("我心中刺了一下。"),
                    ("阿述", "不用再反复提及了，我们都要向前看不是吗？"),
                    N("虽然我在她最痛苦的时候消失了那么久，说这种话仿佛是在为自己开脱。"),
                    ("许薇", "这味道还是一样地香呢。"),
                    N("虽然我不记得给她做过这道菜，但现在她开心就好。"),
                    ("阿述", "那就好好享用吧，薇薇。"),
                    ("阿述", "慢慢吃，别噎着。"),
                    N("我突然感受到一种虚无，今天我们知晓了彼此的心意，但明天一切就会重新开始。"),
                    N("或许一切都没有意义……")));
        }

        static void AddEveningPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚上", StoryPhaseType.Evening,
                Ev("Interact_Chair", "一起看夜空", InteractionKind.WatchSunset,
                    N("我今晚打算陪陪她，免得被那种虚无感吞没。"),
                    N("我和她来到天台，这里的夜空是那么清澈，群星捧月清晰可见。"),
                    N("我把她抱起来，小心地放在躺椅上，然后自己再躺上另一个躺椅。"),
                    ("阿述", "这里的夜空很美。"),
                    ("许薇", "确实很美呢。"),
                    N("夜空的星光在闪烁，如同她的生命。"),
                    N("我害怕，我害怕今天的一切被重置。")),
                Ev("Interact_Desk", "一起吃甜点", InteractionKind.EatTogether,
                    N("我们来到餐厅。我从冰箱里取出冷藏了一天的芝士蛋糕，切好一片，端到餐桌对面坐下。"),
                    ("阿述", "这是你最喜欢的芝士蛋糕。"),
                    N("我心中一颤，而她满是笑容。"),
                    N("她对于生命价值的极端选择，让我意识到她的生命是那样脆弱，困在时间虚空中。"),
                    N("蛋糕盘子上的水珠在闪烁，如同她的生命。"),
                    N("我害怕，我害怕今天的一切被重置。")),
                Ev("Interact_Sofa", "一起看相册", InteractionKind.TalkToPartner,
                    N("我们来到客厅。柜子里有一本相册，记录着我们一路走来的点滴。"),
                    N("我把她抱起来，小心地放在沙发上，在她身旁坐下。"),
                    ("阿述", "每次大日子我们都会翻相册。"),
                    N("我笑了笑，某种意义上，循环往复的这一天也是大日子。"),
                    N("看着照片里一个个生日、游乐园、旅行，我感叹生命的短暂。"),
                    N("她沉浸在回忆里，我不知道她在想什么。"),
                    N("相册塑封上的反光在闪烁，如同她的生命。"),
                    N("我害怕，我害怕今天的一切被重置。")));
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
                    ("阿述", "关灯咯。"),
                    ("许薇", "好的。"),
                    N("我的手指按下，眼前的一切突然陷入一片漆黑。这是在城市里从未体验过的那种黑。"),
                    ("许薇", "阿述，你说未来会怎么样？"),
                    N("她突然说了这么一句，有些突兀。"),
                    ("阿述", "有很多事可以做啊。"),
                    N("我的脑海中闪过一个个画面。"),
                    ("阿述", "我们可以去山顶看日出、去海边看日落。"),
                    ("许薇", "我怎么到山顶上去啊？"),
                    N("她笑我的不着调。"),
                    ("阿述", "有缆车。还有……还有我。"),
                    ("许薇", "你把我抱上去？"),
                    ("阿述", "我把你抱上去。"),
                    N("她笑得很开心。"),
                    ("许薇", "那你不得好好锻炼一下自己了？"),
                    ("阿述", "一切结束了我就去健身。"),
                    ("许薇", "不要忘了练腿！"),
                    ("阿述", "肯定的，我又不傻。"),
                    ("许薇", "你不傻的话，怎么会说爬山这种不切实际的话。"),
                    ("阿述", "呜呜呜~"),
                    N("我拙劣地假哭着。"),
                    ("许薇", "小傻瓜，你怎么会是傻瓜呢。"),
                    N("她还没说完就忍不住笑了起来。"),
                    N("我们有一句没一句地聊着不存在的未来，直到她先一步困得进入梦乡。"),
                    N("虽然是不切实际的想象，但我却不禁在想，结束循环她就会很快迎来生命的终点，但沉浸于这样的循环根本就没有未来的。"),
                    N("我的思绪逐渐模糊，"),
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
                N("要结束这一切吗？"),
                N("我应该结束这一切吗？"));
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
