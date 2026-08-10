using UnityEngine;

namespace JHTGJ.Scene
{
    /// <summary>
    /// Empty marker placed in the scene. Room reads its X position as a walk boundary.
    /// </summary>
    public class WalkBoundMarker : MonoBehaviour
    {
        [SerializeField] BoundSide side = BoundSide.Left;

        public BoundSide Side => side;

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
