namespace DynaBee.Infrastructure.Configurators
{
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class ExpressionIlEmitter
    {
        public static void Emit(
            LambdaExpression expression,
            ILGenerator il,
            IReadOnlyList<Type> parameterTypes,
            Type returnType,
            bool isStatic)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (il == null)
                throw new ArgumentNullException(nameof(il));

            var parameterBindings = BuildParameterBindings(expression, parameterTypes.Count, isStatic);
            var context = new EmitContext(il, expression.Parameters, parameterTypes, parameterBindings);
            EmitExpression(expression.Body, context);

            if (returnType == typeof(void))
            {
                if (expression.Body.Type != typeof(void))
                    il.Emit(OpCodes.Pop);

                il.Emit(OpCodes.Ret);
                return;
            }

            if (!returnType.IsAssignableFrom(expression.Body.Type))
                throw new InvalidOperationException(
                    $"Expression return type '{expression.Body.Type}' cannot be assigned to method return type '{returnType}'.");

            il.Emit(OpCodes.Ret);
        }

        private static void EmitExpression(Expression expression, EmitContext context)
        {
            switch (expression)
            {
                case ConstantExpression constant:
                    EmitConstant(constant, context.Il);
                    return;
                case ParameterExpression parameter:
                    EmitParameter(parameter, context);
                    return;
                case UnaryExpression unary when unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked:
                    EmitExpression(unary.Operand, context);
                    EmitConvert(unary.Operand.Type, unary.Type, context.Il);
                    return;
                case BinaryExpression binary:
                    EmitBinary(binary, context);
                    return;
                case MethodCallExpression call:
                    EmitMethodCall(call, context);
                    return;
                case MemberExpression member:
                    EmitMemberAccess(member, context);
                    return;
                case NewExpression @new:
                    EmitNew(@new, context);
                    return;
                case ConditionalExpression conditional:
                    EmitConditional(conditional, context);
                    return;
                default:
                    throw new NotSupportedException($"Expression node '{expression.NodeType}' is not supported yet.");
            }
        }

        private static void EmitParameter(ParameterExpression expression, EmitContext context)
        {
            var parameterIndex = -1;
            for (var i = 0; i < context.Parameters.Count; i++)
            {
                if (ReferenceEquals(context.Parameters[i], expression))
                {
                    parameterIndex = i;
                    break;
                }
            }

            if (parameterIndex < 0)
                throw new InvalidOperationException($"Parameter '{expression.Name}' is not part of the method signature.");

            context.Il.Emit(OpCodes.Ldarg, context.ParameterBindings[parameterIndex]);
        }

        private static void EmitBinary(BinaryExpression expression, EmitContext context)
        {
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                EmitAndAlso(expression, context);
                return;
            }

            if (expression.NodeType == ExpressionType.OrElse)
            {
                EmitOrElse(expression, context);
                return;
            }

