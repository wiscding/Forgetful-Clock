using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Character
{
    [RequireComponent(typeof(SideViewCharacterController))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSpriteAnimator : MonoBehaviour
    {
        [SerializeField] CharacterAppearanceProfile appearanceProfile;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite[] walkFrames = System.Array.Empty<Sprite>();
        [SerializeField] float walkFrameRate = 10f;
        [SerializeField] bool syncWalkAnimationToSpeed;
        [SerializeField] float walkCycleDistance = 1.2f;

        SideViewCharacterController movement;
        SpriteRenderer spriteRenderer;
        CharacterLightingType currentLighting = (CharacterLightingType)(-1);
        bool wasMoving;
        float frameTimer;
        int frameIndex;

        void Awake()
        {
            movement = GetComponent<SideViewCharacterController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (movement != null)
                movement.RoomChanged += HandleRoomChanged;
        }

        void Start()
        {
            HandleRoomChanged(movement != null ? movement.CurrentRoom : null);
            ApplyIdle();
        }

        void OnDisable()
        {
            if (movement != null)
                movement.RoomChanged -= HandleRoomChanged;
        }

        void LateUpdate()
        {
            if (movement == null || spriteRenderer == null)
                return;

            var moving = movement.IsMoving && walkFrames.Length > 0;
            if (moving)
                PlayWalk();
            else if (wasMoving)
                ApplyIdle();

            wasMoving = moving;
        }

        void HandleRoomChanged(Room room)
        {
            var lighting = room != null
                ? room.CharacterLighting
                : CharacterLightingType.Default;

            if (lighting == currentLighting)
                return;

            currentLighting = lighting;

            if (appearanceProfile != null &&
                appearanceProfile.TryGetAppearance(lighting, out var idle, out var frames))
            {
                idleSprite = idle;
                walkFrames = WalkFrameUtility.PrepareWalkFrames(frames, idle);
            }

            frameTimer = 0f;
            frameIndex = 0;
            ApplyIdle();
        }

        void PlayWalk()
        {
            var frameDuration = 1f / Mathf.Max(GetEffectiveFrameRate(), 1f);
            frameTimer += Time.deltaTime;

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                if (frameTimer > frameDuration)
                    frameTimer = frameDuration;

                frameIndex = (frameIndex + 1) % walkFrames.Length;
            }

            ApplySprite(walkFrames[frameIndex]);
        }

        void ApplyIdle()
        {
            frameTimer = 0f;
            frameIndex = 0;

            if (idleSprite != null)
                ApplySprite(idleSprite);
        }

        void ApplySprite(Sprite sprite)
        {
            if (spriteRenderer == null || sprite == null)
                return;

            if (spriteRenderer.sprite == sprite)
                return;

            spriteRenderer.sprite = sprite;
            movement?.RefreshDisplayScale();
            movement?.SnapFeetToFloor();
        }

        float GetEffectiveFrameRate()
        {
            if (!syncWalkAnimationToSpeed || walkFrames.Length == 0 || movement == null)
                return walkFrameRate;

            var speed = movement.MoveSpeed;
            if (speed <= 0f || walkCycleDistance <= 0f)
                return walkFrameRate;

            return walkFrames.Length * speed / walkCycleDistance;
        }

        public void SetAppearanceProfile(CharacterAppearanceProfile profile)
        {
            appearanceProfile = profile;
            currentLighting = (CharacterLightingType)(-1);
            HandleRoomChanged(movement != null ? movement.CurrentRoom : null);
        }

        public void SetIdleSprite(Sprite sprite) => idleSprite = sprite;

        public void SetWalkFrames(Sprite[] frames) =>
            walkFrames = WalkFrameUtility.PrepareWalkFrames(frames, idleSprite);
    }
}
