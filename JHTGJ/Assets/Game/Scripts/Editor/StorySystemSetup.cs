#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Core;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class StorySystemSetup
    {
        [MenuItem("JHTGJ/Setup Story System (Game Scene)")]
        public static void SetupFromMenu()
        {
            if (!EnsureGameScene())
                return;

            EnsureInGameScene();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "Story System",
                "已在 SampleScene 配置事件树：\n" +
                "· GameSystems → StoryEventTreeManager\n" +
                "· DefaultStoryCampaign（多天剧情总表）\n" +
                "· DefaultDayStorySchedule（第 1 天）\n" +
                "· Day2StorySchedule（第 2 天）\n" +
                "· Day3StorySchedule（第 3 天）\n" +
                "· Day4StorySchedule（第 4 天）\n" +
                "· Day5StorySchedule（第 5 天）\n" +
                "· 对话 UI（若不存在会自动创建）\n" +
                "· 按按钮阶段 UI（StoryChoiceCanvas）\n" +
                "· 结局滚动 UI（EndingScrollCanvas）\n" +
                "· 开场 CG（OpeningCgCanvas + OpeningCgSequence）\n" +
                "· 音频（GameAudioManager + AudioLibrary）\n\n" +
                "推荐：\n" +
                "1. JHTGJ → Populate Story Portrait Library\n" +
                "2. JHTGJ → Populate Night Room Background Library\n" +
                "3. JHTGJ → Populate Opening CG Content\n" +
                "4. JHTGJ → Populate All Story Days (Day 1 + 2 + 3 + 4 + 5)\n" +
                "5. JHTGJ → Populate Audio Library\n" +
                "6. JHTGJ → Add Button Click Sfx To Scene Buttons",
                "OK");
        }

        public static void EnsureInGameScene()
        {
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(DayStoryScheduleCreator.DefaultAssetPath);
            if (schedule == null)
                schedule = DayStoryScheduleCreator.CreateOrLoadDefault(allowOverwritePrompt: false);

            var campaign = AssetDatabase.LoadAssetAtPath<StoryCampaign>(DayStoryScheduleCreator.DefaultCampaignPath);
            if (campaign == null)
                campaign = DayStoryScheduleCreator.CreateOrLoadDefaultCampaign(allowOverwritePrompt: false);

            var systems = GameObject.Find("GameSystems") ?? new GameObject("GameSystems");
            var villaManager = systems.GetComponent<VillaSceneManager>() ?? systems.AddComponent<VillaSceneManager>();
            var storyManager = systems.GetComponent<StoryEventTreeManager>() ?? systems.AddComponent<StoryEventTreeManager>();

            DialogueUICreator.CreateIfMissing();
            var dialogueUi = Object.FindObjectOfType<DialogueUI>(true);
            var choiceUi = Object.FindObjectOfType<StoryChoiceUI>(true);
            if (choiceUi == null)
                choiceUi = StoryChoiceUIBuilder.Build();
            var endingScrollUi = Object.FindObjectOfType<EndingScrollUI>(true);
            if (endingScrollUi == null || IsBodyLabelMissing(endingScrollUi))
                endingScrollUi = EndingScrollUIBuilder.Build();
            else
            {
                var endingCanvas = endingScrollUi.gameObject;
                EndingScrollUIFixer.FixExisting(endingCanvas);
            }
            var openingCgUi = Object.FindObjectOfType<OpeningCgUI>(true);
            if (openingCgUi == null || IsBodyLabelMissing(openingCgUi))
                openingCgUi = OpeningCgUIBuilder.Build();

            if (Object.FindObjectOfType<InteractionPromptUI>(true) == null)
                InteractionPromptUIBuilder.Build();

            var openingCgSequence = AssetDatabase.LoadAssetAtPath<OpeningCgSequence>(OpeningCgContentPopulator.AssetPath);
            if (openingCgSequence == null)
                OpeningCgContentPopulator.PopulateFromMenu();

            StoryPortraitLibraryPopulator.EnsureAsset();
            NightRoomBackgroundLibraryPopulator.EnsureAsset();
            PostCookingDiningLibraryPopulator.EnsureAsset();
            Day1NightEventLibraryPopulator.EnsureAsset();
            Day2NightEventLibraryPopulator.EnsureAsset();
            Day4DuskEventLibraryPopulator.EnsureAsset();

            RoomSelectorSetup.EnsureLivingRoomElevator();
            RoomSelectorSetup.EnsureDiningRoomTravelDoor();

            openingCgSequence = AssetDatabase.LoadAssetAtPath<OpeningCgSequence>(OpeningCgContentPopulator.AssetPath);

            var openingCgPlayer = systems.GetComponent<OpeningCgPlayer>() ?? systems.AddComponent<OpeningCgPlayer>();

            var storySo = new SerializedObject(storyManager);
            storySo.FindProperty("campaign").objectReferenceValue = campaign;
            storySo.FindProperty("schedule").objectReferenceValue = schedule;
            storySo.FindProperty("villaSceneManager").objectReferenceValue = villaManager;
            storySo.FindProperty("dialogueUI").objectReferenceValue = dialogueUi;
            storySo.FindProperty("storyChoiceUI").objectReferenceValue = choiceUi;
            storySo.FindProperty("endingScrollUI").objectReferenceValue = endingScrollUi;
            storySo.ApplyModifiedPropertiesWithoutUndo();

            var openingCgSo = new SerializedObject(openingCgPlayer);
            openingCgSo.FindProperty("sequence").objectReferenceValue = openingCgSequence;
            openingCgSo.FindProperty("openingCgUI").objectReferenceValue = openingCgUi;
            openingCgSo.FindProperty("villaSceneManager").objectReferenceValue = villaManager;
            openingCgSo.ApplyModifiedPropertiesWithoutUndo();

            var protagonist = GameObject.Find("Protagonist");
            if (protagonist != null)
            {
                var interact = protagonist.GetComponent<InteractController>();
                if (interact != null)
                {
                    var interactSo = new SerializedObject(interact);
                    interactSo.FindProperty("storyManager").objectReferenceValue = storyManager;
                    interactSo.FindProperty("sceneManager").objectReferenceValue = villaManager;
                    var promptUi = Object.FindObjectOfType<InteractionPromptUI>(true);
                    if (promptUi != null)
                        interactSo.FindProperty("interactionPromptUI").objectReferenceValue = promptUi;
                    interactSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            var pauseMenu = Object.FindObjectOfType<PauseMenuUI>(true);
            if (pauseMenu != null)
            {
                var pauseSo = new SerializedObject(pauseMenu);
                pauseSo.FindProperty("storyManager").objectReferenceValue = storyManager;
                pauseSo.FindProperty("villaSceneManager").objectReferenceValue = villaManager;
                pauseSo.ApplyModifiedPropertiesWithoutUndo();
            }

            Selection.activeGameObject = systems;
            EditorGUIUtility.PingObject(systems);

            StoryCharacterSetup.EnsureWifeCharacter();
            StoryCharacterSetup.RemoveDuplicateWifeCharacters();
            StoryCharacterSetup.DisableDuplicateInteractMarkers("Interact_Partner");

            var audioLibrary = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AudioLibraryPopulator.AssetPath);
            if (audioLibrary != null)
                AudioLibraryPopulator.EnsureAudioManagerInProject(audioLibrary);
            else
                Debug.LogWarning("[JHTGJ] 未找到 AudioLibrary，请先运行 JHTGJ → Populate Audio Library。");

            Debug.Log("[JHTGJ] Story system wired in game scene.");
        }

        static bool IsBodyLabelMissing(Object uiComponent)
        {
            if (uiComponent == null)
                return true;

            var so = new SerializedObject(uiComponent);
            return so.FindProperty("bodyLabel").objectReferenceValue == null;
        }

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "事件树应配置在 SampleScene（游戏场景）。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            return true;
        }
    }
}
#endif
