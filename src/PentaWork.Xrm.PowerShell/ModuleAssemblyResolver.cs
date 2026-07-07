using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace PentaWork.Xrm.PowerShell
{
    /// <summary>
    /// PowerShell only auto-generates assembly binding redirects for the host executable's own
    /// .config file (e.g. powershell.exe.config), never for a loaded module's .dll.config. Some of
    /// our dependencies (e.g. Scriban, which targets netstandard2.0) pull in BCL facade assemblies
    /// whose exact referenced version doesn't match the version restored next to this module, which
    /// the CLR treats as a hard load failure without a redirect. Resolve those manually instead.
    /// </summary>
    public class ModuleAssemblyResolver : IModuleAssemblyInitializer
    {
        public void OnImport()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromModuleDirectory;
        }

        private static Assembly? ResolveFromModuleDirectory(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            var moduleDirectory = Path.GetDirectoryName(typeof(ModuleAssemblyResolver).Assembly.Location)!;
            var candidatePath = Path.Combine(moduleDirectory, assemblyName + ".dll");
            return File.Exists(candidatePath) ? Assembly.LoadFrom(candidatePath) : null;
        }
    }
}
