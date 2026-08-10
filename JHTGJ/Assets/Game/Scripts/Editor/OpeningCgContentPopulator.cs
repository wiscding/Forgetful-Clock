#if UNITY_EDITOR
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class OpeningCgContentPopulator
    {
        public const string AssetPath = "Assets/Game/Data/OpeningCgSequence.asset";

        [MenuItem("JHTGJ/Populate Opening CG Content")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();

            var sequence = AssetDatabase.LoadAssetAtPath<OpeningCgSequence>(AssetPath);
            if (sequence == null)
            {
                sequence = ScriptableObject.CreateInstance<OpeningCgSequence>();
                AssetDatabase.CreateAsset(sequence, AssetPath);
            }

            Populate(sequence);
            EditorUtility.SetDirty(sequence);
            AssetDatabase.SaveAssets();

            Selection.activeObject = sequence;
            EditorGUIUtility.PingObject(sequence);
            Debug.Log("[JHTGJ] Opening CG content populated.");
        }

        public static void Populate(OpeningCgSequence sequence)
        {
            var so = new SerializedObject(sequence);
            var slides = so.FindProperty("slides");
            slides.ClearArray();

            AddSlide(slides, "Assets/Art/CG/妻子病重.png",
                L(StoryCharacterNames.Protagonist, "为什么？"),
                L(StoryCharacterNames.Protagonist, "为什么会发生这样的事情？"));

            AddSlide(slides, "Assets/Art/CG/开场男人和医生交谈.png",
                L(StoryCharacterNames.Doctor, "「我很抱歉，先生……」"),
                L(StoryCharacterNames.Protagonist, "我不能接受……"),
                L(StoryCharacterNames.Protagonist, "我不接受这种事……"),
                L(StoryCharacterNames.Doctor, "「夫人大概……大概只剩几个月的时间」"),
                L(StoryCharacterNames.Protagonist, "我要救她！"),
                L(StoryCharacterNames.Protagonist, "我不要她离开！"),
                L(StoryCharacterNames.Doctor, "「车祸这种事情……」"),
                L(StoryCharacterNames.Protagonist, "我逃跑了，不愿再听下去……"));

            AddSlide(slides, "Assets/Art/CG/开场男人潜心研究.png",
                L(StoryCharacterNames.Protagonist, "时间机器的最后阶段，"),
                L(StoryCharacterNames.Protagonist, "被勒令停止后东西都放在了家中，"),
                L(StoryCharacterNames.Protagonist, "我没日没夜地接手开发……"),
                L(StoryCharacterNames.Protagonist, "我是天才我是天才……"),
                L(StoryCharacterNames.Protagonist, "我可以的……"),
                L(StoryCharacterNames.Protagonist, "我可以……"));

            AddSlide(slides, "Assets/Art/CG/医生立绘.png",
                L(StoryCharacterNames.Doctor, "「先生，多陪陪你夫人吧，她大概……」"),
                L(StoryCharacterNames.Protagonist, "「我要把她接出去。」"));

            AddSlide(slides, "Assets/Art/CG/医生为难.png",
                L(StoryCharacterNames.Doctor, "「什么，这半夜的……好吧，先生，这边签字……」"),
                L(StoryCharacterNames.Protagonist, "我要救她！"),
                L(StoryCharacterNames.Protagonist, "我能救她！！！"),
                L(StoryCharacterNames.Doctor, "「先生，如果离开病房，夫人最多……七天吧……」"),
                L(StoryCharacterNames.Protagonist, "她留在这里又如何？"),
                L(StoryCharacterNames.Protagonist, "你们谁也救不了她啊！"),
                L(StoryCharacterNames.Doctor, "「好的……」"),
                L(StoryCharacterNames.Protagonist, "我推着轮椅上似乎还在睡梦中的妻子离开了医院。"),
                L(StoryCharacterNames.Protagonist, "我可以的……"));

            AddSlide(slides, "Assets/Art/CG/男人与妻子进入别墅.png",
                L(StoryCharacterNames.Protagonist, "这是我用所有积蓄买下的山野里的别墅，"),
                L(StoryCharacterNames.Protagonist, "没有人会来打扰我们的，"),
                L(StoryCharacterNames.Protagonist, "家里的东西也已经搬过来了，"),
                L(StoryCharacterNames.Protagonist, "我这就去启动机器。"),
                L(StoryCharacterNames.Protagonist, "一切都会好起来的，"),
                L(StoryCharacterNames.Protagonist, "一切都会好起来的……"),
                L(StoryCharacterNames.Wife, "「这里是？」"),
                L(StoryCharacterNames.Protagonist, "「你醒了？」"),
                L(StoryCharacterNames.Protagonist, "「许薇！你终于……」"),
                L(StoryCharacterNames.Protagonist, "她张开双手，"),
                L(StoryCharacterNames.Wife, "「对不起，对不起没有陪你……」"),
                L(StoryCharacterNames.Protagonist, "只有我蹲下靠近才能促成这个拥抱，"),
                L(StoryCharacterNames.Protagonist, "这让我鼻头一酸，"),
                L(StoryCharacterNames.Protagonist, "「之前的再数落你，更重要的是现在你在就好了。」"),
                L(StoryCharacterNames.Wife, "「还能开玩笑，真是精神呢！」"),
                L(StoryCharacterNames.Protagonist, "我笑了，她也笑了。"),
                L(StoryCharacterNames.Wife, "「臭人！就知道花言巧语……」"),
                L(StoryCharacterNames.Protagonist, "我俩你一句我一句地谈闲，"),
                L(StoryCharacterNames.Protagonist, "好像什么都一如既往，"),
                L(StoryCharacterNames.Protagonist, "好像那场车祸根本就没有发生……"),
                L(StoryCharacterNames.Protagonist, "她没有问我关于自己身体的情况，"),
                L(StoryCharacterNames.Protagonist, "也没有问我关于这栋别墅的一切，"),
                L(StoryCharacterNames.Protagonist, "或许她自己最清楚了，"),
                L(StoryCharacterNames.Protagonist, "她一直都这样……"));

            AddSlide(slides, "Assets/Art/CG/开头男人启动机器.png",
                L(StoryCharacterNames.Protagonist, "我要去启动机器，"),
                L(StoryCharacterNames.Protagonist, "即使我一开始就知道它没有经过任何的测试，"),
                L(StoryCharacterNames.Protagonist, "但是……但是她没有时间了！"),
                L(StoryCharacterNames.Protagonist, "我会救她，"),
                L(StoryCharacterNames.Protagonist, "我可以的……"));

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static (string speaker, string text) L(string speaker, string text) => (speaker, text);

        static void AddSlide(
            SerializedProperty slides,
            string imagePath,
            params (string speaker, string text)[] lines)
        {
            slides.InsertArrayElementAtIndex(slides.arraySize);
            var slide = slides.GetArrayElementAtIndex(slides.arraySize - 1);
            slide.FindPropertyRelative("image").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);

            var linesProp = slide.FindPropertyRelative("lines");
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
