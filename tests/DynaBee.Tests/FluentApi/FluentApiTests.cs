namespace DynaBee.Tests.FluentApi
{
    using NSubstitute;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using global::DynaBee;
    using global::DynaBee.FluentApi;
    using global::DynaBee.FluentApi.DependencyInjection;
    using global::DynaBee.FluentApi.Diagnostics;
    using global::DynaBee.FluentApi.Invocation;
    using global::DynaBee.Infrastructure;
    using Xunit;

    public class FluentApiTests
    {
        [Fact]
        public void Build_Class_With_AutoProperty_And_Method_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.PropsAndMethods")
                .AddClass("Person", c => c
                    .AddAutoProperty<string>("Name")
                    .AddMethod("SayHello", typeof(string), m => m
                        .WithParameter<string>("target")
                        .Emits(il =>
                        {
                            var concatMethod = typeof(string).GetMethod(
                                nameof(string.Concat),
                                new[] { typeof(string), typeof(string) });

                            il.Emit(OpCodes.Ldstr, "Hello ");
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Call, concatMethod);
                            il.Emit(OpCodes.Ret);
                        })))
                .Build();

            var type = context.GetClrType("Person");
            var instance = Activator.CreateInstance(type);

            type.GetProperty("Name")!.SetValue(instance, "Mario");
            var storedName = type.GetProperty("Name")!.GetValue(instance);
            var message = type.GetMethod("SayHello")!.Invoke(instance, new object[] { "DynaBee" });

            Assert.Equal("Mario", storedName);
            Assert.Equal("Hello DynaBee", message);
        }

        [Fact]
        public void Build_Class_With_ReadOnly_And_WriteOnly_Properties_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.PropertyAccessors")
                .AddClass("PropertyAccessSample", c => c
                    .AddReadOnlyProperty<string>("Code")
                    .AddWriteOnlyProperty<int>("Secret")
                    .AddAutoProperty<string>("Name", hasGetter: true, hasSetter: true))
                .Build();

            var type = context.GetClrType("PropertyAccessSample");

            var codeProp = type.GetProperty("Code")!;
            Assert.NotNull(codeProp.GetGetMethod());
            Assert.Null(codeProp.GetSetMethod());

            var secretProp = type.GetProperty("Secret")!;
            Assert.Null(secretProp.GetGetMethod());
            Assert.NotNull(secretProp.GetSetMethod());

            var nameProp = type.GetProperty("Name")!;
            Assert.NotNull(nameProp.GetGetMethod());
            Assert.NotNull(nameProp.GetSetMethod());
        }

        [Fact]
        public void Build_Class_With_All_Access_Modifiers_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.AccessModifiers")
                .AddClass("PublicEntity", c => c
                    .AddField<int>("PublicCounter", FieldAccessModifier.Public)
                    .AddField<int>("InternalCounter", FieldAccessModifier.Internal)
                    .AddField<int>("PrivateCounter", FieldAccessModifier.Private)
                    .AddAutoProperty<string>(
                        "Title",
                        hasGetter: true,
                        hasSetter: true,
                        fieldAccessModifier: FieldAccessModifier.Private,
                        getterAccessModifier: MethodAccessModifier.Public,
                        setterAccessModifier: MethodAccessModifier.Private)
                    .AddMethod("Hidden", typeof(int), m => m
                        .WithAccess(MethodAccessModifier.Private)
                        .EmitsExpression((Expression<Func<int>>)(() => 10)))
                    .AddMethod("InternalOp", typeof(int), m => m
                        .WithAccess(MethodAccessModifier.Internal)
                        .EmitsExpression((Expression<Func<int>>)(() => 20)))
                    .AddMethod("ProtectedOp", typeof(int), m => m
                        .WithAccess(MethodAccessModifier.Protected)
                        .EmitsExpression((Expression<Func<int>>)(() => 30)))
                    .AddMethod("PublicOp", typeof(int), m => m
                        .WithAccess(MethodAccessModifier.Public)
                        .EmitsExpression((Expression<Func<int>>)(() => 40))))
                .AddClass("InternalEntity", ClassAccessModifier.Internal, c => c
                    .AddAutoProperty<int>("Id"))
                .Build();

            var publicType = context.GetClrType("PublicEntity");
            Assert.True(publicType.IsPublic);

            var internalType = context.GetClrType("InternalEntity");
            Assert.True(internalType.IsNotPublic);

            var publicField = publicType.GetField("PublicCounter", BindingFlags.Instance | BindingFlags.Public)!;
            Assert.True(publicField.IsPublic);

            var internalField = publicType.GetField("InternalCounter", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(internalField.IsAssembly);

            var privateField = publicType.GetField("PrivateCounter", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(privateField.IsPrivate);

            var titleProperty = publicType.GetProperty("Title")!;
            Assert.NotNull(titleProperty.GetGetMethod(nonPublic: true));
            Assert.NotNull(titleProperty.GetSetMethod(nonPublic: true));
            Assert.True(titleProperty.GetGetMethod(nonPublic: true)!.IsPublic);
            Assert.True(titleProperty.GetSetMethod(nonPublic: true)!.IsPrivate);

            var hiddenMethod = publicType.GetMethod("Hidden", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(hiddenMethod.IsPrivate);

            var internalMethod = publicType.GetMethod("InternalOp", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(internalMethod.IsAssembly);

            var protectedMethod = publicType.GetMethod("ProtectedOp", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(protectedMethod.IsFamily);

            var publicMethod = publicType.GetMethod("PublicOp", BindingFlags.Instance | BindingFlags.Public)!;
            Assert.True(publicMethod.IsPublic);
        }

        [Fact]
        public void Build_Class_With_Class_Property_And_Method_Attributes_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Attributes")
                .AddClass("AttributedEntity", c => c
                    .AddAttribute<AuditAttribute>(a => a
                        .WithConstructorArgument("entity")
                        .WithProperty(nameof(AuditAttribute.Level), 1))
                    .AddProperty<string>("Name", p => p
                        .AddAttribute<AuditAttribute>(a => a
                            .WithConstructorArgument("property")
                            .WithProperty(nameof(AuditAttribute.Level), 2)))
                    .AddMethod("GetValue", typeof(string), m => m
                        .AddAttribute<AuditAttribute>(a => a
                            .WithConstructorArgument("method")
                            .WithProperty(nameof(AuditAttribute.Level), 3))
                        .EmitsExpression((Expression<Func<string>>)(() => "ok"))))
                .Build();

            var type = context.GetClrType("AttributedEntity");

            var classAudit = type.GetCustomAttribute<AuditAttribute>()!;
            Assert.Equal("entity", classAudit.Name);
            Assert.Equal(1, classAudit.Level);

            var propertyAudit = type.GetProperty("Name")!.GetCustomAttribute<AuditAttribute>()!;
            Assert.Equal("property", propertyAudit.Name);
            Assert.Equal(2, propertyAudit.Level);

            var methodAudit = type.GetMethod("GetValue")!.GetCustomAttribute<AuditAttribute>()!;
            Assert.Equal("method", methodAudit.Name);
            Assert.Equal(3, methodAudit.Level);
        }

        [Fact]
        public void Build_Class_With_Property_Fluent_Configuration_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.PropertyBuilder")
                .AddClass("ConfiguredPropertyEntity", c => c
                    .AddProperty<string>("Code", p => p
                        .AsReadOnly()
                        .WithBackingFieldAccess(FieldAccessModifier.Private)
                        .WithGetterAccess(MethodAccessModifier.Protected)
                        .AddAttribute<AuditAttribute>(a => a
                            .WithConstructorArgument("property-builder")
                            .WithProperty(nameof(AuditAttribute.Level), 4))))
                .Build();

            var type = context.GetClrType("ConfiguredPropertyEntity");
            var property = type.GetProperty("Code", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            Assert.NotNull(property.GetGetMethod(nonPublic: true));
            Assert.Null(property.GetSetMethod(nonPublic: true));
            Assert.True(property.GetGetMethod(nonPublic: true)!.IsFamily);

            var attribute = property.GetCustomAttribute<AuditAttribute>()!;
            Assert.Equal("property-builder", attribute.Name);
            Assert.Equal(4, attribute.Level);
        }

        [Fact]
        public void Build_Interface_Struct_Enum_And_Records_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.TypeKinds")
                .AddInterface("IPriceService", c => c
                    .AddProperty<decimal>("TaxRate", p => p.AsReadOnly())
                    .AddMethod<decimal>("Calculate", m => m.WithParameter<decimal>("subtotal")))
                .AddStruct("Money", ClassAccessModifier.Internal, c => c
                    .AddField<decimal>("amount", FieldAccessModifier.Private)
                    .AddProperty<decimal>("Amount", p => p
                        .WithGetterAccess(MethodAccessModifier.Public)
                        .WithSetterAccess(MethodAccessModifier.Public)))
                .AddEnum<int>("SaleStatus", ClassAccessModifier.Public, c => c
                    .AddValue("Pending", 0)
                    .AddValue("Paid", 1)
                    .AddValue("Cancelled", 2))
                .AddRecordClass("ProductRecord", c => c
                    .AddComponent<int>("Id")
                    .AddComponent<string>("Name"))
                .AddRecordStruct("PointRecord", c => c
                    .AddComponent<int>("X")
                    .AddComponent<int>("Y"))
                .Build();

            var interfaceType = context.GetClrType("IPriceService");
            Assert.True(interfaceType.IsInterface);
            Assert.True(interfaceType.IsPublic);

            var calculateMethod = interfaceType.GetMethod("Calculate")!;
            Assert.True(calculateMethod.IsAbstract);
            Assert.True(calculateMethod.IsPublic);

            var taxRateProperty = interfaceType.GetProperty("TaxRate")!;
            Assert.NotNull(taxRateProperty.GetGetMethod());
            Assert.Null(taxRateProperty.GetSetMethod());

            var structType = context.GetClrType("Money");
            Assert.True(structType.IsValueType);
            Assert.True(structType.IsNotPublic);
            Assert.False(structType.IsEnum);

            var enumType = context.GetClrType("SaleStatus");
            Assert.True(enumType.IsEnum);
            Assert.Equal(typeof(int), Enum.GetUnderlyingType(enumType));
            Assert.Equal(new[] { "Pending", "Paid", "Cancelled" }, Enum.GetNames(enumType));

            var recordClassType = context.GetClrType("ProductRecord");
            Assert.True(recordClassType.IsClass);
            Assert.NotNull(recordClassType.GetCustomAttribute<RecordLikeAttribute>());
            Assert.NotNull(recordClassType.GetProperty("Id"));
            Assert.NotNull(recordClassType.GetProperty("Name"));

            var recordStructType = context.GetClrType("PointRecord");
            Assert.True(recordStructType.IsValueType);
            Assert.NotNull(recordStructType.GetCustomAttribute<RecordLikeAttribute>());
            Assert.NotNull(recordStructType.GetProperty("X"));
            Assert.NotNull(recordStructType.GetProperty("Y"));
        }

        [Fact]
        public void Build_Record_Semantics_Provides_Equals_HashCode_ToString_And_Deconstruct()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.RecordSemantics")
                .AddRecordClass("OrderRecord", r => r
                    .AddComponent<int>("Id")
                    .AddComponent<string>("Code"))
                .Build();

            var type = context.GetClrType("OrderRecord");
            var left = Activator.CreateInstance(type)!;
            var right = Activator.CreateInstance(type)!;

            type.GetProperty("Id")!.SetValue(left, 7);
            type.GetProperty("Code")!.SetValue(left, "A");

            type.GetProperty("Id")!.SetValue(right, 7);
            type.GetProperty("Code")!.SetValue(right, "A");

            Assert.True((bool)type.GetMethod("Equals", new[] { typeof(object) })!.Invoke(left, new[] { right })!);
            Assert.Equal(
                (int)type.GetMethod("GetHashCode", Type.EmptyTypes)!.Invoke(left, null)!,
                (int)type.GetMethod("GetHashCode", Type.EmptyTypes)!.Invoke(right, null)!);

            var toString = (string)type.GetMethod("ToString", Type.EmptyTypes)!.Invoke(left, null)!;
            Assert.Contains("OrderRecord", toString);
            Assert.Contains("Id = 7", toString);
            Assert.Contains("Code = A", toString);

            var args = new object[] { 0, string.Empty };
            type.GetMethod("Deconstruct")!.Invoke(left, args);
            Assert.Equal(7, (int)args[0]);
            Assert.Equal("A", (string)args[1]);
        }

        [Fact]
        public void Build_With_Version_Uses_Cache_And_Different_Version_Creates_New_Assembly()
        {
            var first = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Cache")
                .WithVersion("1.0.0")
                .AddClass("A", c => c.AddAutoProperty<int>("Id"))
                .Build();

            var second = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Cache")
                .WithVersion("1.0.0")
                .AddClass("A", c => c.AddAutoProperty<int>("Id"))
                .Build();

            var third = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Cache")
                .WithVersion("2.0.0")
                .AddClass("A", c => c.AddAutoProperty<int>("Id"))
                .Build();

            Assert.Same(first.Assembly, second.Assembly);
            Assert.NotSame(first.Assembly, third.Assembly);
        }

        [Fact]
        public void Diagnostics_Can_Be_Serialized_To_Json()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Diagnostics")
                .AddClass("DiagnosticEntity", c => c
                    .AddProperty<string>("Name")
                    .AddMethod("GetName", typeof(string), m => m
                        .EmitsInjectedLambda<string, string>("Name", x => x)))
                .Build();

            var diagnostics = context.GetDiagnostics();
            var json = context.ToDiagnosticsJson();

            Assert.Equal("Dynabee.Fluent.Tests.Diagnostics", diagnostics.Name);
            Assert.Contains("DiagnosticEntity", json);
            Assert.Contains("GetName", json);
            Assert.Contains("Name", json);
        }

        [Fact]
        public void Metadata_Can_Be_Stored_And_Extracted_From_Type_And_Elements()
        {
            var tableNameKey = new BeeMetadataKey<string>("ef:table:name");
            var columnNameKey = new BeeMetadataKey<string>("ef:column:name");
            var dbTypeKey = new BeeMetadataKey<string>("ef:column:dbtype");
            var methodTagKey = new BeeMetadataKey<string>("ext:method:tag");
            var ctorModeKey = new BeeMetadataKey<string>("ext:ctor:mode");

            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Metadata")
                .AddClass("ProductEntity", c => c
                    .WithMetadata(tableNameKey, "products")
                    .AddProperty<string>("Name", p => p
                        .WithMetadata(columnNameKey, "product_name")
                        .WithMetadata(dbTypeKey, "nvarchar(150)"))
                    .AddMethod("Normalize", typeof(string), m => m
                        .WithMetadata(methodTagKey, "transform")
                        .EmitsExpression((Expression<Func<string>>)(() => "ok")))
                    .AddConstructor(ctor => ctor
                        .WithMetadata(ctorModeKey, "public-default")))
                .Build();

            var typeContext = context.Find("ProductEntity");
            Assert.True(typeContext.TryGetMetadata(tableNameKey, out string tableName));
            Assert.Equal("products", tableName);

            var propertyContext = typeContext.FindOne("Name");
            Assert.True(propertyContext.TryGetMetadata(columnNameKey, out string columnName));
            Assert.True(propertyContext.TryGetMetadata(dbTypeKey, out string dbType));
            Assert.Equal("product_name", columnName);
            Assert.Equal("nvarchar(150)", dbType);

            var methodContext = typeContext.FindOne("Normalize");
            Assert.True(methodContext.TryGetMetadata(methodTagKey, out string methodTag));
            Assert.Equal("transform", methodTag);

            var ctorContext = typeContext.FindOne(".ctor:0");
            Assert.True(ctorContext.TryGetMetadata(ctorModeKey, out string ctorMode));
            Assert.Equal("public-default", ctorMode);
        }

        [Fact]
        public void AddDynaBee_Registers_Generated_Types_In_DI()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.DI")
                .AddClass("InvoiceService", c => c
                    .Implements<IInvoiceService>()
                    .Inject<IUnitOfWork>("UnitOfWork")
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", db => db.SaveChanges())))
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IUnitOfWork>(new TestUnitOfWork(11));
            services.AddDynaBee(context, ServiceLifetime.Transient);

            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IInvoiceService>();
            var rows = service.Commit();

            Assert.Equal(11, rows);
        }

        [Fact]
        public void AddDynaBee_Can_Skip_Interface_Registrations()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.DI.SkipInterfaces")
                .AddClass("InvoiceService", c => c
                    .Implements<IInvoiceService>(registerInDi: false)
                    .Inject<IUnitOfWork>("UnitOfWork")
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", db => db.SaveChanges())))
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IUnitOfWork>(new TestUnitOfWork(13));
            services.AddDynaBee(context, ServiceLifetime.Transient);

            var provider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IInvoiceService>());

            var implementationType = context.GetClrType("InvoiceService");
            var concrete = provider.GetRequiredService(implementationType);
            var rows = (int)implementationType.GetMethod("Commit")!.Invoke(concrete, null)!;

            Assert.Equal(13, rows);
        }

        [Fact]
        public void AddDynaBee_Can_Register_Only_Selected_Interfaces_And_Skip_Concrete_Type()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.DI.FilterInterfaces")
                .AddClass("InvoiceService", c => c
                    .Implements<IInvoiceService>(registerInDi: true)
                    .Implements<IHasUnitOfWork>(registerInDi: false)
                    .RegisterAsConcrete(false)
                    .Inject<IUnitOfWork>("UnitOfWork")
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", db => db.SaveChanges())))
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IUnitOfWork>(new TestUnitOfWork(17));
            services.AddDynaBee(context, ServiceLifetime.Transient);

            var provider = services.BuildServiceProvider();
            var invoiceService = provider.GetRequiredService<IInvoiceService>();
            var rows = invoiceService.Commit();

            Assert.Equal(17, rows);
            var implementationType = context.GetClrType("InvoiceService");
            Assert.Contains(typeof(IInvoiceService), implementationType.GetInterfaces());
            Assert.Contains(typeof(IHasUnitOfWork), implementationType.GetInterfaces());
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IHasUnitOfWork>());
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService(implementationType));
        }

        [Fact]
        public void Build_Class_That_Implements_Interface_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Interfaces")
                .AddClass("Calculator", c => c
                    .Implements<ICalculator>()
                    .AddAutoProperty<string>("Name")
                    .AddMethod("Sum", typeof(int), m => m
                        .WithParameter<int>("x")
                        .WithParameter<int>("y")
                        .Emits(il =>
                        {
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Add);
                            il.Emit(OpCodes.Ret);
                        })))
                .Build();

            var calculator = context.CreateInstance<ICalculator>("Calculator");
            calculator.Name = "Calc";

            Assert.Equal("Calc", calculator.Name);
            Assert.Equal(7, calculator.Sum(3, 4));
        }

        [Fact]
        public void Build_Class_That_Inherits_Base_Class_And_Custom_Constructor_Works()
        {
            var baseConstructor = typeof(WithCtorBase).GetConstructor(new[] { typeof(string) })!;

            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Inheritance")
                .AddClass("Customer", c => c
                    .Inherits<WithCtorBase>()
                    .AddConstructor(ctor => ctor
                        .WithParameter<string>("prefix")
                        .Emits(il =>
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Call, baseConstructor);
                            il.Emit(OpCodes.Ret);
                        })))
                .Build();

            var instance = context.CreateInstance("Customer", "Mr");

            Assert.IsAssignableFrom<WithCtorBase>(instance);
            Assert.Equal("Mr", ((WithCtorBase)instance).Prefix);
        }

        [Fact]
        public void Build_Method_From_Lambda_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Lambda")
                .AddClass("MathOps", c => c
                    .AddMethod("MultiplyByTwo", typeof(int), m => m
                        .WithParameter<int>("x")
                        .EmitsLambda((Func<int, int>)(x => x * 2))))
                .Build();

            var type = context.GetClrType("MathOps");
            var instance = Activator.CreateInstance(type);
            var result = (int)type.GetMethod("MultiplyByTwo")!.Invoke(instance, new object[] { 5 })!;

            Assert.Equal(10, result);
        }

        [Fact]
        public void Build_Method_From_Expression_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Expressions")
                .AddClass("MathOps", c => c
                    .AddMethod("Sum", typeof(int), m => m
                        .WithParameter<int>("x")
                        .WithParameter<int>("y")
                        .EmitsExpression((Expression<Func<int, int, int>>)((x, y) => x + y))))
                .Build();

            var type = context.GetClrType("MathOps");
            var instance = Activator.CreateInstance(type);
            var result = (int)type.GetMethod("Sum")!.Invoke(instance, new object[] { 7, 8 })!;

            Assert.Equal(15, result);
        }

        [Fact]
        public void Build_Method_From_Expression_With_String_Concat_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Expressions.StringConcat")
                .AddClass("Greeter", c => c
                    .AddMethod("SayHello", typeof(string), m => m
                        .WithParameter<string>("target")
                        .EmitsExpression((Expression<Func<string, string>>)(target => "Hello " + target))))
                .Build();

            var type = context.GetClrType("Greeter");
            var instance = Activator.CreateInstance(type);
            var result = (string)type.GetMethod("SayHello")!.Invoke(instance, new object[] { "DynaBee" })!;

            Assert.Equal("Hello DynaBee", result);
        }

        [Fact]
        public void Build_Class_With_Injected_Dependency_And_Call_Dependency_Method_Works()
        {
            var setUnitOfWork = typeof(IHasUnitOfWork).GetProperty(nameof(IHasUnitOfWork.UnitOfWork))!.SetMethod!;
            var baseCtor = typeof(object).GetConstructor(Type.EmptyTypes)!;

            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Dependencies")
                .AddClass("InvoiceService", c => c
                    .Implements<IInvoiceService>()
                    .Implements<IHasUnitOfWork>()
                    .AddAutoProperty<IUnitOfWork>("UnitOfWork")
                    .AddConstructor(ctor => ctor
                        .WithParameter<IUnitOfWork>("unitOfWork")
                        .Emits(il =>
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Call, baseCtor);

                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Callvirt, setUnitOfWork);
                            il.Emit(OpCodes.Ret);
                        }))
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsExpression((Expression<Func<IHasUnitOfWork, int>>)(self => self.UnitOfWork.SaveChanges()))))
                .Build();

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChanges().Returns(3);

            var service = context.CreateInstance<IInvoiceService>("InvoiceService", unitOfWork);
            var result = service.Commit();

            Assert.Equal(3, result);
            unitOfWork.Received(1).SaveChanges();
        }

        [Fact]
        public void Build_Class_With_Injected_Dependency_Without_Host_Interface_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Dependencies.NoHostInterface")
                .AddClass("InvoiceServiceNoHostInterface", c => c
                    .Implements<IInvoiceService>()
                    .Inject<IUnitOfWork>("UnitOfWork")
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", db =>
                        {
                            var affectedRows = db.SaveChanges();
                            return affectedRows;
                        })))
                .Build();

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChanges().Returns(5);

            var service = context.CreateInstance<IInvoiceService>("InvoiceServiceNoHostInterface", unitOfWork);
            var result = service.Commit();

            Assert.Equal(5, result);
            unitOfWork.Received(1).SaveChanges();
        }

        [Fact]
        public void Build_Class_With_Injected_Dependency_Using_Typed_Func_With_Self_Works()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Dependencies.TypedSelf")
                .AddClass("InvoiceServiceTypedSelf", c => c
                    .Implements<IInvoiceService>()
                    .Implements<IHasUnitOfWork>()
                    .Inject<IUnitOfWork>("UnitOfWork")
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsLambdaWithSelf<IHasUnitOfWork, int>(self =>
                        {
                            var rows = self.UnitOfWork.SaveChanges();
                            return rows;
                        })))
                .Build();

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChanges().Returns(9);

            var service = context.CreateInstance<IInvoiceService>("InvoiceServiceTypedSelf", unitOfWork);
            var result = service.Commit();

            Assert.Equal(9, result);
            unitOfWork.Received(1).SaveChanges();
        }

        [Fact]
        public void Registry_And_Provider_Current_Can_AutoRefresh_Without_Explicit_Rebuild()
        {
            var registry = new AssemblyContextRegistry("Dynabee.Fluent.Tests.Registry.Provider");

            registry.Configure(builder => builder
                .AddClass("CalculatorV1", c => c
                    .AddMethod("Value", typeof(int), m => m
                        .EmitsExpression((Expression<Func<int>>)(() => 1)))));

            var provider = new AssemblyContextProvider(registry);

            var v1Type = provider.Current.GetClrType("CalculatorV1");
            var snapshotV1 = provider.Current;
            var v1Instance = Activator.CreateInstance(v1Type)!;
            var v1Value = (int)v1Type.GetMethod("Value")!.Invoke(v1Instance, null)!;
            Assert.Equal(1, v1Value);
            Assert.Equal(0, provider.Generation);

            registry.Configure(builder => builder
                .AddClass("CalculatorV2", c => c
                    .AddMethod("Value", typeof(int), m => m
                        .EmitsExpression((Expression<Func<int>>)(() => 2)))));

            var rebuilt = provider.Current;
            Assert.Equal(1, provider.Generation);
            Assert.NotNull(rebuilt.Find("CalculatorV2"));
            Assert.NotSame(snapshotV1, rebuilt);
            Assert.NotNull(rebuilt.Find("CalculatorV1"));
        }

        [Fact]
        public void Registry_Can_Use_Profile_Definitions()
        {
            var registry = new AssemblyContextRegistry("Dynabee.Fluent.Tests.Registry.Profile");
            registry.AddProfile<InvoiceProfile>();

            var context = registry.BuildSnapshot();

            var serviceType = context.GetClrType("InvoiceServiceFromProfile");
            Assert.NotNull(serviceType.GetMethod("Commit"));
            Assert.Contains(typeof(IInvoiceService), serviceType.GetInterfaces());
        }

        [Fact]
        public void AddDynaBeeRegistry_Registers_Provider_And_Current_Context()
        {
            var services = new ServiceCollection();
            services.AddDynaBeeRegistry("Dynabee.Fluent.Tests.Registry.DI", registry =>
            {
                registry.Configure(builder => builder
                    .AddClass("FromRegistry", c => c
                        .AddAutoProperty<int>("Id")));
            });

            var provider = services.BuildServiceProvider();
            var contextProvider = provider.GetRequiredService<IAssemblyContextProvider>();
            var context = provider.GetRequiredService<IAssemblyContext>();
            var implementationType = context.GetClrType("FromRegistry");

            Assert.Same(contextProvider.Current, context);
            Assert.NotNull(context.Find("FromRegistry"));
            Assert.NotNull(provider.GetRequiredService(implementationType));
        }

        [Fact]
        public void AddDynaBeeProfiles_AutoDiscovers_And_Groups_Profiles_By_Assembly()
        {
            var services = new ServiceCollection();
            services.AddDynaBeeProfiles(ServiceLifetime.Transient, typeof(FluentApiTests).Assembly);

            var provider = services.BuildServiceProvider();
            var catalog = provider.GetRequiredService<IDynaBeeAssemblyCatalog>();

            Assert.Contains("Dynabee.Fluent.Tests.Registry.Profile", catalog.AssemblyNames);
            Assert.Contains("Dynabee.Fluent.Tests.Auto.One", catalog.AssemblyNames);
            Assert.Contains("Dynabee.Fluent.Tests.Auto.Two", catalog.AssemblyNames);

            var firstContext = catalog.GetContext("Dynabee.Fluent.Tests.Auto.One");
            var secondContext = catalog.GetContext("Dynabee.Fluent.Tests.Auto.Two");

            Assert.NotNull(firstContext.Find("AutoOneService"));
            Assert.Throws<KeyNotFoundException>(() => firstContext.Find("AutoTwoService"));
            Assert.NotNull(secondContext.Find("AutoTwoService"));
            Assert.Throws<KeyNotFoundException>(() => secondContext.Find("AutoOneService"));
        }

        [Fact]
        public void EmitsBody_Can_Generate_Expression_Value_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.ExpressionMapper")
                .AddClass("UserMapper", c => c
                    .AddMethod("Map", typeof(UserDto), m => m
                        .WithParameter<User>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var destination = body.DeclareLocal<UserDto>("destination");

                            body.Assign(destination, body.New<UserDto>());
                            body.Assign(
                                body.Property(destination, nameof(UserDto.DisplayName)),
                                body.Concat(
                                    body.Property(source, nameof(User.FirstName)),
                                    body.Constant(" "),
                                    body.Property(source, nameof(User.LastName))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("UserMapper"))!;
            var source = new User { FirstName = "Ada", LastName = "Lovelace" };
            var result = (UserDto)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { source })!;

            Assert.Equal("Ada Lovelace", result.DisplayName);
        }

        [Fact]
        public void EmitsBody_Can_Generate_Null_Substitute_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.NullSubstituteMapper")
                .AddClass("UserMapper", c => c
                    .AddMethod("Map", typeof(UserDto), m => m
                        .WithParameter<User>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var destination = body.DeclareLocal<UserDto>("destination");

                            body.Assign(destination, body.New<UserDto>());
                            body.Assign(
                                body.Property(destination, nameof(UserDto.Name)),
                                body.If(
                                    body.IsNull(body.Property(source, nameof(User.Name))),
                                    body.Constant("Unknown"),
                                    body.Property(source, nameof(User.Name))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("UserMapper"))!;
            var result = (UserDto)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { new User() })!;

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void EmitsBody_Can_Generate_Numeric_Conversion_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.NumericConversionMapper")
                .AddClass("InvoiceMapper", c => c
                    .AddMethod("Map", typeof(InvoiceDto), m => m
                        .WithParameter<Invoice>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<Invoice>("source");
                            var destination = body.DeclareLocal<InvoiceDto>("destination");

                            body.Assign(destination, body.New<InvoiceDto>());
                            body.Assign(
                                body.Property(destination, nameof(InvoiceDto.Total)),
                                body.Convert<decimal>(body.Property(source, nameof(Invoice.Total))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("InvoiceMapper"))!;
            var result = (InvoiceDto)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { new Invoice { Total = 123.45d } })!;

            Assert.Equal(123.45m, result.Total);
        }

        [Fact]
        public void EmitsBody_Can_Generate_Multiple_Source_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.MultipleSourceMapper")
                .AddClass("OrderMapper", c => c
                    .AddMethod("Map", typeof(OrderDto), m => m
                        .WithParameter<Order>("source")
                        .WithParameter<Customer>("customer")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<Order>("source");
                            var customer = body.Parameter<Customer>("customer");
                            var destination = body.DeclareLocal<OrderDto>("destination");

                            body.Assign(destination, body.New<OrderDto>());
                            body.Assign(body.Property(destination, nameof(OrderDto.OrderId)), body.Property(source, nameof(Order.Id)));
                            body.Assign(body.Property(destination, nameof(OrderDto.CustomerName)), body.Property(customer, nameof(Customer.Name)));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("OrderMapper"))!;
            var result = (OrderDto)mapper.GetType().GetMethod("Map")!.Invoke(
                mapper,
                new object[] { new Order { Id = 99 }, new Customer { Name = "Grace" } })!;

            Assert.Equal(99, result.OrderId);
            Assert.Equal("Grace", result.CustomerName);
        }

        [Fact]
        public void EmitsBody_Can_Generate_Nullable_And_Enum_Conversions()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.NullableEnumMapper")
                .AddClass("Mapper", c => c
                    .AddMethod("Map", typeof(AdvancedDto), m => m
                        .WithParameter<AdvancedSource>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<AdvancedSource>("source");
                            var destination = body.DeclareLocal<AdvancedDto>("destination");

                            body.Assign(destination, body.New<AdvancedDto>());
                            body.Assign(
                                body.Property(destination, nameof(AdvancedDto.Quantity)),
                                body.If(
                                    body.IsNull(body.Property(source, nameof(AdvancedSource.Quantity))),
                                    body.Constant(7),
                                    body.Convert<int>(body.Property(source, nameof(AdvancedSource.Quantity)))));
                            body.Assign(
                                body.Property(destination, nameof(AdvancedDto.OptionalQuantity)),
                                body.Convert<int?>(body.Property(source, nameof(AdvancedSource.RawQuantity))));
                            body.Assign(
                                body.Property(destination, nameof(AdvancedDto.Status)),
                                body.Convert<AdvancedStatus>(body.Property(source, nameof(AdvancedSource.StatusCode))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("Mapper"))!;
            var first = (AdvancedDto)mapper.GetType().GetMethod("Map")!.Invoke(
                mapper,
                new object[] { new AdvancedSource { Quantity = null, RawQuantity = 11, StatusCode = 2 } })!;
            var second = (AdvancedDto)mapper.GetType().GetMethod("Map")!.Invoke(
                mapper,
                new object[] { new AdvancedSource { Quantity = 5, RawQuantity = null, StatusCode = 1 } })!;

            Assert.Equal(7, first.Quantity);
            Assert.Equal(11, first.OptionalQuantity);
            Assert.Equal(AdvancedStatus.Paid, first.Status);
            Assert.Equal(5, second.Quantity);
            Assert.Null(second.OptionalQuantity);
            Assert.Equal(AdvancedStatus.Pending, second.Status);
        }

        [Fact]
        public void EmitsBody_Can_Access_Static_Property_And_Field()
        {
            StaticMappingState.Prefix = "ORD";
            StaticMappingState.Counter = 123;

            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.StaticMembers")
                .AddClass("Mapper", c => c
                    .AddMethod("Map", typeof(OrderDto), m => m
                        .WithParameter<Order>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<Order>("source");
                            var destination = body.DeclareLocal<OrderDto>("destination");

                            body.Assign(destination, body.New<OrderDto>());
                            body.Assign(body.Property(destination, nameof(OrderDto.OrderId)), body.Property(source, nameof(Order.Id)));
                            body.Assign(
                                body.Property(destination, nameof(OrderDto.CustomerName)),
                                body.Concat(
                                    body.StaticProperty(typeof(StaticMappingState), nameof(StaticMappingState.Prefix)),
                                    body.Constant("-"),
                                    body.Convert<string>(body.StaticField(typeof(StaticMappingState), nameof(StaticMappingState.Counter)))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("Mapper"))!;
            var result = (OrderDto)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { new Order { Id = 88 } })!;

            Assert.Equal(88, result.OrderId);
            Assert.Equal("ORD-123", result.CustomerName);
        }

        [Fact]
        public void EmitsBody_Can_Call_Instance_Method()
        {
            var formatMethod = typeof(NameFormatter).GetMethod(nameof(NameFormatter.Format))!;
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.InstanceCall")
                .AddClass("Mapper", c => c
                    .AddMethod("Map", typeof(string), m => m
                        .WithParameter<User>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var formatter = body.DeclareLocal<NameFormatter>("formatter");

                            body.Assign(formatter, body.New<NameFormatter>());
                            body.Return(body.Call(formatter, formatMethod, body.Property(source, nameof(User.Name))));
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("Mapper"))!;
            var result = (string)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { new User { Name = "Ada" } })!;

            Assert.Equal("Formatted: Ada", result);
        }

        [Fact]
        public void EmitsBody_Can_Call_Static_Generic_Method()
        {
            var getRequiredService = typeof(ServiceProviderServiceExtensions)
                .GetMethods()
                .Single(x => x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService)
                    && x.IsGenericMethodDefinition
                    && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(NameFormatter));

            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.StaticGenericCall")
                .AddClass("Resolver", c => c
                    .AddMethod("Resolve", typeof(object), m => m
                        .WithParameter<IServiceProvider>("services")
                        .EmitsBody(body =>
                        {
                            var services = body.Parameter<IServiceProvider>("services");
                            body.Return(body.StaticCall(getRequiredService, services));
                        })))
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<NameFormatter>()
                .BuildServiceProvider();
            var resolver = Activator.CreateInstance(context.GetClrType("Resolver"))!;
            var result = resolver.GetType().GetMethod("Resolve")!.Invoke(resolver, new object[] { services });

            Assert.IsType<NameFormatter>(result);
        }

        [Fact]
        public void EmitsBody_Can_Call_Interface_Method_And_Assign_Result()
        {
            var resolveMethod = typeof(IValueResolver<Order, OrderDto, string>).GetMethod(nameof(IValueResolver<Order, OrderDto, string>.Resolve))!;
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.InterfaceCall")
                .AddClass("Mapper", c => c
                    .AddMethod("Map", typeof(OrderDto), m => m
                        .WithParameter<IValueResolver<Order, OrderDto, string>>("resolver")
                        .WithParameter<Order>("source")
                        .WithParameter<TestMapContext>("context")
                        .EmitsBody(body =>
                        {
                            var resolver = body.Parameter<IValueResolver<Order, OrderDto, string>>("resolver");
                            var source = body.Parameter<Order>("source");
                            var mapContext = body.Parameter<TestMapContext>("context");
                            var destination = body.DeclareLocal<OrderDto>("destination");

                            body.Assign(destination, body.New<OrderDto>());
                            body.Assign(
                                body.Property(destination, nameof(OrderDto.TotalText)),
                                body.Call(resolver, resolveMethod, source, destination, mapContext));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("Mapper"))!;
            var result = (OrderDto)mapper.GetType().GetMethod("Map")!.Invoke(
                mapper,
                new object[] { new OrderTotalTextResolver(), new Order { Id = 77 }, new TestMapContext { Prefix = "Order " } })!;

            Assert.Equal("Order 77", result.TotalText);
        }

        [Fact]
        public void EmitsBody_Can_Access_Self_Property_And_Call_Method()
        {
            var formatMethod = typeof(NameFormatter).GetMethod(nameof(NameFormatter.Format))!;
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.Self")
                .AddClass("Mapper", c => c
                    .AddAutoProperty<NameFormatter>("Formatter")
                    .AddMethod("Map", typeof(string), m => m
                        .WithParameter<User>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var formatter = body.Property(body.Self(), "Formatter");

                            body.Return(body.Call(formatter, formatMethod, body.Property(source, nameof(User.Name))));
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("Mapper"))!;
            mapper.GetType().GetProperty("Formatter")!.SetValue(mapper, new NameFormatter());
            var result = (string)mapper.GetType().GetMethod("Map")!.Invoke(mapper, new object[] { new User { Name = "Grace" } })!;

            Assert.Equal("Formatted: Grace", result);
        }

        [Fact]
        public void EmitsBody_Rejects_Incompatible_Method_Call_Arguments()
        {
            var formatMethod = typeof(NameFormatter).GetMethod(nameof(NameFormatter.FormatCount))!;

            var exception = Assert.Throws<InvalidOperationException>(() =>
                DynaBeeBuilder
                    .CreateAssembly("Dynabee.Fluent.Tests.Body.InvalidCall")
                    .AddClass("Mapper", c => c
                        .AddMethod("Map", typeof(string), m => m
                            .EmitsBody(body =>
                            {
                                var formatter = body.DeclareLocal<NameFormatter>("formatter");

                                body.Assign(formatter, body.New<NameFormatter>());
                                body.Return(body.Call(formatter, formatMethod, body.New<User>()));
                            })))
                    .Build());

            Assert.Contains("cannot be assigned or converted", exception.Message);
        }

        [Fact]
        public void EmitsBody_Can_Copy_Array_With_For_Loop()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.ArrayCopy")
                .AddClass("ArrayCopier", c => c
                    .AddMethod("Copy", typeof(int[]), m => m
                        .WithParameter<int[]>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<int[]>("source");
                            var destination = body.DeclareLocal<int[]>("destination");
                            var index = body.DeclareLocal<int>("i");

                            body.If(body.IsNull(source), whenTrue: branch =>
                            {
                                branch.Return(branch.Constant(null, typeof(int[])));
                            });

                            body.Assign(destination, body.NewArray<int>(body.Property(source, nameof(Array.Length))));
                            body.For(
                                initialize: loop => loop.Assign(index, loop.Constant(0)),
                                condition: loop => loop.LessThan(index, loop.Property(source, nameof(Array.Length))),
                                increment: loop => loop.Assign(index, loop.Add(index, loop.Constant(1))),
                                body: loop => loop.Assign(loop.Index(destination, index), loop.Index(source, index)));
                            body.Return(destination);
                        })))
                .Build();

            var copier = Activator.CreateInstance(context.GetClrType("ArrayCopier"))!;
            var copy = (int[])copier.GetType().GetMethod("Copy")!.Invoke(copier, new object[] { new[] { 1, 2, 3 } })!;
            var nullCopy = copier.GetType().GetMethod("Copy")!.Invoke(copier, new object[] { null });

            Assert.Equal(new[] { 1, 2, 3 }, copy);
            Assert.Null(nullCopy);
        }

        [Fact]
        public void EmitsBody_Can_Copy_List_With_For_Loop()
        {
            var addMethod = typeof(List<string>).GetMethod(nameof(List<string>.Add))!;
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.ListCopy")
                .AddClass("ListCopier", c => c
                    .AddMethod("Copy", typeof(List<string>), m => m
                        .WithParameter<List<string>>("source")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<List<string>>("source");
                            var destination = body.DeclareLocal<List<string>>("destination");
                            var index = body.DeclareLocal<int>("i");

                            body.If(body.IsNull(source), whenTrue: branch =>
                            {
                                branch.Return(branch.Constant(null, typeof(List<string>)));
                            });

                            body.Assign(destination, body.New(typeof(List<string>), body.Property(source, nameof(List<string>.Count))));
                            body.For(
                                initialize: loop => loop.Assign(index, loop.Constant(0)),
                                condition: loop => loop.LessThan(index, loop.Property(source, nameof(List<string>.Count))),
                                increment: loop => loop.Assign(index, loop.Add(index, loop.Constant(1))),
                                body: loop => loop.Evaluate(loop.Call(destination, addMethod, loop.Index(source, index))));
                            body.Return(destination);
                        })))
                .Build();

            var copier = Activator.CreateInstance(context.GetClrType("ListCopier"))!;
            var copy = (List<string>)copier.GetType().GetMethod("Copy")!.Invoke(copier, new object[] { new List<string> { "a", "b" } })!;
            var nullCopy = copier.GetType().GetMethod("Copy")!.Invoke(copier, new object[] { null });

            Assert.Equal(new[] { "a", "b" }, copy);
            Assert.Null(nullCopy);
        }

        [Fact]
        public void EmitsBody_Can_Transform_List_With_Method_Call()
        {
            var addMethod = typeof(List<OrderItemDto>).GetMethod(nameof(List<OrderItemDto>.Add))!;
            var mapMethod = typeof(IItemMapper).GetMethod(nameof(IItemMapper.Map))!;
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.ListTransform")
                .AddClass("ItemMapperAdapter", c => c
                    .AddMethod("MapItems", typeof(List<OrderItemDto>), m => m
                        .WithParameter<List<OrderItem>>("source")
                        .WithParameter<IItemMapper>("mapper")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<List<OrderItem>>("source");
                            var mapper = body.Parameter<IItemMapper>("mapper");
                            var destination = body.DeclareLocal<List<OrderItemDto>>("destination");
                            var index = body.DeclareLocal<int>("i");

                            body.If(body.IsNull(source), whenTrue: branch =>
                            {
                                branch.Return(branch.Constant(null, typeof(List<OrderItemDto>)));
                            });

                            body.Assign(destination, body.New(typeof(List<OrderItemDto>), body.Property(source, nameof(List<OrderItem>.Count))));
                            body.For(
                                initialize: loop => loop.Assign(index, loop.Constant(0)),
                                condition: loop => loop.LessThan(index, loop.Property(source, nameof(List<OrderItem>.Count))),
                                increment: loop => loop.Assign(index, loop.Add(index, loop.Constant(1))),
                                body: loop => loop.Evaluate(loop.Call(
                                    destination,
                                    addMethod,
                                    loop.Call(mapper, mapMethod, loop.Index(source, index)))));
                            body.Return(destination);
                        })))
                .Build();

            var adapter = Activator.CreateInstance(context.GetClrType("ItemMapperAdapter"))!;
            var result = (List<OrderItemDto>)adapter.GetType().GetMethod("MapItems")!.Invoke(
                adapter,
                new object[] { new List<OrderItem> { new() { Name = "coffee" }, new() { Name = "tea" } }, new TestItemMapper() })!;

            Assert.Equal(new[] { "COFFEE", "TEA" }, result.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void EmitsBody_Can_Assign_Collection_Member()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Body.CollectionAssignment")
                .AddClass("OrderMapper", c => c
                    .AddMethod("Map", typeof(OrderWithItemDtos), m => m
                        .WithParameter<OrderWithItems>("source")
                        .WithParameter<List<OrderItemDto>>("items")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<OrderWithItems>("source");
                            var items = body.Parameter<List<OrderItemDto>>("items");
                            var destination = body.DeclareLocal<OrderWithItemDtos>("destination");

                            body.Assign(destination, body.New<OrderWithItemDtos>());
                            body.Assign(body.Property(destination, nameof(OrderWithItemDtos.Id)), body.Property(source, nameof(OrderWithItems.Id)));
                            body.Assign(body.Property(destination, nameof(OrderWithItemDtos.Items)), items);
                            body.Return(destination);
                        })))
                .Build();

            var mapper = Activator.CreateInstance(context.GetClrType("OrderMapper"))!;
            var items = new List<OrderItemDto> { new() { Name = "mapped" } };
            var result = (OrderWithItemDtos)mapper.GetType().GetMethod("Map")!.Invoke(
                mapper,
                new object[] { new OrderWithItems { Id = 10 }, items })!;

            Assert.Equal(10, result.Id);
            Assert.Same(items, result.Items);
        }

        [Fact]
        public void CreateBoundMethodInvoker_Can_Invoke_Single_Source_Runtime_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Invocation.SingleSource")
                .AddClass("UserToUserDtoMapper", c => c
                    .AddMethod("Map", typeof(UserDto), m => m
                        .WithParameter<User>("source")
                        .WithParameter<TestMapContext>("context")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var mapContext = body.Parameter<TestMapContext>("context");
                            var destination = body.DeclareLocal<UserDto>("destination");

                            body.Assign(destination, body.New<UserDto>());
                            body.Assign(
                                body.Property(destination, nameof(UserDto.Name)),
                                body.Concat(
                                    body.Property(mapContext, nameof(TestMapContext.Prefix)),
                                    body.Property(source, nameof(User.Name))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = context.CreateInstance("UserToUserDtoMapper");
            var invoker = context.CreateBoundMethodInvoker(
                "UserToUserDtoMapper",
                mapper,
                "Map",
                new[] { typeof(User), typeof(TestMapContext) });

            var result = (UserDto)invoker.Invoke(new object[] { new User { Name = "Ada" }, new TestMapContext { Prefix = "Ms. " } })!;

            Assert.Equal(typeof(UserDto), invoker.ReturnType);
            Assert.Equal(new[] { typeof(User), typeof(TestMapContext) }, invoker.ParameterTypes);
            Assert.Equal("Ms. Ada", result.Name);
        }

        [Fact]
        public void CreateBoundMethodInvoker_Can_Invoke_Multi_Source_Runtime_Mapper()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Invocation.MultiSource")
                .AddClass("OrderCustomerToOrderDtoMapper", c => c
                    .AddMethod("Map", typeof(OrderDto), m => m
                        .WithParameter<Order>("order")
                        .WithParameter<Customer>("customer")
                        .WithParameter<TestMapContext>("context")
                        .EmitsBody(body =>
                        {
                            var order = body.Parameter<Order>("order");
                            var customer = body.Parameter<Customer>("customer");
                            var mapContext = body.Parameter<TestMapContext>("context");
                            var destination = body.DeclareLocal<OrderDto>("destination");

                            body.Assign(destination, body.New<OrderDto>());
                            body.Assign(body.Property(destination, nameof(OrderDto.OrderId)), body.Property(order, nameof(Order.Id)));
                            body.Assign(
                                body.Property(destination, nameof(OrderDto.CustomerName)),
                                body.Concat(
                                    body.Property(mapContext, nameof(TestMapContext.Prefix)),
                                    body.Property(customer, nameof(Customer.Name))));
                            body.Return(destination);
                        })))
                .Build();

            var mapper = context.CreateInstance("OrderCustomerToOrderDtoMapper");
            var invoker = context.CreateBoundMethodInvoker(
                "OrderCustomerToOrderDtoMapper",
                mapper,
                "Map",
                new[] { typeof(Order), typeof(Customer), typeof(TestMapContext) });

            var result = (OrderDto)invoker.Invoke(new object[]
            {
                new Order { Id = 501 },
                new Customer { Name = "Grace" },
                new TestMapContext { Prefix = "Customer: " }
            })!;

            Assert.Equal(501, result.OrderId);
            Assert.Equal("Customer: Grace", result.CustomerName);
        }

        [Fact]
        public void CreateMethodInvoker_Caches_Unbound_Dispatch_Plan()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Invocation.Cache")
                .AddClass("UserToUserDtoMapper", c => c
                    .AddMethod("Map", typeof(UserDto), m => m
                        .WithParameter<User>("source")
                        .WithParameter<TestMapContext>("context")
                        .EmitsBody(body =>
                        {
                            var source = body.Parameter<User>("source");
                            var destination = body.DeclareLocal<UserDto>("destination");

                            body.Assign(destination, body.New<UserDto>());
                            body.Assign(body.Property(destination, nameof(UserDto.Name)), body.Property(source, nameof(User.Name)));
                            body.Return(destination);
                        })))
                .Build();

            var first = context.CreateMethodInvoker("UserToUserDtoMapper", "Map", new[] { typeof(User), typeof(TestMapContext) });
            var second = context.CreateMethodInvoker("UserToUserDtoMapper", "Map", new[] { typeof(User), typeof(TestMapContext) });
            var mapper = context.CreateInstance("UserToUserDtoMapper");

            Assert.Same(first, second);

            for (var i = 0; i < 5; i++)
            {
                var result = (UserDto)first.Invoke(mapper, new object[] { new User { Name = $"User {i}" }, new TestMapContext() })!;
                Assert.Equal($"User {i}", result.Name);
            }
        }

        [Fact]
        public void CreateMethodInvoker_Fails_With_Clear_Errors()
        {
            var context = DynaBeeBuilder
                .CreateAssembly("Dynabee.Fluent.Tests.Invocation.Errors")
                .AddClass("UserToUserDtoMapper", c => c
                    .AddMethod("Map", typeof(UserDto), m => m
                        .WithParameter<User>("source")
                        .WithParameter<TestMapContext>("context")))
                .Build();

            var missingMethod = Assert.Throws<InvalidOperationException>(() =>
                context.CreateMethodInvoker("UserToUserDtoMapper", "Missing", new[] { typeof(User), typeof(TestMapContext) }));
            var missingOverload = Assert.Throws<InvalidOperationException>(() =>
                context.CreateMethodInvoker("UserToUserDtoMapper", "Map", new[] { typeof(Customer), typeof(TestMapContext) }));
            var invoker = context.CreateMethodInvoker("UserToUserDtoMapper", "Map", new[] { typeof(User), typeof(TestMapContext) });
            var mapper = context.CreateInstance("UserToUserDtoMapper");
            var countMismatch = Assert.Throws<InvalidOperationException>(() =>
                invoker.Invoke(mapper, new object[] { new User() }));
            var typeMismatch = Assert.Throws<InvalidOperationException>(() =>
                invoker.Invoke(mapper, new object[] { new Customer(), new TestMapContext() }));

            Assert.Contains("Dynabee.Fluent.Tests.Invocation.Errors", missingMethod.Message);
            Assert.Contains("UserToUserDtoMapper", missingOverload.Message);
            Assert.Contains("Map", countMismatch.Message);
            Assert.Contains(typeof(User).FullName!, typeMismatch.Message);
        }

        public interface ICalculator
        {
            string Name { get; set; }

            int Sum(int x, int y);
        }

        public interface IUnitOfWork
        {
            int SaveChanges();
        }

        public interface IHasUnitOfWork
        {
            IUnitOfWork UnitOfWork { get; set; }
        }

        public interface IInvoiceService
        {
            int Commit();
        }

        public class WithCtorBase
        {
            public WithCtorBase(string prefix)
            {
                Prefix = prefix;
            }

            public string Prefix { get; }
        }

        public sealed class User
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Name { get; set; }
        }

        public sealed class UserDto
        {
            public string DisplayName { get; set; }

            public string Name { get; set; }
        }

        public sealed class Invoice
        {
            public double Total { get; set; }
        }

        public sealed class InvoiceDto
        {
            public decimal Total { get; set; }
        }

        public sealed class Order
        {
            public int Id { get; set; }
        }

        public sealed class Customer
        {
            public string Name { get; set; }
        }

        public sealed class OrderDto
        {
            public int OrderId { get; set; }

            public string CustomerName { get; set; }

            public string TotalText { get; set; }
        }

        public sealed class TestMapContext
        {
            public string Prefix { get; set; } = string.Empty;
        }

        public interface IValueResolver<in TSource, in TDestination, out TMember>
        {
            TMember Resolve(TSource source, TDestination destination, TestMapContext context);
        }

        public sealed class OrderTotalTextResolver : IValueResolver<Order, OrderDto, string>
        {
            public string Resolve(Order source, OrderDto destination, TestMapContext context)
                => $"{context.Prefix}{source.Id}";
        }

        public sealed class NameFormatter
        {
            public string Format(string name)
                => $"Formatted: {name}";

            public string FormatCount(int count)
                => $"Count: {count}";
        }

        public interface IItemMapper
        {
            OrderItemDto Map(OrderItem item);
        }

        public sealed class TestItemMapper : IItemMapper
        {
            public OrderItemDto Map(OrderItem item)
                => new() { Name = item.Name.ToUpperInvariant() };
        }

        public sealed class OrderItem
        {
            public string Name { get; set; }
        }

        public sealed class OrderItemDto
        {
            public string Name { get; set; }
        }

        public sealed class OrderWithItems
        {
            public int Id { get; set; }

            public List<OrderItem> Items { get; set; }
        }

        public sealed class OrderWithItemDtos
        {
            public int Id { get; set; }

            public List<OrderItemDto> Items { get; set; }
        }

        public sealed class AdvancedSource
        {
            public int? Quantity { get; set; }

            public int? RawQuantity { get; set; }

            public int StatusCode { get; set; }
        }

        public sealed class AdvancedDto
        {
            public int Quantity { get; set; }

            public int? OptionalQuantity { get; set; }

            public AdvancedStatus Status { get; set; }
        }

        public enum AdvancedStatus
        {
            Pending = 1,
            Paid = 2
        }

        public static class StaticMappingState
        {
            public static string Prefix { get; set; }

            public static int Counter;
        }

        private sealed class TestUnitOfWork : IUnitOfWork
        {
            private readonly int _result;

            public TestUnitOfWork(int result)
            {
                _result = result;
            }

            public int SaveChanges() => _result;
        }

        private sealed class InvoiceProfile : DynaBeeProfile
        {
            public InvoiceProfile() : base("Dynabee.Fluent.Tests.Registry.Profile")
            {
            }

            public override void Configure(IBeeAssemblyBuilder builder)
            {
                builder.AddClass("InvoiceServiceFromProfile", c => c
                    .Implements<IInvoiceService>()
                    .AddMethod("Commit", typeof(int), m => m
                        .EmitsExpression((Expression<Func<int>>)(() => 101))));
            }
        }

        private sealed class AutoAssemblyOneProfile : DynaBeeProfile
        {
            public AutoAssemblyOneProfile() : base("Dynabee.Fluent.Tests.Auto.One")
            {
            }

            public override void Configure(IBeeAssemblyBuilder builder)
            {
                builder.AddClass("AutoOneService", c => c
                    .AddMethod("Ping", typeof(string), m => m
                        .EmitsExpression((Expression<Func<string>>)(() => "one"))));
            }
        }

        private sealed class AutoAssemblyTwoProfile : DynaBeeProfile
        {
            public AutoAssemblyTwoProfile() : base("Dynabee.Fluent.Tests.Auto.Two")
            {
            }

            public override void Configure(IBeeAssemblyBuilder builder)
            {
                builder.AddClass("AutoTwoService", c => c
                    .AddMethod("Ping", typeof(string), m => m
                        .EmitsExpression((Expression<Func<string>>)(() => "two"))));
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method)]
        public sealed class AuditAttribute : Attribute
        {
            public AuditAttribute(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public int Level { get; set; }
        }
    }
}
