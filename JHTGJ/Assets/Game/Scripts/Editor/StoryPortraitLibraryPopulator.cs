#if UNITY_EDITOR
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class StoryPortraitLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/StoryPortraitLibrary.asset";

        [MenuItem("JHTGJ/Populate Story Portrait Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Story portrait library populated with half-body sprites.");
        }

        public static StoryPortraitLibrary EnsureAsset()
        {
            EnsureResourcesFolder();

            var library = AssetDatabase.LoadAssetAtPath<StoryPortraitLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<StoryPortraitLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("protagonistCasualHalf").objectReferenceValue =
                LoadSprite(StoryPortraitPaths.ProtagonistCasualHalf);
            so.FindProperty("wifeCasualHalf").objectReferenceValue =
                LoadSprite(StoryPortraitPaths.WifeCasualHalf);
            so.FindProperty("protagonistPajamaHalf").objectReferenceValue =
                LoadSprite(StoryPortraitPaths.ProtagonistPajamaHalf);
            so.FindProperty("wifePajamaHalf").objectReferenceValue =
                LoadSprite(StoryPortraitPaths.WifePajamaHalf);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        static void EnsureResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Resources");
            }
        }

        static Sprite LoadSprite(string assetPath) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
#endif
