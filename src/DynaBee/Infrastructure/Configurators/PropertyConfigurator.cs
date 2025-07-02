namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class PropertyConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _beeType;

        public PropertyConfigurator(string name, BeeType type)
        {
            _name = name;
            _beeType = type;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            typeContextBuilder.AddElement(_name, ElementType.Property, BuildAction);
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var typeBuilder = typeContextBuilder.TypeBuilder;

            var type = _beeType.IsReference ? typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder(_beeType).TypeBuilder : _beeType;

            var fieldBuilder = typeBuilder.DefineField($"_{_name}", type, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                                                                _name,
                                                                PropertyAttributes.HasDefault,
                                                                type,
                                                                null
                                                            );

            // 3. Definir getter
            var getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{_name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                type,
                Type.EmptyTypes
            );
            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);           // this
            getIL.Emit(OpCodes.Ldfld, fieldBuilder); // return _name;
            getIL.Emit(OpCodes.Ret);

            // 4. Definir setter
            var setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{_name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { type }
            );

            var setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);          // this
            setIL.Emit(OpCodes.Ldarg_1);          // value
            setIL.Emit(OpCodes.Stfld, fieldBuilder); // _name = value;
            setIL.Emit(OpCodes.Ret);

            // 5. Asociar métodos a la propiedad
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}
