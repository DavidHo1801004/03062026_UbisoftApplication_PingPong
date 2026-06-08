using Developer.Collections;
using Developer.Collections.Events;
using Developer.GameplaySystems;
using Developer.General.Managers;
using Developer.MotorControl.Events;
using LMK.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Developer.MotorControl
{
    /// <summary>
    /// Base class for managing modular controls of a <see cref="Rigidbody2D"/>. <br/>
    /// Motors should always receive input events from a <see cref="PlayerMotorController"/> instead of directly handling inputs.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Developer/Motor Control/Rigidbody Motor 2D")]
    public class RigidbodyMotor2D : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private Rigidbody2D targetBody;

        public Vector2 LinearVelocity { get; set; }

        public float AngularVelocity { get; set; }

        /// <summary>
        /// The controller which this motor is possessed by.
        /// </summary>
        public PlayerMotorController OwnerController { get; private set; }

        public Blackboard Blackboard { get; private set; } = new();

        private readonly EventAggregator aggregator = new();

        /// <inheritdoc cref="UnitGameplayData"/>
        public UnitGameplayData GameplayData { get; private set; } = new();

        private readonly Dictionary<Type, MotorControlModule> moduleLookup = new();

        public MotorControlModule[] AttachedModules 
            => moduleLookup.Values.ToArray();

        // TODO refactor to intent binding through event aggregator instead of direct input binding
        private readonly Dictionary<InputAction, int> referencedInputActions = new();

        private readonly EventBus<InputAction, InputAction.CallbackContext> inputEventBus = new();

        public InputAction[] InputActions
            => referencedInputActions.Keys.ToArray();

        /// <summary>
        /// Invoke on new module added with new unique input action.
        /// </summary>
        internal event Action<InputAction> OnInputActionReferenceAdded;

        /// <summary>
        /// Invoke on module removed that completely remove reference to an input action.
        /// </summary>
        internal event Action<InputAction> OnInputActionReferenceRemoved;



#if UNITY_EDITOR
        private void Reset()
        {
            if (!targetBody) targetBody = GetComponent<Rigidbody2D>();

            targetBody.gravityScale = 0;
        }
#endif

        private void FixedUpdate()
        {
            foreach (var control in moduleLookup.Values)
            {
                if (!control.IsEnabled) continue;

                // Can replace Time.fixedDeltaTime with custom time value.
                control.OnTick(TimeManager.FixedDeltaTime);
            }

            targetBody.linearVelocity = LinearVelocity;
            targetBody.angularVelocity = AngularVelocity;
        }

        private void OnTriggerEnter2D(Collider2D _Collision)
        {
            aggregator.Publish(new MotorTriggerCallback(
                _Motor: this,
                _Phase: CollisionPhase.Enter,
                _OtherCollider: _Collision));
        }

        private void OnTriggerExit2D(Collider2D _Collision)
        {
            aggregator.Publish(new MotorTriggerCallback(
                _Motor: this,
                _Phase: CollisionPhase.Exit,
                _OtherCollider: _Collision));
        }

        private void OnCollisionEnter2D(Collision2D _Collision)
        {
            aggregator.Publish(new MotorCollisionCallback(
                _Motor: this,
                _Phase: CollisionPhase.Enter,
                _Collision: _Collision));
        }

        private void OnCollisionExit2D(Collision2D _Collision)
        {
            aggregator.Publish(new MotorCollisionCallback(
                _Motor: this,
                _Phase: CollisionPhase.Exit,
                _Collision: _Collision));
        }



        /// <summary>
        /// Internally called by <see cref="PlayerMotorController"/> to handle controller assignment.
        /// </summary>
        /// <param name="_ForceUnassign"> If <see langword="true"/>, force unassign of the current controller if valid. </param>
        /// <returns>
        /// Whether any update was made.
        /// </returns>
        internal bool AssignController(PlayerMotorController _NewController, bool _ForceUnassign)
        {
            if (_NewController == OwnerController) return false;

            // Handle force unassign
            if (OwnerController != null)
            {
                if (_ForceUnassign) return false;
                OnUnpossessed(OwnerController);
            }

            OwnerController = _NewController;

            if (OwnerController != null)
                OnPossessed(OwnerController);

            return true;
        }

        /// <summary>
        /// Called when a new controller is assigned to this motor. <br/>
        /// This is called after <see cref="OnUnpossessed"/> during a switch.
        /// </summary>
        protected virtual void OnPossessed(PlayerMotorController _NewController) { }

        /// <summary>
        /// Called when the current controller is no longer assigned to this motor. <br/>
        /// This is called before <see cref="OnPossessed"/> during a switch.
        /// </summary>
        protected virtual void OnUnpossessed(PlayerMotorController _OldController) { }



        /// <summary>
        /// Called by <see cref="PlayerMotorController"/> on any referenced input action callback. <br/>
        /// Only input actions referenced by attached modules will be evaluated.
        /// </summary>
        /// <param name="_Context"></param>
        internal void PropagateInputCallbackContext(InputAction.CallbackContext _Context)
        {
            inputEventBus.Publish(_Context.action, _Context);
        }

        /// <inheritdoc cref="EventBus{TKey, TCallbackContext}.Subscribe(TKey, Action{TCallbackContext})"/>
        public void RegisterInputCallback(InputAction _InputAction, Action<InputAction.CallbackContext> _Handler)
        {
            inputEventBus.Subscribe(_InputAction, _Handler);
        }

        /// <inheritdoc cref="EventBus{TKey, TCallbackContext}.Unsubscribe(TKey, Action{TCallbackContext})"/>
        public bool UnregisterInputCallback(InputAction _InputAction, Action<InputAction.CallbackContext> _Handler)
        {
            return inputEventBus.Unsubscribe(_InputAction, _Handler);
        }

        /// <inheritdoc cref="EventAggregator.Subscribe{TEvent}(Action{TEvent})"/>
        public void RegisterCallback<TEvent>(Action<TEvent> _Handler)
        {
            aggregator.Subscribe(_Handler);
        }

        /// <inheritdoc cref="EventAggregator.Unsubscribe{TEvent}(Action{TEvent})"/>
        public bool UnregisterCallback<TEvent>(Action<TEvent> _Handler)
        {
            return aggregator.Unsubscribe(_Handler);
        }



        /// <summary>
        /// Attach a new <see cref="MotorControlModule"/> to this motor. <br/>
        /// <b>NOTE:</b> Motors do NOT allow multiple modules of the same type.
        /// </summary>
        /// <param name="_ForceDetach"> If <see langword="true"/>, force detach on the target module if 
        ///                             already attached to another motor. </param>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        public bool AddControlModule(MotorControlModule _Module, bool _ForceDetach = false)
        {
            if (_Module == null) return false;

            // If module failed to attach, abort
            if (!_Module.AttachToMotor(this, _ForceDetach)) return false;

            var moduleType = _Module.GetType();
            if (moduleLookup.ContainsKey(moduleType)) return false;

            foreach (var inputAction in _Module.InputActions)
            {
                if (!referencedInputActions.ContainsKey(inputAction))
                {
                    referencedInputActions.Add(inputAction, 0);
                    OnInputActionReferenceAdded?.Invoke(inputAction);
                }
                referencedInputActions[inputAction]++;
            }

            moduleLookup.Add(moduleType, _Module);

            return true;
        }

        /// <summary>
        /// Remove an attached module that matches exactly the given reference.
        /// </summary>
        /// <inheritdoc cref="RemoveControlModule(Type)"/>
        public bool RemoveControlModule(MotorControlModule _Module)
        {
            return RemoveControlModule(_Module.GetType());
        }

        /// <inheritdoc cref="RemoveControlModule(Type)"/>
        public bool RemoveControlModule<T>() where T : MotorControlModule
        {
            return RemoveControlModule(typeof(T));
        }

        /// <summary>
        /// Remove an attached module with the given type.
        /// </summary>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        public bool RemoveControlModule(Type _ModuleType)
        {
            if (!moduleLookup.ContainsKey(_ModuleType)) return false;

            var controlModule = moduleLookup[_ModuleType];
            // If module failed to detach, abort
            if (!controlModule.AttachToMotor(null, true)) return false;

            foreach (var inputAction in moduleLookup[_ModuleType].InputActions)
            {
                if (--referencedInputActions[inputAction] == 0)
                {
                    referencedInputActions.Remove(inputAction);
                    OnInputActionReferenceRemoved?.Invoke(inputAction);
                }
            }

            moduleLookup.Remove(_ModuleType);

            return true;
        }
    }
}
