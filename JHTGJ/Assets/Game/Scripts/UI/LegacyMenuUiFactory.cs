using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public static class LegacyMenuUiFactory
    {
        static readonly Color DropdownBackground = new Color(0.792f, 0.612f, 0.239f, 1f);
        static readonly Color ItemBackground = new Color(0.925f, 0.701f, 0.201f, 1f);
        static readonly Color ItemHighlight = new Color(0.718f, 0.569f, 0.227f, 1f);
        static readonly Color ItemText = new Color(0.12f, 0.1f, 0.08f, 1f);

        public static bool IsTextMeshProDropdown(GameObject go)
        {
            if (go == null)
                return false;

            foreach (var component in go.GetComponents<Component>())
            {
                if (component != null && component.GetType().Name == "TMP_Dropdown")
                    return true;
            }

            return false;
        }

        public static bool TryGetLegacyDropdown(GameObject go, out Dropdown dropdown)
        {
            dropdown = null;
            if (go == null || IsTextMeshProDropdown(go))
                return false;

            return go.TryGetComponent(out dropdown);
        }

        public static Text CreateLabel(Transform parent, string name, string text, Vector2 anchoredPos, int fontSize = 28)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 50f);
            rect.anchoredPosition = anchoredPos;
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            GameUIFontUtility.ConfigureDialogueLabel(label, multiline: false);
            return label;
        }

        public static void CreateTitle(Transform parent, string text)
        {
            CreateLabel(parent, "Title", text, new Vector2(0f, 180f), 48);
        }

        public static Button CreateMenuButton(Transform parent, string name, string text, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 48f);
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            button.colors = colors;

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.text = text;
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            GameUIFontUtility.ConfigureButtonLabel(label);

            go.AddComponent<UISfxButton>();
            return button;
        }

        public static Dropdown CreateResolutionDropdown(Transform parent, string name, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 44f);
            rect.anchoredPosition = anchoredPos;

            go.AddComponent<Image>().color = DropdownBackground;
            var dropdown = go.AddComponent<Dropdown>();
            var colors = dropdown.colors;
            colors.normalColor = DropdownBackground;
            colors.highlightedColor = ItemHighlight;
            colors.pressedColor = ItemHighlight;
            colors.selectedColor = DropdownBackground;
            dropdown.colors = colors;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 2f);
            labelRect.offsetMax = new Vector2(-32f, -2f);
            var label = labelGo.AddComponent<Text>();
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = ItemText;
            GameUIFontUtility.ConfigureButtonLabel(label);

            var arrowGo = new GameObject("Arrow");
            arrowGo.transform.SetParent(go.transform, false);
            var arrowRect = arrowGo.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(24f, 24f);
            arrowRect.anchoredPosition = new Vector2(-16f, 0f);
            var arrow = arrowGo.AddComponent<Text>();
            arrow.text = "▼";
            arrow.fontSize = 18;
            arrow.alignment = TextAnchor.MiddleCenter;
            arrow.color = ItemText;
            GameUIFontUtility.ConfigureButtonLabel(arrow);

            var template = new GameObject("Template");
            template.transform.SetParent(go.transform, false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, 260f);
            templateRect.anchoredPosition = new Vector2(0f, 2f);
            template.AddComponent<Image>().color = DropdownBackground;
            var scroll = template.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = DropdownBackground;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 36f);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 36f);
            var itemLayout = item.AddComponent<LayoutElement>();
            itemLayout.minHeight = 36f;
            itemLayout.preferredHeight = 36f;

            var itemBackground = item.AddComponent<Image>();
            itemBackground.color = ItemBackground;
            var toggle = item.AddComponent<Toggle>();
            toggle.targetGraphic = itemBackground;
            var toggleColors = toggle.colors;
            toggleColors.normalColor = ItemBackground;
            toggleColors.highlightedColor = ItemHighlight;
            toggleColors.pressedColor = ItemHighlight;
            toggleColors.selectedColor = ItemHighlight;
            toggle.colors = toggleColors;

            var itemLabelGo = new GameObject("Item Label");
            itemLabelGo.transform.SetParent(item.transform, false);
            var itemLabelRect = itemLabelGo.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(12f, 0f);
            itemLabelRect.offsetMax = new Vector2(-12f, 0f);
            var itemLabel = itemLabelGo.AddComponent<Text>();
            itemLabel.fontSize = 20;
            itemLabel.alignment = TextAnchor.MiddleLeft;
            itemLabel.color = ItemText;
            GameUIFontUtility.ConfigureButtonLabel(itemLabel);

            dropdown.captionText = label;
            dropdown.itemText = itemLabel;
            dropdown.template = templateRect;
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            template.SetActive(false);
            return dropdown;
        }
    }
}
