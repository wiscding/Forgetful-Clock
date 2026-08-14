using UnityEngine;

namespace JHTGJ.Scene
{
    public class WalkBoundMarker : MonoBehaviour
    {
        [SerializeField] BoundSide side = BoundSide.Left;

        public enum BoundSide
        {
            Left,
            Right
        }

        void OnDrawGizmos()
        {
            Gizmos.color = side == BoundSide.Left ? Color.cyan : Color.magenta;
            var pos = transform.position;
            Gizmos.DrawLine(new Vector3(pos.x, pos.y - 0.5f, 0f), new Vector3(pos.x, pos.y + 4f, 0f));
        }
    }
}
