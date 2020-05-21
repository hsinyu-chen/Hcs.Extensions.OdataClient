using System;
using System.Collections.Generic;
using System.Text;

namespace Hcs.Extensions.OdataClient.Expressions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InjectParentAttribute : Attribute
    {
    }
}
