#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day1NightEventLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/Day1NightEventLibrary.asset";

        [MenuItem("JHTGJ/Populate Day 1 Night Event Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Day 1 night event library populated.");
        }

        public static Day1NightEventLibrary EnsureAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Resources");
            }

            var library = AssetDatabase.LoadAssetAtPath<Day1NightEventLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<Day1NightEventLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("bedroomBackground").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(SpecialWifeInteractPaths.Day1NightEventBedroomBackground);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }
    }
}
#endif
