using Developer.GameplaySystems;

namespace Developer.GameplaySystems.ItemAction.Effects
{
    /// <summary>
    /// Base class for creating appliable effects to units.
    /// </summary>
    public abstract class GameplayEffect
    {
        public abstract string Name { get; }

        public UnitGameplayData Target { get; internal set; }

        protected internal float InitialValue { get; private set; }

        

        public GameplayEffect(float _Magnitude)
            => InitialValue = _Magnitude;

        public override string ToString()
        {
            return $"{Name}: ";
        }



        internal void ApplyTo(UnitGameplayData _Source, UnitGameplayData _Target)
        {
            Target = _Target;
            OnApplied(_Source, _Target);
        }

        internal void Stack(UnitGameplayData _Source, float _Magnitude)
        {
            OnEffectStacked(_Source, _Magnitude);
        }

        internal GameplayEffect Clone()
        {
            var clone = CreateClone();
            if (ReferenceEquals(this, clone))
                throw new System.InvalidOperationException("CreateClone must return a new instance.");
            return clone;
        }



        protected internal void RemoveEffect()
        {
            Target.RemoveEffect(GetType());
            OnRemoved(Target);
        }

        protected abstract GameplayEffect CreateClone();

        protected abstract void OnApplied(UnitGameplayData _Source, UnitGameplayData _Target);

        protected abstract void OnEffectStacked(UnitGameplayData _Source, float _Magnitude);

        protected abstract void OnRemoved(UnitGameplayData _Target);
    }
}
