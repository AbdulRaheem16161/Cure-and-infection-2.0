using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.MyPlayer
{
    public class InputReader : MonoBehaviour, Controls_C.IPlayerActions
    {
        public PlayerStateMachine stateMachine;

        private Controls_C controls;

        public Vector2 MovementInput { get; private set; }

        public bool JumpTriggered;

        public bool IsSprinting;

        public bool SwitchWeapon1Triggered;
        public bool SwitchWeapon2Triggered;
        public bool SwitchWeapon3Triggered;

        public bool AttackTriggered;
        public bool AttackPressed;

        public bool ReloadTriggered;

        public bool CrouchPressed;

        public bool InteractTriggered;

        public bool ThirdPersonCamera = true;
        public bool FirstPersonCamera = false;

        private void Awake()
        {
            controls = new Controls_C();
            controls.Player.SetCallbacks(this);
        }

        private void OnEnable()
        {
            controls.Player.Enable();
        }

        private void OnDisable()
        {
            controls.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                IsSprinting = true;    // Shift pressed
            else if (context.canceled)
                IsSprinting = false;   // Shift released
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && stateMachine.CanJump)
                JumpTriggered = true;
        }

        public void OnAttackold(InputAction.CallbackContext context)
        {
            if (context.performed)
                AttackTriggered = true;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
                CrouchPressed = !CrouchPressed;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                InteractTriggered = !InteractTriggered;
        }

        public void OnCameraMode(InputAction.CallbackContext context)
        {
            if (context.performed)
                FirstPersonCamera = !FirstPersonCamera;
                ThirdPersonCamera = !ThirdPersonCamera;
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            // AttackTriggered becomes true when attack button is pressed.
            if (context.performed)
                AttackTriggered = true;

            // AttackPressed stays true while the attack button is held, false when released.
            if (context.performed)
                AttackPressed = true;
            else if (context.canceled)
                AttackPressed = false;
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
                ReloadTriggered = true;
        }

        public void OnSelectWeapon1(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchWeapon1Triggered = true;
        }

        public void OnSelectWeapon2(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchWeapon2Triggered = true;
        }

        public void OnSelectWeapon3(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchWeapon3Triggered = true;
        }
    }
}
