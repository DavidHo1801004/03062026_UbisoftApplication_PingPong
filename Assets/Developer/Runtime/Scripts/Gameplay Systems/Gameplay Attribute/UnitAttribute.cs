using System.Linq;
using System.Collections.Generic;
using System;

using Developer.GameplaySystems.General;

namespace Developer.GameplaySystems.Attributes
{
    /// <summary>
    /// Store a single channel of operation for attribute mods.
    /// </summary>
    internal class AttributeModChannel
    {
        private const ushort ID_INCREMENT = 16;

        // Track valid ID to assign to active modifiers, allow reuse id.
        private ushort idCount = 0;
        private readonly Stack<ushort> validID = new();

        private readonly Dictionary<int, float> mods = new();

        public float[] Magnitudes => mods.Values.ToArray();

        public bool IsEmpty => mods.Count == 0;

        /// <summary>
        /// Add a mod magnitude to the channel.
        /// </summary>
        /// <returns>
        /// The correspond id to the added mod.
        /// </returns>
        public int AddMod(float _Magnitude)
        {
            var id = GetValidId();
            mods.Add(id, _Magnitude);
            return id;
        }

        /// <summary>
        /// Remove the corresponding mod magnitude with the given index.
        /// </summary>
        public bool RemoveMod(int _Id)
        {
            return mods.Remove(_Id);
        } 

        /// <summary>
        /// Get the first available ID from stack.
        /// </summary>
        private int GetValidId()
        {
            if (validID.Count == 0)
            {
                for (ushort i = 0; i < ID_INCREMENT; i++)
                    validID.Push((ushort)(idCount + i));
                idCount += ID_INCREMENT;
            }

            return validID.Pop();
        }
    }

    /// <summary>
    /// Base class for creating gameplay attributes of units.
    /// </summary>
    [System.Serializable]
    public abstract class UnitAttribute
    {
        /// <summary>
        /// Define groups of similar attributes. <br/>
        /// <b>TEMP for GameplayTag.</b>
        /// </summary>
        public enum Category
        {
            Health,
            Resources,

            MotorStats,
        }

        public readonly struct CallbackContext
        {
            public readonly float oldValue;
            public readonly float newValue;

            public CallbackContext(
                float _OldValue,
                float _NewValue)
            {
                oldValue = _OldValue; 
                newValue = _NewValue;
            }
        }



        public event Action<CallbackContext> OnValueChanged;

        /// <summary>
        /// Internal base value of this attribute.
        /// </summary>
        public float BaseValue { get; private set; }

        private float modifiedValue;
        /// <summary>
        /// Value indicates base value with all modifiers applied.
        /// </summary>
        public float ModifiedValue
        {
            get => modifiedValue;
            set => modifiedValue = value;
        }

        /// <summary>
        /// Used to allow group modification to attributes of the same categories.
        /// </summary>
        protected abstract Category[] Categories { get; }

        /// <summary>
        /// The <see cref="UnitGameplayData"/> that owns this attribute.
        /// </summary>
        protected internal UnitGameplayData OwnerData { get; internal set; }

        private readonly AttributeModChannel[] activeModChannels;



        public UnitAttribute() : this(0) { }

        public UnitAttribute(float _BaseValue)
        {
            BaseValue = _BaseValue;
            modifiedValue = _BaseValue;

            activeModChannels = new AttributeModChannel[System.Enum.GetValues(typeof(AttributeModOp)).Length];
            System.Array.Fill(activeModChannels, new AttributeModChannel());
        }

        public static implicit operator bool(UnitAttribute _Object) => _Object != null;

        

        public UnitAttribute Clone()
        {
            return (UnitAttribute)MemberwiseClone();
        }



        /// <summary>
        /// Called before any valid modification to internal value is made.
        /// </summary>
        /// <param name="_NewValue"> The initial value to modify internal value to. </param>
        /// <returns>
        /// The overridden value of <paramref name="_NewValue"/>. <br/>
        /// Use this to control how internal value are modified for this attribute.
        /// </returns>
        protected virtual float PreModifyValue(float _NewValue)
            => _NewValue;

        /// <summary>
        /// Called after a modification to internal value is applied.
        /// </summary>
        /// <param name="_OldValue"> Value before modification. </param>
        /// <param name="_NewValue"> Value after modification. </param>
        /// <remarks>
        /// This is called before <see cref="OnBaseValueUpdated"/> is invoked.
        /// </remarks>
        protected virtual void PostModifyValue(float _OldValue, float _NewValue) { }



