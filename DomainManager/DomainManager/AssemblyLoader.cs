using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainManager
{
   [Serializable]
    public class AssemblyLoader:MarshalByRefObject
    {
        private IAssemblyResolver _resolver = null;
        public AssemblyLoader(IAssemblyResolver resolver)
        {
            _resolver = resolver;
        }


        public Assembly ReadAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        public Assembly TempDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
