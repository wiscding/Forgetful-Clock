using UnityEngine;

namespace JHTGJ.Character
{
    [RequireComponent(typeof(SideViewCharacterController))]
    [RequireComponent(typeof(InputHandler))]
    public class ProtagonistController : MonoBehaviour
    {
        SideViewCharacterController movement;
        InputHandler input;

        void Awake()
        {
            movement = GetComponent<SideViewCharacterController>();
            input = GetComponent<InputHandler>();
        }

        void Update()
        {
            movement.MoveHorizontal(input.GetHorizontalAxis());
        }
    }
}
