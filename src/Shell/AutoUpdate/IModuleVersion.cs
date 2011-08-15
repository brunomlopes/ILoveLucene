using System;

namespace ILoveLucene.AutoUpdate
{
    public interface IModuleVersion
    {
        string Module { get; }
        Version GetVersion();
    }
}