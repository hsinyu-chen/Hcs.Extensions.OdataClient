using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hcs.Extensions.OdataClient.Expressions
{
    static class ExpressionHelpers
    {
        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsPrimitive ||
                new Type[] {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
                }.Contains(type) ||
                type.IsEnum ||
                Convert.GetTypeCode(type) != TypeCode.Object ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]))
                ;
        }
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
        public static string ToCompareOperator(this ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => "eq",
                ExpressionType.NotEqual => "ne",
                ExpressionType.GreaterThan => "gt",
                ExpressionType.GreaterThanOrEqual => "ge",
                ExpressionType.LessThan => "lt",
                ExpressionType.LessThanOrEqual => "le",
                _ => throw new NotSupportedException($"{nameof(ToCompareOperator)} not support {type}"),
            };
        }
        public static string ToLogicOperator(this ExpressionType type)
        {
            return type switch
            {
                ExpressionType.AndAlso => "and",
                ExpressionType.OrElse => "or",
                _ => throw new NotSupportedException($"{nameof(ToLogicOperator)} not support {type}"),
            };
        }

        public static Expression GetRoot(this MemberExpression member)
        {
            if (member.Expression is MemberExpression m)
            {
                return m.GetRoot();
            }
            return member.Expression;
        }
        public static IEnumerable<string> GetMemberName(this LambdaExpression lambda)
        {
            var exp = lambda.Body;
            while (exp is UnaryExpression u)
            {
                exp = u.Operand;
            }
            if (exp is MemberExpression m)
            {
                return m.GetMemberName();
            }
            throw new NotSupportedException($"can't get member path of {lambda}");
        }
        public static IEnumerable<string> GetMemberName(this MemberExpression member)
        {
            if (member.Expression is MemberExpression m)
            {
                foreach (var s in m.GetMemberName())
                    yield return s;
            }
            if (member.Member.DeclaringType.IsGenericType && member.Member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                yield break;
            }
            yield return member.Member.Name;
        }
        public static Type GetMemberType(this Expression expression)
        {
            Type t = null;
            if (expression is MemberExpression member)
            {
                if (member.Member is PropertyInfo p)
                {
                    t = p.PropertyType;
                }
                else if (member.Member is FieldInfo f)
                {
                    t = f.FieldType;
                }
            }
            else if (expression is UnaryExpression u)
            {
                t = u.Operand.GetMemberType();
            }
            if (t != null)
            {
                t = Nullable.GetUnderlyingType(t) ?? t;
            }
            return t;
        }
        public static string GetEscapedValue(object value, Expression expressionOpside)
        {
            if (value == null)
            {
                return "null";
            }
            var type = expressionOpside.GetMemberType();
            if (type == typeof(string))
                return $"'{((string)value).Replace("'", "\\'")}'";
            else if (type == typeof(bool))
                return value.ToString().ToLowerInvariant();
            else if (type != null && type.IsEnum)
            {
                return $"'{Enum.ToObject(type, value).ToString().Replace("'", "\\'")}'";
            }
            return value.ToString();
        }
        public static string GetMathOperator(this ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Add => "add",
                ExpressionType.Subtract => "sub",
                ExpressionType.Multiply => "mul",
                ExpressionType.Divide => "div",
                ExpressionType.Modulo => "mod",
                _ => throw new NotSupportedException($"{nameof(GetMathOperator)} is not support {type}]"),
            };
        }
    }
}
