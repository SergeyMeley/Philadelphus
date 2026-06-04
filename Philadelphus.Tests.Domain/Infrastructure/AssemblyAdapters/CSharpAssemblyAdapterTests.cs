using System.Reflection;
using System.Security.Cryptography;
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Infrastructure.AssemblyAdapters;
using Philadelphus.Infrastructure.AssemblyAdapters.CSharp;
using Philadelphus.Tests.Domain.FormulaEngine;

namespace Philadelphus.Tests.Domain.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Тесты загрузки доверенных C#-сборок через универсальный адаптер.
    /// </summary>
    public class CSharpAssemblyAdapterTests
    {
        [Fact]
        public void Load_Returns_Module_For_CSharp_Assembly_File()
        {
            var adapter = new CSharpAssemblyAdapter();
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var result = adapter.Load(CreateTrustedLoadRequest(assemblyPath));

            result.IsSuccess.Should().BeTrue();
            result.Modules.Should().ContainSingle();
            result.Modules[0].Language.Should().Be(AssemblyAdapterLanguage.CSharp);
            result.Modules[0].LoadedArtifact.Should().BeAssignableTo<Assembly>();
        }

        [Fact]
        public void CreateInstances_Discovers_Formula_Provider_By_Contract()
        {
            var adapter = new CSharpAssemblyAdapter();
            var loadResult = adapter.Load(CreateTrustedLoadRequest(Assembly.GetExecutingAssembly().Location));

            var providerResult = adapter.CreateInstances<IFormulaProvider>(loadResult);
            var registry = new FormulaRegistry();
            registry.RegisterProvider(providerResult.Instances.OfType<TestExternalFormulaProvider>().Single());

            var evaluator = new FormulaAstEvaluator(registry);
            var result = evaluator.Evaluate("=TEST_PLUGIN_VALUE()", FormulaEngineTestContextFactory.Create());

            providerResult.IsSuccess.Should().BeTrue();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(101L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        [Fact]
        public void CreateInstances_Detects_Duplicate_Formula_Name_Or_Alias_When_Registering_Plugin()
        {
            var adapter = new CSharpAssemblyAdapter();
            var loadResult = adapter.Load(CreateTrustedLoadRequest(Assembly.GetExecutingAssembly().Location));

            var providerResult = adapter.CreateInstances<IFormulaProvider>(loadResult);
            var registry = new FormulaRegistry();
            registry.RegisterProvider(providerResult.Instances.OfType<TestExternalFormulaProvider>().Single());

            var act = () => registry.RegisterProvider(
                providerResult.Instances.OfType<TestConflictingExternalFormulaProvider>().Single());

            act.Should().Throw<FormulaRegistrationException>();
        }

        [Fact]
        public void Load_Returns_Error_For_Broken_Dll()
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dll");
            File.WriteAllText(path, "not a real assembly");

            try
            {
                var adapter = new CSharpAssemblyAdapter();

                var result = adapter.Load(new AssemblyAdapterLoadRequest
                {
                    Path = path,
                    AllowedSha256Hashes = [CalculateSha256Hash(path)]
                });

                result.IsSuccess.Should().BeFalse();
                result.Errors.Should().ContainSingle();
                result.Errors[0].SourcePath.Should().Be(path);
                result.Errors[0].Exception.Should().NotBeNull();
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Load_Returns_Error_For_Missing_File()
        {
            var adapter = new CSharpAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dll"),
                AllowedSha256Hashes = [new string('0', 64)]
            });

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Language.Should().Be(AssemblyAdapterLanguage.CSharp);
        }

        [Fact]
        public void Load_Rejects_CSharp_Assembly_Without_Sha256_Allowlist()
        {
            var adapter = new CSharpAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Assembly.GetExecutingAssembly().Location
            });

            result.IsSuccess.Should().BeFalse();
            result.Modules.Should().BeEmpty();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Contain("список разрешенных SHA-256");
        }

        [Fact]
        public void Load_Rejects_CSharp_Assembly_When_Sha256_Is_Not_Allowed()
        {
            var adapter = new CSharpAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = Assembly.GetExecutingAssembly().Location,
                AllowedSha256Hashes = [new string('0', 64)]
            });

            result.IsSuccess.Should().BeFalse();
            result.Modules.Should().BeEmpty();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Contain("отсутствует в списке разрешенных SHA-256");
        }

        [Fact]
        public void Evaluate_Returns_UnknownFunction_For_Not_Loaded_Plugin_Formula()
        {
            var evaluator = new FormulaAstEvaluator(new FormulaRegistry());

            var result = evaluator.Evaluate("=TEST_PLUGIN_VALUE()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
        }

        private static AssemblyAdapterLoadRequest CreateTrustedLoadRequest(string path)
        {
            return new AssemblyAdapterLoadRequest
            {
                Path = path,
                AllowedSha256Hashes = [CalculateSha256Hash(path)]
            };
        }

        private static string CalculateSha256Hash(string path)
        {
            using var file = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(file)).ToLowerInvariant();
        }
    }

    public sealed class TestExternalFormulaProvider : IFormulaProvider
    {
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "TEST_PLUGIN_VALUE",
                Aliases = ["TEST_PLUGIN_ALIAS"],
                Description = "Тестовый поставщик формул, загруженный из C#-сборки.",
                Evaluator = (_, _) => FormulaResult.Success(101L, SystemBaseType.INTEGER)
            };
        }
    }

    public sealed class TestConflictingExternalFormulaProvider : IFormulaProvider
    {
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "TEST_PLUGIN_ALIAS_CONFLICT",
                Aliases = ["TEST_PLUGIN_VALUE"],
                Description = "Тестовый поставщик формул с конфликтующим псевдонимом.",
                Evaluator = (_, _) => FormulaResult.Success(202L, SystemBaseType.INTEGER)
            };
        }
    }
}
