using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainManager
{
    public interface IAssemblyResolver
    {
        Assembly Resolve(object sender, ResolveEventArgs args);
        List<string> AdditionalResolutionDirectories { get; set; }
    }

    [Serializable]
    public class DefaultAssemblyResolver:IAssemblyResolver
    {
        public string revitDirectory = "";

        public List<string> AdditionalResolutionDirectories { get; set; }

        public DefaultAssemblyResolver(string revitDirectory)
        {
            this.revitDirectory = revitDirectory;
            AdditionalResolutionDirectories = new List<string>();
        }

        public DefaultAssemblyResolver(string revitDirectory, List<string> additionalDirectories)
        {
            this.revitDirectory = revitDirectory;
            AdditionalResolutionDirectories = additionalDirectories;
        }

        public Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var dir = Path.GetDirectoryName(args.RequestingAssembly.Location);
            var depenFile = Path.Combine(dir, new AssemblyName(args.Name).Name + ".dll");
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

     
        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var dir = Path.GetDirectoryName(args.RequestingAssembly.Location);
            var depenFile = Path.Combine(dir, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(depenFile))
            {
                Assembly.LoadFrom(depenFile);
            }
            return null;
        }
    }
}
