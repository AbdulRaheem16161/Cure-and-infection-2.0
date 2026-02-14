using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.GenericStateMachine
{
    public class InputReader : MonoBehaviour, Controls_Generic.IPlayerActions
    {
        private Controls_Generic controls;

        public bool InteractTriggered { get; internal set; }

        private void Start()
        {
            controls = new Controls_Generic();
            controls.Player.SetCallbacks(this);
            controls.Player.Enable();
        }

        private void OnDestroy()
        {
            controls.Player.Disable();
        }

        public void OnJump(InputAction.CallbackContext context)
        {

        }

        public void OnMove(InputAction.CallbackContext context)
        {

        }

        public void OnCrouch(InputAction.CallbackContext context)
        {

        }

        public void OnAttack(InputAction.CallbackContext context)
        {

        }
    }
}
