using System;
using System.Collections.Generic;
using Core;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.Tasks;

namespace ILoveLucene.AutoUpdate
{
    public class VersionCondition : IUpdateCondition
    {
        private readonly Version _current;

        public VersionCondition(Version version)
        {
            Attributes = new Dictionary<string, string>();
            _current = version;
        }

        public bool IsMet(IUpdateTask task)
        {
            var desired = new Version(Attributes["version"]);

            return desired > _current;
        }

        public IDictionary<string, string> Attributes { get; set; }
    }
}