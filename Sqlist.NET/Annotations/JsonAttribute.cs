using System;

namespace Sqlist.NET.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonAttribute : Attribute
    {
    }
}
