using Developer.GameplaySystems;
using Developer.General.Managers;
using Developer.PingPong.GameplayAttributes;
using System.Collections;
using UnityEngine;

namespace Developer.PingPong
{
    public class TeamAreaExtension : FieldExtension
    {
        [SerializeField]
        [Range(0.1f, 0.5f)]
        private float limitLine = 0.2f;
        [SerializeField]
        private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField]
        [Min(0)]
        private float easeDuration = 0.1f;

        [Header("REFERNECES")]
        [SerializeField]
        private SpriteRenderer outlineRenderer;


        protected override void Start()
        {
            base.Start();

            GameplayData.AddAttribute(new SizeAttribute(GameManager.GameplaySettings.teamZonePercentage)).OnValueChanged += OnSizeChange;
            GameplayData.AddAttribute(new MaxSizeAttribute(0.4f));

            limitLine = GameManager.GameplaySettings.teamZonePercentage;

            outlineRenderer.color = GameplayData.ContainTags(UnitTag.TeamA)
                ? GameManager.DisplaySettings.TeamAColor
                : GameManager.DisplaySettings.TeamBColor;
        }



        protected override void OnAttached(PingPongField _Field)
        {
            UpdateSizeDisplay(_Field);
        }

        protected internal override void OnFieldUpdated(PingPongField _Field)
        {
            UpdateSizeDisplay(_Field);
        }

        private void UpdateSizeDisplay(PingPongField _Field)
        {
            limitLine = GameplayData.GetAttribute<SizeAttribute>().ModifiedValue;
            if (outlineRenderer)
            {
                outlineRenderer.size = new Vector2(_Field.Size.x * limitLine, _Field.Size.y);
                outlineRenderer.transform.localPosition = new Vector3(_Field.Size.x * (limitLine - 1f) * 0.5f, 0, 0);
            }
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

            updateSizeCoroutine = StartCoroutine(UpdateSizeCoroutine(limitLine, _NewValue));
        }

        private IEnumerator UpdateSizeCoroutine(float _Start, float _Target)
        {
            float elapsedTime = 0;

            while (elapsedTime < easeDuration)
            {
                elapsedTime += TimeManager.DeltaTime;
                limitLine = Mathf.Lerp(_Start, _Target, easeCurve.Evaluate(elapsedTime / easeDuration));
                UpdateSizeDisplay(AttachedField);
                yield return null;
            }

            limitLine = _Target;
            UpdateSizeDisplay(AttachedField);
        }
        #endregion
    }
}
