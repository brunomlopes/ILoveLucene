using System;
using System.ComponentModel.Composition;

namespace Core.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginConfigurationAttribute : ExportAttribute
    {
    }
}