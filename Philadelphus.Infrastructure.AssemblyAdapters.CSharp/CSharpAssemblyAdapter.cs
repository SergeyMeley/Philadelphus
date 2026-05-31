using System.Reflection;
using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.CSharp
{
    /// <summary>
    /// Loads trusted .NET assemblies and discovers exported types through reflection.
    /// </summary>
    public sealed class CSharpAssemblyAdapter : IAssemblyAdapter
    {
        public AssemblyAdapterLanguage Language => AssemblyAdapterLanguage.CSharp;

        public IReadOnlyCollection<string> SupportedFileExtensions { get; } = [".dll"];

        public AssemblyAdapterLoadResult Load(AssemblyAdapterLoadRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Path);

            var modules = new List<AssemblyAdapterModule>();
            var errors = new List<AssemblyAdapterLoadError>();

            foreach (var path in ResolveAssemblyFiles(request))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    modules.Add(new AssemblyAdapterModule
                    {
                        Language = Language,
                        SourcePath = path,
                        LoadedArtifact = assembly
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(CreateError(path, $"Failed to load C# assembly '{path}'.", ex));
                }
            }

            return new AssemblyAdapterLoadResult(modules, errors);
        }

        public CSharpAssemblyInstanceLoadResult<TContract> CreateInstances<TContract>(
            AssemblyAdapterLoadResult loadResult)
            where TContract : class
        {
            ArgumentNullException.ThrowIfNull(loadResult);

            var instances = new List<TContract>();
            var errors = new List<AssemblyAdapterLoadError>(loadResult.Errors);

            foreach (var module in loadResult.Modules.Where(x => x.Language == Language))
            {
                if (module.LoadedArtifact is not Assembly assembly)
                {
                    errors.Add(CreateError(module.SourcePath, "Loaded C# module does not contain a reflection assembly."));
                    continue;
                }

                foreach (var type in GetLoadableTypes(assembly, module.SourcePath, errors))
                {
                    if (IsPublicContractImplementation<TContract>(type) == false)
                    {
                        continue;
                    }

                    try
                    {
                        if (Activator.CreateInstance(type) is TContract instance)
                        {
                            instances.Add(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(CreateError(
                            module.SourcePath,
                            $"Failed to create '{typeof(TContract).FullName}' instance from type '{type.FullName}'.",
                            ex));
                    }
                }
            }

            return new CSharpAssemblyInstanceLoadResult<TContract>(instances, errors);
        }

        private static bool IsPublicContractImplementation<TContract>(Type type)
        {
            return typeof(TContract).IsAssignableFrom(type)
                && type.IsAbstract == false
                && type.IsInterface == false
                && (type.IsPublic || type.IsNestedPublic);
        }

        private IEnumerable<string> ResolveAssemblyFiles(AssemblyAdapterLoadRequest request)
        {
            if (File.Exists(request.Path))
            {
                return SupportedFileExtensions.Contains(Path.GetExtension(request.Path), StringComparer.OrdinalIgnoreCase)
                    ? [Path.GetFullPath(request.Path)]
                    : [];
            }

            if (Directory.Exists(request.Path))
            {
                var option = request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory
                    .EnumerateFiles(request.Path, "*.dll", option)
                    .Select(Path.GetFullPath)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            return [Path.GetFullPath(request.Path)];
        }

        private IEnumerable<Type> GetLoadableTypes(
            Assembly assembly,
            string sourcePath,
            List<AssemblyAdapterLoadError> errors)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                errors.Add(CreateError(sourcePath, $"Some types from '{sourcePath}' could not be loaded.", ex));
                return ex.Types.Where(type => type is not null)!;
            }
            catch (Exception ex)
            {
                errors.Add(CreateError(sourcePath, $"Types from '{sourcePath}' could not be inspected.", ex));
                return [];
            }
        }

        private AssemblyAdapterLoadError CreateError(
            string sourcePath,
            string message,
            Exception? exception = null)
        {
            return new AssemblyAdapterLoadError
            {
                Language = Language,
                SourcePath = sourcePath,
                Message = message,
                Exception = exception
            };
        }
    }
}
