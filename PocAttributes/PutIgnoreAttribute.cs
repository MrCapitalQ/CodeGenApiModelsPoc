using System;

namespace PocAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PutIgnoreAttribute : Attribute
    { }
}
