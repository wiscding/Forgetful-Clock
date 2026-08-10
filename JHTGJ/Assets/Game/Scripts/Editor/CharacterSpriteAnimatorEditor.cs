#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JHTGJ.Character;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class CharacterSpriteAnimatorEditor
    {
        [MenuItem("JHTGJ/Character/Fill Walk Frames From Selected Spritesheet")]
        public static void FillWalkFramesFromSelection()
        {
            if (!TryGetSpritesFromSelection(out var sprites))
                return;

            var animator = Object.FindObjectOfType<CharacterSpriteAnimator>();
            if (animator == null)
            {
                EditorUtility.DisplayDialog("填充行走帧", "场景里找不到带 CharacterSpriteAnimator 的角色（Protagonist）。", "OK");
                return;
            }

            Undo.RecordObject(animator, "Fill Walk Frames");
            animator.SetWalkFrames(sprites);
            EditorUtility.SetDirty(animator);
            Debug.Log($"[JHTGJ] Assigned {sprites.Length} walk frames to {animator.name}.");
        }

        [MenuItem("JHTGJ/Character/Fill Profile Walk Frames From Selected Spritesheet")]
        public static void FillProfileWalkFramesFromSelection()
        {
            var profile = Selection.objects
                .OfType<CharacterAppearanceProfile>()
                .FirstOrDefault();

            if (!TryGetSpritesFromSelection(out var sprites))
                return;

            if (profile == null)
            {
                EditorUtility.DisplayDialog(
                    "填充外观配置",
                    "请同时选中 Character Appearance Profile 和已切片的行走图。",
                    "OK");
                return;
            }

            if (!TryPromptLightingType(out var lighting))
                return;

            Undo.RecordObject(profile, "Fill Profile Walk Frames");
            var so = new SerializedObject(profile);
            ApplyWalkFramesToProfile(so, lighting, sprites);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            Debug.Log($"[JHTGJ] Assigned {sprites.Length} walk frames to {profile.name} ({lighting}).");
        }

        static bool TryPromptLightingType(out CharacterLightingType lighting)
        {
            lighting = CharacterLightingType.Default;
            var picked = EditorUtility.DisplayDialogComplex(
                "选择打光类型",
                "把选中的行走图填进哪一套打光？",
                "Default",
                "Cancel",
                "Left Side");

            switch (picked)
            {
                case 0:
                    lighting = CharacterLightingType.Default;
                    return true;
                case 1:
                    return false;
                case 2:
                    lighting = CharacterLightingType.LeftSide;
                    return true;
            }

            picked = EditorUtility.DisplayDialogComplex(
                "选择打光类型",
                "继续选择打光类型。",
                "Right Side",
                "Cancel",
                "Top");

            switch (picked)
            {
                case 0:
                    lighting = CharacterLightingType.RightSide;
                    return true;
                case 2:
                    lighting = CharacterLightingType.Top;
                    return true;
                default:
                    return false;
            }
        }

        static void ApplyWalkFramesToProfile(SerializedObject so, CharacterLightingType lighting, Sprite[] sprites)
        {
            SerializedProperty framesProp;

            if (lighting == CharacterLightingType.Default)
            {
                framesProp = so.FindProperty("defaultAppearance.walkFrames");
            }
            else
            {
                var entriesProp = so.FindProperty("lightingAppearances");
                var index = FindLightingEntryIndex(entriesProp, lighting);
                if (index < 0)
                {
                    entriesProp.arraySize++;
                    index = entriesProp.arraySize - 1;
                    entriesProp.GetArrayElementAtIndex(index)
                        .FindPropertyRelative("lighting").enumValueIndex = (int)lighting;
                }

                framesProp = entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("walkFrames");
            }

            framesProp.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++)
                framesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        static int FindLightingEntryIndex(SerializedProperty entriesProp, CharacterLightingType lighting)
        {
            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("lighting").enumValueIndex == (int)lighting)
                    return i;
            }

            return -1;
        }

        static bool TryGetSpritesFromSelection(out Sprite[] sprites)
        {
            sprites = System.Array.Empty<Sprite>();

            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                var found = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Sprite>()
                    .OrderBy(s => s.name, NaturalSpriteNameComparer.Instance)
                    .ToArray();

                if (found.Length > 0)
                {
                    sprites = found;
                    return true;
                }
            }

            EditorUtility.DisplayDialog(
                "填充行走帧",
                "选中项里没有已切片的 Sprite。\n请先把行走图设为 Sprite (Multiple) 并在 Sprite Editor 里 Slice。",
                "OK");
            return false;
        }

        sealed class NaturalSpriteNameComparer : IComparer<string>
        {
            public static readonly NaturalSpriteNameComparer Instance = new NaturalSpriteNameComparer();

            static readonly Regex TrailingNumber = new Regex(@"(\d+)$", RegexOptions.Compiled);

            public int Compare(string a, string b)
            {
                if (a == b)
                    return 0;
                if (a == null)
                    return -1;
                if (b == null)
                    return 1;

                var matchA = TrailingNumber.Match(a);
                var matchB = TrailingNumber.Match(b);

                if (matchA.Success && matchB.Success &&
                    a.Substring(0, matchA.Index) == b.Substring(0, matchB.Index))
                {
                    var numA = int.Parse(matchA.Groups[1].Value);
                    var numB = int.Parse(matchB.Groups[1].Value);
                    return numA.CompareTo(numB);
                }

                return string.Compare(a, b, System.StringComparison.Ordinal);
            }
        }
    }
}
#endif
