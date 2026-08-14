using JHTGJ.Character;
using UnityEngine;

namespace JHTGJ.Scene
{
    [ExecuteAlways]
    public class Room : MonoBehaviour
    {
        [SerializeField] float walkLeft = -9f;
        [SerializeField] float walkRight = 9f;
        [SerializeField] float floorY = -3f;
        [SerializeField] CharacterLightingType characterLighting = CharacterLightingType.Default;
        [SerializeField] WalkBoundMarker leftBound;
        [SerializeField] WalkBoundMarker rightBound;
        [SerializeField] Transform floorReference;
        [SerializeField] Transform playerSpawn;

        public float WalkLeft => walkLeft;
        public float WalkRight => walkRight;
        public float WalkWidth => walkRight - walkLeft;
        public float StepSize => WalkWidth / 3f;
        public float FloorY => floorY;
        public CharacterLightingType CharacterLighting => characterLighting;
        public Vector3 PlayerSpawnPosition => GetSpawnPosition(SpawnSide.Left);

        public Vector3 GetSpawnPosition(SpawnSide side)
        {
            var x = side == SpawnSide.Left ? walkLeft : walkRight;
            return new Vector3(x, floorY, 0f);
        }

        public Vector3 ClampPosition(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, walkLeft, walkRight);
            position.y = floorY;
            return position;
        }

        void Awake() => SyncFromMarkers();
        void OnValidate() => SyncFromMarkers();

        public void SyncFromMarkers()
        {
            if (leftBound != null)
                walkLeft = leftBound.transform.position.x;

            if (rightBound != null)
                walkRight = rightBound.transform.position.x;

            if (floorReference != null)
                floorY = floorReference.position.y;

            if (walkRight < walkLeft)
                (walkLeft, walkRight) = (walkRight, walkLeft);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.6f);
            var center = new Vector3((walkLeft + walkRight) * 0.5f, floorY, 0f);
            var size = new Vector3(WalkWidth, 0.1f, 0f);
            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.yellow;
            for (var i = 0; i <= 3; i++)
            {
                var x = walkLeft + StepSize * i;
                Gizmos.DrawLine(new Vector3(x, floorY - 0.3f, 0f), new Vector3(x, floorY + 0.3f, 0f));
            }

            if (playerSpawn != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawn.position, 0.25f);
            }
        }
    }
}
