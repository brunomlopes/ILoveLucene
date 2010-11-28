using System;

namespace Core.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginConfigurationAttribute : Attribute
    {
    }
}