using System;
using System.ComponentModel.Composition;

namespace Core.API
{
    /// <summary>
    /// Imports configuration class, allowing for recomposition.
    /// (This means the value may change, check MEF's documenation to know when that happens)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ImportConfigurationAttribute : ImportAttribute
    {
        public ImportConfigurationAttribute()
        {
            AllowRecomposition = true;
        }

        public ImportConfigurationAttribute(Type contractType) : base(contractType)
        {
            AllowRecomposition = true;
        }

        public ImportConfigurationAttribute(string contractName) : base(contractName)
        {
            AllowRecomposition = true;
        }

        public ImportConfigurationAttribute(string contractName, Type contractType) : base(contractName, contractType)
        {
            AllowRecomposition = true;
        }
    }
}