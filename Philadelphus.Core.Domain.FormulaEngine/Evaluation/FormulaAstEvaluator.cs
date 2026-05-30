using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Expressions;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;
using Philadelphus.Core.Domain.FormulaEngine.Registry;

namespace Philadelphus.Core.Domain.FormulaEngine.Evaluation
{
    /// <summary>
    /// Вычисляет AST формулы через реестр формул и текущий контекст выполнения.
    /// </summary>
    public sealed class FormulaAstEvaluator
    {
        /// <summary>
        /// Реестр именованных формул и операторных псевдонимов.
        /// </summary>
        private readonly FormulaRegistry _registry;

        /// <summary>
        /// Инициализирует вычислитель AST формулы.
        /// </summary>
        /// <param name="registry">Реестр формул, используемый для вызовов функций и операторов.</param>
        public FormulaAstEvaluator(FormulaRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Разбирает и вычисляет строку формулы.
        /// </summary>
        /// <param name="source">Исходный текст формулы.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вычисления или ошибка разбора/вычисления.</returns>
        public FormulaResult Evaluate(string? source, FormulaExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var parserResult = FormulaParser.Parse(source);
            if (parserResult.IsSuccess == false)
            {
                return FormulaResult.Failure(parserResult.Errors[0]);
            }

            return Evaluate(parserResult.Expression!, context);
        }

        /// <summary>
        /// Вычисляет уже построенное выражение AST.
        /// </summary>
        /// <param name="expression">Выражение AST.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вычисления выражения.</returns>
        public FormulaResult Evaluate(FormulaExpression expression, FormulaExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(expression);
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                return EvaluateExpression(expression, context);
            }
            catch (Exception exception)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.NotImplemented,
                    "При вычислении формулы возникла непредвиденная ошибка.",
                    expression.Span,
                    exception: exception));
            }
        }

        /// <summary>
        /// Вычисляет конкретный тип выражения AST.
        /// </summary>
        /// <param name="expression">Выражение AST.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вычисления выражения.</returns>
        private FormulaResult EvaluateExpression(FormulaExpression expression, FormulaExecutionContext context)
        {
            return expression switch
            {
                LiteralFormulaExpression literal => EvaluateLiteral(literal),
                FunctionCallFormulaExpression functionCall => EvaluateFunctionCall(functionCall, context),
                BinaryFormulaExpression binary => EvaluateBinary(binary, context),
                ConditionalFormulaExpression conditional => EvaluateConditional(conditional, context),
                TreeLeaveReferenceFormulaExpression treeLeaveReference => EvaluateTreeLeaveReference(treeLeaveReference, context),
                ObjectMethodCallFormulaExpression objectMethodCall => EvaluateObjectMethodCall(objectMethodCall, context),
                IdentifierFormulaExpression identifier => FormulaResult.Failure(CreateError(
                    FormulaErrorCode.ParseError,
                    $"Идентификатор '{identifier.Name}' не может быть вычислен как самостоятельное выражение.",
                    identifier.Span,
                    identifier.Name)),
                _ => FormulaResult.Failure(CreateError(
                    FormulaErrorCode.NotImplemented,
                    $"Тип выражения '{expression.GetType().Name}' пока не поддерживается.",
                    expression.Span))
            };
        }

        /// <summary>
        /// Возвращает результат литерала без дополнительного преобразования типа.
        /// </summary>
        /// <param name="literal">Литеральное выражение.</param>
        /// <returns>Результат вычисления литерала.</returns>
        private static FormulaResult EvaluateLiteral(LiteralFormulaExpression literal)
        {
            return FormulaResult.Success(literal.Value, literal.Type);
        }

        /// <summary>
        /// Вычисляет вызов именованной функции через реестр.
        /// </summary>
        /// <param name="functionCall">Выражение вызова функции.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вызова функции.</returns>
        private FormulaResult EvaluateFunctionCall(FunctionCallFormulaExpression functionCall, FormulaExecutionContext context)
        {
            var resolveResult = _registry.Resolve(functionCall.Name);
            if (resolveResult.IsResolved == false)
            {
                return FormulaResult.Failure(resolveResult.Error!);
            }

            var arguments = EvaluateArguments(functionCall.Arguments, context);
            if (arguments.Error is not null)
            {
                return FormulaResult.Failure(arguments.Error);
            }

            return resolveResult.Formula!.Evaluator(context, arguments.Values);
        }

        /// <summary>
        /// Вычисляет бинарный оператор как вызов формулы по операторному alias.
        /// </summary>
        /// <param name="binary">Бинарное выражение.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вычисления оператора.</returns>
        private FormulaResult EvaluateBinary(BinaryFormulaExpression binary, FormulaExecutionContext context)
        {
            var resolveResult = _registry.Resolve(binary.Operator);
            if (resolveResult.IsResolved == false)
            {
                return FormulaResult.Failure(resolveResult.Error!);
            }

            var left = EvaluateExpression(binary.Left, context);
            if (left.IsSuccess == false)
            {
                return left;
            }

            var right = EvaluateExpression(binary.Right, context);
            if (right.IsSuccess == false)
            {
                return right;
            }

            return resolveResult.Formula!.Evaluator(context, new[] { left, right });
        }

        /// <summary>
        /// Вычисляет условный оператор с коротким вычислением только выбранной ветки.
        /// </summary>
        /// <param name="conditional">Условное выражение.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат выбранной ветки условного выражения.</returns>
        private FormulaResult EvaluateConditional(ConditionalFormulaExpression conditional, FormulaExecutionContext context)
        {
            var condition = EvaluateExpression(conditional.Condition, context);
            if (condition.IsSuccess == false)
            {
                return condition;
            }

            if (condition.ValueType != SystemBaseType.BOOL || condition.Value is not bool conditionValue)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Условный оператор ожидает логическое значение в условии.",
                    conditional.Condition.Span,
                    "?"));
            }

            return EvaluateExpression(
                conditionValue ? conditional.WhenTrue : conditional.WhenFalse,
                context);
        }

        /// <summary>
        /// Вычисляет ссылку на лист дерева через рабочее дерево из контекста.
        /// </summary>
        /// <param name="treeLeaveReference">Выражение ссылки на лист.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат со значением листа или ошибка поиска.</returns>
        private static FormulaResult EvaluateTreeLeaveReference(
            TreeLeaveReferenceFormulaExpression treeLeaveReference,
            FormulaExecutionContext context)
        {
            var treeLeave = context.WorkingTree?.ContentLeaves
                .SingleOrDefault(x => x.Uuid == treeLeaveReference.Uuid);

            if (treeLeave is null)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TreeLeaveNotFound,
                    $"Лист дерева '{treeLeaveReference.Uuid}' не найден.",
                    treeLeaveReference.Span,
                    "[]"));
            }

            return ConvertTreeLeaveToResult(treeLeave);
        }

        /// <summary>
        /// Преобразует найденный лист дерева в результат формулы без дублирования доменной типизации.
        /// </summary>
        /// <param name="treeLeave">Найденный лист дерева.</param>
        /// <returns>Результат формулы для листа дерева.</returns>
        private static FormulaResult ConvertTreeLeaveToResult(TreeLeaveModel treeLeave)
        {
            return treeLeave is SystemBaseTreeLeaveModel systemBaseTreeLeave
                ? FormulaResult.Success(
                    systemBaseTreeLeave.TypedValue,
                    systemBaseTreeLeave.SystemBaseType,
                    systemBaseTreeLeave)
                : FormulaResult.FromTreeLeave(treeLeave);
        }

        /// <summary>
        /// Обрабатывает объектные вызовы формулы.
        /// </summary>
        /// <param name="objectMethodCall">Выражение объектного вызова.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат объектного вызова или зарезервированная ошибка.</returns>
        private FormulaResult EvaluateObjectMethodCall(
            ObjectMethodCallFormulaExpression objectMethodCall,
            FormulaExecutionContext context)
        {
            var target = EvaluateExpression(objectMethodCall.Target, context);
            if (target.IsSuccess == false)
            {
                return target;
            }

            return FormulaResult.Failure(CreateError(
                FormulaErrorCode.NotImplemented,
                $"Объектная функция '{objectMethodCall.MethodName}' будет реализована отдельной подзадачей.",
                objectMethodCall.Span,
                objectMethodCall.MethodName));
        }

        /// <summary>
        /// Вычисляет список аргументов функции слева направо.
        /// </summary>
        /// <param name="arguments">Выражения аргументов.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Успешно вычисленные аргументы или первая ошибка.</returns>
        private EvaluatedArguments EvaluateArguments(
            IReadOnlyList<FormulaExpression> arguments,
            FormulaExecutionContext context)
        {
            var results = new List<FormulaResult>(arguments.Count);
            foreach (var argument in arguments)
            {
                var result = EvaluateExpression(argument, context);
                if (result.IsSuccess == false)
                {
                    return new EvaluatedArguments(results, result.Error);
                }

                results.Add(result);
            }

            return new EvaluatedArguments(results, null);
        }

        /// <summary>
        /// Создает ошибку вычисления формулы.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Подробное сообщение ошибки.</param>
        /// <param name="span">Диапазон исходной формулы, к которому относится ошибка.</param>
        /// <param name="functionOrOperator">Имя функции или оператора.</param>
        /// <param name="exception">Исходное исключение.</param>
        /// <returns>Ошибка формулы.</returns>
        private static FormulaError CreateError(
            FormulaErrorCode code,
            string message,
            FormulaTextSpan span,
            string? functionOrOperator = null,
            Exception? exception = null)
        {
            return new FormulaError
            {
                Code = code,
                Message = message,
                Span = span,
                FunctionOrOperator = functionOrOperator,
                Exception = exception
            };
        }

        /// <summary>
        /// Промежуточный результат вычисления аргументов функции.
        /// </summary>
        /// <param name="Values">Успешно вычисленные аргументы.</param>
        /// <param name="Error">Первая ошибка вычисления аргументов.</param>
        private sealed record EvaluatedArguments(
            IReadOnlyList<FormulaResult> Values,
            FormulaError? Error);
    }
}
