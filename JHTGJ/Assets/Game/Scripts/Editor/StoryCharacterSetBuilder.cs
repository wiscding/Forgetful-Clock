#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class StoryCharacterSetBuilder
    {
        const string CharacterSetPath = "Assets/Game/Art/Fonts/GameUICharacters.txt";
        const string ExtraCharacterSourcePath = "Assets/Game/Art/Fonts/汉字.txt";
        const string StorySourcePath = "Assets/Game/Art/Fonts/_StoryTextSource.txt";
        const string StorySchedulePath = DayStoryScheduleCreator.DefaultAssetPath;
        // Curly quotes + ω used in story text.
        const string ExtraSymbols =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz " +
            "x×▼，。！？：；、…—「」《》（）～·—~@#&*+=/\\|<>[]{}_^%$().:" +
            "\u201C\u201D\u2018\u2019\u03C9";

        [MenuItem("JHTGJ/Rebuild Game UI Character Set")]
        public static void RebuildFromMenu()
        {
            var count = RebuildInternal();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "字符表已更新",
                $"共 {count} 个字符已写入：\n{CharacterSetPath}",
                "OK");
        }

        [MenuItem("JHTGJ/Merge 汉字.txt Into Character Set & Regenerate Font")]
        public static void MergeExtraAndRegenerateFontFromMenu()
        {
            var mergeResult = MergeExtraCharacterFile();
            if (!mergeResult.success)
            {
                EditorUtility.DisplayDialog("合并失败", mergeResult.message, "OK");
                return;
            }

            AssetDatabase.Refresh();
            var fontResult = GameUIFontUpdater.RegenerateWithoutRebuild();
            if (!fontResult.success)
            {
                EditorUtility.DisplayDialog("字体生成失败", fontResult.message, "OK");
                return;
            }

            GameUIFontUpdater.ApplyFontToAllGameUi();
            EditorUtility.DisplayDialog(
                "完成",
                $"{mergeResult.message}\n{fontResult.message}\n\n已刷新全部游戏 UI 文字。",
                "OK");
        }

        public static int RebuildInternal()
        {
            var raw = new StringBuilder();
            raw.Append(CollectFromFile(StorySourcePath));
            raw.Append(CollectFromFile(ExtraCharacterSourcePath));
            raw.Append(CollectFromSchedule());
            raw.Append(CollectFromGameAssets());

            var unique = ExtractUniqueCharacters(raw.ToString());
            var directory = Path.GetDirectoryName(CharacterSetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(CharacterSetPath, unique + "\n", new UTF8Encoding(false));
            Debug.Log($"[JHTGJ] 字符表已重建：{unique.Length} 字 → {CharacterSetPath}");
            return unique.Length;
        }

        public static (bool success, string message, int totalCount, int addedCount) MergeExtraCharacterFile()
        {
            var extraPath = Path.GetFullPath(ExtraCharacterSourcePath);
            if (!File.Exists(extraPath))
            {
                return (false,
                    $"找不到补充字符文件：\n{ExtraCharacterSourcePath}\n\n请把 汉字.txt 放到 Assets/Game/Art/Fonts/ 目录下。",
                    0,
                    0);
            }

            var existingPath = Path.GetFullPath(CharacterSetPath);
            var existing = File.Exists(existingPath)
                ? File.ReadAllText(existingPath, Encoding.UTF8)
                : string.Empty;
            var extra = File.ReadAllText(extraPath, Encoding.UTF8);
            var before = ExtractUniqueCharacters(existing);
            var merged = ExtractUniqueCharacters(existing + extra);
            var added = merged.Length - before.Length;

            var directory = Path.GetDirectoryName(existingPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(existingPath, merged + "\n", new UTF8Encoding(false));
            Debug.Log($"[JHTGJ] 已合并 汉字.txt：新增 {added} 字，共 {merged.Length} 字 → {CharacterSetPath}");
            return (true, $"已合并 汉字.txt：新增 {added} 字，共 {merged.Length} 字。", merged.Length, added);
        }

        static string CollectFromSchedule()
        {
            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(StorySchedulePath);
            if (schedule == null)
                return string.Empty;

            var builder = new StringBuilder();
            AppendEventText(builder, schedule.EndingEvent);
            foreach (var phase in schedule.Phases)
            {
                if (phase == null)
                    continue;

                builder.Append(phase.DisplayName);
                foreach (var storyEvent in phase.Events)
                    AppendEventText(builder, storyEvent);
            }

            return builder.ToString();
        }

        static void AppendEventText(StringBuilder builder, StoryEventDefinition storyEvent)
        {
            if (storyEvent == null)
                return;

            builder.Append(storyEvent.Summary);
            builder.Append(storyEvent.ButtonLabel);
            if (storyEvent.Lines == null)
                return;

            foreach (var line in storyEvent.Lines)
            {
                if (line == null)
                    continue;

                builder.Append(line.SpeakerName);
                builder.Append(line.Text);
            }
        }

        static string CollectFromGameAssets()
        {
            var builder = new StringBuilder();
            var root = Path.Combine(Application.dataPath, "Game");
            if (!Directory.Exists(root))
                return string.Empty;

            foreach (var path in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext is not (".cs" or ".asset" or ".unity" or ".txt"))
                    continue;

                if (path.Contains("_StoryTextSource.txt") ||
                    path.Contains("GameUICharacters.txt") ||
                    path.Contains("汉字.txt"))
                    continue;

                try
                {
                    builder.Append(File.ReadAllText(path, Encoding.UTF8));
                }
                catch
                {
                }
            }

            return builder.ToString();
        }

        static string CollectFromFile(string assetPath)
        {
            var fullPath = Path.GetFullPath(assetPath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        }

        static string ExtractUniqueCharacters(string raw)
        {
            var seen = new SortedDictionary<int, char>();

            foreach (var c in raw)
            {
                if (char.IsWhiteSpace(c))
                    continue;

                if (IsIncludedCharacter(c))
                    seen[(int)c] = c;
            }

            foreach (var c in ExtraSymbols)
            {
                if (!char.IsWhiteSpace(c))
                    seen[(int)c] = c;
            }

            var builder = new StringBuilder(seen.Count);
            foreach (var pair in seen)
                builder.Append(pair.Value);

            return builder.ToString();
        }

        static bool IsIncludedCharacter(char c)
        {
            if (c >= 0x4E00 && c <= 0x9FFF)
                return true;

            if (c >= 0x3400 && c <= 0x4DBF)
                return true;

            if (ExtraSymbols.IndexOf(c) >= 0)
                return true;

            return false;
        }
    }
}
#endif