        /// <summary>
        /// Check if the attribute contains any of the given categories.
        /// </summary>
        /// <param name="_Policy">      How should the category matching be handled. </param>
        /// <param name="_Categories">  The list of categories to check for. </param>
        public bool ContainsCategory(MatchPolicy _Policy, params Category[] _Categories)
        {
            switch (_Policy)
            {
                case MatchPolicy.Any:
                    return _Categories.Any(Categories.Contains);

                case MatchPolicy.All:
                    return _Categories.All(Categories.Contains);

                case MatchPolicy.Exact:
                    return _Categories == Categories;

                case MatchPolicy.Only:
                    return Categories.All(_Categories.Contains);
            }

            return false;
        }

        /// <summary>
        /// Applies a value modification using the specified modification method and value type.
        /// </summary>
        /// <param name="_Spec"> Specific data of the modification to apply. </param>
        /// <returns>
        /// A handle to reference the stored 
        /// </returns>
        public ActiveModHandle ApplyMod(in AttributeModSpec _Spec)
        {
            if (_Spec.policy == ModAppPolicy.Instant)
            {
                switch (_Spec.operation)
                {
                    case AttributeModOp.Addition:
                        BaseValue += _Spec.magnitude;
                        break;

                    case AttributeModOp.Multiplication:
                        BaseValue *= _Spec.magnitude;
                        break;

                    case AttributeModOp.Division:
                        if (_Spec.magnitude != 0)
                            BaseValue /= _Spec.magnitude;
                        break;

                    case AttributeModOp.Override:
                        BaseValue = _Spec.magnitude;
                        break;
                }

                UpdateModifiedValue();
                return null;
            }
            else
            {
                var id = activeModChannels[(int)_Spec.operation].AddMod(_Spec.magnitude);

                UpdateModifiedValue();
                return new ActiveModHandle(id, _Spec.operation, this);
            }
        }

        /// <summary>
        /// Reverts a previously applied modification using the provided handle.
        /// </summary>
        /// <param name="_ModificationHandle"> The <see cref="ActiveModHandle"/> representing the modification to revert. </param>
        public bool RemoveMod(ActiveModHandle _ModificationHandle)
        {
            if (_ModificationHandle == null) return false;
            return activeModChannels[(int)_ModificationHandle.operation].RemoveMod(_ModificationHandle.id);
        }



        /// <summary>
        /// Helper function to sum all of the mods in the specified mod channel.
        /// </summary>
        /// <param name="_Bias"> Bias to apply to modifier magnitudes </param>
        private float SumMods(AttributeModChannel _Channel, float _Bias)
        {
            float Sum = _Bias;

            foreach (var modMag in _Channel.Magnitudes)
                Sum += (modMag - _Bias);

            return Sum;
        }

        /// <summary>
        /// Evaluate base value with applied mods.
        /// </summary>
        private float EvaluateModifiedValue()
        {
            if (!activeModChannels[(int)AttributeModOp.Override].IsEmpty)
                return activeModChannels[(int)AttributeModOp.Override].Magnitudes[0];

            float addSum = SumMods(activeModChannels[(int)AttributeModOp.Addition], 0);
            float multSum = SumMods(activeModChannels[(int)AttributeModOp.Multiplication], 1);
            float divSum = SumMods(activeModChannels[(int)AttributeModOp.Division], 1);

            if (divSum == 0)
                divSum = 1;

            return (BaseValue + addSum) * multSum / divSum;
        }

        /// <summary>
        /// Internal method for handling custom value modification logic.
        /// </summary>
        private void UpdateModifiedValue()
        {
            float oldValue = modifiedValue;
            float newValue = PreModifyValue(EvaluateModifiedValue());
            modifiedValue = newValue;
            PostModifyValue(oldValue, newValue);

            OnValueChanged?.Invoke(new CallbackContext(oldValue, newValue));
        }
    }

    /// <summary>
    /// How should searches be executed.
    /// </summary>
    public enum MatchPolicy
    {
        // Set contains any of subset values.
        Any,

        // Set contains all of subset values.
        All,

        // Set contains only listed subset values. Missing set values are irrelevant.
        Only,

        // Set contains all and only the listed subset values.
        Exact,
    }
}
