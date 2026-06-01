using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты вычисления AST формул.
    /// </summary>
    public class FormulaAstEvaluatorTests
    {
        /// <summary>
        /// Проверяет вычисление числового литерала без преобразования через строку.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_Numeric_Literal()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=42", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет вычисление строкового литерала как строкового значения.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_String_Literal()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=\"123\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("123");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет, что выражение без маркера формулы не вычисляется и остается строкой.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_String_Value_When_Plain_Source_Looks_Like_Expression()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("40*5", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("40*5");
            result.ValueType.Should().Be(SystemBaseType.STRING);
            result.TreeLeave.Should().BeNull();
        }

        /// <summary>
        /// Проверяет, что обычное значение без маркера формулы получает наиболее подходящий системный тип.
        /// </summary>
        [Theory]
        [InlineData("40", 40L, SystemBaseType.INTEGER)]
        [InlineData("40.3", 40.3d, SystemBaseType.FLOAT)]
        [InlineData("Истина", true, SystemBaseType.BOOL)]
        [InlineData("2026-05-31", null, SystemBaseType.DATE)]
        [InlineData("14:15:16", null, SystemBaseType.TIME)]
        public void Evaluate_Returns_Typed_Plain_Value_When_Source_Does_Not_Start_With_Formula_Marker(
            string source,
            object? expectedValue,
            SystemBaseType expectedType)
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(source, FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.ValueType.Should().Be(expectedType);
            if (expectedValue is not null)
            {
                result.Value.Should().Be(expectedValue);
            }
        }

        /// <summary>
        /// Проверяет отправку диагностики при ошибке разбора формулы.
        /// </summary>
        [Fact]
        public void Evaluate_Reports_Parse_Error_Diagnostics()
        {
            var evaluator = CreateEvaluator();
            var reporter = new CapturingFormulaDiagnosticsReporter();
            var context = new FormulaExecutionContext
            {
                DiagnosticsReporter = reporter
            };

            var result = evaluator.Evaluate("=1+", context);

            result.IsSuccess.Should().BeFalse();
            reporter.Diagnostics.Should().ContainSingle();
            reporter.Diagnostics[0].Kind.Should().Be(FormulaDiagnosticKind.ParseError);
            reporter.Diagnostics[0].Error.Code.Should().Be(FormulaErrorCode.ParseError);
            reporter.Diagnostics[0].Error.Span.Should().NotBeNull();
        }

        /// <summary>
        /// Проверяет отправку диагностики при ошибке вычисления формулы.
        /// </summary>
        [Fact]
        public void Evaluate_Reports_Evaluation_Error_Diagnostics()
        {
            var evaluator = CreateEvaluator();
            var reporter = new CapturingFormulaDiagnosticsReporter();
            var context = FormulaEngineTestContextFactory.Create();
            context = new FormulaExecutionContext
            {
                SystemBaseWorkingTree = context.SystemBaseWorkingTree,
                TreeLeaveResolver = context.TreeLeaveResolver,
                RepositoryService = context.RepositoryService,
                NotificationService = context.NotificationService,
                DiagnosticsReporter = reporter
            };

            var result = evaluator.Evaluate("=UNKNOWN()", context);

            result.IsSuccess.Should().BeFalse();
            reporter.Diagnostics.Should().ContainSingle();
            reporter.Diagnostics[0].Kind.Should().Be(FormulaDiagnosticKind.EvaluationError);
            reporter.Diagnostics[0].Error.Code.Should().Be(FormulaErrorCode.UnknownFunction);
        }

        /// <summary>
        /// Проверяет, что начальные пробелы перед маркером формулы не выключают вычисление формулы.
        /// </summary>
        [Fact]
        public void Evaluate_Allows_Leading_Whitespace_Before_Formula_Marker()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "СЛОЖИТЬ",
                (_, arguments) => FormulaResult.Success(
                    (double)arguments[0].Value! + (double)arguments[1].Value!,
                    SystemBaseType.NUMERIC),
                "+"));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("  =1+2", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(3d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет вызов зарегистрированной функции с вычисленными аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Calls_Registered_Function()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "ПАРА",
                (_, arguments) => FormulaResult.Success(arguments.Count, SystemBaseType.INTEGER)));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("=ПАРА(1;\"x\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет, что бинарный оператор вычисляется через alias той же зарегистрированной формулы.
        /// </summary>
        [Fact]
        public void Evaluate_Routes_Binary_Operator_Through_Formula_Alias()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "СЛОЖИТЬ",
                (_, arguments) => FormulaResult.Success(
                    (double)arguments[0].Value! + (double)arguments[1].Value!,
                    SystemBaseType.NUMERIC),
                "+"));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("=1+2", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(3d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет, что успешный результат формулы материализуется в системный лист и переиспользуется повторно.
        /// </summary>
        [Fact]
        public void Evaluate_Materializes_Result_To_SystemBase_Leave()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "СЛОЖИТЬ",
                (_, arguments) => FormulaResult.Success(
                    (double)arguments[0].Value! + (double)arguments[1].Value!,
                    SystemBaseType.NUMERIC),
                "+"));
            var evaluator = new FormulaAstEvaluator(registry);
            var context = FormulaEngineTestContextFactory.Create();

            var first = evaluator.Evaluate("=1+2", context);
            var second = evaluator.Evaluate("=1+2", context);

            first.IsSuccess.Should().BeTrue();
            first.TreeLeave.Should().BeOfType<SystemBaseTreeLeaveModel>()
                .Which.StringValue.Should().Be("3");
            second.TreeLeave.Should().BeSameAs(first.TreeLeave);
        }

        /// <summary>
        /// Проверяет короткое вычисление условного оператора.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Evaluates_Only_Selected_Branch()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "ИСТИНА",
                (_, _) => FormulaResult.Success(true, SystemBaseType.BOOL)));
            registry.Register(CreateFormula(
                "ОШИБКА",
                (_, _) => FormulaResult.Failure(new FormulaError
                {
                    Code = FormulaErrorCode.NotImplemented,
                    Message = "Эта ветка не должна вычисляться."
                })));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("=ИСТИНА()?1:ОШИБКА()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1d);
        }

        /// <summary>
        /// Проверяет ошибку типа, если условный оператор получил не BOOL.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Returns_TypeMismatch_For_Non_Boolean_Condition()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=1?\"Да\":\"Нет\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет ошибку поиска неизвестной функции.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_UnknownFunction_For_Unregistered_Function()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=НЕТ()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
            result.Error.FunctionOrOperator.Should().Be("НЕТ");
        }

        /// <summary>
        /// Проверяет, что UUID-ссылка возвращает сам обычный TreeLeaveModel.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Returns_Regular_TreeLeave_Instance()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"=[{treeLeave.Uuid}]", new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree
            });

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(treeLeave);
            result.TreeLeave.Should().BeSameAs(treeLeave);
            result.ValueType.Should().Be(SystemBaseType.USER_DEFINED);
        }

        /// <summary>
        /// Проверяет ошибку поиска UUID-ссылки в рабочем дереве.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Returns_NotFound_When_Leave_Is_Missing()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"=[{Guid.NewGuid()}]", new FormulaExecutionContext
            {
                WorkingTree = new FakeWorkingTreeModel()
            });

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TreeLeaveNotFound);
        }

        /// <summary>
        /// Проверяет, что UUID-ссылка вычисляется через зарегистрированную формулу ЛИСТ.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Requires_List_Formula()
        {
            var evaluator = new FormulaAstEvaluator(new FormulaRegistry());

            var result = evaluator.Evaluate($"=[{Guid.NewGuid()}]", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
            result.Error.FunctionOrOperator.Should().Be("ЛИСТ");
        }

        /// <summary>
        /// Проверяет получение свойства Name через объектную функцию СВОЙСТВО.
        /// </summary>
        [Fact]
        public void Evaluate_Property_Object_Method_Returns_TreeLeave_Name()
        {
            var treeLeave = CreateTreeLeave();
            treeLeave.Name = "Лист для формулы";
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].СВОЙСТВО(Name)",
                CreateMaterializingContext(treeLeave));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Лист для формулы");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет получение свойства CreatedBy через mapping к AuditInfo.
        /// </summary>
        [Fact]
        public void Evaluate_Property_Object_Method_Returns_Audit_CreatedBy()
        {
            var treeLeave = CreateTreeLeave();
            treeLeave.AuditInfo.CreatedBy = "formula-user";
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].СВОЙСТВО(CreatedBy)",
                CreateMaterializingContext(treeLeave));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("formula-user");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет получение системного значения атрибута по названию.
        /// </summary>
        [Fact]
        public void Evaluate_Attribute_Object_Method_Returns_SystemBase_Attribute_Value()
        {
            var treeLeave = CreateTreeLeave();
            var context = CreateMaterializingContext(treeLeave);
            var value = CreateSystemBaseLeave(context, SystemBaseType.STRING, "Красный");
            CreateInheritedAttribute(treeLeave, "Цвет", value);
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].АТРИБУТ(\"Цвет\")",
                context);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Красный");
            result.ValueType.Should().Be(SystemBaseType.STRING);
            result.TreeLeave.Should().BeSameAs(value);
        }

        /// <summary>
        /// Проверяет получение значения атрибута текущего элемента по относительной ссылке.
        /// </summary>
        [Fact]
        public void Evaluate_Relative_Attribute_Function_Returns_Current_Owner_Attribute_Value()
        {
            var treeLeave = CreateTreeLeave();
            var context = CreateMaterializingContext(treeLeave);
            context = new FormulaExecutionContext
            {
                WorkingTree = context.WorkingTree,
                TreeLeaveResolver = context.TreeLeaveResolver,
                SystemBaseWorkingTree = context.SystemBaseWorkingTree,
                CurrentAttributeOwner = treeLeave.ParentNode,
                RepositoryService = context.RepositoryService,
                NotificationService = context.NotificationService,
                Items = context.Items
            };
            var value = CreateSystemBaseLeave(context, SystemBaseType.STRING, "Красный");
            CreateInheritedAttribute(treeLeave, "Цвет", value);
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=АТРИБУТ(\"Цвет\")", context);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Красный");
            result.ValueType.Should().Be(SystemBaseType.STRING);
            result.TreeLeave.Should().BeSameAs(value);
        }

        /// <summary>
        /// Проверяет ошибку, если атрибут с указанным названием не найден.
        /// </summary>
        [Fact]
        public void Evaluate_Attribute_Object_Method_Returns_AttributeNotFound_For_Missing_Attribute()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].АТРИБУТ(\"Цвет\")",
                CreateMaterializingContext(treeLeave));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.AttributeNotFound);
            result.Error.FunctionOrOperator.Should().Be("АТРИБУТ");
        }

        /// <summary>
        /// Проверяет ошибку, если у листа найдено несколько атрибутов с одинаковым названием.
        /// </summary>
        [Fact]
        public void Evaluate_Attribute_Object_Method_Returns_AttributeDuplicate_For_Duplicate_Attributes()
        {
            var treeLeave = CreateTreeLeave();
            var context = CreateMaterializingContext(treeLeave);
            var value = CreateSystemBaseLeave(context, SystemBaseType.STRING, "Красный");
            CreateInheritedAttribute(treeLeave, "Цвет", value);
            CreateInheritedAttribute(treeLeave, "Цвет", value);
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].АТРИБУТ(\"Цвет\")",
                context);

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.AttributeDuplicate);
            result.Error.FunctionOrOperator.Should().Be("АТРИБУТ");
        }

        /// <summary>
        /// Проверяет ошибку типа, если АТРИБУТ вызван не от листа дерева.
        /// </summary>
        [Fact]
        public void Evaluate_Attribute_Object_Method_Returns_TypeMismatch_For_Non_TreeLeave_Target()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=\"text\".АТРИБУТ(\"Цвет\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет ошибку типа, если имя атрибута передано не строковым литералом.
        /// </summary>
        [Fact]
        public void Evaluate_Attribute_Object_Method_Returns_TypeMismatch_For_Non_Text_Attribute_Name()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=[{treeLeave.Uuid}].АТРИБУТ(Цвет)",
                CreateMaterializingContext(treeLeave));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
            result.Error.FunctionOrOperator.Should().Be("АТРИБУТ");
        }

        /// <summary>
        /// Проверяет ошибку для свойства вне whitelist.
        /// </summary>
        [Fact]
        public void Evaluate_Property_Object_Method_Returns_PropertyNotFound_For_Unknown_Property()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"=[{treeLeave.Uuid}].СВОЙСТВО(Unknown)", new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree
            });

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.PropertyNotFound);
            result.Error.FunctionOrOperator.Should().Be("СВОЙСТВО");
        }

        /// <summary>
        /// Проверяет ошибку типа, если СВОЙСТВО вызвано не от листа дерева.
        /// </summary>
        [Fact]
        public void Evaluate_Property_Object_Method_Returns_TypeMismatch_For_Non_TreeLeave_Target()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=\"text\".СВОЙСТВО(Name)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет зарезервированный маршрут неизвестных объектных функций до реализации следующих подзадач.
        /// </summary>
        [Fact]
        public void Evaluate_Object_Method_Call_Returns_NotImplemented_For_Unknown_Method()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"=[{treeLeave.Uuid}].НЕИЗВЕСТНО()", new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree
            });

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
            result.Error.FunctionOrOperator.Should().Be("НЕИЗВЕСТНО");
        }

        /// <summary>
        /// Создает вычислитель с пустым реестром формул.
        /// </summary>
        /// <returns>Вычислитель AST формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new TreeLeaveFormulaProvider());

            return new FormulaAstEvaluator(registry);
        }

        /// <summary>
        /// Создает контекст с рабочим деревом тестового листа и системным деревом для материализации результата.
        /// </summary>
        /// <param name="treeLeave">Лист, который должен быть доступен по UUID в формуле.</param>
        /// <returns>Контекст вычисления формулы.</returns>
        private static FormulaExecutionContext CreateMaterializingContext(TreeLeaveModel treeLeave)
        {
            var context = FormulaEngineTestContextFactory.Create();

            return new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree,
                TreeLeaveResolver = new WorkingTreeTreeLeaveResolver(treeLeave.OwningWorkingTree),
                SystemBaseWorkingTree = context.SystemBaseWorkingTree,
                RepositoryService = context.RepositoryService,
                NotificationService = context.NotificationService,
                Items = context.Items
            };
        }

        /// <summary>
        /// Создает унаследованный атрибут листа с одиночным значением.
        /// </summary>
        /// <param name="owner">Лист, которому доступен атрибут.</param>
        /// <param name="name">Название атрибута.</param>
        /// <param name="value">Значение атрибута.</param>
        /// <returns>Созданный атрибут.</returns>
        private static Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes.ElementAttributeModel CreateInheritedAttribute(
            TreeLeaveModel owner,
            string name,
            TreeLeaveModel value)
        {
            var uuid = Guid.NewGuid();
            var attribute = new Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes.ElementAttributeModel(
                uuid,
                owner.ParentNode,
                uuid,
                owner.ParentNode,
                owner.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes.ElementAttributeModel>())
            {
                Name = name,
                ValueType = value.ParentNode,
                Value = value
            };

            return attribute;
        }

        /// <summary>
        /// Создает системный лист указанного типа в системном дереве контекста.
        /// </summary>
        /// <param name="context">Контекст с системным рабочим деревом.</param>
        /// <param name="type">Системный тип листа.</param>
        /// <param name="stringValue">Строковое значение системного листа.</param>
        /// <returns>Созданный системный лист.</returns>
        private static SystemBaseTreeLeaveModel CreateSystemBaseLeave(
            FormulaExecutionContext context,
            SystemBaseType type,
            string stringValue)
        {
            var node = context.SystemBaseWorkingTree!.ContentNodes
                .OfType<SystemBaseTreeNodeModel>()
                .Single(x => x.SystemBaseType == type);

            var result = new SystemBaseTreeLeaveModel(
                Guid.NewGuid(),
                node,
                context.SystemBaseWorkingTree,
                type,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeLeaveModel>());
            result.StringValue = stringValue;
            return result;
        }

        /// <summary>
        /// Создает тестовое определение формулы.
        /// </summary>
        /// <param name="name">Имя формулы.</param>
        /// <param name="evaluator">Делегат вычисления формулы.</param>
        /// <param name="aliases">Псевдонимы формулы.</param>
        /// <returns>Тестовое определение формулы.</returns>
        private static FormulaDefinition CreateFormula(
            string name,
            FormulaEvaluator evaluator,
            params string[] aliases)
        {
            return new FormulaDefinition
            {
                Name = name,
                Aliases = aliases,
                Description = "Тестовая формула",
                Evaluator = evaluator
            };
        }

        /// <summary>
        /// Создает обычный пользовательский лист дерева.
        /// </summary>
        /// <returns>Тестовый лист дерева.</returns>
        private static TreeLeaveModel CreateTreeLeave()
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.NewGuid(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());

            return new TreeLeaveModel(
                Guid.NewGuid(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
        }

        private sealed class CapturingFormulaDiagnosticsReporter : IFormulaDiagnosticsReporter
        {
            public List<FormulaDiagnostic> Diagnostics { get; } = new();

            public void Report(FormulaDiagnostic diagnostic)
            {
                Diagnostics.Add(diagnostic);
            }
        }
    }
}
