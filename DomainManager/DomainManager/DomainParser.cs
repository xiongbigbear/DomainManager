using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DomainManager
{
    public class DomainParser : MarshalByRefObject
    {
        public AppDomain LocalAppDomain = null;
        public string revitDirectory = "";
        public object[] Parameters = null;
        public List<string> AdditionalResolutionDirectories { get; set; }
      



        public Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var depenFile = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(depenFile);
            if (File.Exists(depenFile))
            {
                return Assembly.LoadFrom(depenFile);
            }

            var dirInfo = new DirectoryInfo(dir);
            var assembly = SearchChildren(args, dirInfo);
            if (assembly != null)
            {
                Console.WriteLine("Found assembly:{0}", assembly.Location);
                return assembly;
            }

            foreach (var path in AdditionalResolutionDirectories)
            {
                var result = AttemptLoadFromDirectory(args, path);
                if (result != null)
                {
                    Console.WriteLine("Found assembly:{0}", result.Location);
                    return result;
                }
            }

            // Search upstream of the test assembly
            for (var i = 0; i < 3; i++)
            {
                dirInfo = dirInfo.Parent;
                assembly = SearchChildren(args, dirInfo);
                if (assembly != null)
                {
                    return assembly;
                }

                depenFile = Path.Combine(dirInfo.FullName, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(depenFile))
                {
                    return Assembly.LoadFrom(depenFile);
                }
            }

            depenFile = Path.Combine(revitDirectory, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(depenFile))
            {
                return Assembly.LoadFrom(depenFile);
            }

            // If the above fail, attempt to load from the GAC
            try
            {
                return Assembly.LoadFrom(args.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private Assembly AttemptLoadFromDirectory(ResolveEventArgs args, string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var dirInfo = new DirectoryInfo(directory);

            var testFile = Path.Combine(dirInfo.FullName, new AssemblyName(args.Name).Name + ".dll");
            return !File.Exists(testFile) ? null : Assembly.ReflectionOnlyLoadFrom(testFile);
        }

        private static Assembly SearchChildren(ResolveEventArgs args, DirectoryInfo dirInfo)
        {
            var children = dirInfo.GetDirectories();
            foreach (var child in children)
            {
                var depenFile = Path.Combine(child.FullName, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(depenFile))
                {
                    return Assembly.ReflectionOnlyLoadFrom(depenFile);
                }
            }
            return null;
        }
        public DomainParser CreateParser()
        {
            CreateAppDomain("AddinDomain");
            string AssemblyPath = Assembly.GetExecutingAssembly().Location;
            DomainParser parser = null;
            try
            {
                 parser = (DomainParser)LocalAppDomain.CreateInstanceFrom(AssemblyPath,
                typeof(DomainParser).FullName).Unwrap();

                //parser = (DomainParser)LocalAppDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(DomainParser).FullName, false, 0, null, new object[] { revitDirectory,AdditionalResolutionDirectories}, CultureInfo.InvariantCulture, null);

            }
            catch (Exception ex)
            {

            }
            return parser;
        }

        public void Run(string assemblyPath,string Name)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
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
                System.Windows.MessageBox.Show(string.Format("{0} type not contain empty Constructor",Name));
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
            Unload();
        }

        private AppDomain CreateAppDomain(string appDomain)
        {
            if (string.IsNullOrEmpty(appDomain))
                appDomain = "addin" + Guid.NewGuid().ToString().GetHashCode().ToString("x");
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationName = appDomain;
            domainSetup.ApplicationBase = Environment.CurrentDirectory;                  
            LocalAppDomain = AppDomain.CreateDomain(appDomain, null, domainSetup);
            AppDomain.CurrentDomain.AssemblyResolve +=Resolve;
            return LocalAppDomain;
        }
        public AssemblyData ReadAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
           
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

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                Assembly assembly = System.Reflection.Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch
            { 
                string[] Parts = args.Name.Split(',');
                string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";
                return System.Reflection.Assembly.LoadFrom(File);
            }
            return null;
        }

        public void Unload()
        {
            if (LocalAppDomain != null)
            {
                AppDomain.Unload(LocalAppDomain);
                LocalAppDomain = null;
            }
        }
    }
}
