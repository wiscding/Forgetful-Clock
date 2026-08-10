#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class Day4DuskEventLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/Day4DuskEventLibrary.asset";

        [MenuItem("JHTGJ/Populate Day 4 Dusk Event Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Day 4 dusk event library populated.");
        }

        public static Day4DuskEventLibrary EnsureAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Resources");
            }

            var library = AssetDatabase.LoadAssetAtPath<Day4DuskEventLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<Day4DuskEventLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("rooftopBackground").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(SpecialWifeInteractPaths.Day4DuskRooftopBackground);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }
    }
}
#endif
