using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hcs.Extensions.Odata.Queryable.Expressions
{
    public delegate object ServiceLocator(IExpressionParser injectTarget, Type serviceType, bool required);
    public delegate void ParserInjector(IExpressionParser injectTarget, ServiceLocator serviceLocator);
    public delegate void ParserParentInjector(IExpressionParser injectTarget, IExpressionParser current);
    public static class ExpressionParserInjectFactory
    {
        static Expression nullConst = Expression.Constant(null);
        public static ParserParentInjector CreateParentInjector(Type type)
        {
            var needInjectParent = type.GetProperties()
                 .Where(x => x.GetCustomAttribute<InjectParentAttribute>() != null)
                 .Where(x => typeof(IExpressionParser).IsAssignableFrom(x.PropertyType))
                 .Select(x => x.SetMethod);
            var targetParameter = Expression.Parameter(typeof(IExpressionParser), "instance");
            var selfParameter = Expression.Parameter(typeof(IExpressionParser), "parent");
            var converted = Expression.Variable(type, "c_instance");
            var expressions = new List<Expression>
            {
                Expression.Assign(converted, Expression.Convert(targetParameter, type))
            };
            expressions.AddRange(needInjectParent.Select(x => Expression.Call(converted, x, selfParameter)));

            var lamd = Expression.Lambda<ParserParentInjector>(
                Expression.Block(new ParameterExpression[] { converted }, expressions),
                    targetParameter,
                    selfParameter);
            return lamd.Compile();
        }
        public static ParserInjector CreateInjector(Type type)
        {
            var needInjectService = type.GetProperties()
                .Select(x => new { attr = x.GetCustomAttribute<InjectAttribute>(), property = x })
                .Where(x => x.attr != null)
                .Select(x => new { x.property.SetMethod, Type = x.property.PropertyType, x.attr.Required });
            var targetParameter = Expression.Parameter(typeof(IExpressionParser), "instance");
            var serviceLocatorParameter = Expression.Parameter(typeof(ServiceLocator));
            var converted = Expression.Variable(type, "c_instance");
            var expressions = new List<Expression>
            {
                Expression.Assign(converted, Expression.Convert(targetParameter, type))
            };
            expressions.AddRange(needInjectService.Select(x =>
            {
                var serviceVar = Expression.Variable(typeof(object), "service");
                return Expression.Block(new ParameterExpression[] { serviceVar },
                    Expression.Assign(serviceVar, Expression.Invoke(serviceLocatorParameter, converted, Expression.Constant(x.Type), Expression.Constant(x.Required))),
                    Expression.Call(converted, x.SetMethod,
                        Expression.Condition(Expression.NotEqual(serviceVar, nullConst),
                            Expression.Convert(serviceVar, x.Type),
                            Expression.Default(x.Type))));
            }));
            var lamd = Expression.Lambda<ParserInjector>(
                Expression.Block(new ParameterExpression[] { converted }, expressions),
                    targetParameter,
                    serviceLocatorParameter);
            return lamd.Compile();
        }
    }
}
