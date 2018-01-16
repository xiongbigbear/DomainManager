using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyHelper
{
    public class AssemblyLoader : MarshalByRefObject
    {
        public AssemblyLoader(IAssemblyResolver resolver)
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                    resolver.Resolve;
        }
        public Assembly ReadAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}
