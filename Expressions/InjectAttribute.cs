using System;

namespace Hcs.Extensions.OdataClient.Expressions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
        public bool Required { get; } = true;
    }
}
