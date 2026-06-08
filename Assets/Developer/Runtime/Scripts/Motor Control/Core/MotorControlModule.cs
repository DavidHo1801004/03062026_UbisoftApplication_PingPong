using Developer.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Developer.MotorControl
{
    /// <summary>
    /// Base control module class for <see cref="RigidbodyMotor2D"/>. <br/>
    /// Custom input binding and controls can be defined in subclasses.
    /// </summary>
    [System.Serializable]
    public abstract class MotorControlModule
    {
        [SerializeField]
        private SerializableDictionary<string, InputActionReference> inputActions = new();

        [SerializeField, HideInInspector]
        private bool isEnabled;
        /// <summary>
        /// Disabled modules does NOT call OnTick(). Modules are disabled by default. <br/>
        /// Since input callback binding is handled in subclasses, a disabled module can still response to input. <br/>
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                if (IsEnabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        [SerializeField, HideInInspector]
        private bool passiveControl;
        /// <summary>
        /// Passive controls does not check for input validation. <br/>
        /// If input action references are already assigned, callbacks will not be invoked.
        /// </summary>
        public bool IsPassiveControl => passiveControl;

        /// <summary>
        /// List of all input action names for this controls.
        /// </summary>
        public abstract string[] InputActionKeysName { get; }

        // All input actions from input actions references
        internal InputAction[] InputActions 
            => inputActions.Values
            .Where(e => e != null)
            .Select(e => e.action)
            .ToArray();

        // All referenced input action maps from input actions
        internal InputActionMap[] InputActionMaps 
            => inputActions.Values
            .Where(e => e != null)
            .Select(e => e.action.actionMap)
            .Distinct()
            .ToArray();

        /// <summary>
        /// The motor this module is attached to.
        /// </summary>
        public RigidbodyMotor2D OwnerMotor { get; private set; }



        /// <summary>
        /// Ensures inputActions contains only matching keys from InputActionKeysName.
        /// </summary>
        public void SyncInputActionKeys()
        {
            if (InputActionKeysName == null) return;
            
            foreach (var key in InputActionKeysName)
            {
                if (!inputActions.ContainsKey(key))
                    inputActions[key] = null;
            }

            var staleKeys = inputActions.Keys
                .Where(k => !System.Array.Exists(InputActionKeysName, name => name == k))
                .ToList();
            foreach (var key in staleKeys)
                inputActions.Remove(key);
        }

        /// <summary>
        /// Return the referenced InputAction with the given key if valid.
        /// </summary>
        protected InputAction GetInputActionWithKey(string _Key)
        {
            if (!inputActions.TryGetValue(_Key, out var inputAction)) return null;
            return inputAction.action;
        }



        /// <summary>
        /// Internally called by <see cref="RigidbodyMotor2D"/> to handle motor attach and detach event.
        /// </summary>
        /// <param name="_ForceDetach"> If <see langword="true"/>, force detach from the current motor if valid. </param>
        /// <returns>
        /// Whether any update was made.
        /// </returns>
        internal bool AttachToMotor(RigidbodyMotor2D _NewMotor, bool _ForceDetach)
        {
            if (OwnerMotor == _NewMotor) return false;

            // Handle force detach
            if (OwnerMotor != null)
            {
                if (!_ForceDetach) return false;
                OnDetached(OwnerMotor);
            }

            OwnerMotor = _NewMotor;

            if (OwnerMotor != null)
                OnAttached(OwnerMotor);

            return true;
        }

        /// <summary>
        /// Called when first attached to a new valid motor. <br/>
        /// This is called after <see cref="OnDetached"/> during a switch.
        /// </summary>
        protected virtual void OnAttached(RigidbodyMotor2D _NewMotor) { }

        /// <summary>
        /// Called when first detached from the current motor. <br/>
        /// This is called before <see cref="OnAttached"/> during a switch.
        /// </summary>
        protected virtual void OnDetached(RigidbodyMotor2D _OldMotor) { }



        /// <summary>
        /// Called once when the module first loaded.
        /// </summary>
        protected internal virtual void OnInit() { }

        /// <summary>
        /// Called on first loaded and on <see cref="IsEnabled"/> = <see langword="true"/>.
        /// </summary>
        protected internal virtual void OnEnabled() { }

        /// <summary>
        /// Called on <see cref="IsEnabled"/> = <see langword="false"/>.
        /// </summary>
        protected internal virtual void OnDisabled() { }

        /// <summary>
        /// Called each FixedUpdate() by the attached <see cref="RigidbodyMotor2D"/> if enabled.
        /// </summary>
        protected internal virtual void OnTick(float _DeltaTime) { }
    }
}
