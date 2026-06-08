using Codice.Client.BaseCommands;
using Developer.GameplaySystems;
using Developer.GameplaySystems.General;
using Developer.General.Managers;
using Developer.PingPong.GameplayAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

namespace Developer.PingPong
{
    [RequireComponent(typeof(GameplayDataComponent), typeof(Rigidbody2D))]
    public class PingPongBall : MonoBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField]
        private Rigidbody2D targetBody;
        [SerializeField]
        private GameplayDataComponent gameplayDataComp;
        [SerializeField]
        private SpriteRenderer mainRenderer;

        public UnitGameplayData GameplayData => gameplayDataComp.GameplayData;

        private bool moving = true;



        private void OnEnable()
        {
            GameManager.EventAggregator.Subscribe<EndRoundEvent>(OnNewRoundEvent);
            GameManager.EventAggregator.Subscribe<ReadyToServeEvent>(OnReadyToServeEvent);
            GameManager.EventAggregator.Subscribe<ServeEvent>(OnServeEvent);
        }

        private void OnDisable()
        {
            GameManager.EventAggregator.Unsubscribe<EndRoundEvent>(OnNewRoundEvent);
            GameManager.EventAggregator.Unsubscribe<ReadyToServeEvent>(OnReadyToServeEvent);
            GameManager.EventAggregator.Unsubscribe<ServeEvent>(OnServeEvent);
        }

        private void Start()
        {
            if (!gameplayDataComp) gameplayDataComp = GetComponent<GameplayDataComponent>();
            if (!targetBody) targetBody = GetComponent<Rigidbody2D>();

            GameplayData.AddAttribute(new MaxSpeedAttribute(GameManager.GameplaySettings.ballSpeed));
            GameplayData.AddTag(UnitTag.CanScore);

            Release(Vector2.left);
        }

        private void Update()
        {
            if (moving)
                GameplayData.ModifyAttributeValue<MaxSpeedAttribute>(new AttributeModSpec(
                    _Magnitude: GameManager.GameplaySettings.ballSpeedIncrement * TimeManager.DeltaTime,
                    _Operation: AttributeModOp.Addition,
                    _Policy: ModAppPolicy.Instant));
        }

        private void FixedUpdate()
        {
            if (moving)
                targetBody.linearVelocity = GetFinalVelocity();
        }

        private void OnCollisionEnter2D(Collision2D _Collision)
        {
            if (_Collision.gameObject.TryGetComponent(out GameplayDataComponent asGAC))
                HandleCollisionWithGAC(asGAC);
        }

        private void OnCollisionExit2D(Collision2D _Collision)
        {
            HandleCollisionExit();
        }



        private void Release(Vector2 _Direction)
        {
            var speed = GameplayData.GetAttribute<MaxSpeedAttribute>().ModifiedValue;
            targetBody.linearVelocity = _Direction.normalized * speed;
            transform.SetParent(null);
        }

        private void HandleCollisionWithGAC(GameplayDataComponent _Other)
        {
            if (_Other.GameplayData.ContainTags(UnitTag.TeamA))
            {
                GameplayData.RemoveTag(UnitTag.TeamB);
                GameplayData.AddTag(UnitTag.TeamA);
                UpdateColor();
            }
            else if (_Other.GameplayData.ContainTags(UnitTag.TeamB))
            {
                GameplayData.RemoveTag(UnitTag.TeamA);
                GameplayData.AddTag(UnitTag.TeamB);
                UpdateColor();
            }
        }

        private void UpdateColor()
        {
            if (GameplayData.ContainTags(UnitTag.TeamA))
            {
                mainRenderer.color = GameManager.DisplaySettings.TeamAColor;
            }
            else if (GameplayData.ContainTags(UnitTag.TeamB))
            {
                mainRenderer.color = GameManager.DisplaySettings.TeamBColor;
            }
            else
            {
                mainRenderer.color = Color.white;
            }
        }

        private void HandleCollisionExit()
        {
            // Randomize bounce 
            float maxHalfAngle = GameManager.GameplaySettings.maxRandomAngle * 0.5f;
            float randomAngle = Random.Range(maxHalfAngle, -maxHalfAngle);
            targetBody.linearVelocity = Quaternion.Euler(0, 0, randomAngle) * targetBody.linearVelocity;
        }

        private Vector2 GetFinalVelocity()
        {
            Vector2 value = targetBody.linearVelocity;

            var speed = GameplayData.GetAttribute<MaxSpeedAttribute>().ModifiedValue;
            value = value.normalized * speed;

            return value;
        }


        #region Callbacks
        private void OnNewRoundEvent(EndRoundEvent _Event)
        {
            moving = false;
            transform.position = Vector3.zero;

            GameplayData.RemoveTag(UnitTag.TeamA);
            GameplayData.RemoveTag(UnitTag.TeamB);
            GameplayData.AddTag(_Event.nextServeTeamTag);
            UpdateColor();

            GameplayData.ModifyAttributeValue<MaxSpeedAttribute>(new AttributeModSpec(
                _Magnitude: GameManager.GameplaySettings.ballSpeed,
                _Operation: AttributeModOp.Override,
                _Policy: ModAppPolicy.Instant));
        }

        private void OnReadyToServeEvent(ReadyToServeEvent _Event)
        {
            transform.SetParent(_Event.servePos);
            transform.localPosition = Vector3.zero;
            targetBody.linearVelocity = Vector2.zero;
        }

        private void OnServeEvent(ServeEvent _Event)
        {
            moving = true;
            Release(_Event.direction);
        }
        #endregion
    }
}
