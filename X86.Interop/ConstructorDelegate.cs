using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace X86.Interop
{
    /// <summary>
    /// Courtesy of Yappi
    /// http://yappi.codeplex.com/
    /// </summary>
    internal static class ConstructorDelegate
    {
            /// <summary>
            /// Searches an instanceType constructor with delegateType-matching signature and constructs delegate of delegateType creating new instance of instanceType.
            /// Instance is casted to delegateTypes's return type. 
            /// Delegate's return type must be assignable from instanceType.
            /// </summary>
            /// <param name="delegateType">Type of delegate, with constructor-corresponding signature to be constructed.</param>
            /// <param name="instanceType">Type of instance to be constructed.</param>
            /// <returns>Delegate of delegateType wich constructs instance of instanceType by calling corresponding instanceType constructor.</returns>
            internal static Delegate Compile(Type delegateType, Type instanceType)
            {
                if (!typeof(Delegate).IsAssignableFrom(delegateType))
                {
                    throw new ArgumentException(String.Format("{0} is not a Delegate type.", delegateType.FullName), nameof(delegateType));
                }
                var invoke = delegateType.GetMethod("Invoke");
                var parameterTypes = invoke.GetParameters().Select(pi => pi.ParameterType).ToArray();
                var resultType = invoke.ReturnType;
                if (!resultType.IsAssignableFrom(instanceType))
                {
                    throw new ArgumentException(String.Format("Delegate's return type ({0}) is not assignable from {1}.", resultType.FullName, instanceType.FullName));
                }
                var ctor = instanceType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                if (ctor == null)
                {
                    throw new ArgumentException("Can't find constructor with delegate's signature", nameof(instanceType));
                }
                var parapeters = parameterTypes.Select(Expression.Parameter).ToArray();

                var newExpression = Expression.Lambda(delegateType,
                    Expression.Convert(Expression.New(ctor, parapeters), resultType),
                    parapeters);
                var @delegate = newExpression.Compile();
                return @delegate;
            }
            internal static TDelegate Compile<TDelegate>(Type instanceType)
            {
                return (TDelegate)(object)Compile(typeof(TDelegate), instanceType);
            }
    }
}
