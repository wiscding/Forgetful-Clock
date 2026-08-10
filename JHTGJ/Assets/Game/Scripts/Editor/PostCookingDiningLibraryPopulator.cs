#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class PostCookingDiningLibraryPopulator
    {
        public const string AssetPath = "Assets/Game/Resources/PostCookingDiningLibrary.asset";

        [MenuItem("JHTGJ/Populate Post Cooking Dining Library")]
        public static void PopulateFromMenu()
        {
            EnsureAsset();
            Debug.Log("[JHTGJ] Post cooking dining library populated.");
        }

        public static PostCookingDiningLibrary EnsureAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game"))
                    AssetDatabase.CreateFolder("Assets", "Game");
                AssetDatabase.CreateFolder("Assets/Game", "Resources");
            }

            var library = AssetDatabase.LoadAssetAtPath<PostCookingDiningLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<PostCookingDiningLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            var so = new SerializedObject(library);
            so.FindProperty("diningRoomBackground").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(SpecialWifeInteractPaths.DaytimeDiningBackground);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }
    }
}
#endif