            EmitExpression(expression.Left, context);
            EmitExpression(expression.Right, context);

            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    context.Il.Emit(OpCodes.Add);
                    break;
                case ExpressionType.Subtract:
                    context.Il.Emit(OpCodes.Sub);
                    break;
                case ExpressionType.Multiply:
                    context.Il.Emit(OpCodes.Mul);
                    break;
                case ExpressionType.Divide:
                    context.Il.Emit(OpCodes.Div);
                    break;
                case ExpressionType.Modulo:
                    context.Il.Emit(OpCodes.Rem);
                    break;
                case ExpressionType.Equal:
                    context.Il.Emit(OpCodes.Ceq);
                    break;
                case ExpressionType.NotEqual:
                    context.Il.Emit(OpCodes.Ceq);
                    context.Il.Emit(OpCodes.Ldc_I4_0);
                    context.Il.Emit(OpCodes.Ceq);
                    break;
                case ExpressionType.GreaterThan:
                    context.Il.Emit(OpCodes.Cgt);
                    break;
                case ExpressionType.LessThan:
                    context.Il.Emit(OpCodes.Clt);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    context.Il.Emit(OpCodes.Clt);
                    context.Il.Emit(OpCodes.Ldc_I4_0);
                    context.Il.Emit(OpCodes.Ceq);
                    break;
                case ExpressionType.LessThanOrEqual:
                    context.Il.Emit(OpCodes.Cgt);
                    context.Il.Emit(OpCodes.Ldc_I4_0);
                    context.Il.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new NotSupportedException($"Binary node '{expression.NodeType}' is not supported yet.");
            }
        }

        private static void EmitAndAlso(BinaryExpression expression, EmitContext context)
        {
            var falseLabel = context.Il.DefineLabel();
            var endLabel = context.Il.DefineLabel();

            EmitExpression(expression.Left, context);
            context.Il.Emit(OpCodes.Brfalse, falseLabel);
            EmitExpression(expression.Right, context);
            context.Il.Emit(OpCodes.Br, endLabel);
            context.Il.MarkLabel(falseLabel);
            context.Il.Emit(OpCodes.Ldc_I4_0);
            context.Il.MarkLabel(endLabel);
        }

        private static void EmitOrElse(BinaryExpression expression, EmitContext context)
        {
            var trueLabel = context.Il.DefineLabel();
            var endLabel = context.Il.DefineLabel();

            EmitExpression(expression.Left, context);
            context.Il.Emit(OpCodes.Brtrue, trueLabel);
            EmitExpression(expression.Right, context);
            context.Il.Emit(OpCodes.Br, endLabel);
            context.Il.MarkLabel(trueLabel);
            context.Il.Emit(OpCodes.Ldc_I4_1);
            context.Il.MarkLabel(endLabel);
        }

        private static void EmitMethodCall(MethodCallExpression expression, EmitContext context)
        {
            if (expression.Object != null)
                EmitExpression(expression.Object, context);

            foreach (var argument in expression.Arguments)
                EmitExpression(argument, context);

            var opcode = expression.Method.IsVirtual && !expression.Method.IsFinal && !expression.Method.DeclaringType!.IsValueType
                ? OpCodes.Callvirt
                : OpCodes.Call;

            context.Il.Emit(opcode, expression.Method);
        }

        private static void EmitMemberAccess(MemberExpression expression, EmitContext context)
        {
            if (expression.Member is PropertyInfo property)
            {
                if (expression.Expression != null)
                    EmitExpression(expression.Expression, context);

                var getter = property.GetGetMethod(true) ?? throw new InvalidOperationException($"Property '{property.Name}' has no getter.");
                var opcode = getter.IsVirtual && !getter.IsFinal && !getter.DeclaringType!.IsValueType ? OpCodes.Callvirt : OpCodes.Call;
                context.Il.Emit(opcode, getter);
                return;
            }

            if (expression.Member is FieldInfo field)
            {
                if (expression.Expression != null)
                {
                    EmitExpression(expression.Expression, context);
                    context.Il.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    context.Il.Emit(OpCodes.Ldsfld, field);
                }

                return;
            }

            throw new NotSupportedException($"Member '{expression.Member.Name}' is not supported.");
        }

        private static void EmitNew(NewExpression expression, EmitContext context)
        {
            foreach (var argument in expression.Arguments)
                EmitExpression(argument, context);

            context.Il.Emit(OpCodes.Newobj, expression.Constructor!);
        }

        private static void EmitConditional(ConditionalExpression expression, EmitContext context)
        {
            var falseLabel = context.Il.DefineLabel();
            var endLabel = context.Il.DefineLabel();

            EmitExpression(expression.Test, context);
            context.Il.Emit(OpCodes.Brfalse, falseLabel);
            EmitExpression(expression.IfTrue, context);
            context.Il.Emit(OpCodes.Br, endLabel);
            context.Il.MarkLabel(falseLabel);
            EmitExpression(expression.IfFalse, context);
            context.Il.MarkLabel(endLabel);
        }

        private static void EmitConvert(Type fromType, Type toType, ILGenerator il)
        {
            if (toType == fromType)
                return;

            if (toType == typeof(object))
            {
                if (fromType.IsValueType)
                    il.Emit(OpCodes.Box, fromType);

                return;
            }

            if (fromType == typeof(object) && toType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, toType);
                return;
            }

            if (!toType.IsValueType)
            {
                il.Emit(OpCodes.Castclass, toType);
                return;
            }

            if (toType == typeof(int)) il.Emit(OpCodes.Conv_I4);
            else if (toType == typeof(long)) il.Emit(OpCodes.Conv_I8);
            else if (toType == typeof(float)) il.Emit(OpCodes.Conv_R4);
            else if (toType == typeof(double)) il.Emit(OpCodes.Conv_R8);
            else if (toType == typeof(short)) il.Emit(OpCodes.Conv_I2);
            else if (toType == typeof(byte)) il.Emit(OpCodes.Conv_U1);
            else if (toType == typeof(bool))
            {
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
            }
            else
            {
                throw new NotSupportedException($"Conversion from '{fromType}' to '{toType}' is not supported.");
            }
        }

        private static void EmitConstant(ConstantExpression expression, ILGenerator il)
        {
            if (expression.Value == null)
            {
                il.Emit(OpCodes.Ldnull);
                return;
            }

            var valueType = expression.Value.GetType();
            if (valueType == typeof(int))
            {
                il.Emit(OpCodes.Ldc_I4, (int)expression.Value);
                return;
            }

            if (valueType == typeof(string))
            {
                il.Emit(OpCodes.Ldstr, (string)expression.Value);
                return;
            }

            if (valueType == typeof(bool))
            {
                il.Emit((bool)expression.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                return;
            }

            if (valueType == typeof(long))
            {
                il.Emit(OpCodes.Ldc_I8, (long)expression.Value);
                return;
            }

            if (valueType == typeof(float))
            {
                il.Emit(OpCodes.Ldc_R4, (float)expression.Value);
                return;
            }

            if (valueType == typeof(double))
            {
                il.Emit(OpCodes.Ldc_R8, (double)expression.Value);
                return;
            }

            throw new NotSupportedException($"Constant of type '{valueType}' is not supported.");
        }

        private sealed class EmitContext
        {
            public EmitContext(
                ILGenerator il,
                IReadOnlyList<ParameterExpression> parameters,
                IReadOnlyList<Type> parameterTypes,
                IReadOnlyList<int> parameterBindings)
            {
                Il = il;
                Parameters = parameters;
                ParameterTypes = parameterTypes;
                ParameterBindings = parameterBindings;
            }

            public ILGenerator Il { get; }

            public IReadOnlyList<ParameterExpression> Parameters { get; }

            public IReadOnlyList<Type> ParameterTypes { get; }

            public IReadOnlyList<int> ParameterBindings { get; }
        }

        private static IReadOnlyList<int> BuildParameterBindings(
            LambdaExpression expression,
            int methodParameterCount,
            bool isStatic)
        {
            var expressionParameterCount = expression.Parameters.Count;

            if (isStatic)
            {
                if (expressionParameterCount != methodParameterCount)
                    throw new InvalidOperationException(
                        "For static methods, expression parameter count must match method parameter count.");

                var staticBindings = new List<int>(expressionParameterCount);
                for (var i = 0; i < expressionParameterCount; i++)
                    staticBindings.Add(i);

                return staticBindings;
            }

            if (expressionParameterCount == methodParameterCount)
            {
                var instanceBindings = new List<int>(expressionParameterCount);
                for (var i = 0; i < expressionParameterCount; i++)
                    instanceBindings.Add(i + 1);

                return instanceBindings;
            }

            if (expressionParameterCount == methodParameterCount + 1)
            {
                var withSelfBindings = new List<int>(expressionParameterCount) { 0 };
                for (var i = 1; i < expressionParameterCount; i++)
                    withSelfBindings.Add(i);

                return withSelfBindings;
            }

            throw new InvalidOperationException(
                "For instance methods, expression must use either (args...) or (self, args...) parameter shape.");
        }
    }
}
