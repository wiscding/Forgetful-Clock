using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JHTGJ.UI
{
    public static class GameUIFontUtility
    {
        public const string FontAssetPath = "Assets/Art/Fonts/syht.asset";
        public const string LegacyFontPath = "Assets/Art/Fonts/SOURCE HAN SERIF SC HEAVY (TRUETYPE).TTF";
        const string ResourcesTmpFontPath = "Fonts/syht";
        const string ResourcesLegacyFontPath = "Fonts/GameLegacyFont";

        static TMP_FontAsset cachedSyhtFont;
        static Font cachedLegacyFont;

        public static TMP_FontAsset DefaultFont
        {
            get
            {
                if (cachedSyhtFont == null)
                {
#if UNITY_EDITOR
                    cachedSyhtFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#endif
                    if (cachedSyhtFont == null)
                        cachedSyhtFont = Resources.Load<TMP_FontAsset>(ResourcesTmpFontPath);
                }

                if (cachedSyhtFont != null)
                    return cachedSyhtFont;

                if (TMP_Settings.defaultFontAsset != null)
                    return TMP_Settings.defaultFontAsset;

                return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
        }

        public static Font DefaultLegacyFont
        {
            get
            {
                if (cachedLegacyFont == null)
                {
#if UNITY_EDITOR
                    cachedLegacyFont = AssetDatabase.LoadAssetAtPath<Font>(LegacyFontPath);
#endif
                    if (cachedLegacyFont == null)
                        cachedLegacyFont = Resources.Load<Font>(ResourcesLegacyFontPath);
                }

                return cachedLegacyFont;
            }
        }

        public static void ApplyDefaultFont(TextMeshProUGUI label)
        {
            if (label == null)
                return;

            var font = DefaultFont;
            if (font != null)
                label.font = font;
        }

        public static void ApplyDefaultFont(Text label)
        {
            if (label == null)
                return;

            var font = DefaultLegacyFont;
            if (font != null)
                label.font = font;
        }

        public static void ConfigureDialogueLabel(TextMeshProUGUI label, bool multiline = true)
        {
            if (label == null)
                return;

            ApplyDefaultFont(label);
            label.enableWordWrapping = multiline;
            label.overflowMode = multiline
                ? TextOverflowModes.Overflow
                : TextOverflowModes.Ellipsis;
            label.richText = true;
        }

        public static void ConfigureDialogueLabel(Text label, bool multiline = true)
        {
            if (label == null)
                return;

            ApplyDefaultFont(label);
            label.supportRichText = true;
            label.horizontalOverflow = multiline
                ? HorizontalWrapMode.Wrap
                : HorizontalWrapMode.Overflow;
            label.verticalOverflow = multiline
                ? VerticalWrapMode.Overflow
                : VerticalWrapMode.Truncate;
            label.color = Color.white;
        }

        public static void ConfigureButtonLabel(TextMeshProUGUI label)
        {
            if (label == null)
                return;

            ApplyDefaultFont(label);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        public static void ConfigureButtonLabel(Text label)
        {
            if (label == null)
                return;

            ApplyDefaultFont(label);
            label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
        }

        public static void StretchDialogueTextRect(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.04f, 0.12f);
            rect.anchorMax = new Vector2(0.88f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
