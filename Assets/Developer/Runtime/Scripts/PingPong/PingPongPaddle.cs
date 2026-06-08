using Developer.GameplaySystems;
using Developer.GameplaySystems.Equipment;
using Developer.GameplaySystems.General;
using Developer.General.Managers;
using Developer.PingPong.GameplayAttributes;
using Developer.UI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;


namespace Developer.PingPong
{
    public class PingPongPaddle : MonoBehaviour
    {
        [SerializeField]
        private UnitTag[] InitialTags;

        [Header("REFERENCES")]
        [SerializeField]
        private Rigidbody2D targetBody;
        [SerializeField]
        private GameplayDataComponent gameplayDataComp;
        [SerializeField]
        private PlayerInput input;
        [SerializeField]
        private Transform servePosition;

        [Space]
        [SerializeField]
        private SegmentedDisplay healthDisplay;
        [SerializeField]
        private SpriteRenderer baseRenderer;

        public UnitGameplayData GameplayData => gameplayDataComp.GameplayData;

        public InventoryHandler Inventory { get; private set; }

        private readonly Dictionary<InputAction, int> itemSlotInputs = new();

        private Vector3 orgPosition;
        private Vector2 inputDirection;
        private bool canServe = false;



        private void OnEnable()
        {
            BindInput();

            GameManager.EventAggregator.Subscribe<GoalEvent>(OnGoalEvent);
            GameManager.EventAggregator.Subscribe<EndRoundEvent>(OnNewRoundEvent);
        }

        private void OnDisable()
        {
            UnbindInput();

            GameManager.EventAggregator.Unsubscribe<GoalEvent>(OnGoalEvent);
            GameManager.EventAggregator.Unsubscribe<EndRoundEvent>(OnNewRoundEvent);
        }

        private void Start()
        {
            orgPosition = transform.position;

            GameplayData.AddAttribute(new MaxSpeedAttribute(GameManager.GameplaySettings.paddleSpeed));
            GameplayData.AddAttribute(new AccelerationAttribute(GameManager.GameplaySettings.paddleAcceleration));
            GameplayData.AddAttribute(new DecelerationAttribute(GameManager.GameplaySettings.paddleDeceleration));
            GameplayData.AddAttribute(new HealthAttribute(GameManager.GameplaySettings.paddleMaxHealth))
                .OnValueChanged += OnHealthUpdated;
            GameplayData.AddAttribute(new MaxHealthAttribute(GameManager.GameplaySettings.paddleMaxHealth))
                .OnValueChanged += OnMaxHealthUpdated;
            GameplayData.AddAttribute(new SizeAttribute(GameManager.GameplaySettings.paddleMaxHealth));

            healthDisplay.MaxValue = GameManager.GameplaySettings.paddleMaxHealth;
            healthDisplay.CurrentValue = GameManager.GameplaySettings.paddleMaxHealth;

            foreach (var tag in InitialTags)
                GameplayData.AddTag(tag);

            baseRenderer.color = GameplayData.ContainTags(UnitTag.TeamA)
                ? GameManager.DisplaySettings.TeamAColor
                : GameManager.DisplaySettings.TeamBColor;
        }

        private void FixedUpdate()
        {
            targetBody.linearVelocity = GetFinalVelocity();
        }



        private void BindInput()
        {
            var moveInput = input.actions["Move"];
            moveInput.performed += OnMove;
            moveInput.canceled += OnMove;

            var primaryInput = input.actions["Primary"];
            primaryInput.performed += OnPrimary;

            for (int i = 0; i < 3; i++)
            {
                var equipmentSlotInput = input.actions[$"Equipment Slot {i + 1}"];
                equipmentSlotInput.performed += OnInteractSlot;

                itemSlotInputs.Add(equipmentSlotInput, i);
            }
        }

