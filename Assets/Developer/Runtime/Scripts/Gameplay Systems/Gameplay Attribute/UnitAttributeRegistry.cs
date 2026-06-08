using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

using Developer.Utilities;

namespace Developer.GameplaySystems.Attributes
{
    public static class UnitAttributeRegistry
    {
        private static readonly Dictionary<Type, Func<float, UnitAttribute>> Factories;



        static UnitAttributeRegistry()
        {
            Factories = ReflectionUtils.GetNonAbstractSubclassesOf<UnitAttribute>()
                .ToDictionary(
                    e => e, 
                    e => CreateFactory(e));
        }



        public static UnitAttribute CreateInstance(Type _AttributeType, float _InitialValue)
        {
            if (!Factories.ContainsKey(_AttributeType))
                throw new InvalidOperationException($"{_AttributeType.Name} is not registered in attribute registry.");

            return Factories[_AttributeType].Invoke(_InitialValue);
        }



        private static Func<float, UnitAttribute> CreateFactory(Type _AttributeType)
        {
            var ctor = _AttributeType.GetConstructor(new Type[] { typeof(float) })
                ?? throw new InvalidOperationException($"{_AttributeType.Name} needs a default single float constructor.");

            var paramExpr = Expression.Parameter(typeof(float));
            var newExpr = Expression.New(ctor, paramExpr);
            var lambda = Expression.Lambda<Func<float, UnitAttribute>>(newExpr, paramExpr);
            return lambda.Compile();
        }
    }
}
