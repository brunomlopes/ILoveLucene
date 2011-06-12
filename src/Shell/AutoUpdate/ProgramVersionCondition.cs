using System;
using System.Collections.Generic;
using Core;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.Tasks;

namespace ILoveLucene.AutoUpdate
{
    public class ProgramVersionCondition : IUpdateCondition
    {
        public ProgramVersionCondition()
        {
            Attributes = new Dictionary<string, string>();
        }

        public bool IsMet(IUpdateTask task)
        {
            // If we're working on devel, don't update
            if (ProgramVersionInformation.Version == "devel")
                return false;

            var current = new Version(ProgramVersionInformation.Version);

            var desired = new Version(Attributes["version"]);

            return desired > current;
        }

        public IDictionary<string, string> Attributes { get; set; }
    }
}