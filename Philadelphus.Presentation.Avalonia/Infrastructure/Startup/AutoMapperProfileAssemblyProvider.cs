using System.Reflection;

using AutoMapper;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Startup
{
    internal static class AutoMapperProfileAssemblyProvider
    {
        public static Assembly[] GetPhiladelphusProfileAssemblies()
        {
            var assembliesByName = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => IsPhiladelphusAssemblyName(assembly.GetName()))
                .GroupBy(x => x.GetName().Name)
                .ToDictionary(x => x.Key!, x => x.First());

            var referencesToLoad = new Queue<AssemblyName>(
                Assembly.GetExecutingAssembly()
                    .GetReferencedAssemblies()
                    .Where(IsPhiladelphusAssemblyName));

            while (referencesToLoad.Count > 0)
            {
                var assemblyName = referencesToLoad.Dequeue();
                if (assemblyName.Name == null || assembliesByName.ContainsKey(assemblyName.Name))
                {
                    continue;
                }

                var assembly = Assembly.Load(assemblyName);
                assembliesByName[assemblyName.Name] = assembly;

                foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies().Where(IsPhiladelphusAssemblyName))
                {
                    if (referencedAssemblyName.Name != null && assembliesByName.ContainsKey(referencedAssemblyName.Name) == false)
                    {
                        referencesToLoad.Enqueue(referencedAssemblyName);
                    }
                }
            }

            return assembliesByName.Values
                .Where(ContainsAutoMapperProfile)
                .ToArray();
        }

        private static bool IsPhiladelphusAssemblyName(AssemblyName assemblyName)
            => assemblyName.Name?.StartsWith("Philadelphus.", StringComparison.Ordinal) == true;

        private static bool ContainsAutoMapperProfile(Assembly assembly)
        {
            IEnumerable<Type> loadableTypes;
            try
            {
                loadableTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                loadableTypes = ex.Types.Where(type => type != null)!;
            }

            return loadableTypes
                .Any(type => typeof(Profile).IsAssignableFrom(type)
                    && type.Namespace?.StartsWith("Philadelphus.", StringComparison.Ordinal) == true);
        }
    }
}
