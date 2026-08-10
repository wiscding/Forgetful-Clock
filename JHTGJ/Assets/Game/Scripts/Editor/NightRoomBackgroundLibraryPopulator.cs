#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class NightRoomBackgroundLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/NightRoomBackgroundLibrary.asset";

        static readonly string[] NightTexturePaths =
        {
            NightRoomBackgroundPaths.FrontHall,
            NightRoomBackgroundPaths.Bedroom,
            NightRoomBackgroundPaths.Bathroom,
            NightRoomBackgroundPaths.Kitchen,
            NightRoomBackgroundPaths.BackGarden,
            NightRoomBackgroundPaths.Rooftop,
            NightRoomBackgroundPaths.LivingRoom,
            NightRoomBackgroundPaths.DiningRoom
        };

        [MenuItem("JHTGJ/Populate Night Room Background Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Night room background library populated.");
        }

        [MenuItem("JHTGJ/Fix Night Room Background Import Settings")]
        public static void FixImportSettingsFromMenu()
        {
            ApplyImportSettingsToAllNightTextures();
            Debug.Log("[JHTGJ] Night room background import settings aligned with day backgrounds.");
        }

        public static NightRoomBackgroundLibrary EnsureAsset()
        {
            ApplyImportSettingsToAllNightTextures();
            EnsureResourcesFolder();

            var library = AssetDatabase.LoadAssetAtPath<NightRoomBackgroundLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<NightRoomBackgroundLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("frontHall").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.FrontHall);
            so.FindProperty("bedroom").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.Bedroom);
            so.FindProperty("bathroom").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.Bathroom);
            so.FindProperty("kitchen").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.Kitchen);
            so.FindProperty("backGarden").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.BackGarden);
            so.FindProperty("rooftop").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.Rooftop);
            so.FindProperty("livingRoom").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.LivingRoom);
            so.FindProperty("diningRoom").objectReferenceValue = LoadSprite(NightRoomBackgroundPaths.DiningRoom);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        static void ApplyImportSettingsToAllNightTextures()
        {
            foreach (var path in NightTexturePaths)
                ApplyRoomBackgroundImportSettings(path);
        }

        public static void ApplyRoomBackgroundImportSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            var changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsToUnits, RoomBackgroundImportSettings.PixelsPerUnit))
            {
                importer.spritePixelsToUnits = RoomBackgroundImportSettings.PixelsPerUnit;
                changed = true;
            }

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            var pivot = new Vector2(RoomBackgroundImportSettings.PivotX, RoomBackgroundImportSettings.PivotY);
            if (settings.spriteMode != (int)SpriteImportMode.Single ||
                settings.spriteAlignment != RoomBackgroundImportSettings.Alignment ||
                settings.spritePivot != pivot)
            {
                settings.spriteMode = (int)SpriteImportMode.Single;
                settings.spriteAlignment = RoomBackgroundImportSettings.Alignment;
                settings.spritePivot = pivot;
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (changed)
                importer.SaveAndReimport();
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
