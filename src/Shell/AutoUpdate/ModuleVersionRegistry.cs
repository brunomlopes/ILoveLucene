using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core;

namespace ILoveLucene.AutoUpdate
{
    public class ModuleVersionRegistry
    {
        private Dictionary<string, IModuleVersion> _moduleVersions;

        [ImportMany]
        public IEnumerable<IModuleVersion> ModuleVersions
        {
            get { return _moduleVersions.Values; }
            set { _moduleVersions = value.ToDictionary(m => m.Module.ToLowerInvariant()); }
        }

        public Version VersionForCoreModule()
        {
            Version version;
            if(!Version.TryParse(ProgramVersionInformation.Version, out version))
            {
                // if core isn't a real version, we're working on devel, don't upgrade
                version = new Version(int.MaxValue, int.MaxValue);
            }
            return version;
        }


        public Version VersionForModule(string module)
        {
            module = module.ToLowerInvariant();
            if(!_moduleVersions.ContainsKey(module))
            {
                return new Version(0,0,0,0);
            }
            return _moduleVersions[module].GetVersion();
        }
    }
}