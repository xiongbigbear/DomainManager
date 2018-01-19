using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DomainManager
{
   [Serializable]
    public class AssemblyLoader
    {
        IAssemblyResolver resolver = null;
        private Assembly assembly = null;
        public AssemblyLoader(IAssemblyResolver _resolver)
        {
            resolver = _resolver;
            //AppDomain.CurrentDomain.AssemblyResolve += resolver.Resolve;
        }
        public AssemblyData LoadAssembly(string assemblyPath)
        {
            AppDomain.CurrentDomain.AssemblyResolve += resolver.Resolve;
            var bytes = File.ReadAllBytes(assemblyPath);
            assembly = Assembly.LoadFrom(assemblyPath);
            if (assembly == null)
            {
                MessageBox.Show("resolve failed");
                return null;
            }
            var types = assembly.GetTypes().Where(x => x.GetAttribue<TransactionAttribute>() != null && x.GetInterface(typeof(IExternalCommand).FullName) != null).ToList();
            if (types.Count == 0)
            {
                return null;
            }

            var model = new AssemblyData() { Name = assembly.GetName().Name, Path = assemblyPath };
            foreach (var item in types)
            {
                model.Children.Add(new AssemblyData()
                {
                    Name = item.FullName,
                    Path = assemblyPath,
                    Parent = model
                });
            }
            return model;
        }

        public void RunAssembly(AssemblyData data,object[] Parameters)
        {
            string assemblyPath = data.Path;
            string Name = data.Name;
            if (assembly == null)
            {
                MessageBox.Show("resolve failed");
                return;
            }
            var type = assembly.GetType(Name);
            if (type == null)
            {
                System.Windows.MessageBox.Show(string.Format(" not found {0} type", Name));
                return;
            }
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, new Type[0], null);
            if (ctor == null)
            {
                System.Windows.MessageBox.Show(string.Format("{0} type not contain empty Constructor", Name));
                return;
            }
            var method = type.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                System.Windows.MessageBox.Show(string.Format("not found {0} Execute", method.Name));
                return;
            }
            var instance = ctor.Invoke(null);
            method.Invoke(instance, Parameters);
        }
    }
}
