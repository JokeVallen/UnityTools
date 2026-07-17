using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 上下文工厂
    /// </summary>
    /// <remarks>
    /// <para>提供一系列创建各场景上下文的静态方法，返回强类型接口。</para>
    /// <para>
    /// 示例：
    /// <code>
    /// var context = ContextFactory.CreateMethodContext(attr, method, target, args);
    /// executor.Execute(context);
    /// </code>
    /// </para>
    /// </remarks>
    public static class ContextFactory
    {
        /// <summary>创建方法上下文</summary>
        public static IMethodContext CreateMethodContext(EasyAttribute attribute, MethodInfo method, object target, object[] arguments, CancellationToken cancellationToken = default)
        {
            return new MethodContext(attribute, method, target, arguments, cancellationToken);
        }

        /// <summary>创建属性上下文</summary>
        public static IPropertyContext CreatePropertyContext(EasyAttribute attribute, PropertyInfo property, PropertyAccessor accessor, object target, object value, CancellationToken cancellationToken = default)
        {
            return new PropertyContext(attribute, property, accessor, target, value, cancellationToken);
        }

        /// <summary>创建字段上下文</summary>
        public static IFieldContext CreateFieldContext(EasyAttribute attribute, FieldInfo field, object target, object value, CancellationToken cancellationToken = default)
        {
            return new FieldContext(attribute, field, target, value, cancellationToken);
        }

        /// <summary>创建类型上下文</summary>
        public static ITypeContext CreateTypeContext(EasyAttribute attribute, Type type, CancellationToken cancellationToken = default)
        {
            return new TypeContext(attribute, type, cancellationToken);
        }

        /// <summary>创建构造函数上下文</summary>
        public static IConstructorContext CreateConstructorContext(EasyAttribute attribute, ConstructorInfo constructor, object target, object[] arguments, CancellationToken cancellationToken = default)
        {
            return new ConstructorContext(attribute, constructor, target, arguments, cancellationToken);
        }

        /// <summary>创建参数上下文</summary>
        public static IParameterContext CreateParameterContext(EasyAttribute attribute, ParameterInfo parameter, object target, object[] arguments, CancellationToken cancellationToken = default)
        {
            var method = (MethodInfo)parameter.Member;
            return new ParameterContext(attribute, method, parameter, target, arguments, cancellationToken);
        }

        /// <summary>创建返回值上下文</summary>
        public static IReturnValueContext CreateReturnValueContext(EasyAttribute attribute, MethodInfo method, object target, CancellationToken cancellationToken = default)
        {
            return new ReturnValueContext(attribute, method, target, cancellationToken);
        }

        /// <summary>创建事件上下文</summary>
        public static IEventContext CreateEventContext(EasyAttribute attribute, EventInfo @event, EventAccessor accessor, object target, Delegate handler, CancellationToken cancellationToken = default)
        {
            return new EventContext(attribute, @event, accessor, target, handler, cancellationToken);
        }

        /// <summary>创建泛型参数上下文</summary>
        public static IGenericParameterContext CreateGenericParameterContext(EasyAttribute attribute, Type genericParameter, MemberInfo declaringMember, object target, CancellationToken cancellationToken = default)
        {
            return new GenericParameterContext(attribute, genericParameter, declaringMember, target, cancellationToken);
        }
    }
}