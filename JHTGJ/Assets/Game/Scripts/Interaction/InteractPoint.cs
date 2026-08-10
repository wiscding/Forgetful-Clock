using JHTGJ.Art;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Interaction
{
    /// <summary>
    /// Marks where the player stands to interact with a prop or door.
    /// Yellow semi-transparent block in the scene; replace with UI prompt later.
    /// </summary>
    [ExecuteAlways]
    public class InteractPoint : MonoBehaviour
    {
        [SerializeField] string interactId;
        [SerializeField] string label;
        [SerializeField] InteractionKind kind = InteractionKind.CookBreakfast;
        [SerializeField] RoomType targetRoom;
        [SerializeField] SpawnSide targetSpawnSide = SpawnSide.Left;
        [SerializeField] float standXOffset;
        [SerializeField] Vector2 markerSize = new Vector2(0.8f, 0.25f);

        public string InteractId => interactId;
        public string Label => label;
        public InteractionKind Kind => kind;
        public RoomType TargetRoom => targetRoom;
        public SpawnSide TargetSpawnSide => targetSpawnSide;
        public float StandX => transform.position.x + standXOffset;

        public void SetAvailable(bool available)
        {
            var block = GetComponent<WhiteboxBlock>();
            if (block != null)
            {
                if (!TryGetComponent(out SpriteRenderer renderer))
                    return;

                renderer.enabled = available;
                return;
            }

            if (TryGetComponent(out SpriteRenderer spriteRenderer))
                spriteRenderer.enabled = available;
        }

        void Awake() => ApplyMarker();
        void OnValidate() => ApplyMarker();

        public void ApplyMarker()
        {
            var block = GetComponent<WhiteboxBlock>();
            if (block == null)
            {
                if (GetComponent<SpriteRenderer>() == null)
                    gameObject.AddComponent<SpriteRenderer>();
                block = gameObject.AddComponent<WhiteboxBlock>();
            }

            var color = kind == InteractionKind.ChangeRoom
                ? new Color(0.3f, 0.95f, 0.55f, 0.55f)
                : kind == InteractionKind.EmergencyStop
                    ? new Color(0.95f, 0.3f, 0.3f, 0.65f)
                    : new Color(1f, 0.92f, 0.2f, 0.55f);

            block.ApplyWith(color, markerSize, 5);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = kind == InteractionKind.ChangeRoom
                ? Color.green
                : kind == InteractionKind.EmergencyStop
                    ? Color.red
                    : Color.yellow;
            var stand = new Vector3(StandX, transform.position.y, 0f);
            Gizmos.DrawWireSphere(stand, 0.2f);
            Gizmos.DrawLine(transform.position, stand);
        }
    }
}
