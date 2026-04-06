# DynaBee API Reference

Comprehensive API documentation for **DynaBee** in Microsoft-style format.

## Table of Contents

1. [Namespace Index](#namespace-index)
2. [Namespace Details](#namespace-details)

## Namespace Index

- [DynaBee](#namespace-dynabee)
- [DynaBee.FluentApi](#namespace-dynabee-fluentapi)
- [DynaBee.FluentApi.DependencyInjection](#namespace-dynabee-fluentapi-dependencyinjection)
- [DynaBee.FluentApi.Diagnostics](#namespace-dynabee-fluentapi-diagnostics)
- [DynaBee.Infrastructure](#namespace-dynabee-infrastructure)
- [DynaBee.Infrastructure.Configurators](#namespace-dynabee-infrastructure-configurators)
- [DynaBee.Tools](#namespace-dynabee-tools)

## Namespace Details

## Namespace: DynaBee
<a id="namespace-dynabee"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [BeeAttribute](#type-dynabee-beeattribute) | Class | Represents a custom attribute declaration for dynamic members. |
| [BeeMetadataKey](#type-dynabee-beemetadatakey`1) | Struct | No description available. |
| [ElementBuilderAction](#type-dynabee-elementbuilderaction) | Delegate | Represents a delegate that applies custom configuration logic to a given . |
| [ElementType](#type-dynabee-elementtype) | Enum | Specifies the kind of element that can be dynamically defined within a type. |
| [IAssemblyConfigurator](#type-dynabee-iassemblyconfigurator) | Interface | Defines a contract for configuring an entire dynamic assembly,             including its types and elements. |
| [IAssemblyContext](#type-dynabee-iassemblycontext) | Interface | Represents an immutable context that provides access to dynamically created types within a specific . |
| [IAssemblyContextBuilder](#type-dynabee-iassemblycontextbuilder) | Interface | Defines a context for managing dynamic type creation within a specific . |
| [IElementConfigurator](#type-dynabee-ielementconfigurator) | Interface | Defines a contract for configuring a specific element             (such as a property, method, field, or constant)             within a dynamic type. |
| [IElementContext](#type-dynabee-ielementcontext) | Interface | Represents an immutable context containing metadata for a specific element             (such as a property, method, field, or constant) within a dynamically generated type. |
| [IElementContextBuilder](#type-dynabee-ielementcontextbuilder) | Interface | Defines a builder context for configuring a specific element (such as a property, method, field, or constant)             within a dynamic type. |
| [ITypeConfigurator](#type-dynabee-itypeconfigurator) | Interface | Defines a contract for configuring a dynamic type and its elements within an assembly context. |
| [ITypeContext](#type-dynabee-itypecontext) | Interface | Represents an immutable context that provides metadata and access to a dynamically generated type             and its defined elements. |
| [ITypeContextBuilder](#type-dynabee-itypecontextbuilder) | Interface | Defines a context that encapsulates metadata and access to a specific dynamic . |

### BeeAttribute
<a id="type-dynabee-beeattribute"></a>

**Namespace:** `DynaBee`

**Kind:** Class

**Description:** Represents a custom attribute declaration for dynamic members.

#### Properties

No public properties.

#### Method Index

1. [Of](#type-dynabee-beeattribute-method-1)
2. [Of](#type-dynabee-beeattribute-method-2)
3. [WithField](#type-dynabee-beeattribute-method-3)
4. [WithProperty](#type-dynabee-beeattribute-method-4)

#### Method: Of
<a id="type-dynabee-beeattribute-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.BeeAttribute Of(object[] constructorArguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `constructorArguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.BeeAttribute.Of(/* constructorArguments: object[] */ default);
```

#### Method: Of
<a id="type-dynabee-beeattribute-method-2"></a>

**Description:** Creates a custom attribute declaration.

**Signature**

```csharp
public DynaBee.BeeAttribute Of(System.Type attributeType, object[] constructorArguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attributeType` | `System.Type` | Yes | No description available. |
| `constructorArguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.BeeAttribute.Of(/* attributeType: System.Type */ default, /* constructorArguments: object[] */ default);
```

#### Method: WithField
<a id="type-dynabee-beeattribute-method-3"></a>

**Description:** Adds a named field assignment for the attribute.

**Signature**

```csharp
public DynaBee.BeeAttribute WithField(string fieldName, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `fieldName` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.BeeAttribute); // replace with a valid instance
var result = instance.WithField(/* fieldName: string */ default, /* value: object */ default);
```

#### Method: WithProperty
<a id="type-dynabee-beeattribute-method-4"></a>

**Description:** Adds a named property assignment for the attribute.

**Signature**

```csharp
public DynaBee.BeeAttribute WithProperty(string propertyName, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `propertyName` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.BeeAttribute); // replace with a valid instance
var result = instance.WithProperty(/* propertyName: string */ default, /* value: object */ default);
```

### BeeMetadataKey`1
<a id="type-dynabee-beemetadatakey`1"></a>

**Namespace:** `DynaBee`

**Kind:** Struct

**Description:** No description available.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | No description available. |

#### Method Index

1. [BeeMetadataKey](#type-dynabee-beemetadatakey`1-method-1)
2. [Equals](#type-dynabee-beemetadatakey`1-method-2)
3. [Equals](#type-dynabee-beemetadatakey`1-method-3)
4. [GetHashCode](#type-dynabee-beemetadatakey`1-method-4)
5. [ToString](#type-dynabee-beemetadatakey`1-method-5)

#### Method: BeeMetadataKey`1
<a id="type-dynabee-beemetadatakey`1-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public BeeMetadataKey`1(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = new DynaBee.BeeMetadataKey<T>(/* name: string */ default);
```

#### Method: Equals
<a id="type-dynabee-beemetadatakey`1-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public bool Equals(object obj)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `obj` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.BeeMetadataKey<T>); // replace with a valid instance
var result = instance.Equals(/* obj: object */ default);
```

#### Method: Equals
<a id="type-dynabee-beemetadatakey`1-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public bool Equals(DynaBee.BeeMetadataKey<T> other)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `other` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.BeeMetadataKey<T>); // replace with a valid instance
var result = instance.Equals(/* other: DynaBee.BeeMetadataKey<T> */ default);
```

#### Method: GetHashCode
<a id="type-dynabee-beemetadatakey`1-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public int GetHashCode()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.BeeMetadataKey<T>); // replace with a valid instance
var result = instance.GetHashCode();
```

#### Method: ToString
<a id="type-dynabee-beemetadatakey`1-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public string ToString()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.BeeMetadataKey<T>); // replace with a valid instance
var result = instance.ToString();
```

### ElementBuilderAction
<a id="type-dynabee-elementbuilderaction"></a>

**Namespace:** `DynaBee`

**Kind:** Delegate

**Description:** Represents a delegate that applies custom configuration logic to a given .

#### Properties

No public properties.

#### Method Index

1. [ElementBuilderAction](#type-dynabee-elementbuilderaction-method-1)
2. [BeginInvoke](#type-dynabee-elementbuilderaction-method-2)
3. [EndInvoke](#type-dynabee-elementbuilderaction-method-3)
4. [Invoke](#type-dynabee-elementbuilderaction-method-4)

#### Method: ElementBuilderAction
<a id="type-dynabee-elementbuilderaction-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public ElementBuilderAction(object object, System.IntPtr method)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `object` | `object` | Yes | No description available. |
| `method` | `System.IntPtr` | Yes | No description available. |

**Example**

```csharp
var instance = new DynaBee.ElementBuilderAction(/* object: object */ default, /* method: System.IntPtr */ default);
```

#### Method: BeginInvoke
<a id="type-dynabee-elementbuilderaction-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public System.IAsyncResult BeginInvoke(DynaBee.ITypeContextBuilder typeContextBuilder, System.AsyncCallback callback, object object)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `typeContextBuilder` | `DynaBee.ITypeContextBuilder` | Yes | No description available. |
| `callback` | `System.AsyncCallback` | Yes | No description available. |
| `object` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.ElementBuilderAction); // replace with a valid instance
var result = instance.BeginInvoke(/* typeContextBuilder: DynaBee.ITypeContextBuilder */ default, /* callback: System.AsyncCallback */ default, /* object: object */ default);
```

#### Method: EndInvoke
<a id="type-dynabee-elementbuilderaction-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public void EndInvoke(System.IAsyncResult result)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `result` | `System.IAsyncResult` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.ElementBuilderAction); // replace with a valid instance
instance.EndInvoke(/* result: System.IAsyncResult */ default);
```

#### Method: Invoke
<a id="type-dynabee-elementbuilderaction-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public void Invoke(DynaBee.ITypeContextBuilder typeContextBuilder)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `typeContextBuilder` | `DynaBee.ITypeContextBuilder` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.ElementBuilderAction); // replace with a valid instance
instance.Invoke(/* typeContextBuilder: DynaBee.ITypeContextBuilder */ default);
```

### ElementType
<a id="type-dynabee-elementtype"></a>

**Namespace:** `DynaBee`

**Kind:** Enum

**Description:** Specifies the kind of element that can be dynamically defined within a type.

#### Properties

No public properties.

#### Method Index

No public methods.

### IAssemblyConfigurator
<a id="type-dynabee-iassemblyconfigurator"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a contract for configuring an entire dynamic assembly,             including its types and elements.

#### Properties

No public properties.

#### Method Index

1. [AddTypeBuilder](#type-dynabee-iassemblyconfigurator-method-1)
2. [Configure](#type-dynabee-iassemblyconfigurator-method-2)

#### Method: AddTypeBuilder
<a id="type-dynabee-iassemblyconfigurator-method-1"></a>

**Description:** Adds a type configurator to the assembly configuration.

**Signature**

```csharp
public DynaBee.IAssemblyConfigurator AddTypeBuilder(DynaBee.ITypeConfigurator typeConfigurator)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `typeConfigurator` | `DynaBee.ITypeConfigurator` | Yes | The  that defines how to build a specific dynamic type. |

**Example**

```csharp
var instance = default(DynaBee.IAssemblyConfigurator); // replace with a valid instance
var result = instance.AddTypeBuilder(/* typeConfigurator: DynaBee.ITypeConfigurator */ default);
```

#### Method: Configure
<a id="type-dynabee-iassemblyconfigurator-method-2"></a>

**Description:** Applies the assembly configuration and returns a builder context             that can be used to generate the final dynamic assembly and its types.

**Signature**

```csharp
public DynaBee.IAssemblyContextBuilder Configure()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.IAssemblyConfigurator); // replace with a valid instance
var result = instance.Configure();
```

### IAssemblyContext
<a id="type-dynabee-iassemblycontext"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Represents an immutable context that provides access to dynamically created types within a specific .

#### Properties

| Name | Type | Description |
|---|---|---|
| `Assembly` | `System.Reflection.Assembly` | Gets the underlying  that contains all dynamically generated types. |
| `Name` | `string` | Gets the unique name assigned to this assembly context, which can be used for identification. |

#### Method Index

1. [Find](#type-dynabee-iassemblycontext-method-1)
2. [Find](#type-dynabee-iassemblycontext-method-2)

#### Method: Find
<a id="type-dynabee-iassemblycontext-method-1"></a>

**Description:** Finds a single  by its unique name.

**Signature**

```csharp
public DynaBee.ITypeContext Find(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | The unique name of the type to find. |

**Example**

```csharp
var instance = default(DynaBee.IAssemblyContext); // replace with a valid instance
var result = instance.Find(/* name: string */ default);
```

#### Method: Find
<a id="type-dynabee-iassemblycontext-method-2"></a>

**Description:** Finds all  instances that match the specified predicate expression.

**Signature**

```csharp
public System.Collections.Generic.IEnumerable<DynaBee.ITypeContext> Find(System.Func<DynaBee.ITypeContext, bool> predicate)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `predicate` | `System.Func<DynaBee.ITypeContext, bool>` | Yes | The filter expression used to select matching type contexts. |

**Example**

```csharp
var instance = default(DynaBee.IAssemblyContext); // replace with a valid instance
var result = instance.Find(/* predicate: System.Func<DynaBee.ITypeContext, bool> */ default);
```

### IAssemblyContextBuilder
<a id="type-dynabee-iassemblycontextbuilder"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a context for managing dynamic type creation within a specific .

#### Properties

| Name | Type | Description |
|---|---|---|
| `ModuleBuilder` | `System.Reflection.Emit.ModuleBuilder` | Gets the underlying  used to define dynamic types. |

#### Method Index

1. [AddTypeBuilder](#type-dynabee-iassemblycontextbuilder-method-1)
2. [Build](#type-dynabee-iassemblycontextbuilder-method-2)
3. [GetTypeBuilder](#type-dynabee-iassemblycontextbuilder-method-3)

#### Method: AddTypeBuilder
<a id="type-dynabee-iassemblycontextbuilder-method-1"></a>

**Description:** Registers a new  in the current context under the specified name.

**Signature**

```csharp
public DynaBee.ITypeContextBuilder AddTypeBuilder(string name, System.Reflection.Emit.TypeBuilder typeBuilder)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | The unique name for the type builder. |
| `typeBuilder` | `System.Reflection.Emit.TypeBuilder` | Yes | The  to associate with the name. |

**Example**

```csharp
var instance = default(DynaBee.IAssemblyContextBuilder); // replace with a valid instance
var result = instance.AddTypeBuilder(/* name: string */ default, /* typeBuilder: System.Reflection.Emit.TypeBuilder */ default);
```

#### Method: Build
<a id="type-dynabee-iassemblycontextbuilder-method-2"></a>

**Description:** Finalizes the assembly context construction and returns an immutable              containing all registered type definitions.

**Signature**

```csharp
public DynaBee.IAssemblyContext Build()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.IAssemblyContextBuilder); // replace with a valid instance
var result = instance.Build();
```

#### Method: GetTypeBuilder
<a id="type-dynabee-iassemblycontextbuilder-method-3"></a>

**Description:** Retrieves a previously registered  by its name.

**Signature**

```csharp
public DynaBee.ITypeContextBuilder GetTypeBuilder(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | The unique name of the type builder to retrieve. |

**Example**

```csharp
var instance = default(DynaBee.IAssemblyContextBuilder); // replace with a valid instance
var result = instance.GetTypeBuilder(/* name: string */ default);
```

### IElementConfigurator
<a id="type-dynabee-ielementconfigurator"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a contract for configuring a specific element             (such as a property, method, field, or constant)             within a dynamic type.

#### Properties

No public properties.

#### Method Index

1. [Configure](#type-dynabee-ielementconfigurator-method-1)

#### Method: Configure
<a id="type-dynabee-ielementconfigurator-method-1"></a>

**Description:** Applies the element configuration to the specified .             This method defines how the element should be added to the dynamic type.

**Signature**

```csharp
public void Configure(DynaBee.ITypeContextBuilder typeContextBuilder)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `typeContextBuilder` | `DynaBee.ITypeContextBuilder` | Yes | The type context builder to which the element configuration will be applied. |

**Example**

```csharp
var instance = default(DynaBee.IElementConfigurator); // replace with a valid instance
instance.Configure(/* typeContextBuilder: DynaBee.ITypeContextBuilder */ default);
```

### IElementContext
<a id="type-dynabee-ielementcontext"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Represents an immutable context containing metadata for a specific element             (such as a property, method, field, or constant) within a dynamically generated type.

#### Properties

| Name | Type | Description |
|---|---|---|
| `ElementType` | `DynaBee.ElementType` | Gets the type of element (e.g., property, method, field, or constant). |
| `Name` | `string` | Gets the unique name assigned to the element. |

#### Method Index

1. [GetMetadata](#type-dynabee-ielementcontext-method-1)
2. [TryGetMetadata](#type-dynabee-ielementcontext-method-2)
3. [TryGetMetadata](#type-dynabee-ielementcontext-method-3)

#### Method: GetMetadata
<a id="type-dynabee-ielementcontext-method-1"></a>

**Description:** Gets metadata attached to this element.

**Signature**

```csharp
public object GetMetadata(string key)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |

**Example**

```csharp
var instance = default(DynaBee.IElementContext); // replace with a valid instance
var result = instance.GetMetadata(/* key: string */ default);
```

#### Method: TryGetMetadata
<a id="type-dynabee-ielementcontext-method-2"></a>

**Description:** Tries to get metadata attached to this element.

**Signature**

```csharp
public bool TryGetMetadata(string key, ref object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |
| `value` | `ref object` | Yes | Metadata value when present. |

**Example**

```csharp
var instance = default(DynaBee.IElementContext); // replace with a valid instance
var result = instance.TryGetMetadata(/* key: string */ default, /* value: ref object */ default);
```

#### Method: TryGetMetadata
<a id="type-dynabee-ielementcontext-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public bool TryGetMetadata(DynaBee.BeeMetadataKey<T> key, ref T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `ref T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.IElementContext); // replace with a valid instance
var result = instance.TryGetMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: ref T */ default);
```

### IElementContextBuilder
<a id="type-dynabee-ielementcontextbuilder"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a builder context for configuring a specific element (such as a property, method, field, or constant)             within a dynamic type.

#### Properties

| Name | Type | Description |
|---|---|---|
| `ElementType` | `DynaBee.ElementType` | Gets the type of element being built (e.g., property, method, field, or constant). |
| `Name` | `string` | Gets the unique name assigned to the element being built. |
| `TypeContextBuilder` | `DynaBee.ITypeContextBuilder` | Gets the parent  that owns this element context. |

#### Method Index

1. [Build](#type-dynabee-ielementcontextbuilder-method-1)
2. [SetMetadata](#type-dynabee-ielementcontextbuilder-method-2)

#### Method: Build
<a id="type-dynabee-ielementcontextbuilder-method-1"></a>

**Description:** Finalizes the element context construction and returns an immutable              representing the completed element definition.

**Signature**

```csharp
public DynaBee.IElementContext Build()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.IElementContextBuilder); // replace with a valid instance
var result = instance.Build();
```

#### Method: SetMetadata
<a id="type-dynabee-ielementcontextbuilder-method-2"></a>

**Description:** Stores metadata in the current element builder context.

**Signature**

```csharp
public void SetMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |
| `value` | `object` | Yes | Metadata value. |

**Example**

```csharp
var instance = default(DynaBee.IElementContextBuilder); // replace with a valid instance
instance.SetMetadata(/* key: string */ default, /* value: object */ default);
```

### ITypeConfigurator
<a id="type-dynabee-itypeconfigurator"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a contract for configuring a dynamic type and its elements within an assembly context.

#### Properties

No public properties.

#### Method Index

1. [AddElementBuilder](#type-dynabee-itypeconfigurator-method-1)
2. [Configure](#type-dynabee-itypeconfigurator-method-2)

#### Method: AddElementBuilder
<a id="type-dynabee-itypeconfigurator-method-1"></a>

**Description:** Adds an  to the type configuration.

**Signature**

```csharp
public DynaBee.ITypeConfigurator AddElementBuilder(DynaBee.IElementConfigurator elementConfigurator)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `elementConfigurator` | `DynaBee.IElementConfigurator` | Yes | The element configurator that defines how to build a specific element             (such as a property, method, field, or constant). |

**Example**

```csharp
var instance = default(DynaBee.ITypeConfigurator); // replace with a valid instance
var result = instance.AddElementBuilder(/* elementConfigurator: DynaBee.IElementConfigurator */ default);
```

#### Method: Configure
<a id="type-dynabee-itypeconfigurator-method-2"></a>

**Description:** Applies the type configuration to the specified .             This method should define the type and its elements in the dynamic assembly.

**Signature**

```csharp
public void Configure(DynaBee.IAssemblyContextBuilder assemblyContextBuilder)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `assemblyContextBuilder` | `DynaBee.IAssemblyContextBuilder` | Yes | The assembly context builder to which the type definition and its elements will be added. |

**Example**

```csharp
var instance = default(DynaBee.ITypeConfigurator); // replace with a valid instance
instance.Configure(/* assemblyContextBuilder: DynaBee.IAssemblyContextBuilder */ default);
```

### ITypeContext
<a id="type-dynabee-itypecontext"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Represents an immutable context that provides metadata and access to a dynamically generated type             and its defined elements.

#### Properties

| Name | Type | Description |
|---|---|---|
| `ClrType` | `System.Type` | Gets the runtime CLR  that was dynamically created. |
| `Name` | `string` | Gets the unique name assigned to the dynamic type. |

#### Method Index

1. [Find](#type-dynabee-itypecontext-method-1)
2. [FindOne](#type-dynabee-itypecontext-method-2)
3. [GetMetadata](#type-dynabee-itypecontext-method-3)
4. [TryGetMetadata](#type-dynabee-itypecontext-method-4)
5. [TryGetMetadata](#type-dynabee-itypecontext-method-5)

#### Method: Find
<a id="type-dynabee-itypecontext-method-1"></a>

**Description:** Finds all elements within the type that match the specified predicate.

**Signature**

```csharp
public System.Collections.Generic.IEnumerable<DynaBee.IElementContext> Find(System.Func<DynaBee.IElementContext, bool> predicate)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `predicate` | `System.Func<DynaBee.IElementContext, bool>` | Yes | A function used to filter elements based on custom conditions. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContext); // replace with a valid instance
var result = instance.Find(/* predicate: System.Func<DynaBee.IElementContext, bool> */ default);
```

#### Method: FindOne
<a id="type-dynabee-itypecontext-method-2"></a>

**Description:** Finds a single element within the type by its unique name.

**Signature**

```csharp
public DynaBee.IElementContext FindOne(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | The unique name of the element to find. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContext); // replace with a valid instance
var result = instance.FindOne(/* name: string */ default);
```

#### Method: GetMetadata
<a id="type-dynabee-itypecontext-method-3"></a>

**Description:** Gets metadata attached to this generated type.

**Signature**

```csharp
public object GetMetadata(string key)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContext); // replace with a valid instance
var result = instance.GetMetadata(/* key: string */ default);
```

#### Method: TryGetMetadata
<a id="type-dynabee-itypecontext-method-4"></a>

**Description:** Tries to get metadata attached to this generated type.

**Signature**

```csharp
public bool TryGetMetadata(string key, ref object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |
| `value` | `ref object` | Yes | Metadata value when present. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContext); // replace with a valid instance
var result = instance.TryGetMetadata(/* key: string */ default, /* value: ref object */ default);
```

#### Method: TryGetMetadata
<a id="type-dynabee-itypecontext-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public bool TryGetMetadata(DynaBee.BeeMetadataKey<T> key, ref T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `ref T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContext); // replace with a valid instance
var result = instance.TryGetMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: ref T */ default);
```

### ITypeContextBuilder
<a id="type-dynabee-itypecontextbuilder"></a>

**Namespace:** `DynaBee`

**Kind:** Interface

**Description:** Defines a context that encapsulates metadata and access to a specific dynamic .

#### Properties

| Name | Type | Description |
|---|---|---|
| `AssemblyBuilderContext` | `DynaBee.IAssemblyContextBuilder` | Gets the parent  that owns this type context. |
| `Name` | `string` | Gets the unique name assigned to the type being built. |
| `TypeBuilder` | `System.Reflection.Emit.TypeBuilder` | Gets the underlying  used to define the dynamic type. |

#### Method Index

1. [AddElement](#type-dynabee-itypecontextbuilder-method-1)
2. [Build](#type-dynabee-itypecontextbuilder-method-2)
3. [SetMetadata](#type-dynabee-itypecontextbuilder-method-3)

#### Method: AddElement
<a id="type-dynabee-itypecontextbuilder-method-1"></a>

**Description:** Adds a new element with the specified name and type to the dynamic type definition             by applying the given  to the underlying .

**Signature**

```csharp
public DynaBee.IElementContextBuilder AddElement(string name, DynaBee.ElementType elementType, DynaBee.ElementBuilderAction buildAction, System.Collections.Generic.IReadOnlyDictionary<string, object> metadata)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | The unique name of the element to add. |
| `elementType` | `DynaBee.ElementType` | Yes | The kind of element being added (e.g., property, method, field, or constant). |
| `buildAction` | `DynaBee.ElementBuilderAction` | Yes | The action that defines how to configure or implement the element. |
| `metadata` | `System.Collections.Generic.IReadOnlyDictionary<string, object>` | No | Optional metadata attached to the element context. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContextBuilder); // replace with a valid instance
var result = instance.AddElement(/* name: string */ default, /* elementType: DynaBee.ElementType */ default, /* buildAction: DynaBee.ElementBuilderAction */ default, /* metadata: System.Collections.Generic.IReadOnlyDictionary<string, object> */ default);
```

#### Method: Build
<a id="type-dynabee-itypecontextbuilder-method-2"></a>

**Description:** Finalizes the type context construction and returns an immutable              representing the completed dynamic type definition.

**Signature**

```csharp
public DynaBee.ITypeContext Build()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.ITypeContextBuilder); // replace with a valid instance
var result = instance.Build();
```

#### Method: SetMetadata
<a id="type-dynabee-itypecontextbuilder-method-3"></a>

**Description:** Stores metadata in the current type builder context.

**Signature**

```csharp
public void SetMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |
| `value` | `object` | Yes | Metadata value. |

**Example**

```csharp
var instance = default(DynaBee.ITypeContextBuilder); // replace with a valid instance
instance.SetMetadata(/* key: string */ default, /* value: object */ default);
```

## Namespace: DynaBee.FluentApi
<a id="namespace-dynabee-fluentapi"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [AssemblyContextExtensions](#type-dynabee-fluentapi-assemblycontextextensions) | Class | Helper methods for consuming generated types from an assembly context. |
| [BeeAssemblyBuilder](#type-dynabee-fluentapi-beeassemblybuilder) | Class | Fluent builder for a dynamic assembly. |
| [BeeAttributeBuilder](#type-dynabee-fluentapi-beeattributebuilder) | Class | Fluent builder for custom attributes. |
| [BeeClassBuilder](#type-dynabee-fluentapi-beeclassbuilder) | Class | Fluent builder for a dynamic class. |
| [BeeConstructorBuilder](#type-dynabee-fluentapi-beeconstructorbuilder) | Class | Fluent builder for a dynamic constructor. |
| [BeeEnumBuilder](#type-dynabee-fluentapi-beeenumbuilder) | Class | Fluent builder for dynamic enums. |
| [BeeInterfaceBuilder](#type-dynabee-fluentapi-beeinterfacebuilder) | Class | Fluent builder for dynamic interfaces. |
| [BeeInterfaceMethodBuilder](#type-dynabee-fluentapi-beeinterfacemethodbuilder) | Class | Fluent builder for dynamic interface method signatures. |
| [BeeInterfacePropertyBuilder](#type-dynabee-fluentapi-beeinterfacepropertybuilder) | Class | Fluent builder for dynamic interface property signatures. |
| [BeeMethodBuilder](#type-dynabee-fluentapi-beemethodbuilder) | Class | Fluent builder for a dynamic method. |
| [BeePropertyBuilder](#type-dynabee-fluentapi-beepropertybuilder) | Class | Fluent builder for a dynamic property. |
| [BeeRecordClassBuilder](#type-dynabee-fluentapi-beerecordclassbuilder) | Class | Fluent builder for dynamic record classes. |
| [BeeRecordStructBuilder](#type-dynabee-fluentapi-beerecordstructbuilder) | Class | Fluent builder for dynamic record structs. |
| [BeeStructBuilder](#type-dynabee-fluentapi-beestructbuilder) | Class | Fluent builder for dynamic structs. |
| [DynaBeeBuilder](#type-dynabee-fluentapi-dynabeebuilder) | Class | Entry point for building dynamic assemblies with DynaBee fluent API. |
| [DynamicAccess](#type-dynabee-fluentapi-dynamicaccess) | Class | Runtime helpers to access generated type members without declaring host interfaces. |
| [RecordLikeAttribute](#type-dynabee-fluentapi-recordlikeattribute) | Class | Marker attribute indicating a generated type is intended to behave as a record-like model. |

### AssemblyContextExtensions
<a id="type-dynabee-fluentapi-assemblycontextextensions"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Helper methods for consuming generated types from an assembly context.

#### Properties

No public properties.

#### Method Index

1. [CreateInstance](#type-dynabee-fluentapi-assemblycontextextensions-method-1)
2. [CreateInstance](#type-dynabee-fluentapi-assemblycontextextensions-method-2)
3. [GetClrType](#type-dynabee-fluentapi-assemblycontextextensions-method-3)

#### Method: CreateInstance
<a id="type-dynabee-fluentapi-assemblycontextextensions-method-1"></a>

**Description:** Creates a new instance for a generated type by logical type name.

**Signature**

```csharp
public object CreateInstance(DynaBee.IAssemblyContext assemblyContext, string typeName, object[] args)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `assemblyContext` | `DynaBee.IAssemblyContext` | Yes | No description available. |
| `typeName` | `string` | Yes | No description available. |
| `args` | `object[]` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.AssemblyContextExtensions.CreateInstance(/* assemblyContext: DynaBee.IAssemblyContext */ default, /* typeName: string */ default, /* args: object[] */ default);
```

#### Method: CreateInstance
<a id="type-dynabee-fluentapi-assemblycontextextensions-method-2"></a>

**Description:** Creates a new instance for a generated type by logical type name.

**Signature**

```csharp
public T CreateInstance(DynaBee.IAssemblyContext assemblyContext, string typeName, object[] args)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `assemblyContext` | `DynaBee.IAssemblyContext` | Yes | No description available. |
| `typeName` | `string` | Yes | No description available. |
| `args` | `object[]` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.AssemblyContextExtensions.CreateInstance(/* assemblyContext: DynaBee.IAssemblyContext */ default, /* typeName: string */ default, /* args: object[] */ default);
```

#### Method: GetClrType
<a id="type-dynabee-fluentapi-assemblycontextextensions-method-3"></a>

**Description:** Gets the generated CLR type by logical type name.

**Signature**

```csharp
public System.Type GetClrType(DynaBee.IAssemblyContext assemblyContext, string typeName)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `assemblyContext` | `DynaBee.IAssemblyContext` | Yes | No description available. |
| `typeName` | `string` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.AssemblyContextExtensions.GetClrType(/* assemblyContext: DynaBee.IAssemblyContext */ default, /* typeName: string */ default);
```

### BeeAssemblyBuilder
<a id="type-dynabee-fluentapi-beeassemblybuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for a dynamic assembly.

#### Properties

No public properties.

#### Method Index

1. [AddClass](#type-dynabee-fluentapi-beeassemblybuilder-method-1)
2. [AddClass](#type-dynabee-fluentapi-beeassemblybuilder-method-2)
3. [AddEnum](#type-dynabee-fluentapi-beeassemblybuilder-method-3)
4. [AddEnum](#type-dynabee-fluentapi-beeassemblybuilder-method-4)
5. [AddEnum](#type-dynabee-fluentapi-beeassemblybuilder-method-5)
6. [AddInterface](#type-dynabee-fluentapi-beeassemblybuilder-method-6)
7. [AddInterface](#type-dynabee-fluentapi-beeassemblybuilder-method-7)
8. [AddRecordClass](#type-dynabee-fluentapi-beeassemblybuilder-method-8)
9. [AddRecordClass](#type-dynabee-fluentapi-beeassemblybuilder-method-9)
10. [AddRecordStruct](#type-dynabee-fluentapi-beeassemblybuilder-method-10)
11. [AddRecordStruct](#type-dynabee-fluentapi-beeassemblybuilder-method-11)
12. [AddStruct](#type-dynabee-fluentapi-beeassemblybuilder-method-12)
13. [AddStruct](#type-dynabee-fluentapi-beeassemblybuilder-method-13)
14. [Build](#type-dynabee-fluentapi-beeassemblybuilder-method-14)
15. [DisableCache](#type-dynabee-fluentapi-beeassemblybuilder-method-15)
16. [EnableCache](#type-dynabee-fluentapi-beeassemblybuilder-method-16)
17. [WithVersion](#type-dynabee-fluentapi-beeassemblybuilder-method-17)

#### Method: AddClass
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-1"></a>

**Description:** Adds a dynamic class to the assembly using public visibility.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddClass(string name, System.Action<DynaBee.FluentApi.BeeClassBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Class name. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeClassBuilder>` | Yes | Class configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddClass(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeClassBuilder> */ default);
```

#### Method: AddClass
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-2"></a>

**Description:** Adds a dynamic class to the assembly.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddClass(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeClassBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Class name. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | Class visibility. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeClassBuilder>` | No | Class configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddClass(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeClassBuilder> */ default);
```

#### Method: AddEnum
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddEnum(string name, System.Action<DynaBee.FluentApi.BeeEnumBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeEnumBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddEnum(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeEnumBuilder> */ default);
```

#### Method: AddEnum
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddEnum(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeEnumBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeEnumBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddEnum(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeEnumBuilder> */ default);
```

#### Method: AddEnum
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-5"></a>

**Description:** Adds a dynamic enum.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddEnum(string name, System.Type underlyingType, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeEnumBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `underlyingType` | `System.Type` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeEnumBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddEnum(/* name: string */ default, /* underlyingType: System.Type */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeEnumBuilder> */ default);
```

#### Method: AddInterface
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-6"></a>

**Description:** Adds a dynamic interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddInterface(string name, System.Action<DynaBee.FluentApi.BeeInterfaceBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfaceBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddInterface(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfaceBuilder> */ default);
```

#### Method: AddInterface
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-7"></a>

**Description:** Adds a dynamic interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddInterface(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeInterfaceBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfaceBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddInterface(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfaceBuilder> */ default);
```

#### Method: AddRecordClass
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-8"></a>

**Description:** Adds a dynamic record class.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddRecordClass(string name, System.Action<DynaBee.FluentApi.BeeRecordClassBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeRecordClassBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddRecordClass(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeRecordClassBuilder> */ default);
```

#### Method: AddRecordClass
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-9"></a>

**Description:** Adds a dynamic record class.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddRecordClass(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeRecordClassBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeRecordClassBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddRecordClass(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeRecordClassBuilder> */ default);
```

#### Method: AddRecordStruct
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-10"></a>

**Description:** Adds a dynamic record struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddRecordStruct(string name, System.Action<DynaBee.FluentApi.BeeRecordStructBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeRecordStructBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddRecordStruct(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeRecordStructBuilder> */ default);
```

#### Method: AddRecordStruct
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-11"></a>

**Description:** Adds a dynamic record struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddRecordStruct(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeRecordStructBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeRecordStructBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddRecordStruct(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeRecordStructBuilder> */ default);
```

#### Method: AddStruct
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-12"></a>

**Description:** Adds a dynamic struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddStruct(string name, System.Action<DynaBee.FluentApi.BeeStructBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeStructBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddStruct(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeStructBuilder> */ default);
```

#### Method: AddStruct
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-13"></a>

**Description:** Adds a dynamic struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder AddStruct(string name, DynaBee.Infrastructure.ClassAccessModifier accessModifier, System.Action<DynaBee.FluentApi.BeeStructBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.ClassAccessModifier` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeStructBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.AddStruct(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.ClassAccessModifier */ default, /* configure: System.Action<DynaBee.FluentApi.BeeStructBuilder> */ default);
```

#### Method: Build
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-14"></a>

**Description:** Builds all configured types and returns the assembly context.

**Signature**

```csharp
public DynaBee.IAssemblyContext Build()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.Build();
```

#### Method: DisableCache
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-15"></a>

**Description:** Disables the in-memory cache for this build operation.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder DisableCache()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.DisableCache();
```

#### Method: EnableCache
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-16"></a>

**Description:** Enables the in-memory cache for this build operation.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder EnableCache()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.EnableCache();
```

#### Method: WithVersion
<a id="type-dynabee-fluentapi-beeassemblybuilder-method-17"></a>

**Description:** Sets a semantic version token for the generated assembly cache key.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder WithVersion(string version)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `version` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAssemblyBuilder); // replace with a valid instance
var result = instance.WithVersion(/* version: string */ default);
```

### BeeAttributeBuilder
<a id="type-dynabee-fluentapi-beeattributebuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for custom attributes.

#### Properties

No public properties.

#### Method Index

1. [WithConstructorArgument](#type-dynabee-fluentapi-beeattributebuilder-method-1)
2. [WithConstructorArguments](#type-dynabee-fluentapi-beeattributebuilder-method-2)
3. [WithField](#type-dynabee-fluentapi-beeattributebuilder-method-3)
4. [WithProperty](#type-dynabee-fluentapi-beeattributebuilder-method-4)

#### Method: WithConstructorArgument
<a id="type-dynabee-fluentapi-beeattributebuilder-method-1"></a>

**Description:** Adds one constructor argument.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAttributeBuilder WithConstructorArgument(object argument)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `argument` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAttributeBuilder); // replace with a valid instance
var result = instance.WithConstructorArgument(/* argument: object */ default);
```

#### Method: WithConstructorArguments
<a id="type-dynabee-fluentapi-beeattributebuilder-method-2"></a>

**Description:** Adds constructor arguments.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAttributeBuilder WithConstructorArguments(object[] arguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `arguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAttributeBuilder); // replace with a valid instance
var result = instance.WithConstructorArguments(/* arguments: object[] */ default);
```

#### Method: WithField
<a id="type-dynabee-fluentapi-beeattributebuilder-method-3"></a>

**Description:** Sets a named field value.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAttributeBuilder WithField(string fieldName, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `fieldName` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAttributeBuilder); // replace with a valid instance
var result = instance.WithField(/* fieldName: string */ default, /* value: object */ default);
```

#### Method: WithProperty
<a id="type-dynabee-fluentapi-beeattributebuilder-method-4"></a>

**Description:** Sets a named property value.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAttributeBuilder WithProperty(string propertyName, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `propertyName` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeAttributeBuilder); // replace with a valid instance
var result = instance.WithProperty(/* propertyName: string */ default, /* value: object */ default);
```

### BeeClassBuilder
<a id="type-dynabee-fluentapi-beeclassbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for a dynamic class.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beeclassbuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beeclassbuilder-method-2)
3. [AddAttribute](#type-dynabee-fluentapi-beeclassbuilder-method-3)
4. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-4)
5. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-5)
6. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-6)
7. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-7)
8. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-8)
9. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-9)
10. [AddAutoProperty](#type-dynabee-fluentapi-beeclassbuilder-method-10)
11. [AddConstructor](#type-dynabee-fluentapi-beeclassbuilder-method-11)
12. [AddField](#type-dynabee-fluentapi-beeclassbuilder-method-12)
13. [AddField](#type-dynabee-fluentapi-beeclassbuilder-method-13)
14. [AddMethod](#type-dynabee-fluentapi-beeclassbuilder-method-14)
15. [AddProperty](#type-dynabee-fluentapi-beeclassbuilder-method-15)
16. [AddProperty](#type-dynabee-fluentapi-beeclassbuilder-method-16)
17. [AddReadOnlyProperty](#type-dynabee-fluentapi-beeclassbuilder-method-17)
18. [AddReadOnlyProperty](#type-dynabee-fluentapi-beeclassbuilder-method-18)
19. [AddVoidMethod](#type-dynabee-fluentapi-beeclassbuilder-method-19)
20. [AddWriteOnlyProperty](#type-dynabee-fluentapi-beeclassbuilder-method-20)
21. [AddWriteOnlyProperty](#type-dynabee-fluentapi-beeclassbuilder-method-21)
22. [Implements](#type-dynabee-fluentapi-beeclassbuilder-method-22)
23. [Implements](#type-dynabee-fluentapi-beeclassbuilder-method-23)
24. [Inherits](#type-dynabee-fluentapi-beeclassbuilder-method-24)
25. [Inherits](#type-dynabee-fluentapi-beeclassbuilder-method-25)
26. [Inject](#type-dynabee-fluentapi-beeclassbuilder-method-26)
27. [RegisterAsConcrete](#type-dynabee-fluentapi-beeclassbuilder-method-27)
28. [WithMetadata](#type-dynabee-fluentapi-beeclassbuilder-method-28)
29. [WithMetadata](#type-dynabee-fluentapi-beeclassbuilder-method-29)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeclassbuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated class.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeclassbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAttribute(object[] constructorArguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `constructorArguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* constructorArguments: object[] */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeclassbuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-5"></a>

**Description:** Adds an auto-property with private backing field and public getter/setter.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-6"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, DynaBee.BeeAttribute[] attributes)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `attributes` | `DynaBee.BeeAttribute[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* attributes: DynaBee.BeeAttribute[] */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-7"></a>

**Description:** Adds an auto-property with custom attributes.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, DynaBee.Infrastructure.BeeType type, DynaBee.BeeAttribute[] attributes)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |
| `attributes` | `DynaBee.BeeAttribute[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* attributes: DynaBee.BeeAttribute[] */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-8"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, bool hasGetter, bool hasSetter)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `hasGetter` | `bool` | Yes | No description available. |
| `hasSetter` | `bool` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* hasGetter: bool */ default, /* hasSetter: bool */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-9"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, bool hasGetter, bool hasSetter, DynaBee.Infrastructure.FieldAccessModifier fieldAccessModifier, DynaBee.Infrastructure.MethodAccessModifier getterAccessModifier, DynaBee.Infrastructure.MethodAccessModifier setterAccessModifier, System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute> attributes)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `hasGetter` | `bool` | Yes | No description available. |
| `hasSetter` | `bool` | Yes | No description available. |
| `fieldAccessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | Yes | No description available. |
| `getterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | No description available. |
| `setterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | No description available. |
| `attributes` | `System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* hasGetter: bool */ default, /* hasSetter: bool */ default, /* fieldAccessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default, /* getterAccessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default, /* setterAccessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default, /* attributes: System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute> */ default);
```

#### Method: AddAutoProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-10"></a>

**Description:** Adds an auto-property with configurable getter/setter visibility.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddAutoProperty(string name, DynaBee.Infrastructure.BeeType type, bool hasGetter, bool hasSetter, DynaBee.Infrastructure.FieldAccessModifier fieldAccessModifier, DynaBee.Infrastructure.MethodAccessModifier getterAccessModifier, DynaBee.Infrastructure.MethodAccessModifier setterAccessModifier, System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute> attributes)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |
| `hasGetter` | `bool` | Yes | No description available. |
| `hasSetter` | `bool` | Yes | No description available. |
| `fieldAccessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | No | No description available. |
| `getterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | No | No description available. |
| `setterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | No | No description available. |
| `attributes` | `System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddAutoProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* hasGetter: bool */ default, /* hasSetter: bool */ default, /* fieldAccessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default, /* getterAccessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default, /* setterAccessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default, /* attributes: System.Collections.Generic.IReadOnlyCollection<DynaBee.BeeAttribute> */ default);
```

#### Method: AddConstructor
<a id="type-dynabee-fluentapi-beeclassbuilder-method-11"></a>

**Description:** Adds a public constructor.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddConstructor(System.Action<DynaBee.FluentApi.BeeConstructorBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeConstructorBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddConstructor(/* configure: System.Action<DynaBee.FluentApi.BeeConstructorBuilder> */ default);
```

#### Method: AddField
<a id="type-dynabee-fluentapi-beeclassbuilder-method-12"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddField(string name, DynaBee.Infrastructure.FieldAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddField(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default);
```

#### Method: AddField
<a id="type-dynabee-fluentapi-beeclassbuilder-method-13"></a>

**Description:** Adds a field.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddField(string name, DynaBee.Infrastructure.BeeType type, DynaBee.Infrastructure.FieldAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddField(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* accessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beeclassbuilder-method-14"></a>

**Description:** Adds a method.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddMethod(string name, DynaBee.Infrastructure.BeeType returnType, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `returnType` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* returnType: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-15"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddProperty(string name, System.Action<DynaBee.FluentApi.BeePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeePropertyBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeePropertyBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-16"></a>

**Description:** Adds a property using fluent configuration.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddProperty(string name, DynaBee.Infrastructure.BeeType type, System.Action<DynaBee.FluentApi.BeePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeePropertyBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeePropertyBuilder> */ default);
```

#### Method: AddReadOnlyProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-17"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddReadOnlyProperty(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddReadOnlyProperty(/* name: string */ default);
```

#### Method: AddReadOnlyProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-18"></a>

**Description:** Adds a read-only auto-property (getter only).

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddReadOnlyProperty(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddReadOnlyProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

#### Method: AddVoidMethod
<a id="type-dynabee-fluentapi-beeclassbuilder-method-19"></a>

**Description:** Adds a void method.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddVoidMethod(string name, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddVoidMethod(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: AddWriteOnlyProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-20"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddWriteOnlyProperty(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddWriteOnlyProperty(/* name: string */ default);
```

#### Method: AddWriteOnlyProperty
<a id="type-dynabee-fluentapi-beeclassbuilder-method-21"></a>

**Description:** Adds a write-only auto-property (setter only).

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder AddWriteOnlyProperty(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.AddWriteOnlyProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beeclassbuilder-method-22"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder Implements(bool registerInDi)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `registerInDi` | `bool` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.Implements(/* registerInDi: bool */ default);
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beeclassbuilder-method-23"></a>

**Description:** Adds an interface implementation.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder Implements(System.Type interfaceType, bool registerInDi)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `interfaceType` | `System.Type` | Yes | No description available. |
| `registerInDi` | `bool` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.Implements(/* interfaceType: System.Type */ default, /* registerInDi: bool */ default);
```

#### Method: Inherits
<a id="type-dynabee-fluentapi-beeclassbuilder-method-24"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder Inherits()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.Inherits();
```

#### Method: Inherits
<a id="type-dynabee-fluentapi-beeclassbuilder-method-25"></a>

**Description:** Sets the base class for the generated type.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder Inherits(System.Type parentType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `parentType` | `System.Type` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.Inherits(/* parentType: System.Type */ default);
```

#### Method: Inject
<a id="type-dynabee-fluentapi-beeclassbuilder-method-26"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder Inject(string propertyName, string parameterName)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `propertyName` | `string` | Yes | No description available. |
| `parameterName` | `string` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.Inject(/* propertyName: string */ default, /* parameterName: string */ default);
```

#### Method: RegisterAsConcrete
<a id="type-dynabee-fluentapi-beeclassbuilder-method-27"></a>

**Description:** Sets whether this dynamic class is registered as concrete type in DI.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder RegisterAsConcrete(bool register)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `register` | `bool` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.RegisterAsConcrete(/* register: bool */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beeclassbuilder-method-28"></a>

**Description:** Stores metadata for this generated class.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder WithMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: string */ default, /* value: object */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beeclassbuilder-method-29"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeClassBuilder WithMetadata(DynaBee.BeeMetadataKey<T> key, T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeClassBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: T */ default);
```

### BeeConstructorBuilder
<a id="type-dynabee-fluentapi-beeconstructorbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for a dynamic constructor.

#### Properties

No public properties.

#### Method Index

1. [BeeConstructorBuilder](#type-dynabee-fluentapi-beeconstructorbuilder-method-1)
2. [Emits](#type-dynabee-fluentapi-beeconstructorbuilder-method-2)
3. [WithMetadata](#type-dynabee-fluentapi-beeconstructorbuilder-method-3)
4. [WithMetadata](#type-dynabee-fluentapi-beeconstructorbuilder-method-4)
5. [WithParameter](#type-dynabee-fluentapi-beeconstructorbuilder-method-5)
6. [WithParameter](#type-dynabee-fluentapi-beeconstructorbuilder-method-6)

#### Method: BeeConstructorBuilder
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public BeeConstructorBuilder()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.FluentApi.BeeConstructorBuilder();
```

#### Method: Emits
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-2"></a>

**Description:** Defines custom constructor body IL. The callback must emit a ret opcode.

**Signature**

```csharp
public DynaBee.FluentApi.BeeConstructorBuilder Emits(System.Action<System.Reflection.Emit.ILGenerator> body)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `body` | `System.Action<System.Reflection.Emit.ILGenerator>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeConstructorBuilder); // replace with a valid instance
var result = instance.Emits(/* body: System.Action<System.Reflection.Emit.ILGenerator> */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-3"></a>

**Description:** Stores metadata for this generated constructor.

**Signature**

```csharp
public DynaBee.FluentApi.BeeConstructorBuilder WithMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeConstructorBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: string */ default, /* value: object */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeConstructorBuilder WithMetadata(DynaBee.BeeMetadataKey<T> key, T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeConstructorBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: T */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeConstructorBuilder WithParameter(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeConstructorBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beeconstructorbuilder-method-6"></a>

**Description:** Adds one constructor parameter.

**Signature**

```csharp
public DynaBee.FluentApi.BeeConstructorBuilder WithParameter(string name, DynaBee.Infrastructure.BeeType parameterType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `parameterType` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeConstructorBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default, /* parameterType: DynaBee.Infrastructure.BeeType */ default);
```

### BeeEnumBuilder
<a id="type-dynabee-fluentapi-beeenumbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic enums.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beeenumbuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beeenumbuilder-method-2)
3. [AddValue](#type-dynabee-fluentapi-beeenumbuilder-method-3)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeenumbuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated enum.

**Signature**

```csharp
public DynaBee.FluentApi.BeeEnumBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeEnumBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeenumbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeEnumBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeEnumBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddValue
<a id="type-dynabee-fluentapi-beeenumbuilder-method-3"></a>

**Description:** Adds a named enum literal value.

**Signature**

```csharp
public DynaBee.FluentApi.BeeEnumBuilder AddValue(string name, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Enum literal name. |
| `value` | `object` | Yes | Enum literal value. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeEnumBuilder); // replace with a valid instance
var result = instance.AddValue(/* name: string */ default, /* value: object */ default);
```

### BeeInterfaceBuilder
<a id="type-dynabee-fluentapi-beeinterfacebuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic interfaces.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beeinterfacebuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beeinterfacebuilder-method-2)
3. [AddMethod](#type-dynabee-fluentapi-beeinterfacebuilder-method-3)
4. [AddMethod](#type-dynabee-fluentapi-beeinterfacebuilder-method-4)
5. [AddProperty](#type-dynabee-fluentapi-beeinterfacebuilder-method-5)
6. [AddProperty](#type-dynabee-fluentapi-beeinterfacebuilder-method-6)
7. [Inherits](#type-dynabee-fluentapi-beeinterfacebuilder-method-7)
8. [Inherits](#type-dynabee-fluentapi-beeinterfacebuilder-method-8)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddMethod(string name, System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder> */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-4"></a>

**Description:** Adds a method signature to the generated interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddMethod(string name, DynaBee.Infrastructure.BeeType returnType, System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Method name. |
| `returnType` | `DynaBee.Infrastructure.BeeType` | Yes | Method return type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder>` | No | Optional method configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* returnType: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfaceMethodBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddProperty(string name, System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-6"></a>

**Description:** Adds a property signature to the generated interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder AddProperty(string name, DynaBee.Infrastructure.BeeType type, System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Property name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Property type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder>` | No | Optional property configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeInterfacePropertyBuilder> */ default);
```

#### Method: Inherits
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-7"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder Inherits()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.Inherits();
```

#### Method: Inherits
<a id="type-dynabee-fluentapi-beeinterfacebuilder-method-8"></a>

**Description:** Adds a base interface to the generated interface.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceBuilder Inherits(System.Type interfaceType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `interfaceType` | `System.Type` | Yes | Base interface type. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceBuilder); // replace with a valid instance
var result = instance.Inherits(/* interfaceType: System.Type */ default);
```

### BeeInterfaceMethodBuilder
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic interface method signatures.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Gets the method name. |
| `ReturnType` | `DynaBee.Infrastructure.BeeType` | Gets the method return type. |

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beeinterfacemethodbuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beeinterfacemethodbuilder-method-2)
3. [WithAccess](#type-dynabee-fluentapi-beeinterfacemethodbuilder-method-3)
4. [WithParameter](#type-dynabee-fluentapi-beeinterfacemethodbuilder-method-4)
5. [WithParameter](#type-dynabee-fluentapi-beeinterfacemethodbuilder-method-5)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated method signature.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceMethodBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceMethodBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceMethodBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceMethodBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: WithAccess
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder-method-3"></a>

**Description:** Sets the access modifier for the generated method signature.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceMethodBuilder WithAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | Method access modifier. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceMethodBuilder); // replace with a valid instance
var result = instance.WithAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceMethodBuilder WithParameter(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceMethodBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beeinterfacemethodbuilder-method-5"></a>

**Description:** Adds a parameter to the interface method signature.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfaceMethodBuilder WithParameter(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Parameter name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Parameter type. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfaceMethodBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

### BeeInterfacePropertyBuilder
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic interface property signatures.

#### Properties

| Name | Type | Description |
|---|---|---|
| `GetterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Gets the getter access modifier. |
| `HasGetter` | `bool` | Gets whether the property defines a getter. |
| `HasSetter` | `bool` | Gets whether the property defines a setter. |
| `Name` | `string` | Gets the property name. |
| `SetterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Gets the setter access modifier. |
| `Type` | `DynaBee.Infrastructure.BeeType` | Gets the property type. |

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-2)
3. [AsReadOnly](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-3)
4. [AsWriteOnly](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-4)
5. [WithGetter](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-5)
6. [WithGetterAccess](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-6)
7. [WithSetter](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-7)
8. [WithSetterAccess](#type-dynabee-fluentapi-beeinterfacepropertybuilder-method-8)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated property signature.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AsReadOnly
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-3"></a>

**Description:** Sets the property to read-only (getter only).

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder AsReadOnly()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.AsReadOnly();
```

#### Method: AsWriteOnly
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-4"></a>

**Description:** Sets the property to write-only (setter only).

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder AsWriteOnly()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.AsWriteOnly();
```

#### Method: WithGetter
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-5"></a>

**Description:** Enables or disables the getter.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder WithGetter(bool enabled)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | `bool` | No | Whether getter should be enabled. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.WithGetter(/* enabled: bool */ default);
```

#### Method: WithGetterAccess
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-6"></a>

**Description:** Sets the getter access modifier.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder WithGetterAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | Getter access modifier. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.WithGetterAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

#### Method: WithSetter
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-7"></a>

**Description:** Enables or disables the setter.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder WithSetter(bool enabled)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | `bool` | No | Whether setter should be enabled. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.WithSetter(/* enabled: bool */ default);
```

#### Method: WithSetterAccess
<a id="type-dynabee-fluentapi-beeinterfacepropertybuilder-method-8"></a>

**Description:** Sets the setter access modifier.

**Signature**

```csharp
public DynaBee.FluentApi.BeeInterfacePropertyBuilder WithSetterAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | Setter access modifier. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeInterfacePropertyBuilder); // replace with a valid instance
var result = instance.WithSetterAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

### BeeMethodBuilder
<a id="type-dynabee-fluentapi-beemethodbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for a dynamic method.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Method name. |
| `ReturnType` | `DynaBee.Infrastructure.BeeType` | Return type. |

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beemethodbuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beemethodbuilder-method-2)
3. [AddAttribute](#type-dynabee-fluentapi-beemethodbuilder-method-3)
4. [AsStatic](#type-dynabee-fluentapi-beemethodbuilder-method-4)
5. [Emits](#type-dynabee-fluentapi-beemethodbuilder-method-5)
6. [EmitsExpression](#type-dynabee-fluentapi-beemethodbuilder-method-6)
7. [EmitsExpression](#type-dynabee-fluentapi-beemethodbuilder-method-7)
8. [EmitsInjectedLambda](#type-dynabee-fluentapi-beemethodbuilder-method-8)
9. [EmitsInjectedLambda](#type-dynabee-fluentapi-beemethodbuilder-method-9)
10. [EmitsLambda](#type-dynabee-fluentapi-beemethodbuilder-method-10)
11. [EmitsLambdaWithSelf](#type-dynabee-fluentapi-beemethodbuilder-method-11)
12. [EmitsLambdaWithSelf](#type-dynabee-fluentapi-beemethodbuilder-method-12)
13. [EmitsLambdaWithSelf](#type-dynabee-fluentapi-beemethodbuilder-method-13)
14. [WithAccess](#type-dynabee-fluentapi-beemethodbuilder-method-14)
15. [WithMetadata](#type-dynabee-fluentapi-beemethodbuilder-method-15)
16. [WithMetadata](#type-dynabee-fluentapi-beemethodbuilder-method-16)
17. [WithParameter](#type-dynabee-fluentapi-beemethodbuilder-method-17)
18. [WithParameter](#type-dynabee-fluentapi-beemethodbuilder-method-18)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beemethodbuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated method.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beemethodbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder AddAttribute(object[] constructorArguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `constructorArguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* constructorArguments: object[] */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beemethodbuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AsStatic
<a id="type-dynabee-fluentapi-beemethodbuilder-method-4"></a>

**Description:** Marks this method as static.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder AsStatic()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.AsStatic();
```

#### Method: Emits
<a id="type-dynabee-fluentapi-beemethodbuilder-method-5"></a>

**Description:** Defines custom method body IL. The callback must emit a ret opcode.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder Emits(System.Action<System.Reflection.Emit.ILGenerator> body)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `body` | `System.Action<System.Reflection.Emit.ILGenerator>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.Emits(/* body: System.Action<System.Reflection.Emit.ILGenerator> */ default);
```

#### Method: EmitsExpression
<a id="type-dynabee-fluentapi-beemethodbuilder-method-6"></a>

**Description:** Defines method logic from an expression tree that is translated to IL.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsExpression(System.Linq.Expressions.LambdaExpression expressionBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `expressionBody` | `System.Linq.Expressions.LambdaExpression` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsExpression(/* expressionBody: System.Linq.Expressions.LambdaExpression */ default);
```

#### Method: EmitsExpression
<a id="type-dynabee-fluentapi-beemethodbuilder-method-7"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsExpression(System.Linq.Expressions.Expression<TDelegate> expressionBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `expressionBody` | `System.Linq.Expressions.Expression<TDelegate>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsExpression(/* expressionBody: System.Linq.Expressions.Expression<TDelegate> */ default);
```

#### Method: EmitsInjectedLambda
<a id="type-dynabee-fluentapi-beemethodbuilder-method-8"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsInjectedLambda(string dependencyProperty, System.Func<TDependency, TResult> lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `dependencyProperty` | `string` | Yes | No description available. |
| `lambdaBody` | `System.Func<TDependency, TResult>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsInjectedLambda(/* dependencyProperty: string */ default, /* lambdaBody: System.Func<TDependency, TResult> */ default);
```

#### Method: EmitsInjectedLambda
<a id="type-dynabee-fluentapi-beemethodbuilder-method-9"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsInjectedLambda(string dependencyProperty, System.Func<TDependency, T1, TResult> lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `dependencyProperty` | `string` | Yes | No description available. |
| `lambdaBody` | `System.Func<TDependency, T1, TResult>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsInjectedLambda(/* dependencyProperty: string */ default, /* lambdaBody: System.Func<TDependency, T1, TResult> */ default);
```

#### Method: EmitsLambda
<a id="type-dynabee-fluentapi-beemethodbuilder-method-10"></a>

**Description:** Defines method logic using a .NET delegate. The delegate parameters             must match method parameters (or include target instance as first argument).

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsLambda(System.Delegate lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `lambdaBody` | `System.Delegate` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsLambda(/* lambdaBody: System.Delegate */ default);
```

#### Method: EmitsLambdaWithSelf
<a id="type-dynabee-fluentapi-beemethodbuilder-method-11"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsLambdaWithSelf(System.Func<TSelf, TResult> lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `lambdaBody` | `System.Func<TSelf, TResult>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsLambdaWithSelf(/* lambdaBody: System.Func<TSelf, TResult> */ default);
```

#### Method: EmitsLambdaWithSelf
<a id="type-dynabee-fluentapi-beemethodbuilder-method-12"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsLambdaWithSelf(System.Func<TSelf, T1, TResult> lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `lambdaBody` | `System.Func<TSelf, T1, TResult>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsLambdaWithSelf(/* lambdaBody: System.Func<TSelf, T1, TResult> */ default);
```

#### Method: EmitsLambdaWithSelf
<a id="type-dynabee-fluentapi-beemethodbuilder-method-13"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder EmitsLambdaWithSelf(System.Func<TSelf, T1, T2, TResult> lambdaBody)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `lambdaBody` | `System.Func<TSelf, T1, T2, TResult>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.EmitsLambdaWithSelf(/* lambdaBody: System.Func<TSelf, T1, T2, TResult> */ default);
```

#### Method: WithAccess
<a id="type-dynabee-fluentapi-beemethodbuilder-method-14"></a>

**Description:** Sets the method access modifier.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder WithAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.WithAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beemethodbuilder-method-15"></a>

**Description:** Stores metadata for this generated method.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder WithMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: string */ default, /* value: object */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beemethodbuilder-method-16"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder WithMetadata(DynaBee.BeeMetadataKey<T> key, T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: T */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beemethodbuilder-method-17"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder WithParameter(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default);
```

#### Method: WithParameter
<a id="type-dynabee-fluentapi-beemethodbuilder-method-18"></a>

**Description:** Adds one parameter to the method.

**Signature**

```csharp
public DynaBee.FluentApi.BeeMethodBuilder WithParameter(string name, DynaBee.Infrastructure.BeeType parameterType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `parameterType` | `DynaBee.Infrastructure.BeeType` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeMethodBuilder); // replace with a valid instance
var result = instance.WithParameter(/* name: string */ default, /* parameterType: DynaBee.Infrastructure.BeeType */ default);
```

### BeePropertyBuilder
<a id="type-dynabee-fluentapi-beepropertybuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for a dynamic property.

#### Properties

| Name | Type | Description |
|---|---|---|
| `BackingFieldAccessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | Backing field access modifier. |
| `GetterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Getter access modifier. |
| `HasGetter` | `bool` | Indicates whether property has getter. |
| `HasSetter` | `bool` | Indicates whether property has setter. |
| `Name` | `string` | Property name. |
| `SetterAccessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Setter access modifier. |
| `Type` | `DynaBee.Infrastructure.BeeType` | Property type. |

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beepropertybuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beepropertybuilder-method-2)
3. [AddAttribute](#type-dynabee-fluentapi-beepropertybuilder-method-3)
4. [AsReadOnly](#type-dynabee-fluentapi-beepropertybuilder-method-4)
5. [AsWriteOnly](#type-dynabee-fluentapi-beepropertybuilder-method-5)
6. [WithBackingFieldAccess](#type-dynabee-fluentapi-beepropertybuilder-method-6)
7. [WithGetter](#type-dynabee-fluentapi-beepropertybuilder-method-7)
8. [WithGetterAccess](#type-dynabee-fluentapi-beepropertybuilder-method-8)
9. [WithMetadata](#type-dynabee-fluentapi-beepropertybuilder-method-9)
10. [WithMetadata](#type-dynabee-fluentapi-beepropertybuilder-method-10)
11. [WithSetter](#type-dynabee-fluentapi-beepropertybuilder-method-11)
12. [WithSetterAccess](#type-dynabee-fluentapi-beepropertybuilder-method-12)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beepropertybuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated property.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beepropertybuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder AddAttribute(object[] constructorArguments)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `constructorArguments` | `object[]` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* constructorArguments: object[] */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beepropertybuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AsReadOnly
<a id="type-dynabee-fluentapi-beepropertybuilder-method-4"></a>

**Description:** Sets the property as read-only.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder AsReadOnly()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.AsReadOnly();
```

#### Method: AsWriteOnly
<a id="type-dynabee-fluentapi-beepropertybuilder-method-5"></a>

**Description:** Sets the property as write-only.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder AsWriteOnly()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.AsWriteOnly();
```

#### Method: WithBackingFieldAccess
<a id="type-dynabee-fluentapi-beepropertybuilder-method-6"></a>

**Description:** Sets backing field access.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithBackingFieldAccess(DynaBee.Infrastructure.FieldAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithBackingFieldAccess(/* accessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default);
```

#### Method: WithGetter
<a id="type-dynabee-fluentapi-beepropertybuilder-method-7"></a>

**Description:** Enables/disables getter.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithGetter(bool enabled)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | `bool` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithGetter(/* enabled: bool */ default);
```

#### Method: WithGetterAccess
<a id="type-dynabee-fluentapi-beepropertybuilder-method-8"></a>

**Description:** Sets getter access.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithGetterAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithGetterAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beepropertybuilder-method-9"></a>

**Description:** Stores metadata for this generated property.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: string */ default, /* value: object */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-fluentapi-beepropertybuilder-method-10"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithMetadata(DynaBee.BeeMetadataKey<T> key, T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `DynaBee.BeeMetadataKey<T>` | Yes | No description available. |
| `value` | `T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithMetadata(/* key: DynaBee.BeeMetadataKey<T> */ default, /* value: T */ default);
```

#### Method: WithSetter
<a id="type-dynabee-fluentapi-beepropertybuilder-method-11"></a>

**Description:** Enables/disables setter.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithSetter(bool enabled)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | `bool` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithSetter(/* enabled: bool */ default);
```

#### Method: WithSetterAccess
<a id="type-dynabee-fluentapi-beepropertybuilder-method-12"></a>

**Description:** Sets setter access.

**Signature**

```csharp
public DynaBee.FluentApi.BeePropertyBuilder WithSetterAccess(DynaBee.Infrastructure.MethodAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `accessModifier` | `DynaBee.Infrastructure.MethodAccessModifier` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeePropertyBuilder); // replace with a valid instance
var result = instance.WithSetterAccess(/* accessModifier: DynaBee.Infrastructure.MethodAccessModifier */ default);
```

### BeeRecordClassBuilder
<a id="type-dynabee-fluentapi-beerecordclassbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic record classes.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beerecordclassbuilder-method-1)
2. [AddComponent](#type-dynabee-fluentapi-beerecordclassbuilder-method-2)
3. [AddComponent](#type-dynabee-fluentapi-beerecordclassbuilder-method-3)
4. [AddMethod](#type-dynabee-fluentapi-beerecordclassbuilder-method-4)
5. [Implements](#type-dynabee-fluentapi-beerecordclassbuilder-method-5)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beerecordclassbuilder-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordClassBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordClassBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddComponent
<a id="type-dynabee-fluentapi-beerecordclassbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordClassBuilder AddComponent(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordClassBuilder); // replace with a valid instance
var result = instance.AddComponent(/* name: string */ default);
```

#### Method: AddComponent
<a id="type-dynabee-fluentapi-beerecordclassbuilder-method-3"></a>

**Description:** Adds a record component as a public property.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordClassBuilder AddComponent(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Component name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Component type. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordClassBuilder); // replace with a valid instance
var result = instance.AddComponent(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beerecordclassbuilder-method-4"></a>

**Description:** Adds a method to the generated record class.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordClassBuilder AddMethod(string name, DynaBee.Infrastructure.BeeType returnType, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Method name. |
| `returnType` | `DynaBee.Infrastructure.BeeType` | Yes | Method return type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | Optional method configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordClassBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* returnType: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beerecordclassbuilder-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordClassBuilder Implements()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordClassBuilder); // replace with a valid instance
var result = instance.Implements();
```

### BeeRecordStructBuilder
<a id="type-dynabee-fluentapi-beerecordstructbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic record structs.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beerecordstructbuilder-method-1)
2. [AddComponent](#type-dynabee-fluentapi-beerecordstructbuilder-method-2)
3. [AddComponent](#type-dynabee-fluentapi-beerecordstructbuilder-method-3)
4. [AddMethod](#type-dynabee-fluentapi-beerecordstructbuilder-method-4)
5. [Implements](#type-dynabee-fluentapi-beerecordstructbuilder-method-5)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beerecordstructbuilder-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordStructBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordStructBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddComponent
<a id="type-dynabee-fluentapi-beerecordstructbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordStructBuilder AddComponent(string name)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordStructBuilder); // replace with a valid instance
var result = instance.AddComponent(/* name: string */ default);
```

#### Method: AddComponent
<a id="type-dynabee-fluentapi-beerecordstructbuilder-method-3"></a>

**Description:** Adds a record component as a public property.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordStructBuilder AddComponent(string name, DynaBee.Infrastructure.BeeType type)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Component name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Component type. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordStructBuilder); // replace with a valid instance
var result = instance.AddComponent(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beerecordstructbuilder-method-4"></a>

**Description:** Adds a method to the generated record struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordStructBuilder AddMethod(string name, DynaBee.Infrastructure.BeeType returnType, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Method name. |
| `returnType` | `DynaBee.Infrastructure.BeeType` | Yes | Method return type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | Optional method configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordStructBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* returnType: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beerecordstructbuilder-method-5"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeRecordStructBuilder Implements()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeRecordStructBuilder); // replace with a valid instance
var result = instance.Implements();
```

### BeeStructBuilder
<a id="type-dynabee-fluentapi-beestructbuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Fluent builder for dynamic structs.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-fluentapi-beestructbuilder-method-1)
2. [AddAttribute](#type-dynabee-fluentapi-beestructbuilder-method-2)
3. [AddField](#type-dynabee-fluentapi-beestructbuilder-method-3)
4. [AddField](#type-dynabee-fluentapi-beestructbuilder-method-4)
5. [AddMethod](#type-dynabee-fluentapi-beestructbuilder-method-5)
6. [AddProperty](#type-dynabee-fluentapi-beestructbuilder-method-6)
7. [AddProperty](#type-dynabee-fluentapi-beestructbuilder-method-7)
8. [AddVoidMethod](#type-dynabee-fluentapi-beestructbuilder-method-8)
9. [Implements](#type-dynabee-fluentapi-beestructbuilder-method-9)
10. [Implements](#type-dynabee-fluentapi-beestructbuilder-method-10)

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beestructbuilder-method-1"></a>

**Description:** Adds a custom attribute to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: AddAttribute
<a id="type-dynabee-fluentapi-beestructbuilder-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddAttribute(System.Action<DynaBee.FluentApi.BeeAttributeBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `configure` | `System.Action<DynaBee.FluentApi.BeeAttributeBuilder>` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddAttribute(/* configure: System.Action<DynaBee.FluentApi.BeeAttributeBuilder> */ default);
```

#### Method: AddField
<a id="type-dynabee-fluentapi-beestructbuilder-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddField(string name, DynaBee.Infrastructure.FieldAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `accessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddField(/* name: string */ default, /* accessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default);
```

#### Method: AddField
<a id="type-dynabee-fluentapi-beestructbuilder-method-4"></a>

**Description:** Adds a field to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddField(string name, DynaBee.Infrastructure.BeeType type, DynaBee.Infrastructure.FieldAccessModifier accessModifier)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Field name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Field type. |
| `accessModifier` | `DynaBee.Infrastructure.FieldAccessModifier` | No | Field access modifier. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddField(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* accessModifier: DynaBee.Infrastructure.FieldAccessModifier */ default);
```

#### Method: AddMethod
<a id="type-dynabee-fluentapi-beestructbuilder-method-5"></a>

**Description:** Adds a method to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddMethod(string name, DynaBee.Infrastructure.BeeType returnType, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Method name. |
| `returnType` | `DynaBee.Infrastructure.BeeType` | Yes | Method return type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | Optional method configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddMethod(/* name: string */ default, /* returnType: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beestructbuilder-method-6"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddProperty(string name, System.Action<DynaBee.FluentApi.BeePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeePropertyBuilder>` | No | No description available. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeePropertyBuilder> */ default);
```

#### Method: AddProperty
<a id="type-dynabee-fluentapi-beestructbuilder-method-7"></a>

**Description:** Adds a property to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddProperty(string name, DynaBee.Infrastructure.BeeType type, System.Action<DynaBee.FluentApi.BeePropertyBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Property name. |
| `type` | `DynaBee.Infrastructure.BeeType` | Yes | Property type. |
| `configure` | `System.Action<DynaBee.FluentApi.BeePropertyBuilder>` | No | Optional property configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddProperty(/* name: string */ default, /* type: DynaBee.Infrastructure.BeeType */ default, /* configure: System.Action<DynaBee.FluentApi.BeePropertyBuilder> */ default);
```

#### Method: AddVoidMethod
<a id="type-dynabee-fluentapi-beestructbuilder-method-8"></a>

**Description:** Adds a void method to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder AddVoidMethod(string name, System.Action<DynaBee.FluentApi.BeeMethodBuilder> configure)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | Yes | Method name. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeMethodBuilder>` | No | Optional method configuration callback. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.AddVoidMethod(/* name: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeMethodBuilder> */ default);
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beestructbuilder-method-9"></a>

**Description:** No description available.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder Implements()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.Implements();
```

#### Method: Implements
<a id="type-dynabee-fluentapi-beestructbuilder-method-10"></a>

**Description:** Adds an interface implementation to the generated struct.

**Signature**

```csharp
public DynaBee.FluentApi.BeeStructBuilder Implements(System.Type interfaceType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `interfaceType` | `System.Type` | Yes | Interface type. |

**Example**

```csharp
var instance = default(DynaBee.FluentApi.BeeStructBuilder); // replace with a valid instance
var result = instance.Implements(/* interfaceType: System.Type */ default);
```

### DynaBeeBuilder
<a id="type-dynabee-fluentapi-dynabeebuilder"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Entry point for building dynamic assemblies with DynaBee fluent API.

#### Properties

No public properties.

#### Method Index

1. [CreateAssembly](#type-dynabee-fluentapi-dynabeebuilder-method-1)

#### Method: CreateAssembly
<a id="type-dynabee-fluentapi-dynabeebuilder-method-1"></a>

**Description:** Creates a new dynamic assembly builder.

**Signature**

```csharp
public DynaBee.FluentApi.BeeAssemblyBuilder CreateAssembly(string assemblyName)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `assemblyName` | `string` | Yes | Logical name of the dynamic assembly. |

**Example**

```csharp
var result = DynaBee.FluentApi.DynaBeeBuilder.CreateAssembly(/* assemblyName: string */ default);
```

### DynamicAccess
<a id="type-dynabee-fluentapi-dynamicaccess"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Runtime helpers to access generated type members without declaring host interfaces.

#### Properties

No public properties.

#### Method Index

1. [GetProperty](#type-dynabee-fluentapi-dynamicaccess-method-1)
2. [SetProperty](#type-dynabee-fluentapi-dynamicaccess-method-2)

#### Method: GetProperty
<a id="type-dynabee-fluentapi-dynamicaccess-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public T GetProperty(object instance, string propertyName)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `instance` | `object` | Yes | No description available. |
| `propertyName` | `string` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.DynamicAccess.GetProperty(/* instance: object */ default, /* propertyName: string */ default);
```

#### Method: SetProperty
<a id="type-dynabee-fluentapi-dynamicaccess-method-2"></a>

**Description:** Sets a property value in an object instance.

**Signature**

```csharp
public void SetProperty(object instance, string propertyName, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `instance` | `object` | Yes | No description available. |
| `propertyName` | `string` | Yes | No description available. |
| `value` | `object` | Yes | No description available. |

**Example**

```csharp
DynaBee.FluentApi.DynamicAccess.SetProperty(/* instance: object */ default, /* propertyName: string */ default, /* value: object */ default);
```

### RecordLikeAttribute
<a id="type-dynabee-fluentapi-recordlikeattribute"></a>

**Namespace:** `DynaBee.FluentApi`

**Kind:** Class

**Description:** Marker attribute indicating a generated type is intended to behave as a record-like model.

#### Properties

No public properties.

#### Method Index

1. [RecordLikeAttribute](#type-dynabee-fluentapi-recordlikeattribute-method-1)

#### Method: RecordLikeAttribute
<a id="type-dynabee-fluentapi-recordlikeattribute-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public RecordLikeAttribute()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.FluentApi.RecordLikeAttribute();
```

## Namespace: DynaBee.FluentApi.DependencyInjection
<a id="namespace-dynabee-fluentapi-dependencyinjection"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [DynaBeeServiceCollectionExtensions](#type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions) | Class | Extensions to register generated DynaBee types in DI. |

### DynaBeeServiceCollectionExtensions
<a id="type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions"></a>

**Namespace:** `DynaBee.FluentApi.DependencyInjection`

**Kind:** Class

**Description:** Extensions to register generated DynaBee types in DI.

#### Properties

No public properties.

#### Method Index

1. [AddDynaBee](#type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions-method-1)
2. [AddDynaBee](#type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions-method-2)

#### Method: AddDynaBee
<a id="type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions-method-1"></a>

**Description:** Registers generated concrete types from an existing assembly context in DI.

**Signature**

```csharp
public Microsoft.Extensions.DependencyInjection.IServiceCollection AddDynaBee(Microsoft.Extensions.DependencyInjection.IServiceCollection services, DynaBee.IAssemblyContext context, Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `services` | `Microsoft.Extensions.DependencyInjection.IServiceCollection` | Yes | No description available. |
| `context` | `DynaBee.IAssemblyContext` | Yes | No description available. |
| `lifetime` | `Microsoft.Extensions.DependencyInjection.ServiceLifetime` | No | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.DependencyInjection.DynaBeeServiceCollectionExtensions.AddDynaBee(/* services: Microsoft.Extensions.DependencyInjection.IServiceCollection */ default, /* context: DynaBee.IAssemblyContext */ default, /* lifetime: Microsoft.Extensions.DependencyInjection.ServiceLifetime */ default);
```

#### Method: AddDynaBee
<a id="type-dynabee-fluentapi-dependencyinjection-dynabeeservicecollectionextensions-method-2"></a>

**Description:** Builds a dynamic assembly and registers generated concrete types in DI.

**Signature**

```csharp
public Microsoft.Extensions.DependencyInjection.IServiceCollection AddDynaBee(Microsoft.Extensions.DependencyInjection.IServiceCollection services, string assemblyName, System.Action<DynaBee.FluentApi.BeeAssemblyBuilder> configure, string version, Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `services` | `Microsoft.Extensions.DependencyInjection.IServiceCollection` | Yes | No description available. |
| `assemblyName` | `string` | Yes | No description available. |
| `configure` | `System.Action<DynaBee.FluentApi.BeeAssemblyBuilder>` | Yes | No description available. |
| `version` | `string` | No | No description available. |
| `lifetime` | `Microsoft.Extensions.DependencyInjection.ServiceLifetime` | No | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.DependencyInjection.DynaBeeServiceCollectionExtensions.AddDynaBee(/* services: Microsoft.Extensions.DependencyInjection.IServiceCollection */ default, /* assemblyName: string */ default, /* configure: System.Action<DynaBee.FluentApi.BeeAssemblyBuilder> */ default, /* version: string */ default, /* lifetime: Microsoft.Extensions.DependencyInjection.ServiceLifetime */ default);
```

## Namespace: DynaBee.FluentApi.Diagnostics
<a id="namespace-dynabee-fluentapi-diagnostics"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [AssemblyDiagnostic](#type-dynabee-fluentapi-diagnostics-assemblydiagnostic) | Class | Diagnostic snapshot of a generated assembly. |
| [DynaBeeDiagnosticsExtensions](#type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions) | Class | Diagnostic helpers for generated assemblies and types. |
| [MemberDiagnostic](#type-dynabee-fluentapi-diagnostics-memberdiagnostic) | Class | Diagnostic snapshot of a generated member. |
| [TypeDiagnostic](#type-dynabee-fluentapi-diagnostics-typediagnostic) | Class | Diagnostic snapshot of a generated type. |

### AssemblyDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-assemblydiagnostic"></a>

**Namespace:** `DynaBee.FluentApi.Diagnostics`

**Kind:** Class

**Description:** Diagnostic snapshot of a generated assembly.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Gets the assembly name. |
| `Types` | `System.Collections.Generic.IReadOnlyCollection<DynaBee.FluentApi.Diagnostics.TypeDiagnostic>` | Gets diagnostic details for generated types. |
| `Version` | `string` | Gets the assembly version text. |

#### Method Index

1. [AssemblyDiagnostic](#type-dynabee-fluentapi-diagnostics-assemblydiagnostic-method-1)

#### Method: AssemblyDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-assemblydiagnostic-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public AssemblyDiagnostic()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.FluentApi.Diagnostics.AssemblyDiagnostic();
```

### DynaBeeDiagnosticsExtensions
<a id="type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions"></a>

**Namespace:** `DynaBee.FluentApi.Diagnostics`

**Kind:** Class

**Description:** Diagnostic helpers for generated assemblies and types.

#### Properties

No public properties.

#### Method Index

1. [GetDiagnostics](#type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions-method-1)
2. [ToDiagnosticsJson](#type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions-method-2)

#### Method: GetDiagnostics
<a id="type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions-method-1"></a>

**Description:** Creates a rich diagnostic snapshot for a generated assembly context.

**Signature**

```csharp
public DynaBee.FluentApi.Diagnostics.AssemblyDiagnostic GetDiagnostics(DynaBee.IAssemblyContext context)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `context` | `DynaBee.IAssemblyContext` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.Diagnostics.DynaBeeDiagnosticsExtensions.GetDiagnostics(/* context: DynaBee.IAssemblyContext */ default);
```

#### Method: ToDiagnosticsJson
<a id="type-dynabee-fluentapi-diagnostics-dynabeediagnosticsextensions-method-2"></a>

**Description:** Serializes diagnostics to JSON.

**Signature**

```csharp
public string ToDiagnosticsJson(DynaBee.IAssemblyContext context, bool indented)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `context` | `DynaBee.IAssemblyContext` | Yes | No description available. |
| `indented` | `bool` | No | No description available. |

**Example**

```csharp
var result = DynaBee.FluentApi.Diagnostics.DynaBeeDiagnosticsExtensions.ToDiagnosticsJson(/* context: DynaBee.IAssemblyContext */ default, /* indented: bool */ default);
```

### MemberDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-memberdiagnostic"></a>

**Namespace:** `DynaBee.FluentApi.Diagnostics`

**Kind:** Class

**Description:** Diagnostic snapshot of a generated member.

#### Properties

| Name | Type | Description |
|---|---|---|
| `AccessModifier` | `string` | Gets the member access modifier. |
| `Attributes` | `System.Collections.Generic.IReadOnlyCollection<string>` | Gets custom attributes declared on the member. |
| `Kind` | `string` | Gets the member kind (method, property, field, etc.). |
| `Name` | `string` | Gets the member name. |
| `Signature` | `string` | Gets a member signature summary. |

#### Method Index

1. [MemberDiagnostic](#type-dynabee-fluentapi-diagnostics-memberdiagnostic-method-1)

#### Method: MemberDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-memberdiagnostic-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public MemberDiagnostic()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.FluentApi.Diagnostics.MemberDiagnostic();
```

### TypeDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-typediagnostic"></a>

**Namespace:** `DynaBee.FluentApi.Diagnostics`

**Kind:** Class

**Description:** Diagnostic snapshot of a generated type.

#### Properties

| Name | Type | Description |
|---|---|---|
| `AccessModifier` | `string` | Gets the type access modifier. |
| `Attributes` | `System.Collections.Generic.IReadOnlyCollection<string>` | Gets custom attributes declared on the type. |
| `FullName` | `string` | Gets the type full name. |
| `Kind` | `string` | Gets the type kind (class, interface, struct, enum, record, etc.). |
| `Members` | `System.Collections.Generic.IReadOnlyCollection<DynaBee.FluentApi.Diagnostics.MemberDiagnostic>` | Gets diagnostic details for members declared on the type. |
| `Name` | `string` | Gets the type short name. |

#### Method Index

1. [TypeDiagnostic](#type-dynabee-fluentapi-diagnostics-typediagnostic-method-1)

#### Method: TypeDiagnostic
<a id="type-dynabee-fluentapi-diagnostics-typediagnostic-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public TypeDiagnostic()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.FluentApi.Diagnostics.TypeDiagnostic();
```

## Namespace: DynaBee.Infrastructure
<a id="namespace-dynabee-infrastructure"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [BeeType](#type-dynabee-infrastructure-beetype) | Struct | Represents a dynamic type abstraction that can be either a direct  reference              or a string-based type name to be resolved later. |
| [ClassAccessModifier](#type-dynabee-infrastructure-classaccessmodifier) | Struct | Represents valid access modifiers for classes when using Reflection.Emit.             Restricts usage to combinations allowed by C# and the .NET runtime. |
| [FieldAccessModifier](#type-dynabee-infrastructure-fieldaccessmodifier) | Struct | Represents valid access modifiers for fields. |
| [IClassConfigurator](#type-dynabee-infrastructure-iclassconfigurator) | Interface | Defines a contract for configuring a dynamic class,             including its base type and other metadata. |
| [MethodAccessModifier](#type-dynabee-infrastructure-methodaccessmodifier) | Struct | Represents valid access modifiers for methods. |

### BeeType
<a id="type-dynabee-infrastructure-beetype"></a>

**Namespace:** `DynaBee.Infrastructure`

**Kind:** Struct

**Description:** Represents a dynamic type abstraction that can be either a direct  reference              or a string-based type name to be resolved later.

#### Properties

No public properties.

#### Method Index

1. [Parse](#type-dynabee-infrastructure-beetype-method-1)
2. [TryParse](#type-dynabee-infrastructure-beetype-method-2)

#### Method: Parse
<a id="type-dynabee-infrastructure-beetype-method-1"></a>

**Description:** Creates a  from a reference type name.

**Signature**

```csharp
public DynaBee.Infrastructure.BeeType Parse(string referenceType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `referenceType` | `string` | Yes | Logical name of another dynamic type. |

**Example**

```csharp
var result = DynaBee.Infrastructure.BeeType.Parse(/* referenceType: string */ default);
```

#### Method: TryParse
<a id="type-dynabee-infrastructure-beetype-method-2"></a>

**Description:** Tries to create a  from a reference type name.

**Signature**

```csharp
public bool TryParse(string referenceType, ref DynaBee.Infrastructure.BeeType beeType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `referenceType` | `string` | Yes | Logical name of another dynamic type. |
| `beeType` | `ref DynaBee.Infrastructure.BeeType` | Yes | Resulting parsed value if successful. |

**Example**

```csharp
var result = DynaBee.Infrastructure.BeeType.TryParse(/* referenceType: string */ default, /* beeType: ref DynaBee.Infrastructure.BeeType */ default);
```

### ClassAccessModifier
<a id="type-dynabee-infrastructure-classaccessmodifier"></a>

**Namespace:** `DynaBee.Infrastructure`

**Kind:** Struct

**Description:** Represents valid access modifiers for classes when using Reflection.Emit.             Restricts usage to combinations allowed by C# and the .NET runtime.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Internal` | `DynaBee.Infrastructure.ClassAccessModifier` | Internal class (visible only within the same assembly).             Equivalent to C#'s 'internal'. |
| `IsDefault` | `bool` | Gets whether this instance is uninitialized (equal to 'default'). |
| `NestedInternal` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested internal class (visible only within the same assembly). |
| `NestedPrivate` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested private class (visible only within the containing class). |
| `NestedPrivateProtected` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested private protected class (visible to derived classes within the same assembly).             Equivalent to C#'s 'private protected'. |
| `NestedProtected` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested protected class (visible only to derived classes). |
| `NestedProtectedInternal` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested protected internal class (visible to derived classes or within the same assembly).             Equivalent to C#'s 'protected internal'. |
| `NestedPublic` | `DynaBee.Infrastructure.ClassAccessModifier` | Nested public class (visible anywhere if the containing class is accessible). |
| `Public` | `DynaBee.Infrastructure.ClassAccessModifier` | Public class (visible from other assemblies). |

#### Method Index

1. [Equals](#type-dynabee-infrastructure-classaccessmodifier-method-1)
2. [GetHashCode](#type-dynabee-infrastructure-classaccessmodifier-method-2)
3. [ToString](#type-dynabee-infrastructure-classaccessmodifier-method-3)

#### Method: Equals
<a id="type-dynabee-infrastructure-classaccessmodifier-method-1"></a>

**Description:** Determines whether this instance is equal to another object.

**Signature**

```csharp
public bool Equals(object obj)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `obj` | `object` | Yes | The object to compare with. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.ClassAccessModifier); // replace with a valid instance
var result = instance.Equals(/* obj: object */ default);
```

#### Method: GetHashCode
<a id="type-dynabee-infrastructure-classaccessmodifier-method-2"></a>

**Description:** Returns a hash code for the current modifier.

**Signature**

```csharp
public int GetHashCode()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.ClassAccessModifier); // replace with a valid instance
var result = instance.GetHashCode();
```

#### Method: ToString
<a id="type-dynabee-infrastructure-classaccessmodifier-method-3"></a>

**Description:** Returns the underlying  as a string for debugging purposes.

**Signature**

```csharp
public string ToString()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.ClassAccessModifier); // replace with a valid instance
var result = instance.ToString();
```

### FieldAccessModifier
<a id="type-dynabee-infrastructure-fieldaccessmodifier"></a>

**Namespace:** `DynaBee.Infrastructure`

**Kind:** Struct

**Description:** Represents valid access modifiers for fields.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Internal` | `DynaBee.Infrastructure.FieldAccessModifier` | Internal field. |
| `IsDefault` | `bool` | Gets whether this instance is uninitialized. |
| `Private` | `DynaBee.Infrastructure.FieldAccessModifier` | Private field. |
| `PrivateProtected` | `DynaBee.Infrastructure.FieldAccessModifier` | Private protected field. |
| `Protected` | `DynaBee.Infrastructure.FieldAccessModifier` | Protected field. |
| `ProtectedInternal` | `DynaBee.Infrastructure.FieldAccessModifier` | Protected internal field. |
| `Public` | `DynaBee.Infrastructure.FieldAccessModifier` | Public field. |

#### Method Index

1. [Equals](#type-dynabee-infrastructure-fieldaccessmodifier-method-1)
2. [GetHashCode](#type-dynabee-infrastructure-fieldaccessmodifier-method-2)
3. [ToString](#type-dynabee-infrastructure-fieldaccessmodifier-method-3)

#### Method: Equals
<a id="type-dynabee-infrastructure-fieldaccessmodifier-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public bool Equals(object obj)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `obj` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.FieldAccessModifier); // replace with a valid instance
var result = instance.Equals(/* obj: object */ default);
```

#### Method: GetHashCode
<a id="type-dynabee-infrastructure-fieldaccessmodifier-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public int GetHashCode()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.FieldAccessModifier); // replace with a valid instance
var result = instance.GetHashCode();
```

#### Method: ToString
<a id="type-dynabee-infrastructure-fieldaccessmodifier-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public string ToString()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.FieldAccessModifier); // replace with a valid instance
var result = instance.ToString();
```

### IClassConfigurator
<a id="type-dynabee-infrastructure-iclassconfigurator"></a>

**Namespace:** `DynaBee.Infrastructure`

**Kind:** Interface

**Description:** Defines a contract for configuring a dynamic class,             including its base type and other metadata.

#### Properties

No public properties.

#### Method Index

1. [AddAttribute](#type-dynabee-infrastructure-iclassconfigurator-method-1)
2. [Implements](#type-dynabee-infrastructure-iclassconfigurator-method-2)
3. [RegisterAsConcrete](#type-dynabee-infrastructure-iclassconfigurator-method-3)
4. [WithMetadata](#type-dynabee-infrastructure-iclassconfigurator-method-4)
5. [WithParentType](#type-dynabee-infrastructure-iclassconfigurator-method-5)

#### Method: AddAttribute
<a id="type-dynabee-infrastructure-iclassconfigurator-method-1"></a>

**Description:** Adds a custom attribute to the dynamic class.

**Signature**

```csharp
public DynaBee.Infrastructure.IClassConfigurator AddAttribute(DynaBee.BeeAttribute attribute)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `attribute` | `DynaBee.BeeAttribute` | Yes | Attribute descriptor. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.IClassConfigurator); // replace with a valid instance
var result = instance.AddAttribute(/* attribute: DynaBee.BeeAttribute */ default);
```

#### Method: Implements
<a id="type-dynabee-infrastructure-iclassconfigurator-method-2"></a>

**Description:** Adds an interface that the dynamic class must implement.

**Signature**

```csharp
public DynaBee.Infrastructure.IClassConfigurator Implements(System.Type interfaceType, bool registerInDi)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `interfaceType` | `System.Type` | Yes | Interface type to implement. |
| `registerInDi` | `bool` | No | True to register this interface in DI; otherwise false. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.IClassConfigurator); // replace with a valid instance
var result = instance.Implements(/* interfaceType: System.Type */ default, /* registerInDi: bool */ default);
```

#### Method: RegisterAsConcrete
<a id="type-dynabee-infrastructure-iclassconfigurator-method-3"></a>

**Description:** Sets whether this dynamic class should be registered as its own concrete type in DI.

**Signature**

```csharp
public DynaBee.Infrastructure.IClassConfigurator RegisterAsConcrete(bool register)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `register` | `bool` | No | True to register concrete type; false to skip it. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.IClassConfigurator); // replace with a valid instance
var result = instance.RegisterAsConcrete(/* register: bool */ default);
```

#### Method: WithMetadata
<a id="type-dynabee-infrastructure-iclassconfigurator-method-4"></a>

**Description:** Stores metadata for the generated class context.

**Signature**

```csharp
public DynaBee.Infrastructure.IClassConfigurator WithMetadata(string key, object value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | `string` | Yes | Metadata key. |
| `value` | `object` | Yes | Metadata value. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.IClassConfigurator); // replace with a valid instance
var result = instance.WithMetadata(/* key: string */ default, /* value: object */ default);
```

#### Method: WithParentType
<a id="type-dynabee-infrastructure-iclassconfigurator-method-5"></a>

**Description:** Specifies the parent (base) type that the dynamically generated class should inherit from.

**Signature**

```csharp
public DynaBee.Infrastructure.IClassConfigurator WithParentType(System.Type parentType)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `parentType` | `System.Type` | Yes | The base  to inherit. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.IClassConfigurator); // replace with a valid instance
var result = instance.WithParentType(/* parentType: System.Type */ default);
```

### MethodAccessModifier
<a id="type-dynabee-infrastructure-methodaccessmodifier"></a>

**Namespace:** `DynaBee.Infrastructure`

**Kind:** Struct

**Description:** Represents valid access modifiers for methods.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Internal` | `DynaBee.Infrastructure.MethodAccessModifier` | Internal method. |
| `IsDefault` | `bool` | Gets whether this instance is uninitialized. |
| `Private` | `DynaBee.Infrastructure.MethodAccessModifier` | Private method. |
| `PrivateProtected` | `DynaBee.Infrastructure.MethodAccessModifier` | Private protected method. |
| `Protected` | `DynaBee.Infrastructure.MethodAccessModifier` | Protected method. |
| `ProtectedInternal` | `DynaBee.Infrastructure.MethodAccessModifier` | Protected internal method. |
| `Public` | `DynaBee.Infrastructure.MethodAccessModifier` | Public method. |

#### Method Index

1. [Equals](#type-dynabee-infrastructure-methodaccessmodifier-method-1)
2. [GetHashCode](#type-dynabee-infrastructure-methodaccessmodifier-method-2)
3. [ToString](#type-dynabee-infrastructure-methodaccessmodifier-method-3)

#### Method: Equals
<a id="type-dynabee-infrastructure-methodaccessmodifier-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public bool Equals(object obj)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `obj` | `object` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.MethodAccessModifier); // replace with a valid instance
var result = instance.Equals(/* obj: object */ default);
```

#### Method: GetHashCode
<a id="type-dynabee-infrastructure-methodaccessmodifier-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public int GetHashCode()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.MethodAccessModifier); // replace with a valid instance
var result = instance.GetHashCode();
```

#### Method: ToString
<a id="type-dynabee-infrastructure-methodaccessmodifier-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public string ToString()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Infrastructure.MethodAccessModifier); // replace with a valid instance
var result = instance.ToString();
```

## Namespace: DynaBee.Infrastructure.Configurators
<a id="namespace-dynabee-infrastructure-configurators"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [LambdaMethodRegistry](#type-dynabee-infrastructure-configurators-lambdamethodregistry) | Class | Registry for runtime delegates used by generated methods implemented through lambdas. |

### LambdaMethodRegistry
<a id="type-dynabee-infrastructure-configurators-lambdamethodregistry"></a>

**Namespace:** `DynaBee.Infrastructure.Configurators`

**Kind:** Class

**Description:** Registry for runtime delegates used by generated methods implemented through lambdas.

#### Properties

No public properties.

#### Method Index

1. [Invoke](#type-dynabee-infrastructure-configurators-lambdamethodregistry-method-1)
2. [Register](#type-dynabee-infrastructure-configurators-lambdamethodregistry-method-2)

#### Method: Invoke
<a id="type-dynabee-infrastructure-configurators-lambdamethodregistry-method-1"></a>

**Description:** Invokes a previously registered delegate.

**Signature**

```csharp
public object Invoke(int id, object instance, object[] args)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | `int` | Yes | Delegate identifier returned by . |
| `instance` | `object` | Yes | Target instance of the generated type, if needed. |
| `args` | `object[]` | Yes | Method arguments. |

**Example**

```csharp
var result = DynaBee.Infrastructure.Configurators.LambdaMethodRegistry.Invoke(/* id: int */ default, /* instance: object */ default, /* args: object[] */ default);
```

#### Method: Register
<a id="type-dynabee-infrastructure-configurators-lambdamethodregistry-method-2"></a>

**Description:** Registers a delegate and returns a unique identifier.

**Signature**

```csharp
public int Register(System.Delegate methodDelegate)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `methodDelegate` | `System.Delegate` | Yes | No description available. |

**Example**

```csharp
var result = DynaBee.Infrastructure.Configurators.LambdaMethodRegistry.Register(/* methodDelegate: System.Delegate */ default);
```

## Namespace: DynaBee.Tools
<a id="namespace-dynabee-tools"></a>

### Type Index

| Type | Kind | Description |
|---|---|---|
| [Immutable`1](#type-dynabee-tools-immutable`1) | Class | No description available. |
| [IValidableArgument](#type-dynabee-tools-ivalidableargument) | Interface | Defines a contract for an argument that can be validated. |

### Immutable`1
<a id="type-dynabee-tools-immutable`1"></a>

**Namespace:** `DynaBee.Tools`

**Kind:** Class

**Description:** No description available.

#### Properties

| Name | Type | Description |
|---|---|---|
| `Value` | `T` | No description available. |

#### Method Index

1. [Immutable`1](#type-dynabee-tools-immutable`1-method-1)
2. [Immutable`1](#type-dynabee-tools-immutable`1-method-2)
3. [IsValid](#type-dynabee-tools-immutable`1-method-3)
4. [Set](#type-dynabee-tools-immutable`1-method-4)

#### Method: Immutable`1
<a id="type-dynabee-tools-immutable`1-method-1"></a>

**Description:** No description available.

**Signature**

```csharp
public Immutable`1()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = new DynaBee.Tools.Immutable<T>();
```

#### Method: Immutable`1
<a id="type-dynabee-tools-immutable`1-method-2"></a>

**Description:** No description available.

**Signature**

```csharp
public Immutable`1(System.Func<T, bool> isValid)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `isValid` | `System.Func<T, bool>` | Yes | No description available. |

**Example**

```csharp
var instance = new DynaBee.Tools.Immutable<T>(/* isValid: System.Func<T, bool> */ default);
```

#### Method: IsValid
<a id="type-dynabee-tools-immutable`1-method-3"></a>

**Description:** No description available.

**Signature**

```csharp
public bool IsValid()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Tools.Immutable<T>); // replace with a valid instance
var result = instance.IsValid();
```

#### Method: Set
<a id="type-dynabee-tools-immutable`1-method-4"></a>

**Description:** No description available.

**Signature**

```csharp
public void Set(T value)
```

**Parameters**

| Name | Type | Required | Description |
|---|---|---|---|
| `value` | `T` | Yes | No description available. |

**Example**

```csharp
var instance = default(DynaBee.Tools.Immutable<T>); // replace with a valid instance
instance.Set(/* value: T */ default);
```

### IValidableArgument
<a id="type-dynabee-tools-ivalidableargument"></a>

**Namespace:** `DynaBee.Tools`

**Kind:** Interface

**Description:** Defines a contract for an argument that can be validated.

#### Properties

No public properties.

#### Method Index

1. [IsValid](#type-dynabee-tools-ivalidableargument-method-1)

#### Method: IsValid
<a id="type-dynabee-tools-ivalidableargument-method-1"></a>

**Description:** Determines whether the current value of the argument is valid.

**Signature**

```csharp
public bool IsValid()
```

**Parameters**

This method does not receive parameters.

**Example**

```csharp
var instance = default(DynaBee.Tools.IValidableArgument); // replace with a valid instance
var result = instance.IsValid();
```

