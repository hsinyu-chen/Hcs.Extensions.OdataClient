using Hcs.Extensions.Odata.Queryable.OdataParsers;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Hcs.Extensions.OdataClient.OdataParsers
{
    public class OdataSelectExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var paramter = node.GetRoot() as ParameterExpression;
            var name = string.Join("/", node.GetMemberName());
            if (!map[paramter].SelectMembers.Any(x => x.MemberPath == name))
            {
                map[paramter].SelectMembers.Add(new SelectMember { MemberPath = name, IsComplexType = !(node.Member is PropertyInfo m && m.PropertyType.IsSimpleType()) });
            }
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Enumerable.Select))
            {
                if (node.Arguments[0] is MemberExpression member)
                {
                    var lamd = node.Arguments[1] as LambdaExpression;
                    AddExpand(lamd, member);
                    return base.VisitMethodCall(node);
                }
                else if (node.Arguments[0] is MethodCallExpression chain)
                {
                    return VistWhere(chain, node);
                }
            }
            else if (node.Method.Name == nameof(Enumerable.Where))
            {
                return VistWhere(node, node);
            }
            throw new NotSupportedException($"Not Supported method call [{node}]");
        }
        protected Expression VistWhere(MethodCallExpression node, MethodCallExpression root)
        {
            var filters = new List<string>();
            MemberExpression BuildFilter(MethodCallExpression sub)
            {
                if (node.Method.DeclaringType == typeof(Enumerable) && node.Method.Name == nameof(Enumerable.Where))
                {
                    var filter = LambdaFilterParser.Parse(sub.Arguments[1] as LambdaExpression);
                    filters.Add(filter);
                    if (node.Arguments[0] is MethodCallExpression next)
                    {
                        return BuildFilter(next);
                    }
                    else if (node.Arguments[0] is MemberExpression m)
                    {
                        return m;
                    }
                }
                throw new NotSupportedException($"Child Select can only has Where beetween member and Select [{sub}]");
            }
            var rootMember = BuildFilter(node);
            AddExpand(node.Arguments[1] as LambdaExpression, rootMember, string.Join(" and ", filters));
            if (root != node)
            {
                return base.Visit(Expression.Call(null, root.Method, rootMember, root.Arguments[1]));
            }
            return root;
        }
        private void AddExpand(LambdaExpression lamd, MemberExpression member, string filter = null)
        {
            var parent = member.GetRoot() as ParameterExpression;
            var path = string.Join("/", member.GetMemberName());
            var c = map[parent].Childs.Where(x => x.MemberPath == path).FirstOrDefault();
            if (c == null)
            {
                map.Add(lamd.Parameters[0], new OdataSelectExpand { MemberPath = path, Filter = filter });

                map[parent].Childs.Add(map[lamd.Parameters[0]]);
            }
            else
            {
                map.Add(lamd.Parameters[0], c);
            }
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return base.Visit(node);
            }
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
                    else if (call.Method.Name == nameof(Enumerable.Where))
                    {
                        return base.Visit(node);
                    }
                }
            }
            else if (node is ParameterExpression)
            {
                return base.Visit(node);
            }
            else if (node is MemberInitExpression)
            {
                return base.Visit(node);
            }
            else if (node.NodeType == ExpressionType.Convert && node is UnaryExpression u)
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
