namespace DynaBee.FluentApi.DependencyInjection
{
    using System.Reflection;

    /// <summary>
    /// Extensions for discovering and registering DynaBee profiles via reflection.
    /// </summary>
    public static class DynaBeeProfileDiscoveryExtensions
    {
        /// <summary>
        /// Discovers all profile implementations from the provided assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>Materialized profile instances.</returns>
        public static IReadOnlyCollection<IDynaBeeProfile> DiscoverProfilesFromAssemblies(
            params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                return Array.Empty<IDynaBeeProfile>();

            var profileTypes = assemblies
                .Where(x => x != null && !x.IsDynamic)
                .Distinct()
                .SelectMany(SafeGetTypes)
                .Where(IsConcreteProfileType)
                .Distinct()
                .OrderBy(x => x.FullName, StringComparer.Ordinal)
                .ToArray();

            var profiles = new List<IDynaBeeProfile>(profileTypes.Length);
            foreach (var profileType in profileTypes)
            {
                if (Activator.CreateInstance(profileType) is IDynaBeeProfile profile)
                    profiles.Add(profile);
            }

            return profiles;
        }

        /// <summary>
        /// Discovers all profile implementations from currently loaded application assemblies.
        /// </summary>
        /// <returns>Materialized profile instances.</returns>
        public static IReadOnlyCollection<IDynaBeeProfile> DiscoverProfilesFromCurrentAppDomain()
            => DiscoverProfilesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        /// Discovers and registers all profile implementations from the provided assemblies.
        /// </summary>
        /// <param name="registry">Target registry.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>The same registry instance.</returns>
        public static IAssemblyContextRegistry AddProfilesFromAssemblies(
            this IAssemblyContextRegistry registry,
            params Assembly[] assemblies)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var profiles = DiscoverProfilesFromAssemblies(assemblies);
            foreach (var profile in profiles)
            {
                if (string.Equals(profile.AssemblyName, registry.AssemblyName, StringComparison.Ordinal))
                    registry.AddProfile(profile);
            }

            return registry;
        }

        /// <summary>
        /// Discovers and registers all profile implementations from currently loaded application assemblies.
        /// </summary>
        /// <param name="registry">Target registry.</param>
        /// <returns>The same registry instance.</returns>
        public static IAssemblyContextRegistry AddProfilesFromCurrentAppDomain(this IAssemblyContextRegistry registry)
            => registry.AddProfilesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null)!;
            }
        }

        private static bool IsConcreteProfileType(Type candidate)
            => typeof(IDynaBeeProfile).IsAssignableFrom(candidate)
               && !candidate.IsAbstract
               && !candidate.IsInterface
               && candidate.GetConstructor(Type.EmptyTypes) != null;
    }
}
