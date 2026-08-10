#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class DefaultStoryContentPopulator
    {
        const string StorageCleanBackgroundPath = StorageRoomBackgroundPaths.CleanBackground;

        [MenuItem("JHTGJ/Populate Default Story Content (Day 1)")]
        public static void PopulateFromMenu()
        {
            StoryPortraitLibraryPopulator.EnsureAsset();
            DayStoryScheduleCreator.EnsureDataFolder();
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(DayStoryScheduleCreator.DefaultAssetPath);
            if (schedule == null)
                schedule = DayStoryScheduleCreator.CreateOrLoadDefault(allowOverwritePrompt: false);
            else
            {
                Populate(schedule);
                EditorUtility.SetDirty(schedule);
                AssetDatabase.SaveAssets();
            }

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log("[JHTGJ] Default story content populated.");
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
            so.FindProperty("includeNightEvent").boolValue = true;

            var phases = so.FindProperty("phases");
            phases.ClearArray();

            AddWakeUpPhase(phases);
            AddCookingPhase(phases);
            AddMorningPhase(phases, wifeCasualFull);
            AddLunchPhase(phases);
            AddAfternoonPhase(phases, wifeCasualFull);
            AddDinnerPhase(phases);
            AddEveningPhase(phases);
            AddNightEventPhase(phases, wifePajamaFull);
            AddBeforeSleepPhase(phases, protagonistPajamaHalf, wifePajamaHalf, wifePajamaFull);

            SetEnding(so.FindProperty("endingEvent"));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddWakeUpPhase(SerializedProperty phases)
        {
            AddPhase(phases, "醒来", StoryPhaseType.WakeUp,
                Ev("Auto_WakeUp", "醒来", InteractionKind.Sleep,
                    N("妻子的声音与笑容都是那样接近，以至于我有点想哭出来。"),
                    ("许薇", "早上好啊，小懒猪！"),
                    ("阿述", "早上好，薇薇。"),
                    N("我回给她一个笑容，立刻着衣起床。"),
                    N("回头看见无法起床的妻子的瞬间，我突然意识到一切已经不一样了。车祸后下肢瘫痪加上器官衰竭的她，现在难以独自行动。"),
                    ("阿述", "来，我来帮你。"),
                    ("许薇", "我还以为你会叫我公主呢。"),
                    N("我愣了一下，这句话上次出现似乎已经是我俩二十出头的时候了。"),
                    ("阿述", "来，爱撒娇的公主殿下，你的王子来接你了。"),
                    ("许薇", "好土啊！"),
                    N("她笑得很开心。"),
                    ("阿述", "你自己要求的！"),
                    N("我感觉自己的脸有点发热，还是先帮她起床吧。"),
                    ("阿述", "想吃点什么呢，薇薇？"),
                    N("我一边帮她穿着衣服一边问道。"),
                    ("许薇", "有没有面包？"),
                    ("阿述", "当然有啦，我就去准备。"),
                    ("许薇", "阿述，我和你一起。"),
                    ("阿述", "没事，你在餐桌旁等着就行。"),
                    N("冰箱里有面包店买的面包，虽然没有刚出炉时的腾腾热气，但是味道想必还是一如既往的好。")));
        }

        static void AddCookingPhase(SerializedProperty phases)
        {
            AddPhase(phases, "做饭", StoryPhaseType.Cooking,
                Ev("Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                    ("许薇", "真好吃！阿述，你还是那么会挑。"),
                    ("阿述", "都这么多年了，我还能不知道你最喜欢的面包是什么吗？"),
                    ("许薇", "嘴倔，被夸了笑一笑就好了。"),
                    N("她做了个鬼脸，我忍不住笑了起来。")));
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
                N("虽然进入循环后一切都会重置，但既然出不去，总要做点什么打发时间。"),
                N("东西虽然都搬进来了，这里却堆得乱七八糟。趁着有空，我决定整理一下。"),
                N("（一段时间后）我翻到了许薇大学时期写的日记。"),
                ("阿述", "好怀念啊……"),
                ("许薇", "你在看什么呢？"),
                N("她不知何时出现在我身后。"),
                ("阿述", "「啊啊啊 o((>ω< ))o，今天程述给我买了铃兰……」"),
                ("许薇", "别看！别看！"),
                N("她拼命挥手想阻止，我却继续念下去。"),
                ("阿述", "「虽然我从未告诉过他，那是我最喜欢的花……」"),
                ("许薇", "不许念了！我生气了！"),
                N("她鼓起嘴，闭上眼睛，双手捂住耳朵，活像一只鸵鸟。"),
                ("阿述", "好好好。"),
                N("我合上日记，站起身来，摇了摇她的肩膀。"),
                ("许薇", "下次不许这样欺负我！"),
                ("阿述", "好好好～"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                ("阿述", "早餐这家面包店的味道一直都很好呢！"),
                ("许薇", "明明是连锁店，结果每家店味道都一样。"),
                ("阿述", "还记得第一次吃就是你带我去的。"),
                ("许薇", "你第一次去还嫌贵，还在我耳边小声抱怨什么买个面包够你在食堂吃一顿了。"),
                ("阿述", "真的吗？我怎么不记得了？阿巴阿巴……"),
                ("许薇", "就知道装傻。"),
                N("我笨拙地装作疑惑，把她看得笑个不停。"),
                ("阿述", "总之现在你最喜欢的是红豆沙馅的，这个我一次就记住了。"),
                ("许薇", "这下倒是记性好了。"),
                ("阿述", "这不是就记得你的事了吗？"),
                ("许薇", "好好，看来不是你嘴硬是我三生有幸。"),
                ("阿述", "投降投降。"),
                N("我举起双手，又把她逗笑了。"),
                N("我们又聊了很久关于大学的事……"));

            AddEvent(eventsProp, "Interact_Flowerpot", "给花浇水", InteractionKind.Reconcile,
                N("我哼着歌给花浇水，身后传来电动轮椅转动的声音。"),
                ("许薇", "这些是什么花？"),
                ("阿述", "斑叶铃兰、重瓣铃兰、君影草……"),
                ("许薇", "前两个我倒是听出来是铃兰了，最后一个是什么？"),
                ("阿述", "又称君影铃兰。"),
                ("许薇", "真没意思！"),
                N("她嘴上这么说，脸上却带着笑。"),
                ("阿述", "你最喜欢的，我就种满了。"),
                ("许薇", "你就不怕我腻吗？"),
                ("阿述", "它们颜色也不一样，形状也不一样，怎么会腻呢？"),
                ("许薇", "不是被花，是被你对我的爱腻到了。"),
                N("她比了个心，还冲我眨了眨眼。"),
                ("阿述", "我一直以为说土味情话是油腻男人的特权呢。"),
                N("话还没说完，她就害羞起来，让我心里一颤。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
            AddCharacterPresence(phase, "Interact_Flowerpot", RoomType.BackGarden, -2f, FacingDirection.Right);
        }

        static void AddLunchPhase(SerializedProperty phases)
        {
            AddPhase(phases, "午饭", StoryPhaseType.LunchTime,
                Ev("Interact_Stove", "灶台/做午饭", InteractionKind.CookLunch,
                    ("许薇", "哇——我们好久都没在餐桌上吃过了。"),
                    ("阿述", "平时那么忙，都只能吃职工食堂的。"),
                    ("许薇", "上得了厅堂、下得了厨房的好阿述～"),
                    N("她唱得不着调，把我逗得呛了口饭。"),
                    N("我们吃得很愉快，恍惚间我感觉仿佛回到了我们热恋的时候。"),
                    N("但每每看见她下方的轮椅时，我便反应过来她是在逞强。")));
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
                N("我躺在躺椅上，看着云，看着太阳。"),
                N("她就像天空一样——"),
                N("身后传来轮椅靠近的声音。"),
                ("许薇", "阿述，在想什么呢？晒太阳不叫我？"),
                ("阿述", "在想你的事呢。"),
                ("许薇", "你怎么不想想你自己的事呢？"),
                ("阿述", "什么事？"),
                ("许薇", "你自己的事我怎么知道呢？"),
                N("她在笑，但我却感到一丝违和。"),
                ("阿述", "我只是先看着眼前的事情罢了。"),
                N("她沉默了一会儿，像把想说的话咽了回去。我没有再追问。"),
                ("阿述", "要躺上来吗？"),
                ("许薇", "不了，屁股懒得动了。"),
                N("我们看着天空，它如同我们的未来一样难以预测。"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                ("阿述", "话说你从多久开始写日记的？"),
                ("许薇", "还在惦记上午的事？"),
                ("阿述", "没有，就是突然想到了。"),
                N("我平淡地回答道。"),
                ("许薇", "写日记啊，我也记不清了，反正年龄不大吧。"),
                ("阿述", "那你现在为什么不写日记了呢？"),
                N("她顿了顿，仿佛在思考着什么。"),
                ("许薇", "写日记是因为不想与过去断舍离吧。"),
                ("阿述", "这思维也太抽象了一点吧。"),
                ("许薇", "你自己不也差不多。"),
                ("阿述", "怎么个差不多法？我又不写日记。"),
                ("许薇", "不是日记，是不想断舍离。"),
                N("她的论断我从未想过，让我陷入沉思。"),
                ("许薇", "人总还是要面对当下的。"),
                ("阿述", "我现在就算是在面对当下吧。"),
                N("她笑而不语，"),
                N("我感到迷惑，"),
                N("无论是对于她的话本身还是她为什么说这话，"),
                N("但她很快就聊到下的话题去了。"));

            AddEvent(eventsProp, "Interact_Sofa", "在客厅看书", InteractionKind.ReadDiary,
                N("我一直很喜欢看书，从很久以前便喜欢。"),
                N("每当我阅读的时候，现实时的烦恼就暂时忘怀了。"),
                N("而遇见她后我更喜欢了，就比如此刻一样。"),
                N("我每次抬头，就会看见一双偷瞄的眼睛；一对上又便会回到书上，余光里还能看见偷偷翘起的嘴角。"),
                N("但是又有什么不一样了——她似乎只是一直带着笑容看着我。"),
                N("而余光里她的双腿又时刻在提醒着我，这始终让我无法忘怀，即使是暂时也做不到。"),
                ("阿述", "这边没有你喜欢的书吗？"),
                ("许薇", "不是，我只是突然发觉你看书的样子很有意思。"),
                ("阿述", "什么意思？"),
                N("她露出了思考的样子。"),
                ("许薇", "说不上来。"),
                N("她目光回到了书上。"),
                ("许薇", "看书看书，不打扰你了，你就当是夸你长得好看吧。"),
                N("我笑了一下，但也为她的反常而略增忧虑。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
        }

        static void AddDinnerPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚饭", StoryPhaseType.Dinner,
                Ev("Interact_Stove", "灶台/做晚饭", InteractionKind.CookDinner,
                    N("许薇在吃饭，但表情并不好看。"),
                    ("阿述", "味道还好吗？"),
                    ("许薇", "……你说过要诚实的。"),
                    N("她顿了顿，像是在组织语言。"),
                    ("许薇", "看见你做饭，我就想起以前我也帮你备菜、陪你聊天。"),
                    ("许薇", "现在什么都由你来照顾，我却什么都帮不上……"),
                    N("她语气很硬，像是在掩饰声音里的哽咽。"),
                    ("许薇", "阿述，我不知道我为什么还活着……"),
                    N("安慰的话在此刻毫无用处。我冲上前，抱住了她。"),
                    N("这一天仿佛回到了蜜月期，却又处处透着违和。"),
                    N("循环、终点……如果一直这样下去，我还能照顾她多久？"),
                    N("我想起患痴呆症的祖母——买菜、做饭、清洁、散步……"),
                    N("某个夜晚她在柜子里翻找，争执中摔断了髋骨，做了手术。"),
                    ("阿述", "我爱你，许薇。"),
                    N("在她的啜泣声中，我不知道这句话是说给她听的，还是说给我自己的。")));
        }

        static void AddEveningPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚上", StoryPhaseType.Evening,
                Ev("Interact_Chair", "一起看夜空", InteractionKind.WatchSunset,
                    N("我今晚应该陪陪她，但最好要找一个契机。"),
                    N("我们来到天台。夜空晴朗，月亮周围的星星清晰可见。"),
                    N("我把她抱起来，小心地放在一张躺椅上，自己在另一张躺椅上躺下。"),
                    ("阿述", "这里的夜空很美。"),
                    ("许薇", "阿述，对不起……"),
                    N("我心中一颤。"),
                    ("许薇", "我今天不应该在那种时候说那些话……"),
                    ("阿述", "怎么会是那种时候呢？"),
                    N("我看向她。"),
                    ("阿述", "许薇，你什么时候都应该说出来，内耗解决不了问题。"),
                    ("许薇", "我只是不想让你难受，明明这些问题根本就没有办法解决。"),
                    ("阿述", "想象你明白我也不想让你难受，我愿意倾听你的烦恼。"),
                    ("许薇", "可是……可是你都为我做了那么多了。"),
                    ("阿述", "你怎么突然自卑起来了，我愿意为你付出。"),
                    ("阿述", "我相信即使我们角色互换，你也会和我做同样的事。"),
                    ("阿述", "可以说我是一个单纯的人，此刻只要这星空能让你放松就好了。"),
                    ("阿述", "所以……笑一个吧，我想看你的笑容。"),
                    N("她一边流着泪水，一边翘起嘴角。"),
                    ("许薇", "你真的……你真的……很不会安慰人呢。"),
                    N("夜空的星光在闪烁，如同我的内心。我害怕，我害怕失去她。")),
                Ev("Interact_Desk", "一起吃甜点", InteractionKind.EatTogether,
                    N("我们来到餐厅。我从冰箱里取出冷藏了一天的芝士蛋糕，切好一片，端到餐桌对面坐下。"),
                    ("许薇", "阿述，对不起……"),
                    N("我心中一颤。"),
                    ("许薇", "我今天不应该在那种时候说那些话……"),
                    ("阿述", "怎么会是那种时候呢？许薇，你什么时候都应该说出来，内耗解决不了问题。"),
                    ("许薇", "我只是不想让你难受，明明这些问题根本就没有办法解决。"),
                    ("阿述", "想象你明白我也不想让你难受，我愿意倾听你的烦恼。"),
                    ("许薇", "可是……可是你都为我做了那么多了。"),
                    ("阿述", "你怎么突然自卑起来了，我愿意为你付出。"),
                    ("阿述", "我相信即使我们角色互换，你也会和我做同样的事。"),
                    ("阿述", "所以……笑一个吧，我想看你的笑容。"),
                    N("她一边流着泪水，一边翘起嘴角。"),
                    ("许薇", "你真的……你真的……很不会安慰人呢。"),
                    N("蛋糕碟上的水珠在闪烁，如同我的内心。我害怕，我害怕失去她。")),
                Ev("Interact_Sofa", "一起看相册", InteractionKind.TalkToPartner,
                    N("我们来到客厅。柜子里有一本相册，记录着我们一路走来的点滴。"),
                    N("我把她抱起来，小心地放在沙发上，在她身旁坐下。"),
                    ("阿述", "每个重要的日子我们都会翻翻相册。"),
                    ("许薇", "阿述，对不起……"),
                    N("我心中一颤。"),
                    ("许薇", "我今天不应该在那种时候说那些话……"),
                    ("阿述", "怎么会是那种时候呢？许薇，你什么时候都应该说出来，内耗解决不了问题。"),
                    ("许薇", "我只是不想让你难受，明明这些问题根本就没有办法解决。"),
                    ("阿述", "想象你明白我也不想让你难受，我愿意倾听你的烦恼。"),
                    ("许薇", "可是……可是你都为我做了那么多了。"),
                    ("阿述", "你怎么突然自卑起来了，我愿意为你付出。"),
                    ("阿述", "可以说我是一个单纯的人，此刻只要这相册能让你放松就好了。"),
                    ("阿述", "所以……笑一个吧，我想看你的笑容。"),
                    N("她一边流着泪水，一边翘起嘴角。"),
                    ("许薇", "你真的……你真的……很不会安慰人呢。"),
                    N("相册封面上的反光在闪烁，如同我的内心。我害怕，我害怕失去她。")));
        }

        static void AddNightEventPhase(SerializedProperty phases, Sprite wifePajamaPresence)
        {
            var phaseIndex = phases.arraySize;
            AddPhase(phases, "夜晚事件", StoryPhaseType.NightEvent,
                Ev("Interact_Partner", "夜晚事件", InteractionKind.TalkToPartner,
                    N("夜深了，该洗漱休息了。"),
                    ("许薇", "可以帮我洗澡吗？"),
                    ("许薇", "我在医院里好多天没洗澡了。"),
                    ("阿述", "没问题。"),
                    N("我在浴室铺好防滑垫，摆好洗澡椅。"),
                    N("她坐在轮椅上自己脱去上衣，我帮她脱下其余衣物。"),
                    N("我看见她身上的一些疤痕，甚至感受到了一些幻痛。"),
                    N("她似乎是有所察觉，若无其事地抱怨道。"),
                    ("许薇", "手术刚刚结束的时候最难受的就是两周不能洗澡。"),
                    ("许薇", "要是你不在的话，准不定臭死你。"),
                    ("阿述", "你身上的味道，臭的也是香的。"),
                    ("许薇", "好恶心！ruaruarua……"),
                    N("我把她抱到浴室里，谨慎地让她坐在椅子上。"),
                    N("旁边墙上还有专门定做的扶手，好让她扶稳。"),
                    N("水温不能太高，水压也要合适。我先用自己的手把这些都调好，再把喷头递给她。"),
                    N("她先是简单地冲了下水，随后看着我露出微笑。"),
                    ("许薇", "帮我洗。"),
                    ("阿述", "Yes, sir!"),
                    N("我们第一次聊电影的时候就是聊的《无间道》。"),
                    N("她喜欢刘德华，我喜欢梁朝伟。后来她又改口说她喜欢梁朝伟——她说发现自己还是喜欢单纯点的好人。"),
                    N("直到后面我向她告白……"),
                    ("阿述", "闭上眼睛。"),
                    ("许薇", "遵命！"),
                    ("许薇", "你连家里的护发素都带过来了。"),
                    ("阿述", "太不方便了，我给你买了同款新的。"),
                    ("许薇", "什么！这个很贵的，给我买新的太浪费了。"),
                    ("许薇", "我还以为你是勤俭持家的好男人呢。"),
                    N("她假装生气，偷偷瞄我反应。"),
                    ("许薇", "好啦好啦，逗你的，我不生气。"),
                    N("她做了个鬼脸，我假装很受伤。"),
                    ("许薇", "别生气别生气，对不起啦，阿述。"),
                    ("阿述", "好恶心！ruaruarua……"),
                    N("清洁工作以我的反击告终。女生洗澡，真是麻烦啊。")));

            var phase = phases.GetArrayElementAtIndex(phaseIndex);
            AddCharacterPresence(phase, "Interact_Partner", RoomType.Bedroom, 2f, FacingDirection.Left, wifePajamaPresence);
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
                    N("关灯。"),
                    ("阿述", "关灯了。"),
                    ("许薇", "好。"),
                    N("我的手指按下开关，房间陷入一片漆黑——是城市里从未体验过的那种黑。"),
                    ("许薇", "阿述，我有个问题你要老实回答。"),
                    ("阿述", "嗯？"),
                    ("许薇", "你之后打算怎么办？"),
                    ("阿述", "什么之后？"),
                    N("我在回避这个问题。"),
                    ("许薇", "就是……我走了之后。"),
                    ("阿述", "许薇，别聊这个好吗？"),
                    N("我感到一阵窒息。"),
                    ("许薇", "可是……"),
                    ("阿述", "不要说了！"),
                    N("我有些烦躁，也有些焦虑。"),
                    ("许薇", "抱歉，阿述。"),
                    N("该道歉的不应该是她。"),
                    ("阿述", "我才该道歉，我有些急躁了。"),
                    ("许薇", "好的好的，睡吧睡吧。"),
                    ("阿述", "抱歉……我可能有点累了……"),
                    N("我不明白，她为什么不怪我消失了一个月。"),
                    N("我不明白，她为什么不生气——明明是我犯了错。"),
                    N("为什么……"),
                    N("为什么出车祸的不是我？"),
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
                N("虽然我保留了这台机器的急停按钮，但大概它永远也不会被用上吧。"));
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
