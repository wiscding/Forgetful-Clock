using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class PauseMenuLegacyTextUtility
    {
        public static bool NeedsLegacyConversion(GameObject pauseMenuCanvas)
        {
            if (pauseMenuCanvas == null)
                return false;

            return pauseMenuCanvas.GetComponentInChildren<TextMeshProUGUI>(true) != null;
        }

        public static void ConvertCanvas(GameObject pauseMenuCanvas)
        {
            if (pauseMenuCanvas == null)
                return;

            FixCanvasScale(pauseMenuCanvas.transform);

            var tmpLabels = pauseMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            var snapshot = (TextMeshProUGUI[])tmpLabels.Clone();
            foreach (var tmp in snapshot)
                ConvertSingleLabel(tmp);

            foreach (var label in pauseMenuCanvas.GetComponentsInChildren<Text>(true))
            {
                if (label.transform.parent != null &&
                    label.transform.parent.GetComponent<Button>() != null)
                {
                    label.color = Color.black;
                    GameUIFontUtility.ConfigureButtonLabel(label);
                }
                else if (label.gameObject.name == "Title")
                {
                    label.color = Color.white;
                    GameUIFontUtility.ConfigureDialogueLabel(label, multiline: false);
                }
                else
                {
                    label.color = Color.white;
                    GameUIFontUtility.ConfigureDialogueLabel(label, multiline: false);
                }
            }
        }

        public static void FixCanvasScale(Transform canvasTransform)
        {
            if (canvasTransform == null)
                return;

            var scale = canvasTransform.localScale;
            if (scale.x == 0f || scale.y == 0f || scale.z == 0f)
                canvasTransform.localScale = Vector3.one;
        }

        static Text ConvertSingleLabel(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return null;

            var go = tmp.gameObject;
            var content = tmp.text;
            var fontSize = Mathf.RoundToInt(tmp.fontSize);
            var color = tmp.color;
            var alignment = MapAlignment(tmp.alignment);
            var richText = tmp.richText;
            var isButtonLabel = go.transform.parent != null &&
                                go.transform.parent.GetComponent<Button>() != null;

            Object.DestroyImmediate(tmp);

            var legacy = go.GetComponent<Text>();
            if (legacy == null)
                legacy = go.AddComponent<Text>();

            if (legacy == null)
                return null;

            legacy.text = content;
            legacy.fontSize = fontSize;
            legacy.color = isButtonLabel ? Color.black : color;
            legacy.alignment = alignment;
            legacy.supportRichText = richText;
            legacy.raycastTarget = false;

            if (isButtonLabel)
                GameUIFontUtility.ConfigureButtonLabel(legacy);
            else
                GameUIFontUtility.ConfigureDialogueLabel(legacy, multiline: false);

            return legacy;
        }

        static TextAnchor MapAlignment(TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.TopLeft:
                    return TextAnchor.UpperLeft;
                case TextAlignmentOptions.Top:
                    return TextAnchor.UpperCenter;
                case TextAlignmentOptions.TopRight:
                    return TextAnchor.UpperRight;
                case TextAlignmentOptions.Left:
                    return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.Center:
                    return TextAnchor.MiddleCenter;
                case TextAlignmentOptions.Right:
                    return TextAnchor.MiddleRight;
                case TextAlignmentOptions.BottomLeft:
                    return TextAnchor.LowerLeft;
                case TextAlignmentOptions.Bottom:
                    return TextAnchor.LowerCenter;
                case TextAlignmentOptions.BottomRight:
                    return TextAnchor.LowerRight;
                default:
                    return TextAnchor.MiddleCenter;
            }
        }
    }
}
