﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainManager
{
    public class WsdlClassParser : MarshalByRefObject
    {
        public AppDomain LocalAppDomain = null;
        public string ErrorMessage = string.Empty;

        /// <summary>
        /// Creates a new instance of the WsdlParser in a new AppDomain
        /// </summary>
        /// <returns></returns>        
        public WsdlClassParser CreateWsdlClassParser()
        {
            this.CreateAppDomain(null);

            string AssemblyPath = Assembly.GetExecutingAssembly().Location;
            WsdlClassParser parser = null;
            try
            {
                parser = (WsdlClassParser)this.LocalAppDomain.CreateInstanceFrom(AssemblyPath,
                                                  typeof(WsdlClassParser).FullName).Unwrap();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            return parser;
        }

        public bool CreateAppDomain(string appDomain)
        {
            if (string.IsNullOrEmpty(appDomain))
                appDomain = "wsdlparser" + Guid.NewGuid().ToString().GetHashCode().ToString("x");

            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationName = appDomain;

            // *** Point at current directory
            domainSetup.ApplicationBase = Environment.CurrentDirectory;   // AppDomain.CurrentDomain.BaseDirectory;                 

            this.LocalAppDomain = AppDomain.CreateDomain(appDomain, null, domainSetup);

            // *** Need a custom resolver so we can load assembly from non current path
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            return true;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                Assembly assembly = System.Reflection.Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch
            { // ignore load error }

                // *** Try to load by filename - split out the filename of the full assembly name
                // *** and append the base path of the original assembly (ie. look in the same dir)
                // *** NOTE: this doesn't account for special search paths but then that never
                //           worked before either.
                string[] Parts = args.Name.Split(',');
                string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";

                return System.Reflection.Assembly.LoadFrom(File);
            }
            return null;
        }

    /// <summary>
    /// 
    /// </summary>
        public void Unload()
        {
            if (this.LocalAppDomain != null)
            {
                AppDomain.Unload(this.LocalAppDomain);
                this.LocalAppDomain = null;
            }
        }
    }
}