        private void UnbindInput()
        {
            var moveInput = input.actions["Move"];
            moveInput.performed -= OnMove;
            moveInput.canceled -= OnMove;

            for (int i = 0; i < 3; i++)
            {
                var equipmentSlotInput = input.actions[$"Equipment Slot {i + 1}"];
                equipmentSlotInput.performed -= OnInteractSlot;

                itemSlotInputs.Remove(equipmentSlotInput);
            }
        }

        private Vector2 GetFinalVelocity()
        {
            Vector2 value = targetBody.linearVelocity;

            float maxSpeed = GameplayData.GetAttribute<MaxSpeedAttribute>().ModifiedValue;
            float acceleration = GameplayData.GetAttribute<AccelerationAttribute>().ModifiedValue;
            float deceleration = GameplayData.GetAttribute<DecelerationAttribute>().ModifiedValue;
            
            for (int i = 0; i < 2; i++)
            {
                var dir = Math.Sign(inputDirection[i]);

                // decelerate with no input
                if (dir == 0)
                    value[i] = Mathf.MoveTowards(value[i], 0, deceleration * TimeManager.FixedDeltaTime);
                // accelerate with input
                else
                    value[i] = Mathf.MoveTowards(value[i], maxSpeed * dir, acceleration * TimeManager.FixedDeltaTime);
            }

            return value;
        }


        #region Input Callbacks
        private void OnMove(InputAction.CallbackContext _Context)
        {
            inputDirection = _Context.ReadValue<Vector2>();
        }

        private void OnPrimary(InputAction.CallbackContext _Context)
        {
            if (canServe)
            {
                Vector2 direction = inputDirection + (Vector2)transform.right;
                GameManager.EventAggregator.Publish(new ServeEvent(direction));
            }
        }

        private void OnInteractSlot(InputAction.CallbackContext _Context)
        {
            var item = Inventory.GetItemAt(itemSlotInputs[_Context.action]);
            item.OnUse(gameObject, gameObject);
        }
        #endregion

        #region Event Callbacks
        private void OnHealthUpdated(GameplaySystems.Attributes.UnitAttribute.CallbackContext _Context)
        {
            healthDisplay.CurrentValue = (int)_Context.newValue;

            if (_Context.newValue == 0)
            {
                string wonTeam = GameplayData.ContainTags(UnitTag.TeamA) ? "Team A" : "Team B";
                GameManager.EventAggregator.Publish(new GameOverEvent(
                    $"Game Over!\n\n" +
                    $"{wonTeam} won!"));
            }
        }

        private void OnMaxHealthUpdated(GameplaySystems.Attributes.UnitAttribute.CallbackContext _Context)
        {
            healthDisplay.MaxValue = (int)_Context.newValue;
        }

        private void OnGoalEvent(GoalEvent _Event)
        {
            if (GameplayData.ContainTags(UnitTag.TeamA) && _Event.GoalData.ContainTags(UnitTag.TeamA)
                || GameplayData.ContainTags(UnitTag.TeamB) && _Event.GoalData.ContainTags(UnitTag.TeamB))
            {
                GameplayData.ModifyAttributeValue<HealthAttribute>(new AttributeModSpec(
                    _Magnitude: -1,
                    _Operation: AttributeModOp.Addition,
                    _Policy: ModAppPolicy.Instant));
            }

            input.actions.Disable();
        }

        private void OnNewRoundEvent(EndRoundEvent _Event)
        {
            input.actions.Enable();
            transform.position = orgPosition;

            if (GameplayData.ContainTags(_Event.nextServeTeamTag))
            {
                GameManager.EventAggregator.Publish(new ReadyToServeEvent(servePosition));
                canServe = true;
            }
        }
        #endregion
    }

    public class ReadyToServeEvent
    {
        public readonly Transform servePos;

        public ReadyToServeEvent(Transform _ServePos)
        {
            servePos = _ServePos;
        }
    }

    public class ServeEvent
    {
        public readonly Vector2 direction;

        public ServeEvent(Vector2 _Direction)
        {
            direction = _Direction;
        }
    }
}