using System;
using System.Collections.Generic;

using Developer.GameplaySystems.General;
using Developer.GameplaySystems.Attributes;
using Developer.GameplaySystems.ItemAction.Effects;

namespace Developer.GameplaySystems
{
    /// <summary>
    /// Contains gameplay tag, attributes and active effects.
    /// </summary>
    [System.Serializable]
    public class UnitGameplayData
    {
        /// <summary>
        /// Invoked on any gameplay effect applied.
        /// </summary>
        public event Action<GameplayEffect> OnCombatEffectApplied;

        /// <summary>
        /// Invoked on any applied gameplay effect removed.
        /// </summary>
        public event Action<GameplayEffect> OnCombatEffectRemoved;

        /// <summary>
        /// Invoked on any <see cref="UnitTag"/> added.
        /// </summary>
        public event Action<UnitTag> OnTagAdded;

        /// <summary>
        /// Invoked on any existing <see cref="UnitTag"/> removed.
        /// </summary>
        public event Action<UnitTag> OnTagRemoved;



        private UnitAttributeSet attributeSet;

        private readonly Dictionary<Type, GameplayEffect> appliedEffects = new();

        private readonly HashSet<UnitTag> tags = new();



        public static implicit operator bool(UnitGameplayData _Object) => _Object != null;

        public override string ToString()
        {
            string result = attributeSet.ToString();

            result += "\nEffects:\n";
            foreach (var effect in appliedEffects.Values)
            {
                result += effect.ToString() + "\n";
            }

            result += "\nTags:\n";
            foreach (var tag in tags)
            {
                result += tag.ToString() + "\n";
            }

            return result;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="UnitGameplayData"/> class with empty attribute set.
        /// </summary>
        internal UnitGameplayData()
        {
            attributeSet = new(this);
        }

        /// <inheritdoc cref="UnitGameplayData()"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitGameplayData"/> class with a name and predefined stat values.
        /// </summary>
        /// <param name="_Attributes"> A collection of key-value pairs defining each attributes and its corresponding initial value. </param>
        /// <remarks>
        /// This constructor initializes base unit data with a name and sets each attribute according to the provided collection. <br/>
        /// Existing attributes are overridden with the specified values.  
        /// </remarks>
        public UnitGameplayData(ICollection<KeyValuePair<Type, int>> _Attributes) 
            : this()
        {
            foreach (var kpAttr in _Attributes)
            {
                if (attributeSet.ContainsAttribute(kpAttr.Key))
                    attributeSet.RemoveAttribute(kpAttr.Key);

                attributeSet.AddAttribute(UnitAttributeRegistry.CreateInstance(kpAttr.Key, kpAttr.Value));
            }
        }



        /// <summary>
        /// Deep clone.
        /// </summary>
        public UnitGameplayData Clone()
        {
            UnitGameplayData clone = new();
            clone.attributeSet = attributeSet.Clone(clone);
            return clone;
        }



        #region Attribute Controls
        /// <inheritdoc cref="UnitAttributeSet.AddAttribute(UnitAttribute)"/>
        public UnitAttribute AddAttribute(UnitAttribute _Template)
        {
            return attributeSet.AddAttribute(_Template);
        }

        /// <summary>
        /// Modify an attribute value if exist.
        /// </summary>
        public ActiveModHandle ModifyAttributeValue<T>(in AttributeModSpec _ModSpec) where T : UnitAttribute
        {
            UnitAttribute attribute = attributeSet.GetAttribute<T>();
            if (!attribute) return null;

            return attribute.ApplyMod(_ModSpec);
        }

        /// <inheritdoc cref="UnitAttributeSet.GetAttributes(MatchPolicy, UnitAttribute.Category[])"/>
        public UnitAttribute[] GetAttributes(MatchPolicy _Policy = MatchPolicy.Any, params UnitAttribute.Category[] _Categories)
            => attributeSet.GetAttributes(_Policy, _Categories);

        /// <inheritdoc cref="UnitAttributeSet.GetAttribute{T}()"/>
        public T GetAttribute<T>() where T : UnitAttribute
            => attributeSet.GetAttribute<T>();

        /// <inheritdoc cref="UnitAttributeSet.GetAttribute(Type)"/>
        public UnitAttribute GetAttribute(Type _AttributeType)
            => attributeSet.GetAttribute(_AttributeType);
        #endregion

        #region Effects Controls
        /// <summary>
        /// Applies a gameplay effect to the unit, stacking or refreshing it if already present.
        /// </summary>
        /// <param name="_Source"> The <see cref="UnitGameplayData"/> that applied the effect. </param>
        /// <param name="_Effect"> The <see cref="GameplayEffect"/> to apply. </param>
        /// <remarks>
        /// If an effect of the same concrete type already exists, the effect is stacked instead.
        /// </remarks>
        public void ApplyEffect(UnitGameplayData _Source, GameplayEffect _Effect)
        {
            Type type = _Effect.GetType(); 
            // First application
            if (!appliedEffects.ContainsKey(type))
            {
                appliedEffects.Add(type, _Effect.Clone());
                appliedEffects[type].ApplyTo(_Source, this);

                OnCombatEffectApplied?.Invoke(appliedEffects[type]);
            }
            // Stack
            else
            {
                appliedEffects[type].Stack(_Source, _Effect.InitialValue);
            }
        }

        /// <summary>
        /// Remove the given effect type from applied effects.
        /// </summary>
        /// <param name="_EffectType"> Effect type to remove. </param>
        internal void RemoveEffect(Type _EffectType) 
        {
            if (!appliedEffects.ContainsKey(_EffectType)) return;

            var effect = appliedEffects[_EffectType];
            appliedEffects.Remove(_EffectType);

            OnCombatEffectRemoved?.Invoke(effect);
        }
        #endregion

        #region Tag Controls
        /// <summary>
        /// Add a new <see cref="UnitTag"/>.
        /// </summary>
        /// <param name="_Tag"> Tag to add. </param>
        public void AddTag(UnitTag _Tag)
        {
            if (tags.Add(_Tag))
                OnTagAdded?.Invoke(_Tag);
        }

        /// <summary>
        /// Remove an existing <see cref="UnitTag"/>.
        /// </summary>
        /// <param name="_Tag"> Tag to remove. </param>
        public void RemoveTag(UnitTag _Tag)
        {
            if (tags.Remove(_Tag))
                OnTagRemoved?.Invoke(_Tag);
        }

        /// <summary>
        /// Check if a tag is present.
        /// </summary>
        /// <param name="_Tag"> Tag to check. </param>
        public bool ContainTags(UnitTag _Tag)
        {
            return tags.Contains(_Tag);
        }
        #endregion
    }

    /// <summary>
    /// All gameplay unit related tags. <br/>
    /// </summary>
    public enum UnitTag
    {
        CanScore,
        TeamA,
        TeamB,
        Destroyable
    }
}

