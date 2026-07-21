using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты базовых контрактов и реестра Formula Engine.
    /// </summary>
    public class FormulaRegistryTests
    {
        /// <summary>
        /// Проверяет поиск зарегистрированной формулы по имени и псевдониму.
        /// </summary>
        [Fact]
        public void Register_Allows_Resolve_By_Name_And_Alias()
        {
            var registry = new FormulaRegistry();
            var formula = CreateFormula("СУММ", "+");

            registry.Register(formula);

            registry.TryResolve("СУММ", out var byName).Should().BeTrue();
            byName.Should().BeSameAs(formula);

            registry.TryResolve("+", out var byAlias).Should().BeTrue();
            byAlias.Should().BeSameAs(formula);
        }

        /// <summary>
        /// Проверяет запрет повторной регистрации имени или псевдонима.
        /// </summary>
        [Fact]
        public void Register_Blocks_Duplicate_Name_Or_Alias()
        {
            var registry = new FormulaRegistry();

            registry.Register(CreateFormula("СУММ", "+"));

            var act = () => registry.Register(CreateFormula("PLUS", "+"));

            act.Should().Throw<FormulaRegistrationException>()
                .WithMessage("*+*");
        }

        /// <summary>
        /// Проверяет запрет конфликта основного имени с уже занятым псевдонимом.
        /// </summary>
        [Fact]
        public void Register_Blocks_Name_Conflict_With_Existing_Alias()
        {
            var registry = new FormulaRegistry();

            registry.Register(CreateFormula("СУММ", "+"));

            var act = () => registry.Register(CreateFormula("+"));

            act.Should().Throw<FormulaRegistrationException>()
                .WithMessage("*+*");
        }

        /// <summary>
        /// Проверяет результат поиска неизвестной формулы.
        /// </summary>
        [Fact]
        public void Resolve_Returns_UnknownFunction_Error_When_Formula_Is_Not_Registered()
        {
            var registry = new FormulaRegistry();

            var result = registry.Resolve("НЕТ");

            result.IsResolved.Should().BeFalse();
            result.Formula.Should().BeNull();
            result.Error.Should().NotBeNull();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
            result.Error.Code.GetDisplayName().Should().Be("#НЕИЗВЕСТНАЯ_ФУНКЦИЯ");
            result.Error.FunctionOrOperator.Should().Be("НЕТ");
        }

        /// <summary>
        /// Проверяет регистрацию всех формул из поставщика.
        /// </summary>
        [Fact]
        public void RegisterProvider_Registers_All_Provider_Formulas()
        {
            var registry = new FormulaRegistry();

            registry.RegisterProvider(new TestFormulaProvider(
                CreateFormula("СУММ", "+"),
                CreateFormula("ПРОИЗВ", "*")));

            registry.TryResolve("+", out _).Should().BeTrue();
            registry.TryResolve("*", out _).Should().BeTrue();
            registry.Formulas.Should().HaveCount(2);
        }

        /// <summary>
        /// Проверяет, что результат с листом хранит сам экземпляр <see cref="TreeLeaveModel" />.
        /// </summary>
        [Fact]
        public void FormulaResult_FromTreeLeave_Preserves_TreeLeave_Instance()
        {
            var treeLeave = CreateTreeLeave();

            var result = FormulaResult.FromTreeLeave(treeLeave);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(treeLeave);
            result.TreeLeave.Should().BeSameAs(treeLeave);
            result.ValueType.Should().Be(treeLeave.SystemBaseType);
        }

        /// <summary>
        /// Проверяет русское отображаемое значение кода ошибки.
        /// </summary>
        [Fact]
        public void FormulaErrorCode_Uses_Russian_Display_Name()
        {
            FormulaErrorCode.ParseError.GetDisplayName().Should().Be("#ОШИБКА_ПАРСИНГА");
        }

        /// <summary>
        /// Проверяет, что metadata системных формул содержит достаточно данных для подсказок редактора.
        /// </summary>
        [Fact]
        public void System_Formulas_Expose_Editor_Metadata()
        {
            var formulas = new IFormulaProvider[]
                {
                    new ArithmeticFormulaProvider(),
                    new ComparisonFormulaProvider(),
                    new TextFormulaProvider(),
                    new ConditionalFormulaProvider(),
                    new TreeLeaveFormulaProvider(),
                    new CollectionFormulaProvider()
                }
                .SelectMany(provider => provider.GetFormulas())
                .ToList();

            formulas.Should().NotBeEmpty();
            formulas.Should().OnlyContain(formula =>
                string.IsNullOrWhiteSpace(formula.Category) == false
                && formula.Examples.Count > 0);

            formulas.Single(formula => formula.Name == "ЕСЛИ")
                .Arguments
                .Where(argument => argument.IsRequired == false)
                .Select(argument => argument.DefaultValue)
                .Should()
                .Equal(true, false);

            formulas.Single(formula => formula.Name == "СУММ")
                .ResultType
                .Should()
                .Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет, что внешние формулы могут предоставлять такую же metadata редактора, как системные.
        /// </summary>
        [Fact]
        public void External_Formulas_Can_Expose_Editor_Metadata()
        {
            var formula = new FormulaDefinition
            {
                Name = "PLUGIN_HELPER",
                Aliases = ["PLUGIN_HELPER_ALIAS"],
                Description = "Plugin formula used by editor metadata tests.",
                Category = "Плагин",
                ResultType = SystemBaseType.STRING,
                Examples = ["=PLUGIN_HELPER()"],
                Evaluator = (_, _) => FormulaResult.Success("ok", SystemBaseType.STRING)
            };

            formula.Category.Should().Be("Плагин");
            formula.ResultType.Should().Be(SystemBaseType.STRING);
            formula.Examples.Should().Contain("=PLUGIN_HELPER()");
        }

        /// <summary>
        /// Создает тестовое определение формулы с указанным именем и псевдонимами.
        /// </summary>
        /// <param name="name">Имя формулы.</param>
        /// <param name="aliases">Псевдонимы формулы.</param>
        /// <returns>Тестовое определение формулы.</returns>
        private static FormulaDefinition CreateFormula(string name, params string[] aliases)
        {
            return new FormulaDefinition
            {
                Name = name,
                Aliases = aliases,
                Description = "Тестовая формула",
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "значение",
                        ExpectedType = SystemBaseType.NUMERIC
                    }
                ],
                Evaluator = (_, _) => FormulaResult.Success(1d, SystemBaseType.NUMERIC)
            };
        }

        /// <summary>
        /// Создает минимальный тестовый лист дерева для проверки результата формулы.
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

        /// <summary>
        /// Тестовый поставщик формул для проверки пакетной регистрации.
        /// </summary>
        private sealed class TestFormulaProvider : IFormulaProvider
        {
            /// <summary>
            /// Формулы, возвращаемые тестовым поставщиком.
            /// </summary>
            private readonly IReadOnlyList<FormulaDefinition> _formulas;

            /// <summary>
            /// Инициализирует тестовый поставщик формул.
            /// </summary>
            /// <param name="formulas">Формулы, которые должен вернуть поставщик.</param>
            public TestFormulaProvider(params FormulaDefinition[] formulas)
            {
                _formulas = formulas;
            }

            /// <summary>
            /// Возвращает формулы тестового поставщика.
            /// </summary>
            /// <returns>Коллекция тестовых формул.</returns>
            public IEnumerable<FormulaDefinition> GetFormulas()
            {
                return _formulas;
            }
        }
    }
}
