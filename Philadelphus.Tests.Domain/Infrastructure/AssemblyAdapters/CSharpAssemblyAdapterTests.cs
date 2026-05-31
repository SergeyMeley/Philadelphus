using System.Reflection;
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Infrastructure.AssemblyAdapters;
using Philadelphus.Infrastructure.AssemblyAdapters.CSharp;
using Philadelphus.Tests.Domain.FormulaEngine;

namespace Philadelphus.Tests.Domain.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Tests for loading trusted C# assemblies through the universal adapter.
    /// </summary>
    public class CSharpAssemblyAdapterTests
    {
        [Fact]
        public void Load_Returns_Module_For_CSharp_Assembly_File()
        {
            var adapter = new CSharpAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Assembly.GetExecutingAssembly().Location
            });

            result.IsSuccess.Should().BeTrue();
            result.Modules.Should().ContainSingle();
            result.Modules[0].Language.Should().Be(AssemblyAdapterLanguage.CSharp);
            result.Modules[0].LoadedArtifact.Should().BeAssignableTo<Assembly>();
        }

        [Fact]
        public void CreateInstances_Discovers_Formula_Provider_By_Contract()
        {
            var adapter = new CSharpAssemblyAdapter();
            var loadResult = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Assembly.GetExecutingAssembly().Location
            });

            var providerResult = adapter.CreateInstances<IFormulaProvider>(loadResult);
            var registry = new FormulaRegistry();
            foreach (var provider in providerResult.Instances)
            {
                registry.RegisterProvider(provider);
            }

            var evaluator = new FormulaAstEvaluator(registry);
            var result = evaluator.Evaluate("=TEST_PLUGIN_VALUE()", FormulaEngineTestContextFactory.Create());

            providerResult.IsSuccess.Should().BeTrue();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(101L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        [Fact]
        public void Load_Returns_Error_For_Missing_File()
        {
            var adapter = new CSharpAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dll")
            });

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Language.Should().Be(AssemblyAdapterLanguage.CSharp);
        }
    }

    public sealed class TestExternalFormulaProvider : IFormulaProvider
    {
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "TEST_PLUGIN_VALUE",
                Description = "Test formula provider loaded from a C# assembly.",
                Evaluator = (_, _) => FormulaResult.Success(101L, SystemBaseType.INTEGER)
            };
        }
    }
}
