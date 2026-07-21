using Microsoft.Extensions.DependencyInjection;

using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Presentation.Services;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Startup
{
    internal static class FormulaEngineServiceCollectionExtensions
    {
        public static void RegisterFormulaEngine(IServiceCollection services)
        {
            services.AddSingleton<IFormulaProvider, ArithmeticFormulaProvider>();
            services.AddSingleton<IFormulaProvider, ComparisonFormulaProvider>();
            services.AddSingleton<IFormulaProvider, TextFormulaProvider>();
            services.AddSingleton<IFormulaProvider, ConditionalFormulaProvider>();
            services.AddSingleton<IFormulaProvider, TreeLeaveFormulaProvider>();
            services.AddSingleton<IFormulaProvider, CollectionFormulaProvider>();

            services.AddSingleton(serviceProvider =>
            {
                var registry = new FormulaRegistry();
                foreach (var provider in serviceProvider.GetServices<IFormulaProvider>())
                {
                    registry.RegisterProvider(provider);
                }

                return registry;
            });

            services.AddSingleton<FormulaAstEvaluator>();
            services.AddSingleton<IFormulaDiagnosticsReporter, FormulaDiagnosticsReporter>();
        }
    }
}
