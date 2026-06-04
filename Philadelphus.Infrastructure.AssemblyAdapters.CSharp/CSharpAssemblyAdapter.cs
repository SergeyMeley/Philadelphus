using System.Reflection;
using System.Security.Cryptography;
using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.CSharp
{
    /// <summary>
    /// Загружает доверенные .NET-сборки и находит экспортируемые типы через reflection.
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
            var allowedSha256Hashes = NormalizeAllowedSha256Hashes(request, errors);

            foreach (var path in ResolveAssemblyFiles(request))
            {
                if (IsAssemblyHashAllowed(path, allowedSha256Hashes, errors) == false)
                {
                    continue;
                }

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
                    errors.Add(CreateError(path, $"Не удалось загрузить C#-сборку '{path}'.", ex));
                }
            }

            return new AssemblyAdapterLoadResult(modules, errors);
        }

        private HashSet<string> NormalizeAllowedSha256Hashes(
            AssemblyAdapterLoadRequest request,
            List<AssemblyAdapterLoadError> errors)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var hash in request.AllowedSha256Hashes)
            {
                var normalizedHash = NormalizeSha256Hash(hash);
                if (normalizedHash is null)
                {
                    errors.Add(CreateError(
                        request.Path,
                        $"Список разрешенных SHA-256 для C#-сборок содержит недопустимое значение '{hash}'."));
                    continue;
                }

                result.Add(normalizedHash);
            }

            return result;
        }

        private bool IsAssemblyHashAllowed(
            string path,
            HashSet<string> allowedSha256Hashes,
            List<AssemblyAdapterLoadError> errors)
        {
            if (allowedSha256Hashes.Count == 0)
            {
                errors.Add(CreateError(
                    path,
                    "Для загрузки C#-сборки требуется явный список разрешенных SHA-256."));
                return false;
            }

            string actualHash;
            try
            {
                actualHash = ComputeSha256Hash(path);
            }
            catch (Exception ex)
            {
                errors.Add(CreateError(path, $"Не удалось вычислить SHA-256-хэш C#-сборки '{path}'.", ex));
                return false;
            }

            if (allowedSha256Hashes.Contains(actualHash))
            {
                return true;
            }

            errors.Add(CreateError(
                path,
                $"C#-сборка '{path}' отсутствует в списке разрешенных SHA-256."));
            return false;
        }

        private static string? NormalizeSha256Hash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            var normalizedHash = hash.Trim();
            const string Prefix = "sha256:";
            if (normalizedHash.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                normalizedHash = normalizedHash[Prefix.Length..];
            }

            return normalizedHash.Length == 64 && normalizedHash.All(Uri.IsHexDigit)
                ? normalizedHash.ToLowerInvariant()
                : null;
        }

        private static string ComputeSha256Hash(string path)
        {
            using var file = File.OpenRead(path);
            var hash = SHA256.HashData(file);
            return Convert.ToHexString(hash).ToLowerInvariant();
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
                    errors.Add(CreateError(module.SourcePath, "Загруженный C#-модуль не содержит reflection-сборку."));
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
                            $"Не удалось создать экземпляр '{typeof(TContract).FullName}' из типа '{type.FullName}'.",
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
                errors.Add(CreateError(sourcePath, $"Не удалось загрузить часть типов из '{sourcePath}'.", ex));
                return ex.Types.Where(type => type is not null)!;
            }
            catch (Exception ex)
            {
                errors.Add(CreateError(sourcePath, $"Не удалось проанализировать типы из '{sourcePath}'.", ex));
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
