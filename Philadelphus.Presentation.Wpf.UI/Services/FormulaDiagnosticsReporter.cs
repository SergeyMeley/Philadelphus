using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// Sends Formula Engine diagnostics to the application notification service and Serilog.
    /// </summary>
    public sealed class FormulaDiagnosticsReporter : IFormulaDiagnosticsReporter
    {
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        public FormulaDiagnosticsReporter(
            ILogger logger,
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);

            _logger = logger.ForContext("SourceContext", nameof(FormulaDiagnosticsReporter));
            _notificationService = notificationService;
        }

        public void Report(FormulaDiagnostic diagnostic)
        {
            ArgumentNullException.ThrowIfNull(diagnostic);

            var message = FormatMessage(diagnostic.Error);
            var criticalLevel = diagnostic.Kind is FormulaDiagnosticKind.PluginLoadError
                    or FormulaDiagnosticKind.UnexpectedException
                ? NotificationCriticalLevelModel.Error
                : NotificationCriticalLevelModel.Warning;

            LogDiagnostic(diagnostic, message);

            _notificationService.SendTextMessage<FormulaDiagnosticsReporter>(
                message,
                criticalLevel: criticalLevel);
        }

        private void LogDiagnostic(FormulaDiagnostic diagnostic, string message)
        {
            if (diagnostic.Kind is FormulaDiagnosticKind.PluginLoadError
                    or FormulaDiagnosticKind.UnexpectedException
                || diagnostic.Error.Exception is not null)
            {
                _logger.Error(diagnostic.Error.Exception, "Formula Engine diagnostic: {Message}", message);
                return;
            }

            _logger.Warning("Formula Engine diagnostic: {Message}", message);
        }

        private static string FormatMessage(FormulaError error)
        {
            var parts = new List<string>
            {
                $"{error.Code.GetDisplayName()}: {error.Message}"
            };

            if (error.Span is { } span)
            {
                parts.Add($"Позиция: {span.Start + 1}, длина: {span.Length}");
            }

            if (string.IsNullOrWhiteSpace(error.FunctionOrOperator) == false)
            {
                parts.Add($"Функция/оператор: {error.FunctionOrOperator}");
            }

            return string.Join(". ", parts);
        }
    }
}
