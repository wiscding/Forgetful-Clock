using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        public const string DefaultPrompt = "按 E 互动";
        public static readonly Color BackgroundColor = new Color(1f, 0.88f, 0.15f, 0.95f);
        public static readonly Color TextColor = new Color(0.12f, 0.08f, 0f, 1f);

        [SerializeField] RectTransform root;
        [SerializeField] Text promptLabel;
        [SerializeField] Vector2 screenOffset = new Vector2(0f, 24f);

        Transform anchorTransform;
        Vector3 anchorWorldOffset;
        Vector3 fixedWorldPosition;
        bool useFixedWorldPosition;
        Camera worldCamera;

        public bool IsVisible => root != null && root.gameObject.activeSelf;

        public void Setup(RectTransform rootRect, Text label)
        {
            root = rootRect;
            promptLabel = label;
            worldCamera = Camera.main;
            ApplyVisualStyle();
            Hide();
        }

        void Awake()
        {
            ApplyVisualStyle();
        }

        void ApplyVisualStyle()
        {
            if (root != null && root.TryGetComponent<Image>(out var background))
                background.color = BackgroundColor;

            if (promptLabel != null)
                promptLabel.color = TextColor;
        }

        public void ShowAt(Transform anchor, Vector3 worldOffset, string text = DefaultPrompt)
        {
            anchorTransform = anchor;
            anchorWorldOffset = worldOffset;
            useFixedWorldPosition = false;
            ShowInternal(text);
        }

        public void ShowAtWorldPosition(Vector3 worldPosition, string text = DefaultPrompt)
        {
            anchorTransform = null;
            fixedWorldPosition = worldPosition;
            useFixedWorldPosition = true;
            ShowInternal(text);
        }

        void ShowInternal(string text)
        {
            if (promptLabel != null)
                promptLabel.text = string.IsNullOrWhiteSpace(text) ? DefaultPrompt : text;

            if (root != null)
                root.gameObject.SetActive(true);

            UpdateScreenPosition();
        }

        public void Hide()
        {
            anchorTransform = null;
            useFixedWorldPosition = false;

            if (root != null)
                root.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!IsVisible)
                return;

            UpdateScreenPosition();
        }

        void UpdateScreenPosition()
        {
            if (root == null)
                return;

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return;

            var worldPosition = useFixedWorldPosition
                ? fixedWorldPosition
                : anchorTransform != null
                    ? anchorTransform.position + anchorWorldOffset
                    : fixedWorldPosition;

            var screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
            root.position = new Vector3(
                screenPosition.x + screenOffset.x,
                screenPosition.y + screenOffset.y,
                0f);
        }
    }
}
