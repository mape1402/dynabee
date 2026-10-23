namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;
using DynaBee.Infrastructure.ContextBuilders;

public sealed class ConfiguratorCoverageTests
{
    [Fact]
    public void MethodConfiguration_Emits_Default_Il_Lambda_And_Static_Method_Bodies()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Methods", type => type
                .AddMethod("DefaultValue", typeof(int))
                .AddMethod("DefaultReference", typeof(string))
                .AddVoidMethod("DefaultVoid")
                .AddMethod("Raw", typeof(int), method => method
                    .WithParameter<int>("value")
                    .Emits(il =>
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Ret);
                    }))
                .AddMethod("Lambda", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsLambda((Func<int, int>)(value => value * 2)))
                .AddVoidMethod("LambdaVoid", method => method
                    .WithParameter<Action>("callback")
                    .EmitsLambda((Action<Action>)(callback => callback())))
                .AddMethod("StaticLambda", typeof(int), method => method
                    .AsStatic()
                    .WithParameter<int>("value")
                    .EmitsLambda((Func<int, int>)(value => value + 3)))
                .AddMethod("StaticExpression", typeof(int), method => method
                    .AsStatic()
                    .WithParameter<int>("value")
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<int, int>>)(value => value + 4))))
            .Build();

        var type = context.GetClrType("Methods");
        var instance = Activator.CreateInstance(type)!;
        var called = false;

        Assert.Equal(0, type.GetMethod("DefaultValue")!.Invoke(instance, null));
        Assert.Null(type.GetMethod("DefaultReference")!.Invoke(instance, null));
        type.GetMethod("DefaultVoid")!.Invoke(instance, null);
        Assert.Equal(8, type.GetMethod("Raw")!.Invoke(instance, new object[] { 7 }));
        Assert.Equal(14, type.GetMethod("Lambda")!.Invoke(instance, new object[] { 7 }));
        type.GetMethod("LambdaVoid")!.Invoke(instance, new object[] { (Action)(() => called = true) });
        Assert.True(called);
        Assert.Equal(10, type.GetMethod("StaticLambda")!.Invoke(null, new object[] { 7 }));
        Assert.Equal(11, type.GetMethod("StaticExpression")!.Invoke(null, new object[] { 7 }));
    }

    [Fact]
    public void PropertyOverrides_Emit_Body_Il_And_Default_Accessors()
    {
        var number = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;
        var text = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Text))!;
        var defaultNumber = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultNumber))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Overrides", type => type
                .Inherits<OverrideBase>()
                .AddAutoProperty<int>("Stored")
                .OverrideProperty(number, property => property
                    .Getter(getter => getter.EmitsBody(body => body.Return(body.Property(body.Self(), "Stored"))))
                    .Setter(setter => setter.EmitsBody(body =>
                    {
                        body.Assign(body.Property(body.Self(), "Stored"), body.Parameter<int>("value"));
                        body.Return();
                    })))
                .OverrideProperty(text, property => property
                    .Getter(getter => getter.Emits(il =>
                    {
                        il.Emit(OpCodes.Ldstr, "from-il");
                        il.Emit(OpCodes.Ret);
                    }))
                    .Setter(setter => setter.Emits(il => il.Emit(OpCodes.Ret))))
                .OverrideProperty(defaultNumber, property => property.Getter(_ => { })))
            .Build();

        var instance = (OverrideBase)context.CreateInstance("Overrides");
        instance.Number = 19;
        instance.Text = "ignored";

        Assert.Equal(19, instance.Number);
        Assert.Equal("from-il", instance.Text);
        Assert.Equal(0, instance.DefaultNumber);
    }

    [Fact]
    public void OverrideConfiguration_Rejects_Invalid_Property_And_Method_Shapes()
    {
        var writable = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;
        var readOnly = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultNumber))!;
        var nonVirtual = typeof(OverrideBase).GetProperty(nameof(OverrideBase.NonVirtual))!;
        var virtualMethod = typeof(OverrideBase).GetMethod(nameof(OverrideBase.VirtualMethod))!;
        var regularMethod = typeof(OverrideBase).GetMethod(nameof(OverrideBase.RegularMethod))!;

        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(writable, _ => { })));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(readOnly, property => property.Setter(_ => { }))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(nonVirtual, property => property.Getter(_ => { }))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(writable, property => property.Setter(setter => setter.ReturnsConstant(1)))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(regularMethod, method => method.EmitsBody(body => body.Return(body.Constant(1))))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(virtualMethod, method => method.AsStatic().EmitsBody(body => body.Return(body.Constant(1))))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(virtualMethod, method => method.WithAccess(MethodAccessModifier.Protected).EmitsBody(body => body.Return(body.Constant(1))))));
    }

    [Fact]
    public void MethodConfigurator_Resolves_All_Runtime_Access_Modifiers()
    {
        Assert.Equal(MethodAccessModifier.Public, MethodConfigurator.GetAccessModifier(GetMethod(nameof(AccessBase.Public))));
        Assert.Equal(MethodAccessModifier.Protected, MethodConfigurator.GetAccessModifier(GetMethod("Protected")));
        Assert.Equal(MethodAccessModifier.Internal, MethodConfigurator.GetAccessModifier(GetMethod("Internal")));
        Assert.Equal(MethodAccessModifier.ProtectedInternal, MethodConfigurator.GetAccessModifier(GetMethod("ProtectedInternal")));
        Assert.Equal(MethodAccessModifier.PrivateProtected, MethodConfigurator.GetAccessModifier(GetMethod("PrivateProtected")));
        Assert.Equal(MethodAccessModifier.Private, MethodConfigurator.GetAccessModifier(GetMethod("Private")));
    }

    [Fact]
    public void MethodConfigurator_Rejects_Incompatible_Override_And_Interface_Contracts()
    {
        var overrideMethod = typeof(ValidationBase).GetMethod(nameof(ValidationBase.Transform))!;
        var protectedOverrideMethod = typeof(ValidationBase).GetMethod("ProtectedTransform", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var interfaceMethod = typeof(IValidationContract).GetMethod(nameof(IValidationContract.Transform))!;
        var plainMethod = typeof(ValidationBase).GetMethod(nameof(ValidationBase.Plain))!;

        Assert.Throws<ArgumentNullException>(() => new MethodConfigurator("Value", typeof(int), null!, null, null, null, null, false, false, default, null));
        Assert.Throws<ArgumentNullException>(() => CreateMethodConfigurator().Configure(null!));

        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(overrideMethod: overrideMethod), typeof(object)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(returnType: typeof(string), overrideMethod: overrideMethod), typeof(ValidationBase)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(parameters: Array.Empty<(string Name, BeeType Type)>(), overrideMethod: overrideMethod), typeof(ValidationBase)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(parameters: new[] { ("value", (BeeType)typeof(string)) }, overrideMethod: overrideMethod), typeof(ValidationBase)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(overrideMethod: protectedOverrideMethod, access: MethodAccessModifier.Public), typeof(ValidationBase)));

        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(isStatic: true, interfaceMethod: interfaceMethod)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(interfaceMethod: plainMethod)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(returnType: typeof(string), interfaceMethod: interfaceMethod)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(parameters: Array.Empty<(string Name, BeeType Type)>(), interfaceMethod: interfaceMethod)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(parameters: new[] { ("value", (BeeType)typeof(string)) }, interfaceMethod: interfaceMethod)));
        Assert.Throws<InvalidOperationException>(() => ConfigureAndBuild(
            CreateMethodConfigurator(interfaceMethod: interfaceMethod, access: MethodAccessModifier.Protected)));
    }

    [Fact]
    public void PropertyOverrides_Emit_Default_Bodies_For_Void_And_Reference_Accessors()
    {
        var number = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;
        var text = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultText))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("DefaultOverrides", type => type
                .Inherits<OverrideBase>()
                .OverrideProperty(number, property => property.Setter(_ => { }))
                .OverrideProperty(text, property => property.Getter(_ => { })))
            .Build();

        var instance = (OverrideBase)context.CreateInstance("DefaultOverrides");
        instance.Number = 42;

        Assert.Equal(0, instance.Number);
        Assert.Null(instance.DefaultText);
    }

    [Fact]
    public void Interface_Properties_And_Clr_Operators_Expose_All_Configured_Shapes()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddInterface("ISetterOnly", type => type
                .AddProperty<int>("Value", property => property
                    .AsWriteOnly()
                    .WithSetterAccess(MethodAccessModifier.Internal)))
            .AddClass("Operators", type =>
            {
                foreach (var kind in Enum.GetValues<BeeOperatorKind>())
                {
                    type.AddOperator(kind, typeof(int), method => method
                        .WithParameter<int>("value")
                        .EmitsBody(body => body.Return(body.Parameter<int>("value"))));
                }
            })
            .Build();

        var property = context.GetClrType("ISetterOnly").GetProperty("Value", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var setter = property.GetSetMethod(true)!;
        var operators = context.GetClrType("Operators");

        Assert.True(setter.IsAssembly);
        Assert.Null(property.GetGetMethod(true));
        Assert.NotNull(operators.GetMethod("op_Addition"));
        Assert.NotNull(operators.GetMethod("op_Subtraction"));
        Assert.NotNull(operators.GetMethod("op_Multiply"));
        Assert.NotNull(operators.GetMethod("op_Division"));
        Assert.NotNull(operators.GetMethod("op_Equality"));
        Assert.NotNull(operators.GetMethod("op_Inequality"));
        Assert.NotNull(operators.GetMethod("op_Implicit"));
        Assert.NotNull(operators.GetMethod("op_Explicit"));
    }

    [Fact]
    public void Expression_Configurator_Emits_Identity_Conversions()
    {
        var number = Expression.Parameter(typeof(int), "value");
        var identity = Expression.Lambda<Func<int, int>>(Expression.Convert(number, typeof(int)), number);
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Conversions", type => type
                .AddMethod("Identity", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsExpression(identity)))
            .Build();

        var instance = context.CreateInstance("Conversions");
        var type = instance.GetType();

        Assert.Equal(7, type.GetMethod("Identity")!.Invoke(instance, new object[] { 7 }));
    }

    [Fact]
    public void ConstructorConfigurator_Rejects_Invalid_Base_Constructor_Forwarding()
    {
        var textConstructor = typeof(ConstructorBaseText).GetConstructor(new[] { typeof(string) })!;
        var intConstructor = typeof(ConstructorBaseInt).GetConstructor(new[] { typeof(int) })!;
        var noParameters = Array.Empty<(string Name, BeeType Type)>();

        Assert.Throws<ArgumentNullException>(() => new ConstructorConfigurator(noParameters, null).Configure(null!));
        Assert.Throws<InvalidOperationException>(() => ConfigureConstructor(
            new ConstructorConfigurator(noParameters, null, baseConstructor: textConstructor),
            typeof(object)));
        Assert.Throws<InvalidOperationException>(() => ConfigureConstructor(
            new ConstructorConfigurator(noParameters, null, baseConstructor: textConstructor, baseConstructorArgumentNames: Array.Empty<string>()),
            typeof(ConstructorBaseText)));
        Assert.Throws<InvalidOperationException>(() => ConfigureConstructor(
            new ConstructorConfigurator(
                new[] { ("value", (BeeType)typeof(string)) },
                null,
                baseConstructor: intConstructor,
                baseConstructorArgumentNames: new[] { "value" }),
            typeof(ConstructorBaseInt)));
        Assert.Throws<InvalidOperationException>(() => ConfigureConstructor(
            new ConstructorConfigurator(
                new[] { ("other", (BeeType)typeof(string)) },
                null,
                baseConstructor: textConstructor,
                baseConstructorArgumentNames: new[] { "missing" }),
            typeof(ConstructorBaseText)));
    }

    [Fact]
    public void Dependency_Constructors_And_Interface_Configurators_Validate_And_Emit_Their_Contracts()
    {
        var dependencyConfigurator = new DependencyConstructorConfigurator(
            new[] { ("Count", typeof(int), "count"), ("Name", typeof(string), "name") });
        var interfaceConfigurator = new InterfaceConfigurator("IConfigured", default);

        Assert.Throws<ArgumentNullException>(() => new DependencyConstructorConfigurator(null!));
        Assert.Throws<ArgumentNullException>(() => dependencyConfigurator.Configure(null!));
        Assert.Throws<InvalidOperationException>(() => ConfigureDependencyConstructor(
            new DependencyConstructorConfigurator(Array.Empty<(string PropertyName, Type PropertyType, string ParameterName)>()),
            typeof(ConstructorBaseText)));
        Assert.Throws<ArgumentNullException>(() => interfaceConfigurator.AddElementBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => interfaceConfigurator.Inherits(null!));
        Assert.Throws<ArgumentException>(() => interfaceConfigurator.Inherits(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => interfaceConfigurator.AddAttribute(null!));

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddInterface("IConfigured", type => type.Inherits<IDisposable>())
            .AddClass("Injected", type => type
                .Inject<int>("Count", "count")
                .Inject<string>("Name", "name"))
            .Build();
        var injected = context.GetClrType("Injected");
        var instance = Activator.CreateInstance(injected, 9, "configured")!;

        Assert.True(context.GetClrType("IConfigured").IsInterface);
        Assert.Equal(9, injected.GetProperty("Count")!.GetValue(instance));
        Assert.Equal("configured", injected.GetProperty("Name")!.GetValue(instance));
    }

    [Fact]
    public void Type_Configurators_Validate_Input_And_Emit_Enums_Structs_And_Interfaces()
    {
        Assert.Throws<ArgumentException>(() => new AssemblyConfigurator(" "));
        Assert.Throws<ArgumentException>(() => new EnumConfigurator(" ", typeof(int), default));
        Assert.Throws<ArgumentNullException>(() => new EnumConfigurator("Values", null!, default));
        Assert.Throws<ArgumentException>(() => new EnumConfigurator("Values", typeof(string), default));
        Assert.Throws<ArgumentException>(() => new EnumConfigurator("Values", typeof(int), default).AddValue(" ", 1));
        Assert.Throws<ArgumentNullException>(() => new EnumConfigurator("Values", typeof(int), default).AddValue("One", null!));
        Assert.Throws<ArgumentNullException>(() => new EnumConfigurator("Values", typeof(int), default).AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => new StructConfigurator(" ", default));
        Assert.Throws<ArgumentNullException>(() => new StructConfigurator("Point", default).AddElementBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new StructConfigurator("Point", default).Implements(null!));
        Assert.Throws<ArgumentException>(() => new StructConfigurator("Point", default).Implements(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => new StructConfigurator("Point", default).AddAttribute(null!));

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddEnum("ByteValues", typeof(byte), ClassAccessModifier.Public, values => values
                .AddValue("One", 1)
                .AddValue("Two", (byte)2))
            .AddStruct("Point", value => value
                .Implements<IEmptyContract>()
                .AddProperty<int>("X"))
            .AddInterface("IReadOnlyContract", contract => contract
                .AddProperty<int>("Value", property => property.AsReadOnly())
                .AddMethod("Get", typeof(int)))
            .Build();

        var enumType = context.GetClrType("ByteValues");
        var structType = context.GetClrType("Point");
        var contractType = context.GetClrType("IReadOnlyContract");

        Assert.True(enumType.IsEnum);
        Assert.Equal(typeof(byte), Enum.GetUnderlyingType(enumType));
        Assert.Equal(2, (byte)Enum.Parse(enumType, "Two"));
        Assert.True(structType.IsValueType);
        Assert.Contains(typeof(IEmptyContract), structType.GetInterfaces());
        Assert.NotNull(contractType.GetProperty("Value")!.GetMethod);
        Assert.Null(contractType.GetProperty("Value")!.SetMethod);
        Assert.True(contractType.GetMethod("Get")!.IsAbstract);
    }

    [Fact]
    public void Member_Configurators_Emit_Properties_Indexers_Fields_And_Events_With_Requested_Accessors()
    {
        Assert.Throws<ArgumentException>(() => new FieldConfigurator(" ", typeof(int), default));
        Assert.Throws<ArgumentNullException>(() => new FieldConfigurator("Value", typeof(int), default).Configure(null!));
        Assert.Throws<ArgumentException>(() => new PropertyConfigurator(" ", typeof(int)));
        Assert.Throws<ArgumentException>(() => new PropertyConfigurator("Value", typeof(int), hasGetter: false, hasSetter: false));
        Assert.Throws<ArgumentException>(() => new InterfaceMethodConfigurator(" ", typeof(void), Array.Empty<(string, BeeType)>(), default, null));
        Assert.Throws<ArgumentNullException>(() => new InterfaceMethodConfigurator("Run", typeof(void), null!, default, null));
        Assert.Throws<ArgumentException>(() => new InterfacePropertyConfigurator(" ", typeof(int), true, false, default, default, null));
        Assert.Throws<ArgumentException>(() => new InterfacePropertyConfigurator("Value", typeof(int), false, false, default, default, null));

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Members", type => type
                .AddField<int>("Secret", FieldAccessModifier.Private)
                .AddProperty<int>("ReadOnly", property => property.AsReadOnly())
                .AddProperty<string>("WriteOnly", property => property.AsWriteOnly())
                .AddIndexer("Values", typeof(string), typeof(int), indexer => indexer
                    .WithGetterAccess(MethodAccessModifier.Private)
                    .WithSetterAccess(MethodAccessModifier.Public))
                .AddEvent<EventHandler>("Changed"))
            .AddInterface("IWriteOnlyContract", contract => contract
                .AddProperty<string>("Value", property => property.AsWriteOnly())
                .AddMethod("Set", typeof(void), method => method.WithParameter<string>("value")))
            .Build();

        var members = context.GetClrType("Members");
        var instance = Activator.CreateInstance(members)!;
        var writeOnly = members.GetProperty("WriteOnly")!;
        var indexer = members.GetProperty("Values", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        Assert.True(members.GetField("Secret", BindingFlags.Instance | BindingFlags.NonPublic)!.IsPrivate);
        Assert.Null(members.GetProperty("ReadOnly")!.SetMethod);
        Assert.Null(writeOnly.GetMethod);
        writeOnly.SetValue(instance, "accepted");
        Assert.True(indexer.GetMethod!.IsPrivate);
        indexer.SetValue(instance, 9, new object[] { "key" });
        Assert.Equal(9, indexer.GetValue(instance, new object[] { "key" }));
        var changed = members.GetEvent("Changed")!;
        changed.AddEventHandler(instance, (EventHandler)((_, _) => { }));
        Assert.NotNull(changed.AddMethod);

        var writeOnlyContract = context.GetClrType("IWriteOnlyContract").GetProperty("Value")!;
        Assert.Null(writeOnlyContract.GetMethod);
        Assert.NotNull(writeOnlyContract.SetMethod);
    }

    [Fact]
    public void Event_And_Record_Configurators_Reject_Invalid_Contracts_And_Emit_Deconstruction()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var typeContext = CreateTypeContext("Events", typeof(object));
            new EventConfigurator("Bad", typeof(string), default, null, null).Configure(typeContext);
            typeContext.Build();
        });
        Assert.Throws<ArgumentNullException>(() => new RecordDeconstructConfigurator(null!));
        Assert.Throws<ArgumentNullException>(() => new RecordDeconstructConfigurator(Array.Empty<(string, BeeType)>()).Configure(null!));

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddRecordClass("Pair", record => record
                .AddComponent<int>("Left")
                .AddComponent<string>("Right"))
            .Build();
        var pairType = context.GetClrType("Pair");
        var pair = Activator.CreateInstance(pairType)!;
        var values = new object[] { null!, null! };

        pairType.GetProperty("Left")!.SetValue(pair, 4);
        pairType.GetProperty("Right")!.SetValue(pair, "four");

        pairType.GetMethod("Deconstruct")!.Invoke(pair, values);

        Assert.Equal(4, values[0]);
        Assert.Equal("four", values[1]);
    }

    [Fact]
    public void Property_Overrides_Support_Constant_And_Default_Accessor_Bodies_And_Reject_Setter_Constants()
    {
        var constant = typeof(OverrideBase).GetProperty(nameof(OverrideBase.ConstantText))!;
        var defaultValue = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultNumber))!;
        var writable = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("OverrideBodies", type => type
                .Inherits<OverrideBase>()
                .OverrideProperty(constant, property => property.Getter(getter => getter.ReturnsConstant("constant")))
                .OverrideProperty(defaultValue, property => property.Getter(getter => getter.EmitsBody(_ => { }))))
            .Build();

        var instance = (OverrideBase)context.CreateInstance("OverrideBodies");
        Assert.Equal("constant", instance.ConstantText);
        Assert.Equal(0, instance.DefaultNumber);
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type
            .OverrideProperty(writable, property => property.Setter(setter => setter.ReturnsConstant(1)))));
    }

    [Fact]
    public void Expression_Emitter_Translates_Operators_Calls_Members_Construction_And_Conversions()
    {
        Expression<Func<int, int, int>> modulo = (left, right) => left % right;
        Expression<Func<bool, bool, bool>> and = (left, right) => left && right;
        Expression<Func<bool, bool, bool>> or = (left, right) => left || right;
        Expression<Func<string, string, string>> concat = (left, right) => left + right;
        Expression<Func<ExpressionProbe, int>> virtualCall = value => value.VirtualValue();
        Expression<Func<ExpressionProbe, int>> property = value => value.Value;
        Expression<Func<int, int>> constructed = value => new ExpressionBox(value).Value;
        Expression<Func<int, long>> widen = value => (long)value;
        Expression<Func<object, int>> unbox = value => (int)value;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Expressions", type => type
                .AddMethod("Modulo", typeof(int), method => method.WithParameter<int>("left").WithParameter<int>("right").EmitsExpression(modulo))
                .AddMethod("And", typeof(bool), method => method.WithParameter<bool>("left").WithParameter<bool>("right").EmitsExpression(and))
                .AddMethod("Or", typeof(bool), method => method.WithParameter<bool>("left").WithParameter<bool>("right").EmitsExpression(or))
                .AddMethod("Concat", typeof(string), method => method.WithParameter<string>("left").WithParameter<string>("right").EmitsExpression(concat))
                .AddMethod("Virtual", typeof(int), method => method.WithParameter<ExpressionProbe>("value").EmitsExpression(virtualCall))
                .AddMethod("Property", typeof(int), method => method.WithParameter<ExpressionProbe>("value").EmitsExpression(property))
                .AddMethod("Constructed", typeof(int), method => method.WithParameter<int>("value").EmitsExpression(constructed))
                .AddMethod("Widen", typeof(long), method => method.WithParameter<int>("value").EmitsExpression(widen))
                .AddMethod("Unbox", typeof(int), method => method.WithParameter<object>("value").EmitsExpression(unbox)))
            .Build();
        var instance = context.CreateInstance("Expressions");
        var type = instance.GetType();

        Assert.Equal(1, type.GetMethod("Modulo")!.Invoke(instance, new object[] { 7, 3 }));
        Assert.False((bool)type.GetMethod("And")!.Invoke(instance, new object[] { true, false })!);
        Assert.True((bool)type.GetMethod("Or")!.Invoke(instance, new object[] { false, true })!);
        Assert.Equal("ab", type.GetMethod("Concat")!.Invoke(instance, new object[] { "a", "b" }));
        Assert.Equal(9, type.GetMethod("Virtual")!.Invoke(instance, new object[] { new ExpressionProbe(4) }));
        Assert.Equal(4, type.GetMethod("Property")!.Invoke(instance, new object[] { new ExpressionProbe(4) }));
        Assert.Equal(5, type.GetMethod("Constructed")!.Invoke(instance, new object[] { 5 }));
        Assert.Equal(6L, type.GetMethod("Widen")!.Invoke(instance, new object[] { 6 }));
        Assert.Equal(7, type.GetMethod("Unbox")!.Invoke(instance, new object[] { 7 }));
    }

    [Fact]
    public void Expression_Emitter_Context_Exposes_The_Declared_Parameter_Types()
    {
        var dynamicMethod = new DynamicMethod("Coverage", typeof(void), Type.EmptyTypes);
        var contextType = typeof(ExpressionIlEmitter).GetNestedType("EmitContext", BindingFlags.NonPublic)!;
        var context = Activator.CreateInstance(
            contextType,
            dynamicMethod.GetILGenerator(),
            Array.Empty<ParameterExpression>(),
            new[] { typeof(int), typeof(string) },
            Array.Empty<int>())!;

        var parameterTypes = (IReadOnlyList<Type>)contextType.GetProperty("ParameterTypes")!.GetValue(context)!;

        Assert.Equal(new[] { typeof(int), typeof(string) }, parameterTypes);
    }

    private static void BuildInvalid(Action<BeeClassBuilder> configure)
        => new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Invalid", type =>
            {
                type.Inherits<OverrideBase>();
                configure(type);
            })
            .Build();

    private static MethodInfo GetMethod(string name)
        => typeof(AccessBase).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static MethodConfigurator CreateMethodConfigurator(
        Type returnType = null,
        IReadOnlyList<(string Name, BeeType Type)> parameters = null,
        bool isStatic = false,
        MethodAccessModifier access = default,
        MethodInfo overrideMethod = null,
        MethodInfo interfaceMethod = null)
        => new(
            "Transform",
            returnType ?? typeof(int),
            parameters ?? new[] { ("value", (BeeType)typeof(int)) },
            null,
            null,
            null,
            null,
            isStatic,
            false,
            access,
            null,
            overrideMethod: overrideMethod,
            interfaceMethod: interfaceMethod);

    private static void ConfigureAndBuild(MethodConfigurator configurator, Type baseType = null)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(NewAssemblyName()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Validation");
        var assemblyContext = new AssemblyContextBuilder("Validation", assembly, module);
        var type = module.DefineType("Generated", TypeAttributes.Public, baseType ?? typeof(object));
        var typeContext = (TypeContextBuilder)assemblyContext.AddTypeBuilder("Generated", type);

        configurator.Configure(typeContext);
        typeContext.Build();
    }

    private static void ConfigureConstructor(ConstructorConfigurator configurator, Type baseType)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(NewAssemblyName()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Constructors");
        var assemblyContext = new AssemblyContextBuilder("Constructors", assembly, module);
        var type = module.DefineType("Generated", TypeAttributes.Public, baseType);
        var typeContext = (TypeContextBuilder)assemblyContext.AddTypeBuilder("Generated", type);

        configurator.Configure(typeContext);
        typeContext.Build();
    }

    private static void ConfigureDependencyConstructor(DependencyConstructorConfigurator configurator, Type baseType)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(NewAssemblyName()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Dependencies");
        var assemblyContext = new AssemblyContextBuilder("Dependencies", assembly, module);
        var type = module.DefineType("Generated", TypeAttributes.Public, baseType);
        var typeContext = (TypeContextBuilder)assemblyContext.AddTypeBuilder("Generated", type);

        configurator.Configure(typeContext);
        typeContext.Build();
    }

    private static TypeContextBuilder CreateTypeContext(string name, Type baseType)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(NewAssemblyName()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Validation");
        var assemblyContext = new AssemblyContextBuilder("Validation", assembly, module);
        return (TypeContextBuilder)assemblyContext.AddTypeBuilder(name, module.DefineType(name, TypeAttributes.Public, baseType));
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.Configurators.{Guid.NewGuid():N}";

    public class OverrideBase
    {
        public virtual int Number { get; set; }

        public virtual string Text { get; set; }

        public virtual int DefaultNumber { get; }

        public virtual string DefaultText => "base";

        public virtual string ConstantText => "base";

        public int NonVirtual { get; set; }

        public virtual int VirtualMethod()
            => 1;

        public int RegularMethod()
            => 1;
    }

    private class AccessBase
    {
        public void Public() { }

        protected void Protected() { }

        internal void Internal() { }

        protected internal void ProtectedInternal() { }

        private protected void PrivateProtected() { }

        private void Private() { }
    }

    public class ValidationBase
    {
        public virtual int Transform(int value)
            => value;

        public int Plain(int value)
            => value;

        protected virtual int ProtectedTransform(int value)
            => value;
    }

    public sealed class ConstructorBaseText
    {
        public ConstructorBaseText(string value)
        {
        }
    }

    public sealed class ConstructorBaseInt
    {
        public ConstructorBaseInt(int value)
        {
        }
    }

    public interface IValidationContract
    {
        int Transform(int value);
    }

    public interface IEmptyContract
    {
    }

    public class ExpressionProbe
    {
        public ExpressionProbe(int value) => Value = value;

        public virtual int Value { get; }

        public virtual int VirtualValue() => Value + 5;
    }

    public sealed class ExpressionBox
    {
        public ExpressionBox(int value) => Value = value;

        public int Value { get; }
    }
}
