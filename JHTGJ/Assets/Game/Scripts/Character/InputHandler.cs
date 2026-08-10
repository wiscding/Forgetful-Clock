using UnityEngine;

namespace JHTGJ.Character
{
    public class InputHandler : MonoBehaviour
    {
        public float GetHorizontalAxis()
        {
            var axis = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                axis -= 1f;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                axis += 1f;

            return Mathf.Clamp(axis, -1f, 1f);
        }
    }
}
