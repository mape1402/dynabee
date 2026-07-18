namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Builds a method body using high-level operations instead of raw IL opcodes.
    /// </summary>
    public interface IBeeMethodBodyBuilder
    {
        /// <summary>
        /// Creates an expression that loads the generated instance that owns the current method.
        /// </summary>
        /// <returns>A value expression representing <c>this</c>.</returns>
        IBeeValueExpression Self();

        /// <summary>
        /// Gets a method parameter by name.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>The matching parameter expression.</returns>
        IBeeParameter Parameter(string name);

        /// <summary>
        /// Gets a typed method parameter by name.
        /// </summary>
        /// <typeparam name="T">Expected parameter type.</typeparam>
        /// <param name="name">Parameter name.</param>
        /// <returns>The matching parameter expression.</returns>
        IBeeParameter Parameter<T>(string name);

        /// <summary>
        /// Declares a local variable.
        /// </summary>
        /// <param name="name">Local variable name.</param>
        /// <param name="type">Local variable type.</param>
        /// <returns>The declared local expression.</returns>
        IBeeLocal DeclareLocal(string name, Type type);

        /// <summary>
        /// Declares a typed local variable.
        /// </summary>
        /// <typeparam name="T">Local variable type.</typeparam>
        /// <param name="name">Local variable name.</param>
        /// <returns>The declared local expression.</returns>
        IBeeLocal DeclareLocal<T>(string name);

        /// <summary>
        /// Creates a value expression that constructs an object using its parameterless constructor.
        /// </summary>
        /// <param name="type">Type to instantiate.</param>
        /// <returns>A construction expression.</returns>
        IBeeValueExpression New(Type type);

        /// <summary>
        /// Creates a value expression that constructs an object using a matching constructor.
        /// </summary>
        /// <param name="type">Type to instantiate.</param>
        /// <param name="arguments">Constructor argument expressions.</param>
        /// <returns>A construction expression.</returns>
        IBeeValueExpression New(Type type, params IBeeValueExpression[] arguments);

        /// <summary>
        /// Creates a typed value expression that constructs an object using its parameterless constructor.
        /// </summary>
        /// <typeparam name="T">Type to instantiate.</typeparam>
        /// <returns>A construction expression.</returns>
        IBeeValueExpression New<T>();

        /// <summary>
        /// Creates a value expression that constructs a one-dimensional array with a runtime length.
        /// </summary>
        /// <param name="elementType">Array element type.</param>
        /// <param name="length">Array length expression.</param>
        /// <returns>An array construction expression.</returns>
        IBeeValueExpression NewArray(Type elementType, IBeeValueExpression length);

        /// <summary>
        /// Creates a typed value expression that constructs a one-dimensional array with a runtime length.
        /// </summary>
        /// <typeparam name="TElement">Array element type.</typeparam>
        /// <param name="length">Array length expression.</param>
        /// <returns>An array construction expression.</returns>
        IBeeValueExpression NewArray<TElement>(IBeeValueExpression length);

        /// <summary>
        /// Creates an array or indexer access expression.
        /// </summary>
        /// <param name="instance">Array or indexer-owning instance expression.</param>
        /// <param name="index">Index expression.</param>
        /// <returns>An assignable index expression when the target supports assignment.</returns>
        IBeeAssignableExpression Index(IBeeValueExpression instance, IBeeValueExpression index);

        /// <summary>
        /// Creates a less-than comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression LessThan(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a less-than-or-equal comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression LessThanOrEqual(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a greater-than comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression GreaterThan(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a greater-than-or-equal comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression GreaterThanOrEqual(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Emits a for loop statement.
        /// </summary>
        /// <param name="initialize">Loop initialization block.</param>
        /// <param name="condition">Loop condition expression factory. The expression must be boolean.</param>
        /// <param name="increment">Loop increment block.</param>
        /// <param name="body">Loop body block.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder For(
            Action<IBeeMethodBodyBuilder> initialize,
            Func<IBeeMethodBodyBuilder, IBeeValueExpression> condition,
            Action<IBeeMethodBodyBuilder> increment,
            Action<IBeeMethodBodyBuilder> body);

        /// <summary>
        /// Emits a foreach loop statement over an enumerable value.
        /// </summary>
        /// <param name="source">Enumerable source expression.</param>
        /// <param name="itemName">Generated loop item local name.</param>
        /// <param name="body">Loop body block that receives the current item expression.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder ForEach(
            IBeeValueExpression source,
            string itemName,
            Action<IBeeLocal, IBeeMethodBodyBuilder> body);

        /// <summary>
        /// Creates an instance method call expression.
        /// </summary>
        /// <param name="instance">Instance expression used as the call target.</param>
        /// <param name="method">Method to call. Closed generic methods are supported.</param>
        /// <param name="arguments">Argument expressions passed to the method.</param>
        /// <returns>A value expression representing the method call result.</returns>
        IBeeValueExpression Call(IBeeValueExpression instance, System.Reflection.MethodInfo method, params IBeeValueExpression[] arguments);

        /// <summary>
        /// Creates an instance method call expression by resolving a method name and exact parameter types.
        /// </summary>
        /// <param name="instance">Instance expression used as the call target.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Exact method parameter types.</param>
        /// <param name="arguments">Argument expressions passed to the method.</param>
        /// <returns>A value expression representing the method call result.</returns>
        IBeeValueExpression Call(
            IBeeValueExpression instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            params IBeeValueExpression[] arguments);

        /// <summary>
        /// Creates a static method call expression.
        /// </summary>
        /// <param name="method">Static method to call. Closed generic methods are supported.</param>
        /// <param name="arguments">Argument expressions passed to the method.</param>
        /// <returns>A value expression representing the method call result.</returns>
        IBeeValueExpression StaticCall(System.Reflection.MethodInfo method, params IBeeValueExpression[] arguments);

        /// <summary>
        /// Creates a static method call expression by resolving a method name and exact parameter types.
        /// </summary>
        /// <param name="declaringType">Type that declares the static method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Exact method parameter types.</param>
        /// <param name="arguments">Argument expressions passed to the method.</param>
        /// <returns>A value expression representing the method call result.</returns>
        IBeeValueExpression StaticCall(
            Type declaringType,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            params IBeeValueExpression[] arguments);

        /// <summary>
        /// Emits a value expression for side effects and discards any returned value.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder Evaluate(IBeeValueExpression expression);

        /// <summary>
        /// Creates an instance property access expression.
        /// </summary>
        /// <param name="instance">Instance expression.</param>
        /// <param name="name">Property name.</param>
        /// <returns>An assignable property expression.</returns>
        IBeeAssignableExpression Property(IBeeValueExpression instance, string name);

        /// <summary>
        /// Creates a static property access expression.
        /// </summary>
        /// <param name="declaringType">Type that declares the static property.</param>
        /// <param name="name">Property name.</param>
        /// <returns>An assignable static property expression.</returns>
        IBeeAssignableExpression StaticProperty(Type declaringType, string name);

        /// <summary>
        /// Creates an instance field access expression.
        /// </summary>
        /// <param name="instance">Instance expression.</param>
        /// <param name="name">Field name.</param>
        /// <returns>An assignable field expression.</returns>
        IBeeAssignableExpression Field(IBeeValueExpression instance, string name);

        /// <summary>
        /// Creates a static field access expression.
        /// </summary>
        /// <param name="declaringType">Type that declares the static field.</param>
        /// <param name="name">Field name.</param>
        /// <returns>An assignable static field expression.</returns>
        IBeeAssignableExpression StaticField(Type declaringType, string name);

        /// <summary>
        /// Creates a constant expression.
        /// </summary>
        /// <param name="value">Constant value.</param>
        /// <returns>A constant expression.</returns>
        IBeeValueExpression Constant(object value);

        /// <summary>
        /// Creates a constant expression with an explicit type.
        /// </summary>
        /// <param name="value">Constant value.</param>
        /// <param name="type">Constant type.</param>
        /// <returns>A constant expression.</returns>
        IBeeValueExpression Constant(object value, Type type);

        /// <summary>
        /// Creates a typed constant expression.
        /// </summary>
        /// <typeparam name="T">Constant type.</typeparam>
        /// <param name="value">Constant value.</param>
        /// <returns>A constant expression.</returns>
        IBeeValueExpression Constant<T>(T value);

        /// <summary>
        /// Creates a default value expression.
        /// </summary>
        /// <param name="type">Default value type.</param>
        /// <returns>A default value expression.</returns>
        IBeeValueExpression Default(Type type);

        /// <summary>
        /// Creates a typed default value expression.
        /// </summary>
        /// <typeparam name="T">Default value type.</typeparam>
        /// <returns>A default value expression.</returns>
        IBeeValueExpression Default<T>();

        /// <summary>
        /// Creates a conversion expression.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="type">Target type.</param>
        /// <returns>A conversion expression.</returns>
        IBeeValueExpression Convert(IBeeValueExpression value, Type type);

        /// <summary>
        /// Creates a typed conversion expression.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <returns>A conversion expression.</returns>
        IBeeValueExpression Convert<T>(IBeeValueExpression value);

        /// <summary>
        /// Creates an equality comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression Equal(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates an inequality comparison expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression NotEqual(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a null check expression.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression IsNull(IBeeValueExpression value);

        /// <summary>
        /// Creates an addition expression. String operands are concatenated.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>An addition expression.</returns>
        IBeeValueExpression Add(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a subtraction expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A subtraction expression.</returns>
        IBeeValueExpression Subtract(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a multiplication expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A multiplication expression.</returns>
        IBeeValueExpression Multiply(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a division expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A division expression.</returns>
        IBeeValueExpression Divide(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a modulo expression.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>A modulo expression.</returns>
        IBeeValueExpression Modulo(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a short-circuiting boolean AND expression.
        /// </summary>
        /// <param name="left">Left boolean value.</param>
        /// <param name="right">Right boolean value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression AndAlso(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a short-circuiting boolean OR expression.
        /// </summary>
        /// <param name="left">Left boolean value.</param>
        /// <param name="right">Right boolean value.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression OrElse(IBeeValueExpression left, IBeeValueExpression right);

        /// <summary>
        /// Creates a boolean negation expression.
        /// </summary>
        /// <param name="value">Boolean value to negate.</param>
        /// <returns>A boolean expression.</returns>
        IBeeValueExpression Not(IBeeValueExpression value);

        /// <summary>
        /// Creates a null-coalescing expression.
        /// </summary>
        /// <param name="value">Nullable or reference value.</param>
        /// <param name="fallback">Fallback value used when <paramref name="value"/> is null.</param>
        /// <returns>A value expression.</returns>
        IBeeValueExpression Coalesce(IBeeValueExpression value, IBeeValueExpression fallback);

        /// <summary>
        /// Creates a string concatenation expression.
        /// </summary>
        /// <param name="values">Values to concatenate.</param>
        /// <returns>A string expression.</returns>
        IBeeValueExpression Concat(params IBeeValueExpression[] values);

        /// <summary>
        /// Creates a conditional value expression.
        /// </summary>
        /// <param name="condition">Boolean condition.</param>
        /// <param name="whenTrue">Value loaded when the condition is true.</param>
        /// <param name="whenFalse">Value loaded when the condition is false.</param>
        /// <returns>A conditional value expression.</returns>
        IBeeValueExpression If(IBeeValueExpression condition, IBeeValueExpression whenTrue, IBeeValueExpression whenFalse);

        /// <summary>
        /// Assigns a value to an assignable target.
        /// </summary>
        /// <param name="target">Assignment target.</param>
        /// <param name="value">Value to assign.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder Assign(IBeeAssignableExpression target, IBeeValueExpression value);

        /// <summary>
        /// Emits a conditional statement.
        /// </summary>
        /// <param name="condition">Boolean condition.</param>
        /// <param name="whenTrue">Body emitted when the condition is true.</param>
        /// <param name="whenFalse">Optional body emitted when the condition is false.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder If(IBeeValueExpression condition, Action<IBeeMethodBodyBuilder> whenTrue, Action<IBeeMethodBodyBuilder> whenFalse = null);

        /// <summary>
        /// Returns from a void method.
        /// </summary>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder Return();

        /// <summary>
        /// Returns a value from the method.
        /// </summary>
        /// <param name="value">Return value.</param>
        /// <returns>The same method body builder.</returns>
        IBeeMethodBodyBuilder Return(IBeeValueExpression value);
    }
}
