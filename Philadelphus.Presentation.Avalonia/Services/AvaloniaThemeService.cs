using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

using global::Avalonia;
using global::Avalonia.Styling;
using global::Avalonia.Threading;

using Microsoft.Extensions.Options;

using Philadelphus.Presentation.Configurations;
using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.Services.Interfaces;

using Serilog;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IThemeService"/>: применяет выбранный режим к
    /// <see cref="Application.RequestedThemeVariant"/> и сохраняет его в appsettings.json
    /// (секция <c>AppearanceConfig:ThemeString</c>). Режим «Как в системе» — это
    /// <see cref="ThemeVariant.Default"/> (Avalonia сам следует теме ОС).
    /// Цвета конкретных тем настраиваются отдельно.
    /// </summary>
    public sealed class AvaloniaThemeService : IThemeService
    {
        private const string SettingsSection = nameof(AppearanceConfig);
        private const string ThemeKey = nameof(AppearanceConfig.ThemeString);
        private const string SettingsFileName = "appsettings.json";

        private AppThemeMode _currentMode;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AvaloniaThemeService" />.
        /// Сразу применяет сохранённый в конфигурации режим темы.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public AvaloniaThemeService(IOptions<AppearanceConfig> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            _currentMode = ParseMode(options.Value?.ThemeString);
            Apply(_currentMode);
        }

        /// <summary>
        /// Разбирает сохранённую строку темы в режим оформления.
        /// По умолчанию (пусто/неизвестно) — «Как в системе».
        /// </summary>
        private static AppThemeMode ParseMode(string? themeString)
        {
            return Enum.TryParse<AppThemeMode>(themeString, ignoreCase: true, out var mode)
                ? mode
                : AppThemeMode.System;
        }

        /// <inheritdoc />
        public AppThemeMode CurrentMode => _currentMode;

        /// <inheritdoc />
        public void SetMode(AppThemeMode mode)
        {
            _currentMode = mode;
            Apply(mode);
            Persist(mode);
        }

        /// <summary>
        /// Применяет режим темы к приложению (на UI-потоке).
        /// </summary>
        private static void Apply(AppThemeMode mode)
        {
            void Set()
            {
                var application = Application.Current;
                if (application == null)
                {
                    return;
                }

                application.RequestedThemeVariant = mode switch
                {
                    AppThemeMode.Light => ThemeVariant.Light,
                    AppThemeMode.Dark => ThemeVariant.Dark,
                    _ => ThemeVariant.Default, // Как в системе — Avalonia следует теме ОС.
                };
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                Set();
            }
            else
            {
                Dispatcher.UIThread.Post(Set);
            }
        }

        /// <summary>
        /// Сохраняет режим темы в appsettings.json, не затрагивая остальные настройки.
        /// </summary>
        private static void Persist(AppThemeMode mode)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
                if (File.Exists(path) == false)
                {
                    Log.Warning("Файл {File} не найден — тема не сохранена.", path);
                    return;
                }

                if (JsonNode.Parse(File.ReadAllText(path)) is not JsonObject root)
                {
                    return;
                }

                if (root[SettingsSection] is not JsonObject section)
                {
                    section = new JsonObject();
                    root[SettingsSection] = section;
                }

                section[ThemeKey] = mode.ToString();

                File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Не удалось сохранить тему в {File}.", SettingsFileName);
            }
        }
    }
}
