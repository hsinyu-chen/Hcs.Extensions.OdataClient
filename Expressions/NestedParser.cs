using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.Expressions
{
    public class NestedParser : IExpressionParser, ICollection<IExpressionParser>
    {
        protected string Name { get; }
        protected List<IExpressionParser> Parsers { get; } = new List<IExpressionParser>();
        protected Func<Expression, bool> IsMatch { get; }
        [InjectParent]
        public IExpressionParser Parent { get; set; }
        public Dictionary<Type, object> Services { get; set; }
        public NestedParser(string name, Func<Expression, bool> isMatch = null, IExpressionParser[] parsers = null, object[] services = null)
        {
            IsMatch = isMatch;
            Name = name;
            Services = new Dictionary<Type, object>();
            if (services != null)
            {
                foreach (var s in services)
                {
                    Services[s.GetType()] = s;
                }
            }
            if (parsers != null)
            {
                foreach (var item in parsers)
                {
                    Add(item);
                }
            }
        }
        bool inited = false;
        public void Init()
        {
            if (inited == false)
            {
                inited = true;
                foreach (var s in Services)
                {
                    if (typeof(IExpressionParser).IsAssignableFrom(s.Key))
                    {
                        var injector = childParserInjector.GetOrAdd(s.Value.GetType(), ExpressionParserInjectFactory.CreateParentInjector);
                        injector((IExpressionParser)s.Value, this);
                        var sinjector = serviceInjector.GetOrAdd(s.Value.GetType(), ExpressionParserInjectFactory.CreateInjector);
                        sinjector((IExpressionParser)s.Value, (t, s, r) => LocateService(t, s, r, Enumerable.Empty<string>()));
                    }
                }
                foreach (var item in Parsers)
                {
                    var injector = childParserInjector.GetOrAdd(item.GetType(), ExpressionParserInjectFactory.CreateParentInjector);
                    injector(item, this);
                    var sinjector = serviceInjector.GetOrAdd(item.GetType(), ExpressionParserInjectFactory.CreateInjector);
                    sinjector(item, (t, s, r) => LocateService(t, s, r, Enumerable.Empty<string>()));
                }
            }
        }
        static ConcurrentDictionary<Type, ParserInjector> serviceInjector = new ConcurrentDictionary<Type, ParserInjector>();
        static ConcurrentDictionary<Type, ParserParentInjector> childParserInjector = new ConcurrentDictionary<Type, ParserParentInjector>();
        class ServiceNotFoundException : Exception
        {
            public ServiceNotFoundException(IExpressionParser injectTarget, Type service, IEnumerable<string> serachPath)
                : base($"{injectTarget.GetType()} required service [{service.Name}] not found in {(string.Join("=>", serachPath.Select(x => $"[{x}]")))}")
            {

            }
        }
        protected object LocateService(IExpressionParser injectTarget, Type serviceType, bool required, IEnumerable<string> serachPath)
        {
            if (serviceType == GetType())
            {
                return this;
            }
            if (Services.ContainsKey(serviceType))
            {
                return Services[serviceType];
            }
            if (Parent is NestedParser nestedParser)
            {
                return nestedParser.LocateService(injectTarget, serviceType, required, serachPath.Append(Name));
            }
            if (required)
            {
                throw new ServiceNotFoundException(injectTarget, serviceType, serachPath.Append(Name));
            }
            else
            {
                return null;
            }
        }
        public void Add(IExpressionParser item)
        {
            Parsers.Add(item);
        }

        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            Init();
            if (IsMatch == null || IsMatch(context.Node))
            {
                foreach (var parser in Parsers)
                {
                    if (parser.TryParse(context, withParameterName, out string innerOutput))
                    {
                        output = innerOutput;
                        return true;
                    }
                }
                throw new NotSupportedException($"{Name} is not supported[{context.Node}]");
            }
            output = null;
            return false;
        }

        #region ICollection
        IEnumerator IEnumerable.GetEnumerator() => Parsers.GetEnumerator();
        public int Count => Parsers.Count;

        public bool IsReadOnly => false;



        public void Clear() => Parsers.Clear();

        public bool Contains(IExpressionParser item) => Parsers.Contains(item);

        public void CopyTo(IExpressionParser[] array, int arrayIndex) => Parsers.CopyTo(array, arrayIndex);

        public IEnumerator<IExpressionParser> GetEnumerator() => Parsers.GetEnumerator();

        public bool Remove(IExpressionParser item) => Parsers.Remove(item);
        #endregion
    }
}
