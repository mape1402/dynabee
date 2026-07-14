namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;

    /// <summary>
    /// Abstraction for configuring and building dynamic assemblies.
    /// </summary>
    public interface IBeeAssemblyBuilder
    {
        /// <summary>
        /// Sets a semantic version token for the generated assembly cache key.
        /// </summary>
        BeeAssemblyBuilder WithVersion(string version);

        /// <summary>
        /// Disables in-memory cache for this build operation.
        /// </summary>
        BeeAssemblyBuilder DisableCache();

        /// <summary>
        /// Enables in-memory cache for this build operation.
        /// </summary>
        BeeAssemblyBuilder EnableCache();

        /// <summary>
        /// Adds a dynamic class using public visibility.
        /// </summary>
        BeeAssemblyBuilder AddClass(string name, Action<BeeClassBuilder> configure);

        /// <summary>
        /// Adds a dynamic class.
        /// </summary>
        BeeAssemblyBuilder AddClass(string name, ClassAccessModifier accessModifier, Action<BeeClassBuilder> configure = null);

        /// <summary>
        /// Adds a dynamic interface using public visibility.
        /// </summary>
        BeeAssemblyBuilder AddInterface(string name, Action<BeeInterfaceBuilder> configure);

        /// <summary>
        /// Adds a dynamic interface.
        /// </summary>
        BeeAssemblyBuilder AddInterface(string name, ClassAccessModifier accessModifier, Action<BeeInterfaceBuilder> configure = null);

        /// <summary>
        /// Adds a dynamic struct using public visibility.
        /// </summary>
        BeeAssemblyBuilder AddStruct(string name, Action<BeeStructBuilder> configure);

        /// <summary>
        /// Adds a dynamic struct.
        /// </summary>
        BeeAssemblyBuilder AddStruct(string name, ClassAccessModifier accessModifier, Action<BeeStructBuilder> configure = null);

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        BeeAssemblyBuilder AddEnum<TUnderlying>(string name, Action<BeeEnumBuilder> configure)
            where TUnderlying : struct;

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        BeeAssemblyBuilder AddEnum<TUnderlying>(string name, ClassAccessModifier accessModifier, Action<BeeEnumBuilder> configure)
            where TUnderlying : struct;

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        BeeAssemblyBuilder AddEnum(string name, Type underlyingType, ClassAccessModifier accessModifier, Action<BeeEnumBuilder> configure = null);

        /// <summary>
        /// Adds a dynamic record class.
        /// </summary>
        BeeAssemblyBuilder AddRecordClass(string name, Action<BeeRecordClassBuilder> configure);

        /// <summary>
        /// Adds a dynamic record class.
        /// </summary>
        BeeAssemblyBuilder AddRecordClass(string name, ClassAccessModifier accessModifier, Action<BeeRecordClassBuilder> configure = null);

        /// <summary>
        /// Adds a dynamic record struct.
        /// </summary>
        BeeAssemblyBuilder AddRecordStruct(string name, Action<BeeRecordStructBuilder> configure);

        /// <summary>
        /// Adds a dynamic record struct.
        /// </summary>
        BeeAssemblyBuilder AddRecordStruct(string name, ClassAccessModifier accessModifier, Action<BeeRecordStructBuilder> configure = null);

        /// <summary>
        /// Builds all configured types and returns the assembly context.
        /// </summary>
        IAssemblyContext Build();
    }
}
