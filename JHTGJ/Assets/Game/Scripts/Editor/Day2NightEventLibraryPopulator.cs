#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day2NightEventLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/Day2NightEventLibrary.asset";

        [MenuItem("JHTGJ/Populate Day 2 Night Event Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Day 2 night event library populated.");
        }

        public static Day2NightEventLibrary EnsureAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Resources");
            }

            var library = AssetDatabase.LoadAssetAtPath<Day2NightEventLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<Day2NightEventLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("kitchenBackground").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(SpecialWifeInteractPaths.Day2NightEventKitchenBackground);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }
    }
}
#endif
