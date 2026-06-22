# Передача контекста: тёмная тема (Avalonia) — НЕ ЗАВЕРШЕНО

## Статус
Реализован каркас переключения темы. **Проблема: выпадающий список тем на ленте «Вид» не открывается.** Нужно починить список и перепроверить остальную реализацию. Сборку под net10.0 делает пользователь на Windows (в Linux-песочнице нет dotnet SDK).

## Что требовалось (исходная задача)
На панель инструментов (Ribbon) на вкладку **«Вид»** добавить группу **«Тема»** с выпадающим списком: **Светлая, Тёмная, Как в системе** (по умолчанию). Выбор сохраняется в `appsettings.json`. **Цвета конкретных тем — отдельным коммитом** (в этом коммите только селектор + сохранение + применение ThemeVariant).

## Постоянные правила проекта (действуют всегда)
- Avalonia-код максимально похож на WPF; «заодно»-улучшения — в тех-долг, не применять.
- Сохранять авторские комментарии WPF и переносить закомментированные блоки (адаптируя под Avalonia).
- WPF и Avalonia синхронны; правки shared-кода затрагивают оба — это ок для настоящих фиксов; на Windows поведение не меняется.
- Предпочитать дефолтную стилизацию Avalonia, а не микро-значения WPF.
- Вместо Expander для боковых панелей — ToggleButton + LayoutTransformControl + IsVisible↔IsChecked (как в LaunchWindow).
- **Всегда писать текст коммита без отдельного запроса.**
- Пользователь: кратко и по делу. Общение на русском.

## Ветка / папки
- Ветка: `feature/#65575392-avalonia`, рабочая папка `D:\MelSV_Projects\Philadelphus.General`.
- Сборка: фильтр решения `Philadelphus.Avalonia.slnf` (только Avalonia-граф, без WPF).

## Сделанные изменения (файлы)

1. **`Philadelphus.Core.Domain/Configurations/AppThemeMode.cs`** (новый) — enum `AppThemeMode { System=0, Light=1, Dark=2 }`.

2. **`Philadelphus.Core.Domain/Configurations/ApplicationSettingsConfig.cs`** — добавлены:
   - `public string ThemeString { get; set; }`
   - `[JsonIgnore] public AppThemeMode Theme` — парсит ThemeString (Enum.TryParse, ignoreCase), дефолт `System`.

3. **`Philadelphus.Presentation.Avalonia/appsettings.json`** — в секцию `ApplicationSettingsConfig` добавлен ключ `"ThemeString": "System"` (после RedisOptions). Имя секции именно `ApplicationSettingsConfig`.

4. **`Philadelphus.Presentation/Services/Interfaces/IThemeService.cs`** (новый) — `AppThemeMode CurrentMode { get; }`, `void SetMode(AppThemeMode mode)`.

5. **`Philadelphus.Presentation/ViewModels/ControlsVMs/ThemeSettingsVM.cs`** (новый) — `ViewModelBase`. Свойства:
   - `IReadOnlyList<ThemeModeOptionVM> Modes` = [Light «Светлая», Dark «Тёмная», System «Как в системе»].
   - `ThemeModeOptionVM SelectedMode` — при set вызывает `_themeService.SetMode(value.Mode)`.
   - Вложенный класс `ThemeModeOptionVM { AppThemeMode Mode; string DisplayName; ToString()=DisplayName }`.
   - Ctor(IThemeService); начальный SelectedMode = Modes по CurrentMode.

6. **`Philadelphus.Presentation/ViewModels/ControlsVMs/MainWindowVM.cs`** — добавлено:
   - поле `private readonly ThemeSettingsVM? _themeSettingsVM;`
   - свойство `public ThemeSettingsVM? ThemeSettingsVM { get => _themeSettingsVM; }`
   - в конце конструктора: `_themeSettingsVM = _serviceProvider.GetService<ThemeSettingsVM>();` (на WPF не зарегистрирован → null → Windows без изменений). **Важно:** изначально пробовал опциональный параметр конструктора `ThemeSettingsVM? themeSettingsVM = null` — заменено на GetService, т.к. MainWindowVM создаётся через `MainWindowVMFactory` → `ActivatorUtilities.CreateInstance`.

7. **`Philadelphus.Presentation.Avalonia/Services/AvaloniaThemeService.cs`** (новый) — `IThemeService`:
   - Ctor(IOptions<ApplicationSettingsConfig>): читает Theme, сразу `Apply`.
   - `Apply(mode)`: на UI-потоке (Dispatcher.UIThread.CheckAccess/Post) ставит `Application.Current.RequestedThemeVariant` = Light/Dark/`ThemeVariant.Default` (System).
   - `Persist(mode)`: читает `AppContext.BaseDirectory/appsettings.json` как JsonNode, пишет `ApplicationSettingsConfig:ThemeString = mode.ToString()`, сохраняет WriteIndented; try/catch + Serilog.

8. **`Philadelphus.Presentation.Avalonia/App.axaml.cs`**:
   - В ConfigureServices (перед `AddPhiladelphusPresentation()`): `services.AddSingleton<IThemeService, AvaloniaThemeService>();` и `services.AddTransient<ThemeSettingsVM>();`
   - В `InitializeAsync` (перед регистрацией окон): `_ = _host!.Services.GetRequiredService<IThemeService>();` — применяет сохранённую тему до показа окон.

