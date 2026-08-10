#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using JHTGJ.Story;
using JHTGJ.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace JHTGJ.EditorTools
{
    public static class GameUIFontUpdater
    {
        const string FontAssetPath = GameUIFontUtility.FontAssetPath;
        const string CharacterSetPath = "Assets/Game/Art/Fonts/GameUICharacters.txt";
        const string SourceTtfPath = "Assets/Art/Fonts/SOURCE HAN SERIF SC HEAVY (TRUETYPE).TTF";
        const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        const string StorySchedulePath = DayStoryScheduleCreator.DefaultAssetPath;

        static readonly string[] UiCanvasNames =
        {
            "DialogueCanvas",
            "RoomSelectionCanvas",
            "PauseMenuCanvas",
            "StoryChoiceCanvas"
        };

        [MenuItem("JHTGJ/Regenerate Game UI Font (Fix Chinese Text)")]
        public static void RegenerateFromMenu()
        {
            StoryCharacterSetBuilder.RebuildInternal();
            var result = RegenerateInternal();
            if (!result.success)
            {
                EditorUtility.DisplayDialog("字体修复失败", result.message, "OK");
                return;
            }

            ApplyFontToAllGameUi();
            EditorUtility.DisplayDialog("字体修复完成", result.message + "\n\n已刷新全部游戏 UI 文字。", "OK");
        }

        public static (bool success, string message) RegenerateWithoutRebuild() => RegenerateInternal();

        public static void MergeExtraAndRegenerateFontBatch()
        {
            var mergeResult = StoryCharacterSetBuilder.MergeExtraCharacterFile();
            if (!mergeResult.success)
            {
                Debug.LogError($"[JHTGJ] {mergeResult.message}");
                EditorApplication.Exit(1);
                return;
            }

            RegenerateFromCommandLineInternal();
        }

        public static void RegenerateFromCommandLine()
        {
            StoryCharacterSetBuilder.RebuildInternal();
            RegenerateFromCommandLineInternal();
        }

        static void RegenerateFromCommandLineInternal()
        {
            AssetDatabase.Refresh();
            var fontResult = RegenerateInternal();
            if (!fontResult.success)
            {
                Debug.LogError($"[JHTGJ] {fontResult.message}");
                EditorApplication.Exit(1);
                return;
            }

            ApplyFontToAllGameUi();
            Debug.Log($"[JHTGJ] {fontResult.message}");
            EditorApplication.Exit(0);
        }

        [MenuItem("JHTGJ/Update Game UI Font (Add Missing Characters)")]
        public static void UpdateFromMenu() => RegenerateFromMenu();

        [MenuItem("JHTGJ/Apply Game Font To All UI")]
        public static void ApplyFontToAllGameUi()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (font == null)
            {
                Debug.LogWarning("[JHTGJ] syht 字体未找到，无法应用到 UI。");
                return;
            }

            AttachFallbackFont(font);
            EditorUtility.SetDirty(font);

            var count = 0;
            foreach (var canvasName in UiCanvasNames)
            {
                var canvas = GameObject.Find(canvasName);
                if (canvas == null)
                    continue;

                foreach (var label in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    label.font = font;
                    label.ForceMeshUpdate(true);
                    EditorUtility.SetDirty(label);
                    count++;
                }
            }

            Debug.Log($"[JHTGJ] 已将 syht 字体应用到 {count} 个 TextMeshPro 组件。");
        }

        [MenuItem("JHTGJ/Apply Game Font To Room Selection UI")]
        public static void ApplyFontToRoomSelectionUi() => ApplyFontToAllGameUi();

        public static void UpdateSilently()
        {
            var result = RegenerateInternal();
            if (result.success)
            {
                ApplyFontToAllGameUi();
                Debug.Log($"[JHTGJ] {result.message}");
            }
            else
            {
                Debug.LogWarning($"[JHTGJ] {result.message}");
            }
        }

        static (bool success, string message) RegenerateInternal()
        {
            if (!File.Exists(CharacterSetPath))
                return (false, $"找不到字符表：\n{CharacterSetPath}");

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceTtfPath);
            if (sourceFont == null)
                return (false, $"找不到源字体 TTF：\n{SourceTtfPath}");

            var characters = LoadUniqueCharacters();
            if (string.IsNullOrEmpty(characters))
                return (false, "字符表为空。");

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            var pointSize = existing != null && existing.faceInfo.pointSize > 0 ? existing.faceInfo.pointSize : 42;
            var padding = existing != null && existing.atlasPadding > 0 ? existing.atlasPadding : 6;
            var renderMode = existing != null && existing.atlasRenderMode != 0
                ? existing.atlasRenderMode
                : GlyphRenderMode.SDFAA;

            const int atlasSize = 4096;
            var rebuilt = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                pointSize,
                padding,
                renderMode,
                atlasSize,
                atlasSize,
                AtlasPopulationMode.Dynamic,
                true);

            if (rebuilt == null)
                return (false, "CreateFontAsset 失败。");

            AttachFallbackFont(rebuilt);

            // Source Han only covers CJK; ASCII/symbols ($, ^, ×, ω…) must use fallback.
            var primaryCharacters = FilterForSourceHan(characters);
            if (string.IsNullOrEmpty(primaryCharacters))
                return (false, "字符表中没有可写入思源宋体的汉字。");

            if (!rebuilt.TryAddCharacters(primaryCharacters, out var missing))
            {
                if (!string.IsNullOrEmpty(missing))
                    return (false, "以下汉字/中文标点无法写入 syht：\n" + missing);

                return (false, "写入汉字失败，请检查 TTF 是否包含所需字形。");
            }

            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[JHTGJ] 部分汉字未能写入 syht atlas：{missing}");

            var bakedCount = rebuilt.characterTable != null ? rebuilt.characterTable.Count : 0;
            if (bakedCount < primaryCharacters.Length)
            {
                Debug.LogWarning(
                    $"[JHTGJ] syht 写入不完整：请求 {primaryCharacters.Length}，实际 {bakedCount}。" +
                    " ASCII 与符号将走 LiberationSans fallback。");
            }

            var fallbackOnlyCount = 0;
            foreach (var c in characters)
            {
                if (!ShouldBakeInSourceHan(c))
                    fallbackOnlyCount++;
            }

            if (fallbackOnlyCount > 0)
            {
                Debug.Log(
                    $"[JHTGJ] {fallbackOnlyCount} 个 ASCII/符号字符改由 fallback 字体渲染：" +
                    "LiberationSans SDF");
            }

            if (!SaveFontAssetReplacing(FontAssetPath, rebuilt))
                return (false, "保存 syht.asset 失败。");

            var savedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (savedFont != null)
            {
                savedFont.ReadFontAssetDefinition();
                TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, savedFont);
            }

            return (true,
                $"已重建 syht 字体：{bakedCount} 个汉字/中文标点 + fallback 覆盖 {fallbackOnlyCount} 个 ASCII/符号。");
        }

        static void AttachFallbackFont(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
                return;

            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
            fontAsset.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            fontAsset.fallbackFontAssetTable.Clear();

            if (fallback != null)
                fontAsset.fallbackFontAssetTable.Add(fallback);
            else
                Debug.LogWarning($"[JHTGJ] 未找到 fallback 字体：{FallbackFontPath}");
        }

        static string FilterForSourceHan(string characters)
        {
            var builder = new StringBuilder(characters.Length);
            foreach (var c in characters)
            {
                if (ShouldBakeInSourceHan(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }

        static bool ShouldBakeInSourceHan(char c)
        {
            var code = (int)c;
            if (code >= 0x4E00 && code <= 0x9FFF)
                return true;

            if (code >= 0x3400 && code <= 0x4DBF)
                return true;

            // CJK Symbols and Punctuation (、。 「」 etc.)
            if (code >= 0x3000 && code <= 0x303F)
                return true;

            // Halfwidth and Fullwidth Forms (，！？ etc.)
            if (code >= 0xFF00 && code <= 0xFFEF)
                return true;

            // General punctuation used in dialogue (" " … —)
            if (code >= 0x2010 && code <= 0x2027)
                return true;

            return false;
        }

        static bool SaveFontAssetReplacing(string assetPath, TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
                return false;

            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);

            fontAsset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            if (fontAsset.atlasTextures != null)
            {
                for (var i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    var texture = fontAsset.atlasTextures[i];
                    if (texture == null)
                        continue;

                    texture.name = i == 0 ? fontAsset.name + " Atlas" : fontAsset.name + " Atlas " + i;
                    if (AssetDatabase.GetAssetPath(texture) != assetPath)
                        AssetDatabase.AddObjectToAsset(texture, fontAsset);
                }
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Atlas Material";
                if (AssetDatabase.GetAssetPath(fontAsset.material) != assetPath)
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        static string LoadUniqueCharacters()
        {
            if (!File.Exists(CharacterSetPath))
                StoryCharacterSetBuilder.RebuildInternal();

            var raw = File.ReadAllText(CharacterSetPath);
            var builder = new StringBuilder();
            var seen = new HashSet<char>();

            foreach (var c in raw)
            {
                if (char.IsWhiteSpace(c) || seen.Contains(c))
                    continue;

                seen.Add(c);
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
#endif
