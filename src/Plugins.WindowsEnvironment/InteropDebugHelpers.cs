using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Plugins.WindowsEnvironment
{
    public class InteropDebugHelpers
    {
        private static void DebugPrintAllTypes(string name, object comObject, Assembly interopAss)
        {
            foreach (var type in GetAllTypes(comObject, interopAss))
            {
                Debug.WriteLine(new {name, type});
            }
        }

        public static Type[] GetAllTypes(object comObject, Assembly interopAss)
        {
            // get the com object and fetch its IUnknown
            IntPtr iunkwn = Marshal.GetIUnknownForObject(comObject);

            // enum all the types defined in the interop assembly
            Type[] excelTypes = interopAss.GetTypes();

            // find all types it implements
            ArrayList implTypes = new ArrayList();
            foreach (Type currType in excelTypes)
            {
                // com interop type must be an interface with valid iid
                Guid iid = currType.GUID;
                if (!currType.IsInterface || iid == Guid.Empty)
                    continue;

                // query supportability of current interface on object
                IntPtr ipointer;
                Marshal.QueryInterface(iunkwn, ref iid, out ipointer);

                if (ipointer != IntPtr.Zero)
                    implTypes.Add(currType);
            }

            // no implemented type found
            return (Type[]) implTypes.ToArray(typeof (Type));
        }
    }
}