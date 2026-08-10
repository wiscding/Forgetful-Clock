#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class DayStoryScheduleCreator
    {
        public const string DefaultAssetPath = "Assets/Game/Data/DefaultDayStorySchedule.asset";
        public const string DefaultCampaignPath = "Assets/Game/Data/DefaultStoryCampaign.asset";

        [MenuItem("JHTGJ/Create Default Day Story Schedule")]
        public static DayStorySchedule CreateOrLoadDefault(bool allowOverwritePrompt = true)
        {
            EnsureDataFolder();

            var existing = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(DefaultAssetPath);
            if (existing != null && allowOverwritePrompt)
            {
                if (!EditorUtility.DisplayDialog(
                        "已存在",
                        "DefaultDayStorySchedule 已存在，是否覆盖为模板内容？",
                        "覆盖",
                        "保留现有"))
                {
                    Selection.activeObject = existing;
                    EditorGUIUtility.PingObject(existing);
                    return existing;
                }
            }

            var schedule = existing != null ? existing : ScriptableObject.CreateInstance<DayStorySchedule>();
            PopulateDefaultSchedule(schedule);

            if (existing == null)
                AssetDatabase.CreateAsset(schedule, DefaultAssetPath);
            else
                EditorUtility.SetDirty(schedule);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = schedule;
            EditorGUIUtility.PingObject(schedule);
            Debug.Log($"[JHTGJ] Day story schedule ready at {DefaultAssetPath}");
            return schedule;
        }

        [MenuItem("JHTGJ/Create Default Story Campaign")]
        public static StoryCampaign CreateOrLoadDefaultCampaign(bool allowOverwritePrompt = true)
        {
            EnsureDataFolder();

            var day1 = CreateOrLoadDefault(allowOverwritePrompt: false);
            var day2 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day2StoryContentPopulator.Day2AssetPath);
            var day3 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day3StoryContentPopulator.Day3AssetPath);

            var existing = AssetDatabase.LoadAssetAtPath<StoryCampaign>(DefaultCampaignPath);
            if (existing != null && allowOverwritePrompt)
            {
                if (!EditorUtility.DisplayDialog(
                        "已存在",
                        "DefaultStoryCampaign 已存在，是否重新绑定第 1、2、3 天 Schedule？",
                        "重新绑定",
                        "保留现有"))
                {
                    Selection.activeObject = existing;
                    EditorGUIUtility.PingObject(existing);
                    return existing;
                }
            }

            var campaign = existing != null ? existing : ScriptableObject.CreateInstance<StoryCampaign>();
            BindCampaignDays(campaign, day1, day2, day3);

            if (existing == null)
                AssetDatabase.CreateAsset(campaign, DefaultCampaignPath);
            else
                EditorUtility.SetDirty(campaign);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = campaign;
            EditorGUIUtility.PingObject(campaign);
            Debug.Log($"[JHTGJ] Story campaign ready at {DefaultCampaignPath}");
            return campaign;
        }

        public static void EnsureCampaignIncludesDay2(DayStorySchedule day2Schedule)
        {
            EnsureCampaignIncludesAllDays();
        }

        public static void EnsureCampaignIncludesAllDays()
        {
            var day1 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(DefaultAssetPath);
            var day2 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day2StoryContentPopulator.Day2AssetPath);
            var day3 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day3StoryContentPopulator.Day3AssetPath);
            var day4 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day4StoryContentPopulator.Day4AssetPath);
            var day5 = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(Day5StoryContentPopulator.Day5AssetPath);

            var campaign = AssetDatabase.LoadAssetAtPath<StoryCampaign>(DefaultCampaignPath);
            if (campaign == null)
            {
                campaign = ScriptableObject.CreateInstance<StoryCampaign>();
                BindCampaignDays(campaign, day1, day2, day3, day4, day5);
                AssetDatabase.CreateAsset(campaign, DefaultCampaignPath);
            }
            else
            {
                BindCampaignDays(campaign, day1, day2, day3, day4, day5);
                EditorUtility.SetDirty(campaign);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[JHTGJ] DefaultStoryCampaign 已绑定全部天数剧情。");
        }

        static void BindCampaignDays(StoryCampaign campaign, params DayStorySchedule[] daySchedules)
        {
            var so = new SerializedObject(campaign);
            var days = so.FindProperty("days");
            days.ClearArray();

            if (daySchedules == null)
                return;

            foreach (var schedule in daySchedules)
            {
                if (schedule == null)
                    continue;

                days.InsertArrayElementAtIndex(days.arraySize);
                days.GetArrayElementAtIndex(days.arraySize - 1).objectReferenceValue = schedule;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void EnsureDataFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Data"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Data");
            }
        }

        static void PopulateDefaultSchedule(DayStorySchedule schedule)
        {
            DefaultStoryContentPopulator.Populate(schedule);
        }
    }
}
#endif
