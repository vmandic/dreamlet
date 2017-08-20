/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace FastExpressionCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>Compiles expression to delegate by emitting the IL directly.
    /// The emitter is ~20 times faster than Expression.Compile.</summary>
    public static partial class ExpressionCompiler
    {
        /// <summary>First tries to compile fast and if failed (null result), then falls back to Expression.Compile.</summary>
        /// <typeparam name="T">Type of compiled delegate return result.</typeparam>
        /// <param name="lambdaExpr">Expr to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static Func<T> Compile<T>(Expression<Func<T>> lambdaExpr)
        {
            return TryCompile<Func<T>>(lambdaExpr.Body, lambdaExpr.Parameters, Empty<Type>(), typeof(T))
                ?? lambdaExpr.Compile();
        }

        /// <summary>Compiles lambda expression to <typeparamref name="TDelegate"/>.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate Compile<TDelegate>(LambdaExpression lambdaExpr)
            where TDelegate : class
        {
            return TryCompile<TDelegate>(lambdaExpr) ?? (TDelegate)(object)lambdaExpr.Compile();
        }

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/>.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate TryCompile<TDelegate>(LambdaExpression lambdaExpr)
            where TDelegate : class
        {
            var paramExprs = lambdaExpr.Parameters;
            var paramTypes = GetParamExprTypes(paramExprs);
            var expr = lambdaExpr.Body;
            return TryCompile<TDelegate>(expr, paramExprs, paramTypes, expr.Type);
        }

        /// <summary>Performant method to get parameter types from parameter expressions.</summary>
        public static Type[] GetParamExprTypes(IList<ParameterExpression> paramExprs)
        {
            var paramsCount = paramExprs.Count;
            if (paramsCount == 0)
                return Empty<Type>();

            if (paramsCount == 1)
                return new[] { paramExprs[0].Type };

            var paramTypes = new Type[paramsCount];
            for (var i = 0; i < paramTypes.Length; i++)
                paramTypes[i] = paramExprs[i].Type;
            return paramTypes;
        }

        /// <summary>Compiles expression to delegate by emitting the IL. 
        /// If sub-expressions are not supported by emitter, then the method returns null.
        /// The usage should be calling the method, if result is null then calling the Expression.Compile.</summary>
        /// <param name="bodyExpr">Lambda body.</param>
        /// <param name="paramExprs">Lambda parameter expressions.</param>
        /// <param name="paramTypes">The types of parameters.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>Result delegate or null, if unable to compile.</returns>
        public static TDelegate TryCompile<TDelegate>(
            Expression bodyExpr,
            IList<ParameterExpression> paramExprs,
            Type[] paramTypes,
            Type returnType) where TDelegate : class
        {
            ClosureInfo ignored = null;
            return (TDelegate)TryCompile(ref ignored,
                typeof(TDelegate), paramTypes, returnType, bodyExpr, paramExprs);
        }

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/>.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate TryCompile<TDelegate>(LambdaExpressionInfo lambdaExpr)
            where TDelegate : class
        {
            var paramExprs = lambdaExpr.Parameters;
            var paramTypes = GetParamExprTypes(paramExprs);
            var expr = lambdaExpr.Body;
            return TryCompile<TDelegate>(expr, paramExprs, paramTypes, expr.Type);
        }

        /// <summary>Compiles expression to delegate by emitting the IL. 
        /// If sub-expressions are not supported by emitter, then the method returns null.
        /// The usage should be calling the method, if result is null then calling the Expression.Compile.</summary>
        /// <param name="bodyExpr">Lambda body.</param>
        /// <param name="paramExprs">Lambda parameter expressions.</param>
        /// <param name="paramTypes">The types of parameters.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>Result delegate or null, if unable to compile.</returns>
        public static TDelegate TryCompile<TDelegate>(
            ExpressionInfo bodyExpr,
            IList<ParameterExpression> paramExprs,
            Type[] paramTypes,
            Type returnType) where TDelegate : class
        {
            ClosureInfo ignored = null;
            return (TDelegate)TryCompile(ref ignored,
                typeof(TDelegate), paramTypes, returnType, bodyExpr, paramExprs);
        }

        private struct Expr
        {
            public static implicit operator Expr(Expression expr)
            {
                return expr == null ? default(Expr) : new Expr(expr, expr.NodeType, expr.Type);
            }

            public static implicit operator Expr(ExpressionInfo expr)
            {
                return expr == null ? default(Expr) : new Expr(expr, expr.NodeType, expr.Type);
            }

            public object Expression;
            public ExpressionType NodeType;
            public Type Type;

            private Expr(object expression, ExpressionType nodeType, Type type)
            {
                Expression = expression;
                NodeType = nodeType;
                Type = type;
            }
        }

        private static object TryCompile(ref ClosureInfo closureInfo,
            Type delegateType, Type[] paramTypes, Type returnType,
            Expr bodyExpr, IList<ParameterExpression> paramExprs,
            bool isNestedLambda = false)
        {
            if (!TryCollectBoundConstants(ref closureInfo, bodyExpr, paramExprs))
                return null;

            if (closureInfo == null)
            {
                var method = new DynamicMethod(string.Empty, returnType, paramTypes,
                    typeof(ExpressionCompiler), skipVisibility: true);
                if (!TryEmit(method, bodyExpr, paramExprs, closureInfo))
                    return null;
                return method.CreateDelegate(delegateType);
            }

            var closureObject = closureInfo.ConstructClosure(closureTypeOnly: isNestedLambda);
            var closureAndParamTypes = GetClosureAndParamTypes(paramTypes, closureInfo.ClosureType);

            var methodWithClosure = new DynamicMethod(string.Empty, returnType, closureAndParamTypes,
                typeof(ExpressionCompiler), skipVisibility: true);

            if (!TryEmit(methodWithClosure, bodyExpr, paramExprs, closureInfo))
                return null;

            if (isNestedLambda) // include closure as the first parameter, BUT don't bound to it. It will be bound later in EmitNestedLambda.
                return methodWithClosure.CreateDelegate(GetFuncOrActionType(closureAndParamTypes, returnType));

            return methodWithClosure.CreateDelegate(delegateType, closureObject);
        }

        private static bool TryEmit(DynamicMethod method,
            Expr bodyExpr, IList<ParameterExpression> paramExprs,
            ClosureInfo closureInfo)
        {
            var il = method.GetILGenerator();
            if (!EmittingVisitor.TryEmit(bodyExpr, paramExprs, il, closureInfo))
                return false;

            il.Emit(OpCodes.Ret); // emits return from generated method
            return true;
        }

        private static Type[] GetClosureAndParamTypes(Type[] paramTypes, Type closureType)
        {
            var paramCount = paramTypes.Length;
            if (paramCount == 0)
                return new[] { closureType };

            if (paramCount == 1)
                return new[] { closureType, paramTypes[0] };

            var closureAndParamTypes = new Type[paramCount + 1];
            closureAndParamTypes[0] = closureType;
            Array.Copy(paramTypes, 0, closureAndParamTypes, 1, paramCount);
            return closureAndParamTypes;
        }

        private struct ConstantInfo
        {
            public object ConstantExpr;
            public Type Type;
            public object Value;
            public ConstantInfo(object constantExpr, object value, Type type)
            {
                ConstantExpr = constantExpr;
                Value = value;
                Type = type;
            }
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }

        private static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        private static T[] Append<T>(this T[] source, T value)
        {
            if (source == null || source.Length == 0)
                return new[] { value };
            if (source.Length == 1)
                return new[] { source[0], value };
            if (source.Length == 2)
                return new[] { source[0], source[1], value };
            var sourceLength = source.Length;
            var result = new T[sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[sourceLength] = value;
            return result;
        }

        private static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null || source.Length == 0)
                return -1;
            if (source.Length == 1)
                return predicate(source[0]) ? 0 : -1;
            for (var i = 0; i < source.Length; ++i)
                if (predicate(source[i]))
                    return i;
            return -1;
        }

        private sealed class ClosureInfo
        {
            // Closed values used by expression and by its nested lambdas
            public ConstantInfo[] Constants = Empty<ConstantInfo>();

            // Parameters not passed through lambda parameter list But used inside lambda body.
            // The top expression should not! contain non passed parameters. 
            public ParameterExpression[] NonPassedParameters = Empty<ParameterExpression>();

            // All nested lambdas recursively nested in expression
            public NestedLambdaInfo[] NestedLambdas = Empty<NestedLambdaInfo>();

            // Field infos are needed to load field of closure object on stack in emitter
            // It is also an indicator that we use typed Closure object and not an array
            public FieldInfo[] Fields { get; private set; }

            // Type of constructed closure, is known after ConstructClosure call
            public Type ClosureType { get; private set; }

            // Known after ConstructClosure call
            public int ClosedItemCount { get; private set; }

            public void AddConstant(object expr, object value, Type type)
            {
                if (Constants.Length == 0 ||
                    Constants.IndexOf(it => it.ConstantExpr == expr) == -1)
                    Constants = Constants.Append(new ConstantInfo(expr, value, type));
            }

            public void AddConstant(ConstantInfo info)
            {
                if (Constants.Length == 0 ||
                    Constants.IndexOf(it => it.ConstantExpr == info.ConstantExpr) == -1)
                    Constants = Constants.Append(info);
            }

            public void AddNonPassedParam(ParameterExpression expr)
            {
                if (NonPassedParameters.Length == 0 ||
                    NonPassedParameters.IndexOf(it => it == expr) == -1)
                    NonPassedParameters = NonPassedParameters.Append(expr);
            }

            public void AddNestedLambda(object lambdaExpr, object lambda, ClosureInfo closureInfo, bool isAction)
            {
                if (NestedLambdas.Length == 0 ||
                    NestedLambdas.IndexOf(it => it.LambdaExpr == lambdaExpr) == -1)
                    NestedLambdas = NestedLambdas.Append(new NestedLambdaInfo(closureInfo, lambdaExpr, lambda, isAction));
            }

            public void AddNestedLambda(NestedLambdaInfo info)
            {
                if (NestedLambdas.Length == 0 ||
                    NestedLambdas.IndexOf(it => it.LambdaExpr == info.LambdaExpr) == -1)
                    NestedLambdas = NestedLambdas.Append(info);
            }

            public object ConstructClosure(bool closureTypeOnly)
            {
                var constants = Constants;
                var nonPassedParams = NonPassedParameters;
                var nestedLambdas = NestedLambdas;

                var constPlusParamCount = constants.Length + nonPassedParams.Length;
                var totalItemCount = constPlusParamCount + nestedLambdas.Length;

                ClosedItemCount = totalItemCount;

                var closureCreateMethods = Closure.CreateMethods;

                // Construct the array based closure when number of values is bigger than
                // number of fields in biggest supported Closure class.
                if (totalItemCount > closureCreateMethods.Length)
                {
                    ClosureType = typeof(ArrayClosure);

                    if (closureTypeOnly)
                        return null;

                    var items = new object[totalItemCount];
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            items[i] = constants[i].Value;

                    // skip non passed parameters as it is only for nested lambdas

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            items[constPlusParamCount + i] = nestedLambdas[i].Lambda;

                    return new ArrayClosure(items);
                }

                // Construct the Closure Type and optionally Closure object with closed values stored as fields:
                object[] fieldValues = null;
                var fieldTypes = new Type[totalItemCount];
                if (closureTypeOnly)
                {
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            fieldTypes[i] = constants[i].Type;

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            fieldTypes[constPlusParamCount + i] = nestedLambdas[i].Lambda.GetType();
                }
                else
                {
                    fieldValues = new object[totalItemCount];

                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                        {
                            var constantExpr = constants[i];
                            fieldTypes[i] = constantExpr.Type;
                            fieldValues[i] = constantExpr.Value;
                        }

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                        {
                            var lambda = nestedLambdas[i].Lambda;
                            fieldValues[constPlusParamCount + i] = lambda;
                            fieldTypes[constPlusParamCount + i] = lambda.GetType();
                        }
                }

                var createClosureMethod = closureCreateMethods[totalItemCount - 1];
                var createClosure = createClosureMethod.MakeGenericMethod(fieldTypes);
                ClosureType = createClosure.ReturnType;

                var fields = ClosureType.GetTypeInfo().DeclaredFields;
                Fields = fields as FieldInfo[] ?? fields.ToArray();

                if (fieldValues == null)
                    return null;
                return createClosure.Invoke(null, fieldValues);
            }
        }

        #region Closures

        internal static class Closure
        {
            private static readonly IEnumerable<MethodInfo> _methods =
                typeof(Closure).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] CreateMethods =
                _methods as MethodInfo[] ?? _methods.ToArray();

            public static Closure<T1> CreateClosure<T1>(T1 v1)
            {
                return new Closure<T1>(v1);
            }

            public static Closure<T1, T2> CreateClosure<T1, T2>(T1 v1, T2 v2)
            {
                return new Closure<T1, T2>(v1, v2);
            }

            public static Closure<T1, T2, T3> CreateClosure<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
            {
                return new Closure<T1, T2, T3>(v1, v2, v3);
            }

            public static Closure<T1, T2, T3, T4> CreateClosure<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                return new Closure<T1, T2, T3, T4>(v1, v2, v3, v4);
            }

            public static Closure<T1, T2, T3, T4, T5> CreateClosure<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4,
                T5 v5)
            {
                return new Closure<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5);
            }

            public static Closure<T1, T2, T3, T4, T5, T6> CreateClosure<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3,
                T4 v4, T5 v5, T6 v6)
            {
                return new Closure<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7> CreateClosure<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2,
                T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(v1, v2, v3, v4, v5, v6, v7, v8, v9);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
            }
        }

        internal sealed class Closure<T1>
        {
            public T1 V1;

            public Closure(T1 v1)
            {
                V1 = v1;
            }
        }

        internal sealed class Closure<T1, T2>
        {
            public T1 V1;
            public T2 V2;

            public Closure(T1 v1, T2 v2)
            {
                V1 = v1;
                V2 = v2;
            }
        }

        internal sealed class Closure<T1, T2, T3>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;

            public Closure(T1 v1, T2 v2, T3 v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5, T6>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
            }
        }

        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;
            public T10 V10;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
                V10 = v10;
            }
        }

        internal sealed class ArrayClosure
        {
            public readonly object[] Constants;

            public static FieldInfo ArrayField = typeof(ArrayClosure).GetTypeInfo().DeclaredFields.First(f => !f.IsStatic);
            public static ConstructorInfo Constructor = typeof(ArrayClosure).GetTypeInfo().DeclaredConstructors.First();

            public ArrayClosure(object[] constants)
            {
                Constants = constants;
            }
        }

        #endregion

        #region Nested Lambdas

        private struct NestedLambdaInfo
        {
            public ClosureInfo ClosureInfo;

            public object LambdaExpr; // to find the lambda in bigger parent expression
            public object Lambda;
            public bool IsAction;

            public NestedLambdaInfo(ClosureInfo closureInfo, object lambdaExpr, object lambda, bool isAction)
            {
                ClosureInfo = closureInfo;
                Lambda = lambda;
                LambdaExpr = lambdaExpr;
                IsAction = isAction;
            }
        }

        internal static class CurryClosureFuncs
        {
            private static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureFuncs).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

            public static Func<R> Curry<C, R>(Func<C, R> f, C c) { return () => f(c); }
            public static Func<T1, R> Curry<C, T1, R>(Func<C, T1, R> f, C c) { return t1 => f(c, t1); }
            public static Func<T1, T2, R> Curry<C, T1, T2, R>(Func<C, T1, T2, R> f, C c) { return (t1, t2) => f(c, t1, t2); }
            public static Func<T1, T2, T3, R> Curry<C, T1, T2, T3, R>(Func<C, T1, T2, T3, R> f, C c) { return (t1, t2, t3) => f(c, t1, t2, t3); }
            public static Func<T1, T2, T3, T4, R> Curry<C, T1, T2, T3, T4, R>(Func<C, T1, T2, T3, T4, R> f, C c) { return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4); }
            public static Func<T1, T2, T3, T4, T5, R> Curry<C, T1, T2, T3, T4, T5, R>(Func<C, T1, T2, T3, T4, T5, R> f, C c) { return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5); }
            public static Func<T1, T2, T3, T4, T5, T6, R> Curry<C, T1, T2, T3, T4, T5, T6, R>(Func<C, T1, T2, T3, T4, T5, T6, R> f, C c) { return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6); }
        }

        internal static class CurryClosureActions
        {
            private static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureActions).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

            internal static Action Curry<C>(Action<C> a, C c) { return () => a(c); }
            internal static Action<T1> Curry<C, T1>(Action<C, T1> f, C c) { return t1 => f(c, t1); }
            internal static Action<T1, T2> Curry<C, T1, T2>(Action<C, T1, T2> f, C c) { return (t1, t2) => f(c, t1, t2); }
            internal static Action<T1, T2, T3> Curry<C, T1, T2, T3>(Action<C, T1, T2, T3> f, C c) { return (t1, t2, t3) => f(c, t1, t2, t3); }
            internal static Action<T1, T2, T3, T4> Curry<C, T1, T2, T3, T4>(Action<C, T1, T2, T3, T4> f, C c) { return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4); }
            internal static Action<T1, T2, T3, T4, T5> Curry<C, T1, T2, T3, T4, T5>(Action<C, T1, T2, T3, T4, T5> f, C c) { return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5); }
            internal static Action<T1, T2, T3, T4, T5, T6> Curry<C, T1, T2, T3, T4, T5, T6>(Action<C, T1, T2, T3, T4, T5, T6> f, C c) { return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6); }
        }

        #endregion

        #region Collect Bound Constants

        private static bool IsBoundConstant(object value)
        {
            if (value == null)
                return false;

            var typeInfo = value.GetType().GetTypeInfo();
            return !typeInfo.IsPrimitive
                   && !(value is string)
                   && !(value is Type)
                   && !typeInfo.IsEnum;
        }

        // @paramExprs is required for nested lambda compilation
        private static bool TryCollectBoundConstants(
            ref ClosureInfo closure, Expr e, IList<ParameterExpression> paramExprs)
        {
            var expr = e.Expression;
            if (expr == null)
                return false;

            switch (e.NodeType)
            {
                case ExpressionType.Constant:
                    var constExprInfo = expr as ConstantExpressionInfo;
                    var value = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression)expr).Value;
                    if (value is Delegate || IsBoundConstant(value))
                        (closure ?? (closure = new ClosureInfo())).AddConstant(expr, value, e.Type);
                    break;

                case ExpressionType.Parameter:
                    // if parameter is used But no passed (not in parameter expressions)
                    // it means parameter is provided by outer lambda and should be put in closure for current lambda
                    var exprInfo = expr as ParameterExpressionInfo;
                    var paramExpr = exprInfo ?? (ParameterExpression)expr;
                    if (paramExprs.IndexOf(paramExpr) == -1)
                        (closure ?? (closure = new ClosureInfo())).AddNonPassedParam(paramExpr);
                    break;

                case ExpressionType.Call:
                    var callExprInfo = expr as MethodCallExpressionInfo;
                    if (callExprInfo != null)
                        return (callExprInfo.Object == null
                            || TryCollectBoundConstants(ref closure, callExprInfo.Object, paramExprs))
                            && TryCollectBoundConstants(ref closure, callExprInfo.Arguments, paramExprs);

                    var callExpr = (MethodCallExpression)expr;
                    return (callExpr.Object == null
                        || TryCollectBoundConstants(ref closure, callExpr.Object, paramExprs))
                        && TryCollectBoundConstants(ref closure, callExpr.Arguments, paramExprs);

                case ExpressionType.MemberAccess:
                    var memberExprInfo = expr as MemberExpressionInfo;
                    if (memberExprInfo != null)
                        return memberExprInfo.Expression == null
                            || TryCollectBoundConstants(ref closure, memberExprInfo.Expression, paramExprs);

                    var memberExpr = ((MemberExpression)expr).Expression;
                    return memberExpr == null
                        || TryCollectBoundConstants(ref closure, memberExpr, paramExprs);

                case ExpressionType.New:
                    var newExprInfo = expr as NewExpressionInfo;
                    return newExprInfo != null
                        ? TryCollectBoundConstants(ref closure, newExprInfo.Arguments, paramExprs)
                        : TryCollectBoundConstants(ref closure, ((NewExpression)expr).Arguments, paramExprs);

                case ExpressionType.NewArrayInit:
                    return TryCollectBoundConstants(ref closure, ((NewArrayExpression)expr).Expressions, paramExprs);

                // property initializer
                case ExpressionType.MemberInit:
                    var memberInitExpr = (MemberInitExpression)expr;
                    if (!TryCollectBoundConstants(ref closure, memberInitExpr.NewExpression, paramExprs))
                        return false;

                    var memberBindings = memberInitExpr.Bindings;
                    for (var i = 0; i < memberBindings.Count; ++i)
                    {
                        var memberBinding = memberBindings[i];
                        if (memberBinding.BindingType == MemberBindingType.Assignment &&
                            !TryCollectBoundConstants(ref closure, ((MemberAssignment)memberBinding).Expression, paramExprs))
                            return false;
                    }
                    break;

                // nested lambda expression
                case ExpressionType.Lambda:

                    // 1. Try to compile nested lambda in place
                    // 2. Check that parameters used in compiled lambda are passed or closed by outer lambda
                    // 3. Add the compiled lambda to closure of outer lambda for later invocation

                    object lambda;
                    Type lambdaReturnType;
                    ClosureInfo nestedClosure = null;

                    var lambdaExprInfo = expr as LambdaExpressionInfo;
                    if (lambdaExprInfo != null)
                    {
                        var lambdaParamExprs = lambdaExprInfo.Parameters;
                        lambdaReturnType = lambdaExprInfo.Body.Type;
                        lambda = TryCompile(ref nestedClosure,
                            lambdaExprInfo.Type, GetParamExprTypes(lambdaParamExprs), lambdaReturnType,
                            lambdaExprInfo.Body, lambdaParamExprs, isNestedLambda: true);
                    }
                    else
                    {
                        var lambdaExpr = (LambdaExpression)expr;
                        var lambdaParamExprs = lambdaExpr.Parameters;
                        lambdaReturnType = lambdaExpr.Body.Type;
                        lambda = TryCompile(ref nestedClosure,
                            lambdaExpr.Type, GetParamExprTypes(lambdaParamExprs), lambdaReturnType,
                            lambdaExpr.Body, lambdaParamExprs, isNestedLambda: true);
                    }

                    if (lambda == null)
                        return false;

                    // add the nested lambda into closure
                    (closure ?? (closure = new ClosureInfo()))
                        .AddNestedLambda(expr, lambda, nestedClosure, isAction: lambdaReturnType == typeof(void));

                    if (nestedClosure == null)
                        break;

                    // if nested non passed parameter is no matched with any outer passed parameter, 
                    // then ensure it goes to outer non passed parameter.
                    // But check that have non passed parameter in root expression is invalid.
                    var nestedNonPassedParams = nestedClosure.NonPassedParameters;
                    if (nestedNonPassedParams.Length != 0)
                        for (var i = 0; i < nestedNonPassedParams.Length; i++)
                        {
                            var nestedNonPassedParam = nestedNonPassedParams[i];
                            if (paramExprs.Count == 0 ||
                                paramExprs.IndexOf(nestedNonPassedParam) == -1)
                                closure.AddNonPassedParam(nestedNonPassedParam);
                        }

                    // Promote found constants and nested lambdas into outer closure
                    var nestedConstants = nestedClosure.Constants;
                    if (nestedConstants.Length != 0)
                        for (var i = 0; i < nestedConstants.Length; i++)
                            closure.AddConstant(nestedConstants[i]);

                    var nestedNestedLambdas = nestedClosure.NestedLambdas;
                    if (nestedNestedLambdas.Length != 0)
                        for (var i = 0; i < nestedNestedLambdas.Length; i++)
                            closure.AddNestedLambda(nestedNestedLambdas[i]);

                    break;

                case ExpressionType.Invoke:
                    var invocationExpr = (InvocationExpression)expr;
                    return TryCollectBoundConstants(ref closure, invocationExpr.Expression, paramExprs)
                           && TryCollectBoundConstants(ref closure, invocationExpr.Arguments, paramExprs);

                case ExpressionType.Conditional:
                    var conditionalExpr = (ConditionalExpression)expr;
                    return TryCollectBoundConstants(ref closure, conditionalExpr.Test, paramExprs)
                           && TryCollectBoundConstants(ref closure, conditionalExpr.IfTrue, paramExprs)
                           && TryCollectBoundConstants(ref closure, conditionalExpr.IfFalse, paramExprs);

                default:
                    var unaryExpr = expr as UnaryExpression;
                    if (unaryExpr != null)
                        return TryCollectBoundConstants(ref closure, unaryExpr.Operand, paramExprs);

                    var binaryExpr = expr as BinaryExpression;
                    if (binaryExpr != null)
                        return TryCollectBoundConstants(ref closure, binaryExpr.Left, paramExprs)
                            && TryCollectBoundConstants(ref closure, binaryExpr.Right, paramExprs);
                    break;
            }

            return true;
        }

        private static bool TryCollectBoundConstants(ref ClosureInfo closure, ExpressionInfo[] exprs, IList<ParameterExpression> paramExprs)
        {
            for (var i = 0; i < exprs.Length; i++)
                if (!TryCollectBoundConstants(ref closure, exprs[i], paramExprs))
                    return false;
            return true;
        }

        private static bool TryCollectBoundConstants(ref ClosureInfo closure, IList<Expression> exprs, IList<ParameterExpression> paramExprs)
        {
            for (var i = 0; i < exprs.Count; i++)
                if (!TryCollectBoundConstants(ref closure, exprs[i], paramExprs))
                    return false;
            return true;
        }

        /// <summary>Construct delegate type (Func or Action) from given input and return parameter types.</summary>
        public static Type GetFuncOrActionType(Type[] paramTypes, Type returnType)
        {
            if (returnType == typeof(void))
            {
                switch (paramTypes.Length)
                {
                    case 0: return typeof(Action);
                    case 1: return typeof(Action<>).MakeGenericType(paramTypes);
                    case 2: return typeof(Action<,>).MakeGenericType(paramTypes);
                    case 3: return typeof(Action<,,>).MakeGenericType(paramTypes);
                    case 4: return typeof(Action<,,,>).MakeGenericType(paramTypes);
                    case 5: return typeof(Action<,,,,>).MakeGenericType(paramTypes);
                    case 6: return typeof(Action<,,,,,>).MakeGenericType(paramTypes);
                    case 7: return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);
                    default:
                        throw new NotSupportedException(
                            string.Format("Action with so many ({0}) parameters is not supported!", paramTypes.Length));
                }
            }

            paramTypes = paramTypes.Append(returnType);
            switch (paramTypes.Length)
            {
                case 1: return typeof(Func<>).MakeGenericType(paramTypes);
                case 2: return typeof(Func<,>).MakeGenericType(paramTypes);
                case 3: return typeof(Func<,,>).MakeGenericType(paramTypes);
                case 4: return typeof(Func<,,,>).MakeGenericType(paramTypes);
                case 5: return typeof(Func<,,,,>).MakeGenericType(paramTypes);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(paramTypes);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(paramTypes);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(paramTypes);
                default:
                    throw new NotSupportedException(
                        string.Format("Func with so many ({0}) parameters is not supported!", paramTypes.Length));
            }
        }

        #endregion

        /// <summary>Supports emitting of selected expressions, e.g. lambdaExpr are not supported yet.
        /// When emitter find not supported expression it will return false from <see cref="TryEmit"/>, so I could fallback
        /// to normal and slow Expression.Compile.</summary>
        private static class EmittingVisitor
        {
            private static readonly MethodInfo _getTypeFromHandleMethod = typeof(Type).GetTypeInfo()
                .DeclaredMethods.First(m => m.Name == "GetTypeFromHandle");

            public static bool TryEmit(Expr e, IList<ParameterExpression> paramExprs,
                ILGenerator il, ClosureInfo closure)
            {
                var expr = e.Expression;
                var exprNodeType = e.NodeType;

                if ((int)exprNodeType == 46) // Support for ExpressionType.Assign in .NET < 4.0
                    return EmitAssignment(paramExprs, il, closure, expr);

                switch (exprNodeType)
                {
                    case ExpressionType.Parameter:
                        var pInfo = expr as ParameterExpressionInfo;
                        return EmitParameter(pInfo != null ? pInfo.ParamExpr : (ParameterExpression)expr, paramExprs, il, closure);
                    case ExpressionType.Convert:
                        return EmitConvert((UnaryExpression)expr, paramExprs, il, closure);
                    case ExpressionType.ArrayIndex:
                        return EmitArrayIndex((BinaryExpression)expr, paramExprs, il, closure);
                    case ExpressionType.Constant:
                        return EmitConstant(e, il, closure);
                    case ExpressionType.Call:
                        return EmitMethodCall(e, paramExprs, il, closure);
                    case ExpressionType.MemberAccess:
                        return EmitMemberAccess(e, paramExprs, il, closure);
                    case ExpressionType.New:
                        return EmitNew(e, paramExprs, il, closure);
                    case ExpressionType.NewArrayInit:
                        return EmitNewArray((NewArrayExpression)expr, paramExprs, il, closure);
                    case ExpressionType.MemberInit:
                        return EmitMemberInit((MemberInitExpression)expr, paramExprs, il, closure);
                    case ExpressionType.Lambda:
                        return EmitNestedLambda(expr, paramExprs, il, closure);

                    case ExpressionType.Invoke:
                        return EmitInvokeLambda((InvocationExpression)expr, paramExprs, il, closure);

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        return EmitComparison((BinaryExpression)expr, paramExprs, il, closure);

                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        return EmitLogicalOperator((BinaryExpression)expr, paramExprs, il, closure);

                    case ExpressionType.Conditional:
                        return EmitTernararyOperator((ConditionalExpression)expr, paramExprs, il, closure);

                    //case ExpressionType.Coalesce:
                    default:
                        return false;
                }
            }

            // todo: review implementation for robustness
            private static bool EmitAssignment(IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure, object expr)
            {
                var assignExpr = (BinaryExpression)expr;
                if (!TryEmit(assignExpr.Right, paramExprs, il, closure))
                    return false;

                var lValueExpr = assignExpr.Left;
                if (lValueExpr.NodeType == ExpressionType.MemberAccess)
                {
                    ; // todo: OpCodes.Stfld
                }
                else if (lValueExpr.NodeType == ExpressionType.Parameter)
                {
                    var paramExpr = (ParameterExpression)lValueExpr;
                    var paramIndex = paramExprs.IndexOf(paramExpr);
                    if (paramIndex != -1)
                    {
                        if (closure != null)
                            paramIndex += 1;
                        il.Emit(OpCodes.Starg, paramIndex);
                        LoadParamArg(il, paramIndex);
                        return true;
                    }

                    ; // todo: For parameter in closure, probably also a OpCodes.Stfld
                }

                return false;
            }

            private static bool EmitParameter(ParameterExpression p, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var paramIndex = ps.IndexOf(p);

                // if parameter is passed, then just load it on stack
                if (paramIndex != -1)
                {
                    if (closure != null)
                        paramIndex += 1; // shift parameter indices by one, because the first one will be closure
                    LoadParamArg(il, paramIndex);
                    return true;
                }

                // if parameter isn't passed, then it is passed into some outer lambda,
                // so it should be loaded from closure. Then the closure is null will be an invalid case.
                if (closure == null)
                    return false;

                var nonPassedParamIndex = closure.NonPassedParameters.IndexOf(it => it == p);
                if (nonPassedParamIndex == -1)
                    return false;  // what??? no chance

                var closureItemIndex = closure.Constants.Length + nonPassedParamIndex;

                il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                if (closure.Fields != null)
                    il.Emit(OpCodes.Ldfld, closure.Fields[closureItemIndex]);
                else
                    LoadArrayClosureItem(il, closureItemIndex, p.Type);

                return true;
            }

            private static void LoadParamArg(ILGenerator il, int paramIndex)
            {
                switch (paramIndex)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        if (paramIndex <= byte.MaxValue)
                            il.Emit(OpCodes.Ldarg_S, (byte)paramIndex);
                        else
                            il.Emit(OpCodes.Ldarg, paramIndex);
                        break;
                }
            }

            private static bool EmitBinary(BinaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                return TryEmit(e.Left, ps, il, closure)
                       && TryEmit(e.Right, ps, il, closure);
            }

            private static bool EmitMany(IList<Expression> es, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                for (int i = 0, n = es.Count; i < n; i++)
                    if (!TryEmit(es[i], ps, il, closure))
                        return false;
                return true;
            }

            private static bool EmitMany(IList<ExpressionInfo> es, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                for (int i = 0, n = es.Count; i < n; i++)
                    if (!TryEmit(es[i], ps, il, closure))
                        return false;
                return true;
            }

            private static bool EmitConvert(UnaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!TryEmit(e.Operand, ps, il, closure))
                    return false;

                var targetType = e.Type;
                var sourceType = e.Operand.Type;
                if (targetType == sourceType)
                    return true; // do nothing, no conversion is needed

                if (targetType == typeof(object))
                {
                    if (sourceType.GetTypeInfo().IsValueType)
                        il.Emit(OpCodes.Box, sourceType); // for valuy type to object, just box a value
                    return true; // for reference type we don't need to convert
                }

                // Just unbox type object to the target value type
                if (targetType.GetTypeInfo().IsValueType &&
                    sourceType == typeof(object))
                {
                    il.Emit(OpCodes.Unbox_Any, targetType);
                    return true;
                }

                if (targetType == typeof(int))
                    il.Emit(OpCodes.Conv_I4);
                else if (targetType == typeof(float))
                    il.Emit(OpCodes.Conv_R4);
                else if (targetType == typeof(uint))
                    il.Emit(OpCodes.Conv_U4);
                else if (targetType == typeof(sbyte))
                    il.Emit(OpCodes.Conv_I1);
                else if (targetType == typeof(byte))
                    il.Emit(OpCodes.Conv_U1);
                else if (targetType == typeof(short))
                    il.Emit(OpCodes.Conv_I2);
                else if (targetType == typeof(ushort))
                    il.Emit(OpCodes.Conv_U2);
                else if (targetType == typeof(long))
                    il.Emit(OpCodes.Conv_I8);
                else if (targetType == typeof(ulong))
                    il.Emit(OpCodes.Conv_U8);
                else if (targetType == typeof(double))
                    il.Emit(OpCodes.Conv_R8);
                else
                    il.Emit(OpCodes.Castclass, targetType);

                return true;
            }

            private static bool EmitConstant(Expr e, ILGenerator il, ClosureInfo closure)
            {
                var expr = e.Expression;
                var constExprInfo = expr as ConstantExpressionInfo;
                var constantValue = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression)expr).Value;
                if (constantValue == null)
                {
                    il.Emit(OpCodes.Ldnull);
                    return true;
                }

                var constantActualType = constantValue.GetType();
                if (constantActualType.GetTypeInfo().IsEnum)
                    constantActualType = Enum.GetUnderlyingType(constantActualType);

                if (constantActualType == typeof(int))
                {
                    EmitLoadConstantInt(il, (int)constantValue);
                }
                else if (constantActualType == typeof(char))
                {
                    EmitLoadConstantInt(il, (char)constantValue);
                }
                else if (constantActualType == typeof(short))
                {
                    EmitLoadConstantInt(il, (short)constantValue);
                }
                else if (constantActualType == typeof(byte))
                {
                    EmitLoadConstantInt(il, (byte)constantValue);
                }
                else if (constantActualType == typeof(ushort))
                {
                    EmitLoadConstantInt(il, (ushort)constantValue);
                }
                else if (constantActualType == typeof(sbyte))
                {
                    EmitLoadConstantInt(il, (sbyte)constantValue);
                }
                else if (constantActualType == typeof(uint))
                {
                    unchecked
                    {
                        EmitLoadConstantInt(il, (int)(uint)constantValue);
                    }
                }
                else if (constantActualType == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)constantValue);
                }
                else if (constantActualType == typeof(ulong))
                {
                    unchecked
                    {
                        il.Emit(OpCodes.Ldc_I8, (long)(ulong)constantValue);
                    }
                }
                else if (constantActualType == typeof(float))
                {
                    il.Emit(OpCodes.Ldc_R8, (float)constantValue);
                }
                else if (constantActualType == typeof(double))
                {
                    il.Emit(OpCodes.Ldc_R8, (double)constantValue);
                }
                else if (constantActualType == typeof(bool))
                {
                    il.Emit((bool)constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                }
                else if (constantValue is string)
                {
                    il.Emit(OpCodes.Ldstr, (string)constantValue);
                }
                else if (constantValue is Type)
                {
                    il.Emit(OpCodes.Ldtoken, (Type)constantValue);
                    il.Emit(OpCodes.Call, _getTypeFromHandleMethod);
                }
                else if (closure != null)
                {
                    var constantIndex = closure.Constants.IndexOf(it => it.ConstantExpr == expr);
                    if (constantIndex == -1)
                        return false;

                    il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                    if (closure.Fields != null)
                        il.Emit(OpCodes.Ldfld, closure.Fields[constantIndex]);
                    else
                        LoadArrayClosureItem(il, constantIndex, e.Type);

                }
                else return false;

                // boxing the value type, otherwise we can get a strange result when 0 is treated as Null.
                if (e.Type == typeof(object) && constantActualType.GetTypeInfo().IsValueType)
                    il.Emit(OpCodes.Box, constantValue.GetType()); // using normal type for Enum instead of underlying type

                return true;
            }

            // The @skipCastOrUnboxing option is for use-case when we loading and immediately storing the item, 
            // it may happen when copying from one object array to another.
            private static void LoadArrayClosureItem(ILGenerator il, int closedItemIndex, Type closedItemType)
            {
                // load array field
                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField);

                // load array item index
                EmitLoadConstantInt(il, closedItemIndex);

                // load item from index
                il.Emit(OpCodes.Ldelem_Ref);

                // Cast or unbox the object item depending if it is a class or value type
                if (closedItemType.GetTypeInfo().IsValueType)
                    il.Emit(OpCodes.Unbox_Any, closedItemType);
                else
                    il.Emit(OpCodes.Castclass, closedItemType);
            }

            private static bool EmitNew(Expr e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var newExprInfo = e.Expression as NewExpressionInfo;
                if (newExprInfo != null)
                {
                    if (!EmitMany(newExprInfo.Arguments, ps, il, closure))
                        return false;
                    il.Emit(OpCodes.Newobj, newExprInfo.Constructor);
                }
                else
                {
                    var newExpr = (NewExpression)e.Expression;
                    if (!EmitMany(newExpr.Arguments, ps, il, closure))
                        return false;
                    il.Emit(OpCodes.Newobj, newExpr.Constructor);
                }
                return true;
            }

            private static bool EmitNewArray(NewArrayExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var elems = e.Expressions;
                var arrType = e.Type;
                var elemType = arrType.GetElementType();
                if (elemType == null)
                    return false;

                var isElemOfValueType = elemType.GetTypeInfo().IsValueType;

                var arrVar = il.DeclareLocal(arrType);

                EmitLoadConstantInt(il, elems.Count);
                il.Emit(OpCodes.Newarr, elemType);
                il.Emit(OpCodes.Stloc, arrVar);

                for (int i = 0, n = elems.Count; i < n; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    EmitLoadConstantInt(il, i);

                    // loading element address for later copying of value into it.
                    if (isElemOfValueType)
                        il.Emit(OpCodes.Ldelema, elemType);

                    if (!TryEmit(elems[i], ps, il, closure))
                        return false;

                    if (isElemOfValueType)
                        il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                    else
                        il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                return true;
            }

            private static bool EmitArrayIndex(BinaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!EmitBinary(e, ps, il, closure))
                    return false;
                il.Emit(OpCodes.Ldelem_Ref);
                return true;
            }

            private static bool EmitMemberInit(MemberInitExpression mi, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!EmitNew(mi.NewExpression, ps, il, closure))
                    return false;

                var obj = il.DeclareLocal(mi.Type);
                il.Emit(OpCodes.Stloc, obj);

                var bindings = mi.Bindings;
                for (int i = 0, n = bindings.Count; i < n; i++)
                {
                    var binding = bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                        return false;
                    il.Emit(OpCodes.Ldloc, obj);

                    if (!TryEmit(((MemberAssignment)binding).Expression, ps, il, closure))
                        return false;

                    var prop = binding.Member as PropertyInfo;
                    if (prop != null)
                    {
                        var propSetMethodName = "set_" + prop.Name;
                        var setMethod = prop.DeclaringType.GetTypeInfo()
                            .DeclaredMethods.FirstOrDefault(m => m.Name == propSetMethodName);
                        if (setMethod == null)
                            return false;
                        EmitMethodCall(il, setMethod);
                    }
                    else
                    {
                        var field = binding.Member as FieldInfo;
                        if (field == null)
                            return false;
                        il.Emit(OpCodes.Stfld, field);
                    }
                }

                il.Emit(OpCodes.Ldloc, obj);
                return true;
            }

            private static bool EmitMethodCall(Expr e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprObj = e.Expression;
                var exprInfo = exprObj as MethodCallExpressionInfo;
                if (exprInfo != null)
                {
                    if (exprInfo.Object != null)
                    {
                        if (!TryEmit(exprInfo.Object, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, exprInfo.Object.Type);
                    }

                    if (exprInfo.Arguments.Length != 0 &&
                        !EmitMany(exprInfo.Arguments, ps, il, closure))
                        return false;
                }
                else
                {
                    var expr = (MethodCallExpression)exprObj;
                    if (expr.Object != null)
                    {
                        if (!TryEmit(expr.Object, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, expr.Object.Type);
                    }

                    if (expr.Arguments.Count != 0 &&
                        !EmitMany(expr.Arguments, ps, il, closure))
                        return false;
                }

                var method = exprInfo != null ? exprInfo.Method : ((MethodCallExpression)exprObj).Method;
                EmitMethodCall(il, method);
                return true;
            }

            private static void IfValueTypeStoreAndLoadValueAddress(ILGenerator il, Type ownerType)
            {
                if (ownerType.GetTypeInfo().IsValueType)
                {
                    var valueVar = il.DeclareLocal(ownerType);
                    il.Emit(OpCodes.Stloc, valueVar);
                    il.Emit(OpCodes.Ldloca, valueVar);
                }
            }

            private static bool EmitMemberAccess(Expr e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprObj = e.Expression;
                var exprInfo = exprObj as MemberExpressionInfo;
                if (exprInfo != null)
                {
                    if (exprInfo.Expression != null)
                    {
                        if (!TryEmit(exprInfo.Expression, ps, il, closure)) return false;
                        IfValueTypeStoreAndLoadValueAddress(il, exprInfo.Expression.Type);
                    }
                }
                else
                {
                    var instanceExpr = ((MemberExpression)exprObj).Expression;
                    if (instanceExpr != null)
                    {
                        if (!TryEmit(instanceExpr, ps, il, closure)) return false;
                        IfValueTypeStoreAndLoadValueAddress(il, instanceExpr.Type);
                    }
                }

                var member = exprInfo != null ? exprInfo.Member : ((MemberExpression)exprObj).Member;
                var field = member as FieldInfo;
                if (field != null)
                {
                    il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
                    return true;
                }

                var prop = member as PropertyInfo;
                if (prop != null)
                {
                    var propGetMethodName = "get_" + prop.Name;
                    var getMethod = prop.DeclaringType.GetTypeInfo()
                        .DeclaredMethods.FirstOrDefault(m => m.Name == propGetMethodName);
                    if (getMethod == null)
                        return false;
                    EmitMethodCall(il, getMethod);
                }
                return true;
            }

            private static bool EmitNestedLambda(object lambdaExpr,
                IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure)
            {
                // First, find in closed compiled lambdas the one corresponding to the current lambda expression.
                // Situation with not found lambda is not possible/exceptional,
                // it means that we somehow skipped the lambda expression while collecting closure info.
                var outerNestedLambdas = closure.NestedLambdas;
                var outerNestedLambdaIndex = outerNestedLambdas.IndexOf(it => it.LambdaExpr == lambdaExpr);
                if (outerNestedLambdaIndex == -1)
                    return false;

                var nestedLambdaInfo = outerNestedLambdas[outerNestedLambdaIndex];
                var nestedLambda = nestedLambdaInfo.Lambda;

                var outerConstants = closure.Constants;
                var outerNonPassedParams = closure.NonPassedParameters;

                // Load compiled lambda on stack counting the offset
                outerNestedLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                if (closure.Fields != null)
                    il.Emit(OpCodes.Ldfld, closure.Fields[outerNestedLambdaIndex]);
                else
                    LoadArrayClosureItem(il, outerNestedLambdaIndex, nestedLambda.GetType());

                // If lambda does not use any outer parameters to be set in closure, then we're done
                var nestedClosureInfo = nestedLambdaInfo.ClosureInfo;
                if (nestedClosureInfo == null)
                    return true;

                // If closure is array-based, the create a new array to represent closure for the nested lambda
                var isNestedArrayClosure = nestedClosureInfo.Fields == null;
                if (isNestedArrayClosure)
                {
                    EmitLoadConstantInt(il, nestedClosureInfo.ClosedItemCount); // size of array
                    il.Emit(OpCodes.Newarr, typeof(object));
                }

                // Load constants on stack
                var nestedConstants = nestedClosureInfo.Constants;
                if (nestedConstants.Length != 0)
                {
                    for (var nestedConstIndex = 0; nestedConstIndex < nestedConstants.Length; nestedConstIndex++)
                    {
                        var nestedConstant = nestedConstants[nestedConstIndex];

                        // Find constant index in the outer closure
                        var outerConstIndex = outerConstants.IndexOf(it => it.ConstantExpr == nestedConstant.ConstantExpr);
                        if (outerConstIndex == -1)
                            return false; // some error is here

                        if (isNestedArrayClosure)
                        {
                            // Duplicate nested array on stack to store the item, and load index to where to store
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstIndex);
                        }

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerConstIndex]);
                        else
                            LoadArrayClosureItem(il, outerConstIndex, nestedConstant.Type);

                        if (isNestedArrayClosure)
                        {
                            if (nestedConstant.Type.GetTypeInfo().IsValueType)
                                il.Emit(OpCodes.Box, nestedConstant.Type);
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                        }
                    }
                }

                // Load used and closed parameter values on stack
                var nestedNonPassedParams = nestedClosureInfo.NonPassedParameters;
                for (var nestedParamIndex = 0; nestedParamIndex < nestedNonPassedParams.Length; nestedParamIndex++)
                {
                    var nestedUsedParam = nestedNonPassedParams[nestedParamIndex];

                    // Duplicate nested array on stack to store the item, and load index to where to store
                    if (isNestedArrayClosure)
                    {
                        il.Emit(OpCodes.Dup);
                        EmitLoadConstantInt(il, nestedConstants.Length + nestedParamIndex);
                    }

                    var paramIndex = paramExprs.IndexOf(nestedUsedParam);
                    if (paramIndex != -1) // load param from input params
                    {
                        // +1 is set cause of added first closure argument
                        LoadParamArg(il, 1 + paramIndex);
                    }
                    else // load parameter from outer closure
                    {
                        if (outerNonPassedParams.Length == 0)
                            return false; // impossible, better to throw?

                        var outerParamIndex = outerNonPassedParams.IndexOf(it => it == nestedUsedParam);
                        if (outerParamIndex == -1)
                            return false; // impossible, better to throw?

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerConstants.Length + outerParamIndex]);
                        else
                            LoadArrayClosureItem(il, outerConstants.Length + outerParamIndex, nestedUsedParam.Type);
                    }

                    if (isNestedArrayClosure)
                    {
                        if (nestedUsedParam.Type.GetTypeInfo().IsValueType)
                            il.Emit(OpCodes.Box, nestedUsedParam.Type);
                        il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }
                }

                // Load nested lambdas on stack
                var nestedNestedLambdas = nestedClosureInfo.NestedLambdas;
                if (nestedNestedLambdas.Length != 0)
                {
                    for (var nestedLambdaIndex = 0; nestedLambdaIndex < nestedNestedLambdas.Length; nestedLambdaIndex++)
                    {
                        var nestedNestedLambda = nestedNestedLambdas[nestedLambdaIndex];

                        // Find constant index in the outer closure
                        var outerLambdaIndex = outerNestedLambdas.IndexOf(it => it.LambdaExpr == nestedNestedLambda.LambdaExpr);
                        if (outerLambdaIndex == -1)
                            return false; // some error is here

                        // Duplicate nested array on stack to store the item, and load index to where to store
                        if (isNestedArrayClosure)
                        {
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstants.Length + nestedNonPassedParams.Length + nestedLambdaIndex);
                        }

                        outerLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerLambdaIndex]);
                        else
                            LoadArrayClosureItem(il, outerLambdaIndex, 
                                nestedNestedLambda.Lambda.GetType());

                        if (isNestedArrayClosure)
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }
                }

                // Create nested closure object composed of all constants, params, lambdas loaded on stack
                if (isNestedArrayClosure)
                    il.Emit(OpCodes.Newobj, ArrayClosure.Constructor);
                else
                    il.Emit(OpCodes.Newobj,
                        nestedClosureInfo.ClosureType.GetTypeInfo().DeclaredConstructors.First());

                EmitMethodCall(il, GetCurryClosureMethod(nestedLambda, nestedLambdaInfo.IsAction));
                return true;
            }

            private static MethodInfo GetCurryClosureMethod(object lambda, bool isAction)
            {
                var lambdaTypeArgs = lambda.GetType().GetTypeInfo().GenericTypeArguments;
                return isAction
                    ? CurryClosureActions.Methods[lambdaTypeArgs.Length - 1].MakeGenericMethod(lambdaTypeArgs)
                    : CurryClosureFuncs.Methods[lambdaTypeArgs.Length - 2].MakeGenericMethod(lambdaTypeArgs);
            }

            private static bool EmitInvokeLambda(InvocationExpression e, IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure)
            {
                if (!TryEmit(e.Expression, paramExprs, il, closure) ||
                    !EmitMany(e.Arguments, paramExprs, il, closure))
                    return false;

                var invokeMethod = e.Expression.Type.GetTypeInfo().DeclaredMethods.First(m => m.Name == "Invoke");
                EmitMethodCall(il, invokeMethod);
                return true;
            }

            private static bool EmitComparison(BinaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!TryEmit(e.Left, ps, il, closure) ||
                    !TryEmit(e.Right, ps, il, closure))
                    return false;

                switch (e.NodeType)
                {
                    case ExpressionType.Equal:
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.LessThan:
                        il.Emit(OpCodes.Clt);
                        break;
                    case ExpressionType.GreaterThan:
                        il.Emit(OpCodes.Cgt);
                        break;
                    case ExpressionType.NotEqual:
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        il.Emit(OpCodes.Cgt);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        il.Emit(OpCodes.Clt);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                }
                return true;
            }

            private static bool EmitLogicalOperator(BinaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!TryEmit(e.Left, ps, il, closure))
                    return false;

                var labelSkipRight = il.DefineLabel();
                var isAnd = e.NodeType == ExpressionType.AndAlso;
                il.Emit(isAnd ? OpCodes.Brfalse : OpCodes.Brtrue, labelSkipRight);

                if (!TryEmit(e.Right, ps, il, closure))
                    return false;

                var labelDone = il.DefineLabel();
                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelSkipRight); // label the second branch
                il.Emit(isAnd ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);

                il.MarkLabel(labelDone);
                return true;
            }

            private static bool EmitTernararyOperator(ConditionalExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!TryEmit(e.Test, ps, il, closure))
                    return false;

                var labelIfFalse = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, labelIfFalse);

                if (!TryEmit(e.IfTrue, ps, il, closure))
                    return false;

                var labelDone = il.DefineLabel();
                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelIfFalse);
                if (!TryEmit(e.IfFalse, ps, il, closure))
                    return false;

                il.MarkLabel(labelDone);
                return true;
            }

            private static void EmitMethodCall(ILGenerator il, MethodInfo method)
            {
                il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
            }

            private static void EmitLoadConstantInt(ILGenerator il, int i)
            {
                switch (i)
                {
                    case -1:
                        il.Emit(OpCodes.Ldc_I4_M1);
                        break;
                    case 0:
                        il.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        il.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        il.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        il.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        il.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        il.Emit(OpCodes.Ldc_I4_8);
                        break;
                    default:
                        il.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }
        }
    }

    /// <summary>Base expression.</summary>
    public abstract class ExpressionInfo
    {
        /// <summary>Expression node type.</summary>
        public abstract ExpressionType NodeType { get; }

        /// <summary>All expressions should have a Type.</summary>
        public abstract Type Type { get; }

        /// <summary>Allow to change parameter expression as info interchangeable.</summary>
        public static implicit operator ExpressionInfo(ParameterExpression paramExpr)
        {
            return new ParameterExpressionInfo(paramExpr);
        }

        /// <summary>Analog of Expression.Constant</summary>
        public static ConstantExpressionInfo Constant(object value, Type type = null)
        {
            return new ConstantExpressionInfo(value, type);
        }

        /// <summary>Analog of Expression.Convert</summary>
        public static ConvertExpressionInfo Convert(ExpressionInfo operand, Type targetType)
        {
            return new ConvertExpressionInfo(operand, targetType);
        }

        /// <summary>Analog of Expression.New</summary>
        public static NewExpressionInfo New(ConstructorInfo ctor, params ExpressionInfo[] arguments)
        {
            return new NewExpressionInfo(ctor, arguments);
        }

        /// <summary>Static method call</summary>
        public static MethodCallExpressionInfo Call(MethodInfo method, params ExpressionInfo[] arguments)
        {
            return new MethodCallExpressionInfo(null, method, arguments);
        }

        /// <summary>Instance method call</summary>
        public static MethodCallExpressionInfo Call(
            ExpressionInfo instance, MethodInfo method, params ExpressionInfo[] arguments)
        {
            return new MethodCallExpressionInfo(instance, method, arguments);
        }

        /// <summary>Static property</summary>
        public static PropertyExpressionInfo Property(PropertyInfo property)
        {
            return new PropertyExpressionInfo(null, property);
        }

        /// <summary>Instance property</summary>
        public static PropertyExpressionInfo Property(ExpressionInfo instance, PropertyInfo property)
        {
            return new PropertyExpressionInfo(instance, property);
        }

        /// <summary>Static field</summary>
        public static FieldExpressionInfo Field(FieldInfo field)
        {
            return new FieldExpressionInfo(null, field);
        }

        /// <summary>Instance field</summary>
        public static FieldExpressionInfo Property(ExpressionInfo instance, FieldInfo field)
        {
            return new FieldExpressionInfo(instance, field);
        }

        /// <summary>Analog of Expression.Lambda</summary>
        public static LambdaExpressionInfo Lambda(ExpressionInfo body, params ParameterExpression[] parameters)
        {
            return new LambdaExpressionInfo(body, parameters);
        }
    }

    /// <summary>Wraps ParameterExpression and just it.</summary>
    public class ParameterExpressionInfo : ExpressionInfo
    {
        /// <summary>Wrapped parameter expression.</summary>
        public ParameterExpression ParamExpr { get; }

        /// <summary>Allow to change parameter expression as info interchangeable.</summary>
        public static implicit operator ParameterExpression(ParameterExpressionInfo info)
        {
            return info.ParamExpr;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.Parameter; } }

        /// <inheritdoc />
        public override Type Type { get { return ParamExpr.Type; } }

        /// <summary>Optional name.</summary>
        public string Name { get { return ParamExpr.Name; } }

        /// <summary>Constructor</summary>
        public ParameterExpressionInfo(ParameterExpression paramExpr)
        {
            ParamExpr = paramExpr;
        }
    }

    /// <summary>Analog of ConstantExpression.</summary>
    public class ConstantExpressionInfo : ExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.Constant; } }

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>Value of constant.</summary>
        public readonly object Value;

        /// <summary>Constructor</summary>
        public ConstantExpressionInfo(object value, Type type = null)
        {
            Value = value;
            Type = type ?? (value == null ? typeof(object) : value.GetType());
        }
    }

    /// <summary>Analog of Convert expression.</summary>
    public class ConvertExpressionInfo : ExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.Convert; } }

        /// <summary>Target type.</summary>
        public override Type Type { get { return _targetType; } }
        private readonly Type _targetType;

        /// <summary>Operand to cast to a target type.</summary>
        public readonly ExpressionInfo Operand;

        /// <summary>Constructor</summary>
        public ConvertExpressionInfo(ExpressionInfo operand, Type targetType)
        {
            Operand = operand;
            _targetType = targetType;
        }
    }

    /// <summary>Base class for expressions with arguments.</summary>
    public abstract class ArgumentsExpressionInfo : ExpressionInfo
    {
        /// <summary>List of arguments</summary>
        public readonly ExpressionInfo[] Arguments;

        /// <summary>Constructor</summary>
        protected ArgumentsExpressionInfo(ExpressionInfo[] arguments)
        {
            Arguments = arguments;
        }
    }

    /// <summary>Analog of NewExpression</summary>
    public class NewExpressionInfo : ArgumentsExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.New; } }

        /// <inheritdoc />
        public override Type Type { get { return Constructor.DeclaringType; } }

        /// <summary>The constructor info.</summary>
        public readonly ConstructorInfo Constructor;

        /// <summary>Construct from constructor info and argument expressions</summary>
        public NewExpressionInfo(ConstructorInfo constructor, params ExpressionInfo[] arguments) : base(arguments)
        {
            Constructor = constructor;
        }
    }

    /// <summary>Analog of MethodCallExpression</summary>
    public class MethodCallExpressionInfo : ArgumentsExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.Call; } }

        /// <inheritdoc />
        public override Type Type { get { return Method.ReturnType; } }

        /// <summary>The method info.</summary>
        public readonly MethodInfo Method;

        /// <summary>Instance expression, null if static.</summary>
        public readonly ExpressionInfo Object;

        /// <summary>Construct from method info and argument expressions</summary>
        public MethodCallExpressionInfo(
            ExpressionInfo @object, MethodInfo method, params ExpressionInfo[] arguments) : base(arguments)
        {
            Object = @object;
            Method = method;
        }
    }

    /// <summary>Analog of MemberExpression</summary>
    public abstract class MemberExpressionInfo : ExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.MemberAccess; } }

        /// <summary>Member info.</summary>
        public readonly MemberInfo Member;

        /// <summary>Instance expression, null if static.</summary>
        public readonly ExpressionInfo Expression;

        /// <summary>Constructs with</summary>
        protected MemberExpressionInfo(ExpressionInfo expression, MemberInfo member)
        {
            Expression = expression;
            Member = member;
        }
    }

    /// <summary>Analog of PropertyExpression</summary>
    public class PropertyExpressionInfo : MemberExpressionInfo
    {
        /// <inheritdoc />
        public override Type Type { get { return ((PropertyInfo)Member).PropertyType; } }

        /// <summary>Construct from property info</summary>
        public PropertyExpressionInfo(ExpressionInfo instance, PropertyInfo property)
            : base(instance, property) { }
    }

    /// <summary>Analog of PropertyExpression</summary>
    public class FieldExpressionInfo : MemberExpressionInfo
    {
        /// <inheritdoc />
        public override Type Type { get { return ((FieldInfo)Member).FieldType; } }

        /// <summary>Construct from field info</summary>
        public FieldExpressionInfo(ExpressionInfo instance, FieldInfo field)
            : base(instance, field) { }
    }

    /// <summary>LambdaExpression</summary>
    public class LambdaExpressionInfo : ExpressionInfo
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get { return ExpressionType.Lambda; } }

        /// <inheritdoc />
        public override Type Type { get { return _type; } }

        private readonly Type _type;

        /// <summary>Lambda body.</summary>
        public readonly ExpressionInfo Body;

        /// <summary>List of parameters.</summary>
        public readonly ParameterExpression[] Parameters;

        /// <summary>Constructor</summary>
        public LambdaExpressionInfo(ExpressionInfo body, ParameterExpression[] parameters)
        {
            Body = body;
            Parameters = parameters;
            _type = ExpressionCompiler.GetFuncOrActionType(ExpressionCompiler.GetParamExprTypes(parameters), Body.Type);
        }
    }

    /// <summary>Typed lambda expression.</summary>
    public sealed class ExpressionInfo<TDelegate> : LambdaExpressionInfo
    {
        /// <summary>Type of lambda</summary>
        public Type DelegateType { get { return typeof(TDelegate); } }

        /// <summary>Constructor</summary>
        public ExpressionInfo(ExpressionInfo body, ParameterExpression[] parameters) : base(body, parameters) { }
    }
}