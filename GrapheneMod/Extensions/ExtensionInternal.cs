using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Logger = GrapheneMod.Logging.Logging;

namespace GrapheneMod.Extensions
{
    internal static class ExtensionInternal
    {
        #region Core Variables
        private static Dictionary<string, Assembly> _Extensions = new Dictionary<string, Assembly>();
        private static Dictionary<Assembly, List<Type>> _Buffer_IExtensions = new Dictionary<Assembly, List<Type>>();

        private static Dictionary<string, string> _AssemblyNames = new Dictionary<string, string>();
        #endregion

        #region Core Properties
        public static ReadOnlyCollection<Assembly> Extensions => _Extensions.Values.ToList().AsReadOnly();
        #endregion

        #region Extensions Paths
        public static string ExtensionsDirectory => GrapheneMod.CurrentDirectory + "/Extensions";
        #endregion

        static ExtensionInternal()
        {
            // Setup .NET events
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        #region Core Functions
        // Register functions
        public static void RegisterAssemblyName(string path)
        {
            // Run checks
            if (!path.EndsWith(".dll"))
                return;
            if (!File.Exists(path))
                return;

            // Save assembly name
            AssemblyName name = AssemblyName.GetAssemblyName(path);
            if (!_AssemblyNames.ContainsKey(name.FullName))
                _AssemblyNames.Add(name.FullName, path);
        }
        public static Assembly RegisterAssembly(Assembly assembly)
        {
            // Save the assembly
            if (!_Extensions.ContainsKey(assembly.FullName))
                _Extensions.Add(assembly.FullName, assembly);
            return assembly;
        }

        // Resolve functions
        public static Assembly ResolveAssemblyName(string name)
        {
            // Check if the assembly is a loaded extension
            if (_Extensions.TryGetValue(name, out Assembly assembly))
                return assembly;

            // Check if the assembly is in the module names
            if (_AssemblyNames.TryGetValue(name, out string path))
                return RegisterAssembly(Assembly.LoadFile(path));

            return null;
        }

        // Execution functions
        public static void LoadAssemblies()
        {
            foreach(string key in _AssemblyNames.Keys)
                if (!_Extensions.ContainsKey(key))
                    RegisterAssembly(Assembly.LoadFile(_AssemblyNames[key]));
        }
        public static void LoadExtensions()
        {
            foreach(Assembly assembly in _Extensions.Values)
            {
                // Variable declaration
                string name = assembly.GetName().Name;

                Logger.LogImportant("Loading extension: " + name);
                foreach(Type @class in assembly.GetTypes())
                {
                    // Check for extensions
                    if (typeof(IExtension).IsAssignableFrom(@class))
                    {
                        if (!_Buffer_IExtensions.ContainsKey(assembly))
                            _Buffer_IExtensions.Add(assembly, new List<Type>());
                        _Buffer_IExtensions[assembly].Add(@class);
                    }

                    // Check for extension parsers
                    if (typeof(ExtensionParser<IExtension>).IsAssignableFrom(@class))
                    {

                    }
                }
                Logger.LogImportant("Loaded extension: " + name);
            }
        }
        #endregion

        #region .NET Event Functions
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = ResolveAssemblyName(args.Name);

            if (assembly == null)
                Logger.LogError("Unable to resolve the dependency/extension " + args.Name + " make sure it is in your Extensions folder!");
            return assembly;
        }
        #endregion
    }
}