9. **`Philadelphus.Presentation.Avalonia/App.axaml`** — `RequestedThemeVariant="Light"` → `RequestedThemeVariant="Default"`.

10. **`Philadelphus.Presentation.Avalonia/Views/Windows/MainWindow.axaml`** — закомментированная `<!-- <ribbon:RibbonTab Header="Вид" /> -->` заменена на:
```xml
<ribbon:RibbonTab Header="Вид" DataContext="{Binding ThemeSettingsVM}">
    <ribbon:RibbonTab.Groups>
        <ribbon:RibbonGroupBox Header="Тема">
            <StackPanel Spacing="2">
                <TextBlock Text="Тема оформления" />
                <ComboBox ItemsSource="{Binding Modes}"
                          SelectedItem="{Binding SelectedMode}"
                          MinWidth="160">
                    <ComboBox.ItemTemplate>
                        <DataTemplate><TextBlock Text="{Binding DisplayName}" /></DataTemplate>
                    </ComboBox.ItemTemplate>
                </ComboBox>
            </StackPanel>
        </ribbon:RibbonGroupBox>
    </ribbon:RibbonTab.Groups>
</ribbon:RibbonTab>
```

## ПРОБЛЕМА (что чинить)
Выпадающий список тем **не открывается**. Симптом по словам пользователя: «список не открывается» (ComboBox не реагирует на клик / либо вкладка «Вид» пустая).

### Анализ (не доведён до конца)
- Паттерн идентичен рабочим вкладкам: «Расширения» (`DataContext="{Binding ExtensionsControlVM}"` на RibbonTab + ComboBox) и «Формулы»→«Пересчёт» (ComboBox в RibbonGroupBox). Если те открываются — проблема в данных/привязке именно «Темы»; если нет — системная проблема popup'ов ComboBox в Ribbon.Avalonia 0.9.0.
- Разбор `ActivatorUtilities.CreateInstance`: оптимальный параметр-конструктор ДОЛЖЕН резолвиться из DI (GetService→default), т.е. ThemeSettingsVM скорее всего НЕ null. Поэтому склонялся к версии «popup в ribbon не открывается», но не подтвердил.
- **Нерешённый вопрос для пользователя (нужно задать в новом чате):** «Открываются ли ДРУГИЕ выпадающие списки на ленте — Формулы→Пересчёт формул, Расширения→выбор расширения?»
  - Если ДА → проблема специфична для «Темы» (проверить, что ThemeSettingsVM != null и Modes не пуст; пустой ItemsSource = ComboBox «не открывается»).
  - Если НЕТ → системная проблема Ribbon.Avalonia: raw `ComboBox` popup не работает внутри RibbonGroupBox. Возможные пути: использовать родной контрол библиотеки (RibbonComboBox/RibbonDropDownButton, если есть в 0.9.0), либо вынести селектор иначе. Нужно изучить Ribbon.Avalonia 0.9.0 (исходники/сэмпл; пакет в `C:\Users\smele\.nuget\packages\ribbon.avalonia\0.9.0\`).

### Идеи диагностики
- Проверить, что `ThemeSettingsVM` приходит не-null (лог/брейкпоинт в ctor MainWindowVM).
- Проверить число элементов в `Modes` (должно быть 3).
- Если VM ок и items есть, а popup не открывается — копать Ribbon.Avalonia (clip/overlay/перехват pointer); сравнить с тем, открываются ли Extensions/Formulas ComboBox.
- net10.0 SDK только на Windows; в этой среде `dotnet` нет — сборку и запуск выполняет пользователь.

## Технические факты среды
- Avalonia 11.3.x, net10.0. Namespace проекта `...Avalonia` конфликтует с фреймворком `Avalonia` → в code-behind `global::Avalonia.*`.
- Ribbon.Avalonia 0.9.0: `clr-namespace:Ribbon.Avalonia;assembly=Ribbon.Avalonia`; вкладки строго внутри `<ribbon:Ribbon.Tabs>`; host — обычный `Window`.
- Тема: `Application.Current.RequestedThemeVariant` = `ThemeVariant.Light/Dark/Default`; Default = следовать ОС. FluentTheme имеет встроенные Light/Dark — базовые поверхности меняются и без кастомных цветов, но в App.axaml много хардкод-цветов (например Grid `#FFF1F1F1`), которые не переключатся до отдельного «цветового» коммита.
- В песочнице Linux: пути bash `/sessions/friendly-admiring-lovelace/mnt/Philadelphus.General/...`; dotnet недоступен.

## После починки
- Дать текст коммита (правило: всегда без запроса). Черновик уже был:
  `feat(avalonia): переключатель темы (Светлая/Тёмная/Как в системе)` — селектор на вкладке «Вид», применение RequestedThemeVariant, сохранение в appsettings.json (ApplicationSettingsConfig:ThemeString), цвета — отдельным коммитом.
- Перепроверить всю реализацию из списка выше на корректность сборки и поведения.
