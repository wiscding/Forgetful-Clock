#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace JHTGJ.EditorTools
{
    public static class AudioLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Data/AudioLibrary.asset";

        const string MenuBgmPath = "Assets/Audio/BGM/菜单bgm1. Echoes of Solitude (Loop).mp3";
        const string NormalBgmPath = "Assets/Audio/BGM/常态bgm4. Fading Memories (Loop).mp3";
        const string EndingBgmPath = "Assets/Audio/BGM/结局cg6. Embers of Hope (Loop).mp3";
        const string OpeningBgmPath = "Assets/Audio/BGM/开场cg3. Tears in the Rain (Loop).mp3";
        const string LastDayBgmPath = "Assets/Audio/BGM/第32天（最后一天）bgm2. Whispers of Yesterday (Loop).mp3";
        const string ConflictBgmPath = "Assets/Audio/BGM/冲突点bgm7. Silent Longing (Loop).mp3";
        const string ButtonSfxPath = "Assets/Audio/SFX/635915__earth_cord__button-push.wav";

        [MenuItem("JHTGJ/Populate Audio Library")]
        public static void PopulateFromMenu()
        {
            DayStoryScheduleCreator.EnsureDataFolder();

            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("menuBgm").objectReferenceValue = LoadClip(MenuBgmPath);
            so.FindProperty("normalBgm").objectReferenceValue = LoadClip(NormalBgmPath);
            so.FindProperty("endingBgm").objectReferenceValue = LoadClip(EndingBgmPath);
            so.FindProperty("openingBgm").objectReferenceValue = LoadClip(OpeningBgmPath);
            so.FindProperty("lastDayBgm").objectReferenceValue = LoadClip(LastDayBgmPath);
            so.FindProperty("conflictBgm").objectReferenceValue = LoadClip(ConflictBgmPath);
            so.FindProperty("buttonClickSfx").objectReferenceValue = LoadClip(ButtonSfxPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            EnsureAudioManagerInProject(library);
            EnsureResourcesAudioLibrary(library);

            Selection.activeObject = library;
            EditorGUIUtility.PingObject(library);
            Debug.Log("[JHTGJ] Audio library populated.");
        }

        [MenuItem("JHTGJ/Add Button Click Sfx To Scene Buttons")]
        public static void AddButtonSfxToSceneButtons()
        {
            var count = 0;
            foreach (var button in Object.FindObjectsOfType<Button>(true))
            {
                if (button.GetComponent<UISfxButton>() != null)
                    continue;

                button.gameObject.AddComponent<UISfxButton>();
                count++;
            }

            Debug.Log($"[JHTGJ] Added UISfxButton to {count} button(s).");
        }

        public static void EnsureResourcesAudioLibrary(AudioLibrary library)
        {
            if (library == null)
                return;

            const string resourcesFolder = "Assets/Resources";
            const string resourcesAssetPath = resourcesFolder + "/AudioLibrary.asset";

            if (!AssetDatabase.IsValidFolder(resourcesFolder))
                AssetDatabase.CreateFolder("Assets", "Resources");

            var existing = AssetDatabase.LoadAssetAtPath<AudioLibrary>(resourcesAssetPath);
            if (existing == library)
                return;

            if (existing != null)
            {
                EditorUtility.CopySerialized(library, existing);
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                return;
            }

            AssetDatabase.CopyAsset(AssetPath, resourcesAssetPath);
            AssetDatabase.SaveAssets();
        }

        public static void EnsureAudioManagerInProject(AudioLibrary library)
        {
            WireAudioManagerInScene(SceneManager.GetActiveScene(), library);

            var mainMenuPath = "Assets/Game/Scenes/MainMenu.unity";
            if (SceneManager.GetActiveScene().path != mainMenuPath &&
                System.IO.File.Exists(mainMenuPath))
            {
                var mainMenuScene = EditorSceneManager.OpenScene(mainMenuPath, OpenSceneMode.Additive);
                WireAudioManagerInScene(mainMenuScene, library);
                EditorSceneManager.CloseScene(mainMenuScene, removeScene: true);
            }
        }

        static void WireAudioManagerInScene(UnityScene scene, AudioLibrary library)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var settings = root.GetComponentInChildren<GameSettingsManager>(true);
                if (settings == null)
                    continue;

                var audio = settings.GetComponent<GameAudioManager>();
                if (audio == null)
                    audio = settings.gameObject.AddComponent<GameAudioManager>();

                var so = new SerializedObject(audio);
                so.FindProperty("library").objectReferenceValue = library;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(audio);
            }
        }

        static AudioClip LoadClip(string path) =>
            AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
#endif
