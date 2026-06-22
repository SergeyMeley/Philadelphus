using System.Collections.Generic;
using System.Linq;

using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления выбора темы оформления (выпадающий список на вкладке ленты «Вид»).
    /// При выборе режим применяется и сохраняется через <see cref="IThemeService"/>.
    /// </summary>
    public class ThemeSettingsVM : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private ThemeModeOptionVM _selectedMode;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ThemeSettingsVM" />.
        /// </summary>
        /// <param name="themeService">Сервис управления темой.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ThemeSettingsVM(IThemeService themeService)
        {
            ArgumentNullException.ThrowIfNull(themeService);

            _themeService = themeService;

            Modes = new List<ThemeModeOptionVM>
            {
                new(AppThemeMode.Light, "Светлая"),
                new(AppThemeMode.Dark, "Тёмная"),
                new(AppThemeMode.System, "Как в системе"),
            };

            _selectedMode = Modes.FirstOrDefault(mode => mode.Mode == _themeService.CurrentMode)
                ?? Modes.First();
        }

        /// <summary>
        /// Доступные режимы темы.
        /// </summary>
        public IReadOnlyList<ThemeModeOptionVM> Modes { get; }

        /// <summary>
        /// Выбранный режим темы. При изменении тема применяется и сохраняется.
        /// </summary>
        public ThemeModeOptionVM SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (value == null || ReferenceEquals(_selectedMode, value))
                {
                    return;
                }

                _selectedMode = value;
                OnPropertyChanged();
                _themeService.SetMode(value.Mode);
            }
        }
    }

    /// <summary>
    /// Элемент выбора темы: режим + отображаемое имя.
    /// </summary>
    public class ThemeModeOptionVM
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ThemeModeOptionVM" />.
        /// </summary>
        /// <param name="mode">Режим темы.</param>
        /// <param name="displayName">Отображаемое имя.</param>
        public ThemeModeOptionVM(AppThemeMode mode, string displayName)
        {
            Mode = mode;
            DisplayName = displayName;
        }

        /// <summary>
        /// Режим темы.
        /// </summary>
        public AppThemeMode Mode { get; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        public string DisplayName { get; }

        /// <inheritdoc />
        public override string ToString() => DisplayName;
    }
}
