using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Character
{
    /// <summary>
    /// Side-view character with continuous horizontal movement inside the current room.
    /// </summary>
    [ExecuteAlways]
    public class SideViewCharacterController : MonoBehaviour
    {
        [SerializeField] Room room;
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] Color placeholderColor = new Color(0.85f, 0.55f, 0.45f);
        [SerializeField] int sortingOrder = 10;
        [SerializeField] bool spriteFacesRightByDefault;
        [SerializeField] bool autoFitHeight = true;
        [SerializeField] float targetWorldHeight = 3f;

        SpriteRenderer spriteRenderer;
        static Sprite sharedPlaceholderSprite;

        public bool IsMoving { get; private set; }
        public FacingDirection Facing { get; private set; } = FacingDirection.Right;
        public bool GameplayVisible { get; private set; } = true;
        public Room CurrentRoom => room;
        public float MoveSpeed => moveSpeed;

        public event System.Action<Room> RoomChanged;

        void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            EnsurePlaceholderSprite();
            ApplyGameplayVisibility();
        }

        void OnValidate()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            EnsurePlaceholderSprite();
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            EnsurePlaceholderSprite();
            ApplyGameplayVisibility();
        }

        public void SetGameplayVisible(bool visible)
        {
            GameplayVisible = visible;
            ApplyGameplayVisibility();
        }

        void ApplyGameplayVisibility()
        {
            foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
                renderer.enabled = GameplayVisible;
        }

        void Start()
        {
            if (room != null)
            {
                transform.position = room.ClampPosition(transform.position);
                SnapFeetToFloor();
            }
        }

        public void SetRoom(Room targetRoom, SpawnSide spawnSide = SpawnSide.Left)
        {
            room = targetRoom;
            IsMoving = false;
            if (room == null)
                return;

            transform.position = room.GetSpawnPosition(spawnSide);
            SetFacing(spawnSide == SpawnSide.Left ? FacingDirection.Right : FacingDirection.Left);
            SnapFeetToFloor();
            RoomChanged?.Invoke(room);
        }

        public void MoveHorizontal(float axis)
        {
            if (room == null || axis == 0f)
            {
                IsMoving = false;
                return;
            }

            IsMoving = true;
            SetFacing(axis > 0f ? FacingDirection.Right : FacingDirection.Left);

            var position = transform.position;
            position.x += axis * moveSpeed * Time.deltaTime;
            position.x = Mathf.Clamp(position.x, room.WalkLeft, room.WalkRight);
            transform.position = position;
        }

        public void SnapFeetToFloor()
        {
            if (room == null || spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            var position = transform.position;
            position.y += room.FloorY - spriteRenderer.bounds.min.y;
            transform.position = position;
        }

        void SetFacing(FacingDirection direction)
        {
            Facing = direction;
            if (spriteRenderer == null)
                return;

            spriteRenderer.flipX = spriteFacesRightByDefault
                ? direction == FacingDirection.Left
                : direction == FacingDirection.Right;
        }

        public void RefreshDisplayScale()
        {
            if (!autoFitHeight || spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            var spriteHeight = spriteRenderer.sprite.bounds.size.y;
            if (spriteHeight <= 0f)
                return;

            var uniform = targetWorldHeight / spriteHeight;
            transform.localScale = new Vector3(uniform, uniform, 1f);
        }

        void EnsurePlaceholderSprite()
        {
            if (spriteRenderer == null)
                return;

            spriteRenderer.sortingOrder = sortingOrder;

            if (GetComponent<CharacterSpriteAnimator>() != null)
                return;

            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = GetSharedPlaceholderSprite();

            spriteRenderer.color = placeholderColor;
        }

        static Sprite GetSharedPlaceholderSprite()
        {
            if (sharedPlaceholderSprite != null)
                return sharedPlaceholderSprite;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            sharedPlaceholderSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0f),
                1f);

            return sharedPlaceholderSprite;
        }

#if UNITY_EDITOR
        public void EnsureVisibleInEditor()
        {
            EnsurePlaceholderSprite();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
