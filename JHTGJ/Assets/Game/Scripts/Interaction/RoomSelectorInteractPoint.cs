using System.Collections.Generic;
using JHTGJ.Art;
using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Interaction
{
    [ExecuteAlways]
    public class RoomSelectorInteractPoint : MonoBehaviour
    {
        [SerializeField] string label = "选择目的地";
        [SerializeField] float standXOffset;
        [SerializeField] Vector2 markerSize = new Vector2(0.9f, 0.3f);
        [SerializeField] List<RoomDestinationEntry> destinations = new List<RoomDestinationEntry>();

        public string Label => label;
        public float StandX => transform.position.x + standXOffset;
        public IReadOnlyList<RoomDestinationEntry> Destinations => destinations;

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

            var color = new Color(0.35f, 0.75f, 0.98f, 0.6f);
            block.ApplyWith(color, markerSize, 5);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.35f, 0.75f, 0.98f, 0.9f);
            var stand = new Vector3(StandX, transform.position.y, 0f);
            Gizmos.DrawWireSphere(stand, 0.22f);
            Gizmos.DrawLine(transform.position, stand);
        }
    }
}
