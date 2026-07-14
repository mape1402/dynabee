namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Helpers for adding reusable profiles to an assembly context registry.
    /// </summary>
    public static class AssemblyContextRegistryExtensions
    {
        /// <summary>
        /// Adds a profile instance to the registry.
        /// </summary>
        /// <param name="registry">Target registry.</param>
        /// <param name="profile">Profile instance.</param>
        /// <returns>The same registry instance.</returns>
        public static IAssemblyContextRegistry AddProfile(
            this IAssemblyContextRegistry registry,
            IDynaBeeProfile profile)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            registry.AddProfile(profile);
            return registry;
        }

        /// <summary>
        /// Adds a profile by type using a public parameterless constructor.
        /// </summary>
        /// <typeparam name="TProfile">Profile type.</typeparam>
        /// <param name="registry">Target registry.</param>
        /// <returns>The same registry instance.</returns>
        public static IAssemblyContextRegistry AddProfile<TProfile>(
            this IAssemblyContextRegistry registry)
            where TProfile : IDynaBeeProfile, new()
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            registry.AddProfile(new TProfile());
            return registry;
        }
    }
}
