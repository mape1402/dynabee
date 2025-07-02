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
        public IAssemblyContextBuilder Configure()
        {
            var assemblyName = new AssemblyName(_name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var assemblyContextBuilder = new AssemblyContextBuilder(_name, assemblyBuilder, moduleBuilder);

            foreach (var typeBuilder in _typeBuilders)
                typeBuilder.Configure(assemblyContextBuilder); 

            return assemblyContextBuilder;
        }
    }
}
