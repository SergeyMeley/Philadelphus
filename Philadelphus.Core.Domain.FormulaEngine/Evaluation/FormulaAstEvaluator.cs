using System.Globalization;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Expressions;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.Helpers;

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

            if (IsFormulaSource(source) == false)
            {
                return EvaluatePlainValue(source);
            }

            var parserResult = FormulaParser.Parse(source);
            if (parserResult.IsSuccess == false)
            {
                var result = FormulaResult.Failure(parserResult.Errors[0]);
                ReportFailure(result, context, FormulaDiagnosticKind.ParseError);
                return result;
            }

            var evaluationResult = EvaluateCore(parserResult.Expression!, context);
            ReportFailure(evaluationResult, context, ResolveDiagnosticKind(evaluationResult));
            return evaluationResult;
        }

        /// <summary>
        /// Проверяет, является ли пользовательский ввод формулой.
        /// </summary>
        /// <param name="source">Исходный пользовательский ввод.</param>
        /// <returns>true, если ввод начинается с маркера формулы после начальных пробелов; иначе false.</returns>
        private static bool IsFormulaSource(string? source)
        {
            return string.IsNullOrEmpty(source) == false
                && source.TrimStart().StartsWith("=", StringComparison.Ordinal);
        }

        private static FormulaResult EvaluatePlainValue(string? source)
        {
            var value = source ?? string.Empty;
            var trimmedValue = value.Trim();

            if (long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
            {
                return FormulaResult.Success(integerValue, SystemBaseType.INTEGER);
            }

            if (double.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue)
                && double.IsNaN(floatValue) == false
                && double.IsInfinity(floatValue) == false)
            {
                return FormulaResult.Success(floatValue, SystemBaseType.FLOAT);
            }

            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.BOOL, trimmedValue, out var boolValue, out _))
            {
                return FormulaResult.Success(boolValue, SystemBaseType.BOOL);
            }

            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.DATETIME, trimmedValue, out var dateTimeValue, out _))
            {
                return FormulaResult.Success(dateTimeValue, SystemBaseType.DATETIME);
            }

            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.DATE, trimmedValue, out var dateValue, out _))
            {
                return FormulaResult.Success(dateValue, SystemBaseType.DATE);
            }

            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.TIME, trimmedValue, out var timeValue, out _))
            {
                return FormulaResult.Success(timeValue, SystemBaseType.TIME);
            }

            return FormulaResult.Success(value, SystemBaseType.STRING);
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

            var result = EvaluateCore(expression, context);
            ReportFailure(result, context, ResolveDiagnosticKind(result));
            return result;
        }

        /// <summary>
        /// Вычисляет AST без отправки диагностики вызывающему приложению.
        /// </summary>
        /// <param name="expression">Выражение AST.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат вычисления выражения.</returns>
        private FormulaResult EvaluateCore(FormulaExpression expression, FormulaExecutionContext context)
        {
            try
            {
                var result = EvaluateExpression(expression, context);
                return FormulaResultMaterializer.Materialize(result, context, expression.Span);
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
        /// Отправляет ошибку Formula Engine в диагностический приемник контекста.
        /// </summary>
        /// <param name="result">Результат вычисления.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <param name="kind">Тип диагностического события.</param>
        private static void ReportFailure(
            FormulaResult result,
            FormulaExecutionContext context,
            FormulaDiagnosticKind kind)
        {
            if (result.Error is null || context.DiagnosticsReporter is null)
            {
                return;
            }

            context.DiagnosticsReporter.Report(new FormulaDiagnostic
            {
                Kind = kind,
                Error = result.Error
            });
        }

        /// <summary>
        /// Определяет тип диагностического события по ошибке результата.
        /// </summary>
        /// <param name="result">Результат вычисления.</param>
        /// <returns>Тип диагностического события.</returns>
        private static FormulaDiagnosticKind ResolveDiagnosticKind(FormulaResult result)
        {
            if (result.Error?.Code == FormulaErrorCode.PluginLoadError)
            {
                return FormulaDiagnosticKind.PluginLoadError;
            }

            return result.Error?.Exception is null
                ? FormulaDiagnosticKind.EvaluationError
                : FormulaDiagnosticKind.UnexpectedException;
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
            if (string.Equals(functionCall.Name, "АТРИБУТ", StringComparison.OrdinalIgnoreCase))
            {
                return EvaluateRelativeAttributeFunction(functionCall, context);
            }

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

            var result = resolveResult.Formula!.Evaluator(context, arguments.Values);
            return FormulaResultMaterializer.Materialize(result, context, functionCall.Span, functionCall.Name);
        }

        private static FormulaResult EvaluateRelativeAttributeFunction(
            FunctionCallFormulaExpression functionCall,
            FormulaExecutionContext context)
        {
            if (functionCall.Arguments.Count != 1)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'АТРИБУТ' ожидает один аргумент.",
                    functionCall.Span,
                    functionCall.Name));
            }

            if (context.CurrentAttributeOwner == null)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Формула 'АТРИБУТ' без указания листа требует текущий элемент в контексте вычисления.",
                    functionCall.Span,
                    functionCall.Name));
            }

            if (TryGetAttributeName(functionCall.Arguments[0], out var attributeName) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Аргумент формулы 'АТРИБУТ' должен быть строковым названием атрибута.",
                    functionCall.Arguments[0].Span,
                    functionCall.Name));
            }

            var attributes = context.CurrentAttributeOwner.Attributes
                .Where(x => string.Equals(x.Name, attributeName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (attributes.Count == 0)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeNotFound,
                    $"Атрибут '{attributeName}' не найден у текущего элемента.",
                    functionCall.Arguments[0].Span,
                    functionCall.Name));
            }

            if (attributes.Count > 1)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeDuplicate,
                    $"У текущего элемента найдено несколько атрибутов с названием '{attributeName}'.",
                    functionCall.Arguments[0].Span,
                    functionCall.Name));
            }

            var attribute = attributes[0];
            if (attribute.IsCollectionValue)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    $"Атрибут '{attributeName}' содержит коллекцию значений, а формула 'АТРИБУТ' ожидает одиночное значение.",
                    functionCall.Arguments[0].Span,
                    functionCall.Name));
            }

            return attribute.Value is null
                ? FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeNotFound,
                    $"Атрибут '{attributeName}' найден, но значение атрибута не задано.",
                    functionCall.Arguments[0].Span,
                    functionCall.Name))
                : FormulaResult.FromTreeLeave(attribute.Value);
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

            var result = resolveResult.Formula!.Evaluator(context, new[] { left, right });
            return FormulaResultMaterializer.Materialize(result, context, binary.Span, binary.Operator);
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

            if (ConditionalFormulaProvider.TryGetCondition(condition, out var conditionValue) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Условный оператор ожидает логическое значение в условии.",
                    conditional.Condition.Span,
                    "?"));
            }

            var result = EvaluateExpression(
                conditionValue ? conditional.WhenTrue : conditional.WhenFalse,
                context);
            return FormulaResultMaterializer.Materialize(result, context, conditional.Span, "?");
        }

        /// <summary>
        /// Вычисляет ссылку на лист дерева через рабочее дерево из контекста.
        /// </summary>
        /// <param name="treeLeaveReference">Выражение ссылки на лист.</param>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <returns>Результат со значением листа или ошибка поиска.</returns>
        private FormulaResult EvaluateTreeLeaveReference(
            TreeLeaveReferenceFormulaExpression treeLeaveReference,
            FormulaExecutionContext context)
        {
            var resolveResult = _registry.Resolve("ЛИСТ");
            if (resolveResult.IsResolved == false)
            {
                return FormulaResult.Failure(CopyErrorWithSpan(resolveResult.Error!, treeLeaveReference.Span));
            }

            var arguments = new[]
            {
                FormulaResult.Success(0, SystemBaseType.INTEGER),
                FormulaResult.Success(treeLeaveReference.Uuid.ToString(), SystemBaseType.STRING)
            };

            var result = resolveResult.Formula!.Evaluator(context, arguments);
            return FormulaResultMaterializer.Materialize(result, context, treeLeaveReference.Span, "ЛИСТ");
        }

        /// <summary>
        /// Копирует ошибку resolver'а и добавляет диапазон исходного выражения.
        /// </summary>
        /// <param name="error">Исходная ошибка поиска листа.</param>
        /// <param name="span">Диапазон ссылки на лист в формуле.</param>
        /// <returns>Ошибка формулы с диапазоном исходного выражения.</returns>
        private static FormulaError CopyErrorWithSpan(FormulaError error, FormulaTextSpan span)
        {
            return new FormulaError
            {
                Code = error.Code,
                Message = error.Message,
                Span = span,
                FunctionOrOperator = error.FunctionOrOperator,
                Exception = error.Exception
            };
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

            if (string.Equals(objectMethodCall.MethodName, "СВОЙСТВО", StringComparison.OrdinalIgnoreCase))
            {
                return EvaluatePropertyObjectMethod(objectMethodCall, target);
            }

            if (string.Equals(objectMethodCall.MethodName, "АТРИБУТ", StringComparison.OrdinalIgnoreCase))
            {
                return EvaluateAttributeObjectMethod(objectMethodCall, target);
            }

            return FormulaResult.Failure(CreateError(
                FormulaErrorCode.NotImplemented,
                $"Объектная функция '{objectMethodCall.MethodName}' будет реализована отдельной подзадачей.",
                objectMethodCall.Span,
                objectMethodCall.MethodName));
        }

        /// <summary>
        /// Вычисляет объектную функцию АТРИБУТ для результата, содержащего лист дерева.
        /// </summary>
        /// <param name="objectMethodCall">Выражение объектного вызова АТРИБУТ.</param>
        /// <param name="target">Вычисленный объект, от которого запрошен атрибут.</param>
        /// <returns>Значение атрибута или диагностическая ошибка.</returns>
        private static FormulaResult EvaluateAttributeObjectMethod(
            ObjectMethodCallFormulaExpression objectMethodCall,
            FormulaResult target)
        {
            if (objectMethodCall.Arguments.Count != 1)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Объектная функция 'АТРИБУТ' ожидает один аргумент.",
                    objectMethodCall.Span,
                    objectMethodCall.MethodName));
            }

            if (target.TreeLeave is null)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Объектная функция 'АТРИБУТ' может вызываться только от листа дерева.",
                    objectMethodCall.Target.Span,
                    objectMethodCall.MethodName));
            }

            if (TryGetAttributeName(objectMethodCall.Arguments[0], out var attributeName) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Аргумент объектной функции 'АТРИБУТ' должен быть строковым названием атрибута.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
            }

            var attributes = target.TreeLeave.Attributes
                .Where(x => string.Equals(x.Name, attributeName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (attributes.Count == 0)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeNotFound,
                    $"Атрибут '{attributeName}' не найден у листа дерева.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
            }

            if (attributes.Count > 1)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeDuplicate,
                    $"У листа дерева найдено несколько атрибутов с названием '{attributeName}'.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
            }

            var attribute = attributes[0];
            if (attribute.IsCollectionValue)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    $"Атрибут '{attributeName}' содержит коллекцию значений, а формула 'АТРИБУТ' ожидает одиночное значение.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
            }

            return attribute.Value is null
                ? FormulaResult.Failure(CreateError(
                    FormulaErrorCode.AttributeNotFound,
                    $"Атрибут '{attributeName}' найден, но значение атрибута не задано.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName))
                : FormulaResult.FromTreeLeave(attribute.Value);
        }

        /// <summary>
        /// Вычисляет объектную функцию СВОЙСТВО для результата, содержащего лист дерева.
        /// </summary>
        /// <param name="objectMethodCall">Выражение объектного вызова СВОЙСТВО.</param>
        /// <param name="target">Вычисленный объект, от которого запрошено свойство.</param>
        /// <returns>Значение свойства листа или диагностическая ошибка.</returns>
        private static FormulaResult EvaluatePropertyObjectMethod(
            ObjectMethodCallFormulaExpression objectMethodCall,
            FormulaResult target)
        {
            if (objectMethodCall.Arguments.Count != 1)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Объектная функция 'СВОЙСТВО' ожидает один аргумент.",
                    objectMethodCall.Span,
                    objectMethodCall.MethodName));
            }

            if (target.TreeLeave is null)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Объектная функция 'СВОЙСТВО' может вызываться только от листа дерева.",
                    objectMethodCall.Target.Span,
                    objectMethodCall.MethodName));
            }

            if (TryGetPropertyName(objectMethodCall.Arguments[0], out var propertyName) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Аргумент объектной функции 'СВОЙСТВО' должен быть именем свойства.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
            }

            return TryResolveTreeLeaveProperty(target.TreeLeave, propertyName, out var value)
                ? FormulaResult.Success(value, SystemBaseType.STRING)
                : FormulaResult.Failure(CreateError(
                    FormulaErrorCode.PropertyNotFound,
                    $"Свойство '{propertyName}' не найдено в whitelist свойств листа дерева.",
                    objectMethodCall.Arguments[0].Span,
                    objectMethodCall.MethodName));
        }

        /// <summary>
        /// Извлекает имя свойства из аргумента СВОЙСТВО без вычисления его как самостоятельной переменной.
        /// </summary>
        /// <param name="argument">AST-аргумент объектной функции.</param>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>true, если аргумент задает имя свойства; иначе false.</returns>
        private static bool TryGetPropertyName(FormulaExpression argument, out string propertyName)
        {
            propertyName = argument switch
            {
                IdentifierFormulaExpression identifier => identifier.Name,
                LiteralFormulaExpression { Type: SystemBaseType.STRING, Value: string value } => value,
                _ => string.Empty
            };

            return string.IsNullOrWhiteSpace(propertyName) == false;
        }

        /// <summary>
        /// Извлекает название атрибута из аргумента АТРИБУТ без вычисления как выражения.
        /// </summary>
        /// <param name="argument">AST-аргумент объектной функции.</param>
        /// <param name="attributeName">Название атрибута.</param>
        /// <returns>true, если аргумент задает непустое строковое название атрибута; иначе false.</returns>
        private static bool TryGetAttributeName(FormulaExpression argument, out string attributeName)
        {
            attributeName = argument is LiteralFormulaExpression { Type: SystemBaseType.STRING, Value: string value }
                ? value
                : string.Empty;

            return string.IsNullOrWhiteSpace(attributeName) == false;
        }

        /// <summary>
        /// Возвращает разрешенное свойство листа дерева по явному whitelist.
        /// </summary>
        /// <param name="treeLeave">Лист дерева.</param>
        /// <param name="propertyName">Запрошенное имя свойства.</param>
        /// <param name="value">Строковое значение свойства.</param>
        /// <returns>true, если свойство поддержано; иначе false.</returns>
        private static bool TryResolveTreeLeaveProperty(
            TreeLeaveModel treeLeave,
            string propertyName,
            out string? value)
        {
            value = propertyName switch
            {
                var name when string.Equals(name, nameof(TreeLeaveModel.Name), StringComparison.OrdinalIgnoreCase)
                    => treeLeave.Name,
                var name when string.Equals(name, nameof(TreeLeaveModel.AuditInfo.CreatedBy), StringComparison.OrdinalIgnoreCase)
                    => treeLeave.AuditInfo.CreatedBy,
                _ => null
            };

            return value is not null;
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
