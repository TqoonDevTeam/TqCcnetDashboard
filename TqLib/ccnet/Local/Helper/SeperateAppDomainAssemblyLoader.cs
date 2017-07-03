using Exortech.NetReflector;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace TqLib.ccnet.Local.Helper
{
    public class SeperateAppDomainAssemblyLoader
    {
        public string CCNETServiceDirectory { get; set; } = string.Empty;

        public static bool IsDefinedAttributeData<T>(Type type)
        {
            return type.GetCustomAttributesData().Any(t => t.AttributeType.Equals(typeof(T)));
        }

        public static bool IsDefinedAttributeData<T>(PropertyInfo type)
        {
            return type.GetCustomAttributesData().Any(t => t.AttributeType.Equals(typeof(T)));
        }

        public PluginTypeInfo[] GetAssemblyTypes(FileInfo assemblyLocation)
        {
            PluginTypeInfo[] types = new PluginTypeInfo[] { };

            if (string.IsNullOrEmpty(assemblyLocation.Directory.FullName))
            {
                throw new InvalidOperationException(
                    "Directory can't be null or empty.");
            }

            if (!Directory.Exists(assemblyLocation.Directory.FullName))
            {
                throw new InvalidOperationException(
                   string.Format(CultureInfo.CurrentCulture,
                   "Directory not found {0}",
                   assemblyLocation.Directory.FullName));
            }

            AppDomain childDomain = BuildChildDomain(
                AppDomain.CurrentDomain);

            try
            {
                Type loaderType = typeof(AssemblyLoader);
                if (loaderType.Assembly != null)
                {
                    var loader =
                        (AssemblyLoader)childDomain.
                            CreateInstanceFrom(
                            loaderType.Assembly.Location,
                            loaderType.FullName).Unwrap();
                    loader.SetCCNETServiceDirectory(CCNETServiceDirectory);
                    loader.LoadAssembly(
                        assemblyLocation.FullName);

                    types =
                        loader.GetTypes(
                        assemblyLocation.Directory.FullName, assemblyLocation.FullName);
                }
                return types;
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }
        }

        private AppDomain BuildChildDomain(AppDomain parentDomain)
        {
            Evidence evidence = new Evidence(parentDomain.Evidence);
            AppDomainSetup setup = parentDomain.SetupInformation;
            return AppDomain.CreateDomain("DiscoveryRegion",
                evidence, setup);
        }

        private class AssemblyLoader : MarshalByRefObject
        {
            private string CCNETServiceDirectory = string.Empty;

            internal void SetCCNETServiceDirectory(string _CCNETServiceDirectory)
            {
                CCNETServiceDirectory = _CCNETServiceDirectory;
            }

            [SuppressMessage("Microsoft.Performance",
                "CA1822:MarkMembersAsStatic")]
            internal PluginTypeInfo[] GetTypes(string path, string dll)
            {
                PluginTypeInfo[] types = new PluginTypeInfo[] { };

                DirectoryInfo directory = new DirectoryInfo(path);
                ResolveEventHandler resolveEventHandler =
                    (s, e) =>
                    {
                        return OnReflectionOnlyResolve(
                            e, directory);
                    };

                AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler;

                types = Assembly.LoadFrom(dll).GetTypes().Where(t => IsDefinedAttributeData<ReflectorTypeAttribute>(t)).Select(t => new PluginTypeInfo(t)).ToArray();

                AppDomain.CurrentDomain.AssemblyResolve -= resolveEventHandler;

                return types;
            }

            private Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
            {
                Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                AssemblyName assemblyName = new AssemblyName(args.Name);

                string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.LoadFrom(dependentAssemblyFilename);
                }

                dependentAssemblyFilename = Path.Combine(CCNETServiceDirectory, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.LoadFrom(dependentAssemblyFilename);
                }

                return Assembly.LoadFrom(args.Name);
            }

            [SuppressMessage("Microsoft.Performance",
                "CA1822:MarkMembersAsStatic")]
            internal void LoadAssembly(String assemblyPath)
            {
                try
                {
                    Assembly.LoadFrom(assemblyPath);
                }
                catch (FileNotFoundException)
                {
                    /* Continue loading assemblies even if an assembly
                     * can not be loaded in the new AppDomain. */
                }
            }
        }
    }

    [Serializable]
    public class PluginTypeInfo
    {
        public string PluginName { get; private set; }

        public PropertyAttributeInfo[] PropertyInfos { get; private set; }

        public PluginTypeInfo(Type pluginType)
        {
            PluginName = GetPluginName(pluginType);
            PropertyInfos = pluginType.GetProperties().Where(t => SeperateAppDomainAssemblyLoader.IsDefinedAttributeData<ReflectorPropertyAttribute>(t)).Select(t => new PropertyAttributeInfo(t)).ToArray();
        }

        private string GetPluginName(Type pluginType)
        {
            var attr = pluginType.GetCustomAttributesData().First(t => typeof(ReflectorTypeAttribute).Equals(t.AttributeType));
            return attr.ConstructorArguments[0].Value.ToString();
        }

        [Serializable]
        public class PropertyAttributeInfo
        {
            public string PropertyTypeName { get; private set; }
            public AttributeData Attribute { get; private set; }

            public PropertyAttributeInfo(PropertyInfo prop)
            {
                PropertyTypeName = prop.PropertyType.Name;
                var attrData = prop.GetCustomAttributesData().FirstOrDefault(t => typeof(ReflectorPropertyAttribute).Equals(t.AttributeType));
                Attribute = new AttributeData();
                Attribute.Name = attrData.ConstructorArguments[0].Value.ToString();
                Attribute.Description = attrData.NamedArguments.FirstOrDefault(t => t.MemberName == "Description").TypedValue.Value?.ToString() ?? string.Empty;
                Attribute.Required = (bool)(attrData.NamedArguments.FirstOrDefault(t => t.MemberName == "Required").TypedValue.Value ?? true);
            }

            [Serializable]
            public class AttributeData
            {
                public string Description { get; set; }
                public string Name { get; set; }
                public bool Required { get; set; }
            }
        }
    }
}