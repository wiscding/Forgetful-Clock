using JHTGJ.Character;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEngine;

namespace JHTGJ.Interaction
{
    [ExecuteAlways]
    public class StoryCharacterInteractPoint : MonoBehaviour
    {
        const string VisualChildName = "CharacterVisual";

        [SerializeField] string interactId = "Interact_Partner";
        [SerializeField] string label = "妻子";
        [SerializeField] InteractionKind kind = InteractionKind.TalkToPartner;
        [SerializeField] float standXOffset = -2.5f;
        [SerializeField] CharacterAppearanceProfile defaultAppearanceProfile;
        [SerializeField] Sprite defaultIdleSprite;
        [SerializeField] FacingDirection facing = FacingDirection.Left;
        [SerializeField] bool spriteFacesRightByDefault;
        [SerializeField] int sortingOrder = 8;
        [SerializeField] bool autoFitHeight = true;
        [SerializeField] float targetWorldHeight = 7.5f;

        SpriteRenderer characterRenderer;
        Room currentRoom;
        float defaultStandXOffset;

        public string InteractId => interactId;
        public string Label => label;
        public InteractionKind Kind => kind;
        public float StandX => transform.position.x + standXOffset;
        public bool IsPresenceActive => gameObject.activeSelf;

        void Awake()
        {
            defaultStandXOffset = standXOffset;
            EnsureVisual();
        }

        void OnValidate() => EnsureVisual();

        public void SetPresenceActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void ApplyEmbeddedBackgroundInteract(Room room, float localX, float standOffset, FacingDirection wifeFacing)
        {
            if (room == null)
                return;

            currentRoom = room;
            transform.SetParent(room.transform, false);
            transform.localPosition = new Vector3(localX, room.FloorY, 0f);
            standXOffset = standOffset;
            ApplyFacing(wifeFacing);

            EnsureVisual();
            if (characterRenderer != null)
            {
                characterRenderer.sprite = null;
                characterRenderer.enabled = false;
            }
        }

        public void ApplyPresence(StoryPhaseCharacterPresence presence, Room room)
        {
            if (presence == null || room == null)
                return;

            standXOffset = defaultStandXOffset;
            currentRoom = room;
            transform.SetParent(room.transform, false);

            if (presence.UseScenePosition)
            {
                var position = transform.localPosition;
                position.y = room.FloorY;
                transform.localPosition = position;
            }
            else
            {
                transform.localPosition = new Vector3(presence.LocalX, room.FloorY, 0f);
            }

            var profile = presence.AppearanceProfile != null
                ? presence.AppearanceProfile
                : defaultAppearanceProfile;
            var idle = presence.IdleSprite != null
                ? presence.IdleSprite
                : defaultIdleSprite;

            ApplyAppearance(profile, idle, room);
            ApplyFacing(presence.Facing);

            EnsureVisual();
            if (characterRenderer != null)
                characterRenderer.enabled = true;

            if (characterRenderer != null && characterRenderer.sprite == null)
            {
                Debug.LogWarning(
                    $"[Story] {label}（{interactId}）缺少 Idle Sprite，请在 StoryCharacter 或 Character Presences 中指定立绘。");
            }
        }

        public void ApplyAppearance(CharacterAppearanceProfile profile, Sprite idleSprite, Room room)
        {
            EnsureVisual();
            currentRoom = room;

            Sprite sprite = idleSprite;
            if (profile != null &&
                profile.TryGetAppearance(
                    room != null ? room.CharacterLighting : CharacterLightingType.Default,
                    out var profileIdle,
                    out _))
            {
                sprite = profileIdle != null ? profileIdle : sprite;
            }

            if (sprite != null)
                characterRenderer.sprite = sprite;

            characterRenderer.color = Color.white;
            characterRenderer.sortingOrder = sortingOrder;
            RefreshDisplayScale();
            SnapFeetToFloor();
        }

        public void ApplyFacing(FacingDirection direction)
        {
            facing = direction;
            EnsureVisual();

            if (characterRenderer == null)
                return;

            characterRenderer.flipX = spriteFacesRightByDefault
                ? direction == FacingDirection.Left
                : direction == FacingDirection.Right;
        }

        public void SnapFeetToFloor()
        {
            if (currentRoom == null || characterRenderer == null || characterRenderer.sprite == null)
                return;

            var delta = currentRoom.FloorY - characterRenderer.bounds.min.y;
            if (Mathf.Abs(delta) <= 0.0001f)
                return;

            var position = transform.position;
            position.y += delta;
            transform.position = position;
        }

        void RefreshDisplayScale()
        {
            if (!autoFitHeight || characterRenderer == null || characterRenderer.sprite == null)
                return;

            transform.localScale = Vector3.one;

            var spriteHeight = characterRenderer.sprite.bounds.size.y;
            if (spriteHeight <= 0f)
                return;

            var uniform = targetWorldHeight / spriteHeight;
            transform.localScale = new Vector3(uniform, uniform, 1f);
        }

        void EnsureVisual()
        {
            var visual = transform.Find(VisualChildName);
            if (visual == null)
            {
                var go = new GameObject(VisualChildName);
                go.transform.SetParent(transform, false);
                visual = go.transform;
            }

            characterRenderer = visual.GetComponent<SpriteRenderer>();
            if (characterRenderer == null)
                characterRenderer = visual.gameObject.AddComponent<SpriteRenderer>();

            characterRenderer.sortingOrder = sortingOrder;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.75f, 0.85f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);

            Gizmos.color = Color.yellow;
            var stand = new Vector3(StandX, transform.position.y, 0f);
            Gizmos.DrawWireSphere(stand, 0.2f);
            Gizmos.DrawLine(transform.position, stand);
        }
    }
}
