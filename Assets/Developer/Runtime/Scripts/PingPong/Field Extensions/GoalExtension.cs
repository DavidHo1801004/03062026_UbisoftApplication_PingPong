using Developer.GameplaySystems;
using Developer.General.Managers;
using Developer.PingPong.GameplayAttributes;
using System.Collections;
using UnityEngine;

namespace Developer.PingPong
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class GoalExtension : FieldExtension
    {
        [Header("VISUAL")]
        [SerializeField]
        private Vector2 size;
        [SerializeField]
        private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField]
        private float easeDuration = 0.2f;
        
        [Header("REFERENCES")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private SpriteMask mask;



#if UNITY_EDITOR
        private void Reset()
        {
            UpdateSizeDisplay();
        }
#endif

        protected override void Start()
        {
            base.Start();

            GameplayData.AddAttribute(new SizeAttribute(GameManager.GameplaySettings.goalSize)).OnValueChanged += OnSizeChange;
            GameplayData.AddAttribute(new MaxSizeAttribute(GameManager.GameplaySettings.fieldSize.y - 2f));

            size.y = GameManager.GameplaySettings.goalSize;

            spriteRenderer.color = GameplayData.ContainTags(UnitTag.TeamA)
                ? GameManager.DisplaySettings.TeamAColor
                : GameManager.DisplaySettings.TeamBColor;
        }

        private void OnTriggerEnter2D(Collider2D _Collision)
        {
            if (_Collision.TryGetComponent(out GameplayDataComponent asGAC))
                if (asGAC.GameplayData.ContainTags(UnitTag.CanScore))
                {
                    GameManager.EventAggregator.Publish(new GoalEvent(gameplayDataComp));
                }
        }



        protected override void OnAttached(PingPongField _Field)
        {
            SetSize(GameplayData.GetAttribute<SizeAttribute>().ModifiedValue);
        }

        protected internal override void OnFieldUpdated(PingPongField _Field)
        {
            UpdateSizeDisplay();
        }

        private void UpdateSizeDisplay()
        {
            if (spriteRenderer)
                spriteRenderer.size = size;

            if (mask)
                mask.transform.localScale = size;
        }

        private void OnSizeChange(GameplaySystems.Attributes.UnitAttribute.CallbackContext _Context)
        {
            SetSize(_Context.newValue);
        }

        #region Coroutine Update
        private Coroutine updateSizeCoroutine;

        public void SetSize(float _NewValue)
        {
            if (updateSizeCoroutine != null)
                StopCoroutine(updateSizeCoroutine);

            updateSizeCoroutine = StartCoroutine(UpdateSizeCoroutine(size.y, _NewValue));
        }

        private IEnumerator UpdateSizeCoroutine(float _Start, float _Target)
        {
            float elapsedTime = 0;

            while (elapsedTime < easeDuration)
            {
                elapsedTime += TimeManager.DeltaTime;
                size.y = Mathf.Lerp(_Start, _Target, easeCurve.Evaluate(elapsedTime / easeDuration));
                UpdateSizeDisplay();
                yield return null;
            }

            size.y = _Target;
            UpdateSizeDisplay();
        }
        #endregion
    }

    public class GoalEvent
    {
        public GameplayDataComponent GDC;

        public UnitGameplayData GoalData => GDC.GameplayData;

        public GoalEvent(GameplayDataComponent _GDC)
        {
            GDC = _GDC;
        }
    }
}
