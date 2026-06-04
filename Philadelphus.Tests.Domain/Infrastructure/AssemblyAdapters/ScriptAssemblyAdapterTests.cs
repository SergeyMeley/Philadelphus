using FluentAssertions;
using Philadelphus.Infrastructure.AssemblyAdapters;
using Philadelphus.Infrastructure.AssemblyAdapters.JavaScript;
using Philadelphus.Infrastructure.AssemblyAdapters.Python;

namespace Philadelphus.Tests.Domain.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Тесты контрактов зарезервированных скриптовых адаптеров.
    /// </summary>
    public class ScriptAssemblyAdapterTests
    {
        [Fact]
        public void Python_Adapter_Is_Reserved_Without_Runtime_Loading()
        {
            var adapter = new PythonScriptAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = "formula.py"
            });

            adapter.Language.Should().Be(AssemblyAdapterLanguage.Python);
            adapter.SupportedFileExtensions.Should().Contain(".py");
            result.Modules.Should().BeEmpty();
            result.Errors.Should().ContainSingle();
        }

        [Fact]
        public void JavaScript_Adapter_Is_Reserved_Without_Runtime_Loading()
        {
            var adapter = new JavaScriptAssemblyAdapter();

            var result = adapter.Load(new AssemblyAdapterLoadRequest
            {
                Path = "formula.js"
            });

            adapter.Language.Should().Be(AssemblyAdapterLanguage.JavaScript);
            adapter.SupportedFileExtensions.Should().Contain([".js", ".mjs", ".cjs"]);
            result.Modules.Should().BeEmpty();
            result.Errors.Should().ContainSingle();
        }
    }
}
