namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class AssemblyConfigurator : IAssemblyConfigurator
    {
        private readonly List<ITypeConfigurator> _typeBuilders = new();
        private readonly Dictionary<string, object> _metadata = new();
        private readonly string _name;

        public AssemblyConfigurator(string name)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyConfigurator AddTypeBuilder(ITypeConfigurator typeConfigurator)
        {
            _typeBuilders.Add(typeConfigurator);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyConfigurator WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyContextBuilder Configure()
        {
            var assemblyName = new AssemblyName(_name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var assemblyContextBuilder = new AssemblyContextBuilder(_name, assemblyBuilder, moduleBuilder);
            foreach (var metadata in _metadata)
                assemblyContextBuilder.SetMetadata(metadata.Key, metadata.Value);

            foreach (var typeBuilder in _typeBuilders)
                typeBuilder.Configure(assemblyContextBuilder); 

            return assemblyContextBuilder;
        }
    }
}
