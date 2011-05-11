using System;

namespace ElevationHelper.Services
{
    public static class Addresses
    {
        public static string AddressForType(Type t)
        {
            return "net.pipe://localhost/ILoveLucene.ElevationPipe/" + t.FullName;
        }
    }
}