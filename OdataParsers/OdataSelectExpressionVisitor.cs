using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Hcs.Extensions.OdataClient.OdataParsers
{
    class OdataSelectExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var paramter = node.GetRoot() as ParameterExpression;
            var name = string.Join("/", node.GetMemberName());
            if (!map[paramter].SelectMembers.Any(x => x.MemberPath == name))
            {
                map[paramter].SelectMembers.Add(new SelectMember { MemberPath=name, IsComplexType=!(node.Member is PropertyInfo m && m.PropertyType.IsSimpleType()) });
            }
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var member = node.Arguments[0] as MemberExpression;
            var parent = member.GetRoot() as ParameterExpression;
            var lamd = node.Arguments[1] as LambdaExpression;
            var path = string.Join("/", member.GetMemberName());
            var c = map[parent].Childs.Where(x => x.MemberPath == path).FirstOrDefault();
            if (c == null)
            {
                map.Add(lamd.Parameters[0], new OdataSelectExpand { MemberPath = path });

                map[parent].Childs.Add(map[lamd.Parameters[0]]);
            }
            else
            {
                map.Add(lamd.Parameters[0], c);
            }
            return base.VisitMethodCall(node);
        }
        public override Expression Visit(Expression node)
        {
            if (node is LambdaExpression || node is NewExpression || node is MemberExpression)
            {
                return base.Visit(node);
            }
            else if (node is MethodCallExpression call)
            {
                if (call.Method.DeclaringType == typeof(Enumerable))
                {
                    if (call.Method.Name == nameof(Enumerable.Select))
                    {
                        return base.Visit(node);
                    }
                }
            }
            else if (node is ParameterExpression)
            {
                return base.Visit(node);
            }
            if (node != null)
            {
                throw new NotSupportedException($"Select not support {node.GetType()}[{node}]");
            }
            return base.Visit(node);
        }
        Dictionary<ParameterExpression, OdataSelectExpand> map;
        public OdataSelectExpand Parse(LambdaExpression expression)
        {
            map = new Dictionary<ParameterExpression, OdataSelectExpand>();
            var root = new OdataSelectExpand();
            map.Add(expression.Parameters[0], root);
            Visit(expression);
            return root;
        }
    }
}
