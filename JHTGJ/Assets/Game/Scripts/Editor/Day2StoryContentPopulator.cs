#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day2StoryContentPopulator
    {
        public const string Day2AssetPath = "Assets/Game/Data/Day2StorySchedule.asset";

        const string StorageCleanBackgroundPath = StorageRoomBackgroundPaths.CleanBackground;

        [MenuItem("JHTGJ/Populate All Story Days (Day 1 + 2 + 3 + 4 + 5)")]
        public static void PopulateAllDaysFromMenu()
        {
            StoryPortraitLibraryPopulator.EnsureAsset();
            NightRoomBackgroundLibraryPopulator.EnsureAsset();
            PostCookingDiningLibraryPopulator.EnsureAsset();
            Day1NightEventLibraryPopulator.EnsureAsset();
            Day2NightEventLibraryPopulator.EnsureAsset();
            Day4DuskEventLibraryPopulator.EnsureAsset();
            DefaultStoryContentPopulator.PopulateFromMenu();
            Day2StoryContentPopulator.PopulateFromMenu();
            Day3StoryContentPopulator.PopulateFromMenu();
            Day4StoryContentPopulator.PopulateFromMenu();
            Day5StoryContentPopulator.PopulateFromMenu();
        }

        [MenuItem("JHTGJ/Populate All Story Days (Day 1 + 2 + 3 + 4)")]
        public static void PopulateDay1Through4FromMenu()
        {
            DefaultStoryContentPopulator.PopulateFromMenu();
            Day2StoryContentPopulator.PopulateFromMenu();
            Day3StoryContentPopulator.PopulateFromMenu();
            Day4StoryContentPopulator.PopulateFromMenu();
        }

        [MenuItem("JHTGJ/Populate All Story Days (Day 1 + 2 + 3)")]
        public static void PopulateDay1Through3FromMenu()
        {
            DefaultStoryContentPopulator.PopulateFromMenu();
            Day2StoryContentPopulator.PopulateFromMenu();
            Day3StoryContentPopulator.PopulateFromMenu();
        }

        [MenuItem("JHTGJ/Populate All Story Days (Day 1 + 2)")]
        public static void PopulateDay1And2FromMenu()
        {
            DefaultStoryContentPopulator.PopulateFromMenu();
            Day2StoryContentPopulator.PopulateFromMenu();
        }

        [MenuItem("JHTGJ/Populate Day 2 Story Content")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day2AssetPath);
            if (schedule == null)
            {
                schedule = ScriptableObject.CreateInstance<DayStorySchedule>();
                AssetDatabase.CreateAsset(schedule, Day2AssetPath);
            }

            Populate(schedule);
            EditorUtility.SetDirty(schedule);
            AssetDatabase.SaveAssets();

            DayStoryScheduleCreator.EnsureCampaignIncludesAllDays();

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log("[JHTGJ] Day 2 story content populated.");
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
                    N("我一边帮她穿着衣服一边问道。"),
                    ("许薇", "我还是也要一样的！"),
                    N("我意识到她说的还是我们最常吃的面包。"),
                    ("阿述", "当然有啦，我就去准备。"),
                    ("许薇", "阿述，我和你一起。"),
                    ("阿述", "没事，你在餐桌旁等着就行。"),
                    N("冰箱里有面包店买的面包，虽然没有刚出炉时的腾腾热气，但是味道想必还是一如既往的好。")));
        }

        static void AddCookingPhase(SerializedProperty phases)
        {
            AddPhase(phases, "做饭", StoryPhaseType.Cooking,
                Ev("Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                    ("许薇", "还是那么好吃！阿述。"),
                    ("阿述", "都这么多年了，我还能不知道你最喜欢的面包是什么吗？"),
                    ("许薇", "杠精，被夸了笑一笑就好了。"),
                    N("她做出一个鬼脸，我不禁笑了出来。")));
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
                ("阿述", "真是怀念呢……"),
                ("许薇", "不准看！我的大学日记！"),
                N("她不知不觉中出现在了我的身后。"),
                ("阿述", "啊啊啊 o((>ω< ))o，今天程述给我买了一朵铃兰……"),
                ("许薇", "你怎么还在念出来！"),
                N("她无奈地摇着头。"),
                ("阿述", "明明我没有给他说过这是我最喜欢的花的……"),
                ("许薇", "还在念，还在念！"),
                ("阿述", "该不会是我最好的朋友给他说的吧？好朋友噢好朋友……"),
                ("许薇", "我要生气了！"),
                N("她嘟着嘴，闭上眼，双手把耳朵捂着，不知怎的让我想起了鸵鸟。"),
                ("阿述", "好好好。"),
                N("我合上日记，站起身来，摇了摇她的肩膀。"),
                ("许薇", "不许再这样欺负我了！"),
                ("阿述", "好好好～"),
                N("我发现她看我的眼神中有着一种慈祥。"));

            AddEvent(eventsProp, "Interact_Partner", "与伴侣交谈", InteractionKind.TalkToPartner,
                ("阿述", "早餐这家面包店的味道一直都很好呢！"),
                ("许薇", "连锁店怎么做到每家店味道都一样的呢？"),
                N("我感觉到了点违和感。"),
                ("阿述", "大概是因为统一采购统一配方吧。"),
                ("阿述", "话说你还记得第一次吃就是你带我去的。"),
                ("许薇", "记得记得，你第一次去还嫌贵，还在抱怨什么买个面包够你在食堂吃一顿了。"),
                ("阿述", "我很小声的，就你听得见。"),
                ("许薇", "才怪！当时旁边有个叔叔听到了，看你的眼神都慈爱起来了。"),
                ("阿述", "真的吗？我怎么不记得了？阿巴阿巴……"),
                ("许薇", "装傻装太多会变成笨蛋哦。"),
                N("我笨拙地装作疑惑，让她笑了笑。"),
                ("阿述", "总之现在你最喜欢的是红豆沙馅的，这个我一次就记住了。"),
                ("许薇", "这下倒是记性好了。还不是因为它是当时哪里最便宜的？"),
                ("阿述", "原来是这样吗？你都没告诉我。"),
                ("许薇", "不过后面吃多了就喜欢上了，要不是这样我准还要瘦几斤。"),
                ("阿述", "够瘦了够瘦了，现在这样我正喜欢。至少我没有记错你的事了吧？"),
                ("许薇", "好好，被你记住我最喜欢的面包是我三生有幸。"),
                ("阿述", "投降投降。"),
                N("我举起双手，又把她逗笑了。"),
                N("我们又聊了很久关于大学的事，心中却始终有些违和感……"));

            AddEvent(eventsProp, "Interact_Flowerpot", "给花浇水", InteractionKind.Reconcile,
                N("我哼着歌浇着水，听到背后电动轮椅的声音。"),
                ("许薇", "这些铃兰都是什么种类的啊？"),
                ("阿述", "斑叶铃兰、重瓣铃兰、君影草……"),
                ("许薇", "前两个我倒是听出来是铃兰了，最后一个倒是个神奇名字。"),
                ("阿述", "不过它确实就是铃兰，话说你怎么知道这是铃兰？"),
                ("许薇", "真是小看我！我最喜欢的花我还不知道长什么样吗？"),
                N("她嘴上说着，脸上却露出笑容。"),
                ("阿述", "对呀，你最喜欢的，我就种满了。"),
                ("许薇", "它们颜色也不一样，形状也不一样，怎么看也看不腻呢。"),
                ("阿述", "是啊，怎么会腻呢？"),
                N("总感觉我的话好像都被她抢着说了。"),
                ("许薇", "可是你这样种花不怕你对我的爱把我腻到吗？"),
                N("她单手比了个心，露出意味深长的笑容。"),
                ("阿述", "我的第一次土味情话居然是被你塞到嘴里的。"),
                ("许薇", "哇，好油腻。"),
                N("我在她还没说完之前就害羞了，总有种被她牵着鼻子走的感觉。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
            AddCharacterPresence(phase, "Interact_Flowerpot", RoomType.BackGarden, -2f, FacingDirection.Right);
        }

        static void AddLunchPhase(SerializedProperty phases)
        {
            AddPhase(phases, "午饭", StoryPhaseType.LunchTime,
                Ev("Interact_Stove", "灶台/做午饭", InteractionKind.CookLunch,
                    N("该吃饭了，她专门嘱咐我她不想吃土豆炖肉，可是她怎么知道我打算做土豆炖肉呢？"),
                    N("或许她是看见我提前洗好的土豆了吧。"),
                    ("许薇", "哇——好新奇的菜。"),
                    ("阿述", "平时那么忙，哪有时间研究什么新菜品。"),
                    ("许薇", "上得了厅堂、下得了厨房的好阿述～"),
                    N("她唱得调平平的，把我逗得喷了一下。"),
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
                N("我躺在躺椅上，看着天上的白云。太阳藏在云后，但光芒仍然难以掩盖。我觉得她就像此刻的天空。"),
                N("我听到轮椅转动的声音。"),
                ("许薇", "阿述，在想我呢？"),
                ("阿述", "嗯啊。"),
                ("许薇", "我好开心。"),
                ("阿述", "怎么突然这么说？"),
                ("许薇", "你也可以想想我们的事。"),
                N("她还在笑着，但我却感到一点违和。"),
                ("阿述", "这有什么区别吗？"),
                N("她沉默了一会儿，像把想说的话咽了回去。我没有再追问。"),
                ("阿述", "要躺上来吗？"),
                ("许薇", "好啊，让我也晒晒太阳吧。"),
                N("我小心地把她抱到另一张躺椅上，自己再躺下来。"),
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
                N("我每次抬头的间隙，就会看见一双偷瞄的眼睛；一对上又便会回到书上，然后余光里还能看见偷偷翘起的嘴角。"),
                N("但是又有什么不一样了，她似乎只是一直带着笑容看着我。"),
                N("而余光里她的双腿又时刻在提醒着我，这始终让我无法忘怀，即使是暂时也做不到。"),
                ("许薇", "这边没有你喜欢的书吗？"),
                ("阿述", "不是，我只是看累了休息一下眼睛。"),
                ("许薇", "休息眼睛怎么看着我呢？"),
                N("她露出了得逞的表情。"),
                ("阿述", "你凭什么说我在看你呢？自恋鬼。"),
                N("她露出了一个鬼脸，我不禁感慨她怎么这么能捉弄我。"),
                ("许薇", "看书看书，不打扰你了，我就是想看看我老公的帅脸啦。"),
                N("我感到肉麻的同时又忍不住笑了出来，随后目光便重新回到书本上。"),
                N("只是余光里她彻底不看书了，一直盯着我微笑，弄得我也一直没能再度沉浸到书本里。"));

            AddCharacterPresence(phase, "Interact_Partner", RoomType.DiningRoom, 5f, FacingDirection.Left, wifeCasual);
        }

        static void AddDinnerPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚饭", StoryPhaseType.Dinner,
                Ev("Interact_Stove", "灶台/做晚饭", InteractionKind.CookDinner,
                    N("许薇在吃饭，一边左右摇晃着身体，看上去很开心。"),
                    ("阿述", "怎么了，薇薇？什么事这么开心？"),
                    ("许薇", "阿述，你一直陪着我我就很开心啦。"),
                    ("阿述", "我也一样。"),
                    N("虽然我在她最痛苦的时候消失了那么久，没有说这种话的资格。"),
                    ("许薇", "我有个解决不了的烦恼。"),
                    ("阿述", "怎么还没说出来就在解决不了了。"),
                    ("许薇", "如果我消失了你怎么做？"),
                    N("我感受到一股情绪的冲击。"),
                    ("许薇", "你的生活还要继续……"),
                    N("我好像知道她要说什么了。"),
                    ("许薇", "我们迟早也会分别。"),
                    ("阿述", "怎么……"),
                    N("她没有给我打马虎眼的机会。"),
                    ("许薇", "但今天和你在一起我真的很开心……"),
                    ("阿述", "我……"),
                    N("她说得很强硬，似乎是想在我忍不住打断她之前结束对话。"),
                    ("许薇", "我想你也一样，阿述……"),
                    N("我已经意识到这是些许安慰的话语。"),
                    ("许薇", "但我希望到那个时候你能忘掉我。"),
                    N("她说了出来我想象中的话语，而我能做的却只是沉默。"),
                    N("我其实是知道的，今天一天仿佛回到热恋期的违和。"),
                    N("假如没有循环也没有终点，一直相互拥抱又相互囚禁，我自己也不知道我究竟能做到哪一天。"),
                    N("突然我想起了地下室的时间机器，那里有一个可以结束这一切循环的按钮。"),
                    N("同时浮现在心中的是一种恐惧，那就是……"),
                    N("今天是第几次循环了？"),
                    N("外面的时间还在流逝，我和她的父母、亲人、朋友都未曾知晓这里发生的一切，"),
                    N("我不知道到底过了多久，又到底发生了什么？"),
                    N("如果有一天，我是说如果，如果我们结束了循环，究竟要怎样面对那些人？"),
                    N("我感到一阵寒意爬上心头。"),
                    ("许薇", "阿述！阿述！程述！"),
                    N("她把我从思绪中拉出。"),
                    ("阿述", "我在，我在。"),
                    N("我撑出了一个笑容。"),
                    ("许薇", "再不吃你做的饭都要凉了。"),
                    ("阿述", "好的，好的，我马上吃。"),
                    N("又让她担心了，现在最重要的还是好好地陪着她吧。")));
        }

        static void AddEveningPhase(SerializedProperty phases)
        {
            AddPhase(phases, "晚上", StoryPhaseType.Evening,
                Ev("Interact_Chair", "一起看夜空", InteractionKind.WatchSunset,
                    N("我今晚打算陪陪她，免得脑子里想那些有的没的。"),
                    N("我和她来到天台，这里的夜空是那么清澈，群星捧月清晰可见。"),
                    N("我把她抱起来，小心地放在躺椅上，然后自己再躺上另一个躺椅。"),
                    ("阿述", "这里的夜空很美。"),
                    ("许薇", "阿述，你没事吧？"),
                    N("我心中一颤。"),
                    ("许薇", "你今晚很不对劲。我是说，整个人看上去如同失神了一般。"),
                    ("阿述", "没事，我只是想到了一些不太好的事情。"),
                    N("她神情严肃地看向我。"),
                    ("许薇", "阿述，你应该说出来，内耗解决不了问题。"),
                    ("阿述", "不会的不会的，我能消化的。"),
                    ("许薇", "你记得我们之间的约定吗？"),
                    ("阿述", "两人之间要坦诚……"),
                    N("我自然而然地回答了这个答案，这让我自己都有点意外了。"),
                    ("许薇", "阿述，我相信即使我们角色互换，你也会和我说同样的话。"),
                    ("许薇", "所以……"),
                    ("许薇", "说出来吧，阿述。"),
                    N("我一瞬间感受到泪水涌上眼眶，一股情绪推动着我全盘道出。"),
                    ("阿述", "所以，今天过去一切都会再度复原。"),
                    ("许薇", "你真的……你真的……"),
                    ("许薇", "好厉害啊，阿述！"),
                    N("比起对于这件事本身震惊，她似乎更加对于我一个人完成这件事而惊讶。"),
                    N("她耐心地倾听着我一股脑讲述着这个过程，附和着我对于自己的研究的雀跃。"),
                    N("夜空的星光在闪烁，如同我的内心。"),
                    N("我害怕，我害怕今天的回忆被重置。")),
                Ev("Interact_Desk", "一起吃甜点", InteractionKind.EatTogether,
                    N("我们来到餐厅。我从冰箱里取出冷藏了一天的芝士蛋糕，切好一片，端到餐桌对面坐下。"),
                    ("阿述", "这是你最喜欢的芝士蛋糕。"),
                    ("许薇", "阿述，你没事吧？"),
                    N("我心中一颤。"),
                    ("许薇", "你今晚很不对劲。我是说，整个人看上去如同失神了一般。"),
                    ("阿述", "没事，我只是想到了一些不太好的事情。"),
                    N("她神情严肃地看向我。"),
                    ("许薇", "阿述，你应该说出来，内耗解决不了问题。"),
                    ("阿述", "不会的不会的，我能消化的。"),
                    ("许薇", "你记得我们之间的约定吗？"),
                    ("阿述", "两人之间要坦诚……"),
                    N("我自然而然地回答了这个答案，这让我自己都有点意外了。"),
                    ("许薇", "阿述，我相信即使我们角色互换，你也会和我说同样的话。"),
                    ("许薇", "所以……"),
                    ("许薇", "说出来吧，阿述。"),
                    N("我一瞬间感受到泪水涌上眼眶，一股情绪推动着我全盘道出。"),
                    ("阿述", "所以，今天过去一切都会再度复原。"),
                    ("许薇", "你真的……你真的……"),
                    ("许薇", "好厉害啊，阿述！"),
                    N("比起对于这件事本身震惊，她似乎更加对于我一个人完成这件事而惊讶。"),
                    N("她耐心地倾听着我一股脑讲述着这个过程，附和着我对于自己的研究的雀跃。"),
                    N("蛋糕盘子上的水滴在闪烁，如同我的内心。"),
                    N("我害怕，我害怕今天的回忆被重置。")),
                Ev("Interact_Sofa", "一起看相册", InteractionKind.TalkToPartner,
                    N("我们来到客厅。柜子里有一本相册，记录着我们一路走来的点滴。"),
                    N("我把她抱起来，小心地放在沙发上，在她身旁坐下。"),
                    ("阿述", "每次大日子我们都会翻相册。"),
                    ("许薇", "阿述，你没事吧？"),
                    N("我心中一颤。"),
                    ("许薇", "你今晚很不对劲。我是说，整个人看上去如同失神了一般。"),
                    ("阿述", "没事，我只是想到了一些不太好的事情。"),
                    N("她神情严肃地看向我。"),
                    ("许薇", "阿述，你应该说出来，内耗解决不了问题。"),
                    ("阿述", "不会的不会的，我能消化的。"),
                    ("许薇", "你记得我们之间的约定吗？"),
                    ("阿述", "两人之间要坦诚……"),
                    N("我自然而然地回答了这个答案，这让我自己都有点意外了。"),
                    ("许薇", "阿述，我相信即使我们角色互换，你也会和我说同样的话。"),
                    ("许薇", "所以……"),
                    ("许薇", "说出来吧，阿述。"),
                    N("我一瞬间感受到泪水涌上眼眶，一股情绪推动着我全盘道出。"),
                    ("阿述", "所以，今天过去一切都会再度复原。"),
                    ("许薇", "你真的……你真的……"),
                    ("许薇", "好厉害啊，阿述！"),
                    N("比起对于这件事本身震惊，她似乎更加对于我一个人完成这件事而惊讶。"),
                    N("她耐心地倾听着我一股脑讲述着这个过程，附和着我对于自己的研究的雀跃。"),
                    N("相册塑封上的反光在闪烁，如同我的内心。"),
                    N("我害怕，我害怕今天的回忆被重置。")));
        }

        static void AddNightEventPhase(SerializedProperty phases, Sprite wifePajamaPresence)
        {
            var phaseIndex = phases.arraySize;
            AddPhase(phases, "夜晚事件", StoryPhaseType.NightEvent,
                Ev("Interact_Partner", "夜晚事件", InteractionKind.TalkToPartner,
                    N("今天时间不早了，但厨房里却传来声响，那边发生什么了？"),
                    N("她正在柜子里翻找着什么。"),
                    ("阿述", "薇薇，在找什么呢？"),
                    ("许薇", "既然今天的一切会被重置，那我们为什么不一直玩到重置之前呢？"),
                    ("阿述", "挺好的主意欸。"),
                    N("她真是乐观，当我还担心着今天的回忆被重置时，她已经在计划创造新的回忆了。"),
                    N("或许我也不应该沉溺于那么虚无的想法了。"),
                    ("阿述", "所以你有什么主意吗？"),
                    ("许薇", "我打算先泡一点咖啡。"),
                    ("阿述", "那在上面的柜子，我来帮你。"),
                    N("我从上方的柜子拿出了咖啡豆和咖啡机。"),
                    ("许薇", "可恶啊，阿述，你居然这么考虑不周。"),
                    ("许薇", "居然把我最喜欢的咖啡放到我够不到的地方。"),
                    N("她虽然是玩笑的语气，但却让我有点羞愧。"),
                    ("阿述", "抱歉。"),
                    ("许薇", "哼，罚你不准动手，让我来泡咖啡。"),
                    N("我无法控制地露出担心的表情，被开水烫到了怎么办？不小心把东西打翻了怎么办？"),
                    ("许薇", "不用担心，阿述。"),
                    ("许薇", "我不会强求，需要帮忙我会叫你的。"),
                    ("许薇", "我想做一点事情，这样会让我感觉自己还活着。"),
                    ("阿述", "好吧。"),
                    N("她温柔的嗓音仍然很好地安慰了我。我在一旁静静地看着她一步步进行。"),
                    N("我想起家里几位长辈——车祸、癌症、脑梗……有的已经离开，有的带着痴呆离开。"),
                    N("他们都在失去行动能力后有着同样的迷茫，尽管他们什么都没做错。"),
                    N("书上说劳动创造了人，我觉得失去劳动能力的人更容易失去生命的意义。"),
                    N("或许让她享受此刻泡咖啡的过程，比任何安慰的话都更有用。"),
                    ("许薇", "阿述，杯子在哪里？"),
                    ("阿述", "也放在上面，抱歉。"),
                    N("我取下两个杯子递给她。"),
                    ("许薇", "我刚刚跟你开玩笑的，怎么这么较真呢？"),
                    N("她带着笑容回复，同时将泡好的咖啡从壶里倒入杯中。"),
                    ("许薇", "来，不醉不欢。"),
                    ("阿述", "怎么搞得跟喝酒一样？"),
                    N("她的话让我笑了起来。我抿了一口咖啡，温热、苦涩与香气完美搭配。"),
                    ("阿述", "好喝。"),
                    ("许薇", "厉害吧？"),
                    N("我用力地点头，看着她的笑脸。关于她泡咖啡的记忆涌上心头，已经很久没看过她这样了。")));

            var phase = phases.GetArrayElementAtIndex(phaseIndex);
            AddCharacterPresence(phase, "Interact_Partner", RoomType.Kitchen, 2f, FacingDirection.Left, wifePajamaPresence);
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
                    ("许薇", "等重置结束后你打算怎么办？"),
                    ("阿述", "你为什么会这么问？"),
                    ("许薇", "就问问呗。"),
                    ("阿述", "首先，我也无法保证哪个循环里我会不会结束循环。"),
                    ("阿述", "我只希望不是因为我们吵架了。"),
                    ("许薇", "肯定不会的。"),
                    N("她笑着向我许下毫无效力的保证。"),
                    ("阿述", "好的好的。"),
                    ("阿述", "总之，我会去好好跟岳父岳母说一下。"),
                    ("许薇", "也得跟你的父母说一下。"),
                    ("阿述", "当然，然后我会好好地陪你度过剩下的时间。"),
                    ("许薇", "突然变得坚强起来了呢。"),
                    ("阿述", "总感觉你这话说的像长辈一样。"),
                    ("阿述", "然后我大概会好好生活吧。"),
                    ("许薇", "说好咯！一定、一定要好好生活哦！"),
                    ("阿述", "哎，一言为定。"),
                    N("我笑着向她许下毫无效力的保证。"),
                    N("突然间，意识逐渐模糊，今天的一切都将重新循环。"),
                    ("许薇", "我爱你，阿述……"),
                    ("许薇", "你也要爱你自己……"),
                    N("我还没来得及开口回应。"),
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
                N("虽然我保留了这台机器的急停按钮，"),
                N("但我真的有勇气按下它吗？"));
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
