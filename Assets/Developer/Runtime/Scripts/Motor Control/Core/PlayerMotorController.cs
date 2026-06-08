using UnityEngine;
using UnityEngine.InputSystem;

namespace Developer.MotorControl
{
    /// <summary>
    /// Base controller class for <see cref="RigidbodyMotor2D"/>. <br/>
    /// Motors should not directly handle inputs, instead should be possessed by a controller which propagate intents to attached modules.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerMotorController : MonoBehaviour
    {
        [SerializeField]
        private RigidbodyMotor2D initialMotor;

        public RigidbodyMotor2D PossessedMotor { get; private set; }



        private void Awake()
        {
            if (initialMotor != null)
                Possess(initialMotor);
        }



        /// <summary>
        /// Possess a new <see cref="RigidbodyMotor2D"/>. <br/>
        /// If <paramref name="_ForceUnassign"/> is set to <see langword="true"/>, this call will override the existing controller (if has)
        /// on the target motor. The overridden controller will be forced to call Possess(null)
        /// </summary>
        /// <param name="_TargetMotor"> The motor to possess. </param>
        /// <param name="_ForceUnassign"> Should this controller override existing controller (if has) on the target motor. </param>
        public bool Possess(RigidbodyMotor2D _TargetMotor, bool _ForceUnassign = true)
        {
            if (PossessedMotor == _TargetMotor) return false;

            // If controller failed to assign controller, abort 
            if (!_TargetMotor.AssignController(this, _ForceUnassign)) return false;

            if (PossessedMotor != null)
            {
                PossessedMotor.OnInputActionReferenceAdded -= PossessedMotor_OnInputActionReferenceAdded;
                PossessedMotor.OnInputActionReferenceRemoved -= PossessedMotor_OnInputActionReferenceRemoved;
                UnbindInputActions(PossessedMotor.InputActions);
            }

            PossessedMotor = _TargetMotor;

            if (PossessedMotor != null)
            {
                PossessedMotor.OnInputActionReferenceAdded += PossessedMotor_OnInputActionReferenceAdded;
                PossessedMotor.OnInputActionReferenceRemoved += PossessedMotor_OnInputActionReferenceRemoved;
                BindInputActions(PossessedMotor.InputActions);
            }

            return true;
        }

        // Handle input binding for new input action.
        private void PossessedMotor_OnInputActionReferenceAdded(InputAction _InputAction)
        {
            BindInputActions(_InputAction);
        }

        // Handle input unbinding for unreferenced input action.
        private void PossessedMotor_OnInputActionReferenceRemoved(InputAction _InputAction)
        {
            UnbindInputActions(_InputAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_InputActions">  </param>
        private void BindInputActions(params InputAction[] _InputActions)
        {
            foreach (var inputAction in _InputActions)
            {
                inputAction.Enable();
            }

            foreach (var inputAction in _InputActions)
            {
                inputAction.started += InputPhaseUpdateCallback;
                inputAction.performed += InputPhaseUpdateCallback;
                inputAction.canceled += InputPhaseUpdateCallback;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_InputActions"></param>
        private void UnbindInputActions(params InputAction[] _InputActions)
        {
            foreach (var inputAction in _InputActions)
            {
                inputAction.started -= InputPhaseUpdateCallback;
                inputAction.performed -= InputPhaseUpdateCallback;
                inputAction.canceled -= InputPhaseUpdateCallback;
            }
        }

        private void InputPhaseUpdateCallback(InputAction.CallbackContext _Callback)
        {
            PossessedMotor.PropagateInputCallbackContext(_Callback);
        }
    }
}
