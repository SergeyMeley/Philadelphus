# 14. Состояние перед Avalonia (итог сессии вычистки WPF)

Документ фиксирует состояние после полного выноса портируемого слоя из WPF в общий проект `Philadelphus.Presentation`. Цель — дать готовый контекст для отдельного чата по созданию Avalonia-приложения.

Ветка: `feature/#65575392-avalonia`.

## Итог: §8 закрыт

Из `Philadelphus.Presentation.Wpf.UI` вычищена вся портируемая логика и ViewModel-слой. В WPF остался только реально платформенный код. Дальше — bootstrap Avalonia (Этап 4).

## Что теперь в shared (`Philadelphus.Presentation`, net10.0, без WPF)

- **ViewModels** — весь контентный слой + оконно-композиционный кластер: `MainWindowVM`, `LaunchWindowVM`, `ApplicationVM`, `ApplicationCommandsVM`. Навигация — через абстракцию `IWindowService.Show/Hide/ShowOrActivate(vm)` (WPF API в VM нет).
- **Factories/Interfaces** — `IMainWindowVMFactory`, `IRepositoryExplorerControlVMFactory`, `IExcelImportDesignerVMFactory`, `IExtensionsControlVMFactory`, `IInfrastructureRepositoryFactory`.
- **Factories/Implementations** — 4 DI-фабрики VM: `ExcelImportDesignerVMFactory`, `ExtensionsControlVMFactory`, `MainWindowVMFactory`, `RepositoryExplorerControlVMFactory` (создают VM через `ActivatorUtilities`, платформенно-нейтральны).
- **Converters/Logic** — чистая логика конвертеров: `BooleanToColorLogic`, `StateToColorLogic`, `CriticalLevelToIconLogic`, `MainEntityToIconLogic`, `IconResolver`, `EnumDisplayLogic`, `LastLaunchToDaysAgoLogic`, `UtcToLocalTimeLogic`, `SelectedIndexLogic`.
- **Enums** — `ConverterColor` (палитра цветов конвертеров), `AppIcon` (идентичность динамической иконки).
- **Services** — `FormulaDiagnosticsReporter`, интерфейс `IImportProgressReporter`, Excel-цепочка импорта, `ConfigurationService` и пр.
- Негенерик-маркер `IMainEntityVM` (для диспетчеризации сущностей в `IconResolver`).

## Что осталось в WPF (платформенный слой — переписывается под Avalonia)

- **Views** — XAML + code-behind (в основном тонкий glue; исключение — `SplashWindow.xaml.cs` ~339 строк).
- **App.xaml.cs** (~476) — bootstrap: Host, DI-регистрация, Serilog, AutoMapper-скан, плагины.
- **Behaviors** (9) — WPF attached behaviors (`DataGrid*`, `Formula*`, `DiagramBehavior`).
- **Converters** — тонкие `IValueConverter`-обёртки поверх shared-логики.
- **Helpers** — `AppIconImageSource` (`AppIcon → ImageSource`, site-of-origin), `ConverterColorBrushes` (`ConverterColor → Brush`).
- **Services (WPF)** — `FileDialogService`, `WpfDialogService`, `WpfWindowService`, `WpfDispatcherService`, `WpfImportProgressReporter`; `RepositoryExplorerPreviewConfigurator` (мёртвый код — обход визуального дерева).
- **`InfrastructureRepositoryFactory`** — зависит от конкретных `Infrastructure.Persistence.EF.PostgreSQL/SQLite`, поэтому остался в точке композиции (в shared нельзя — нарушило бы слои).
- **Infrastructure** — WPF-копии `RelayCommand`/`AsyncRelayCommand` (через `CommandManager.RequerySuggested`); в shared есть свои платформо-нейтральные версии.

## Иконки

Лежат файлами в **solution-level папке `Assets/Icons/`** (не embedded, без отдельного проекта). WPF линкует их как `Content` (`CopyToOutputDirectory`) и грузит через **site-of-origin** (`pack://siteoforigin:,,,/Icons/...`). Avalonia будет линковать те же файлы как `AvaloniaResource`/по пути; будущий веб — статикой. `AppIconImageSource` (WPF) — единственное место материализации `AppIcon → ImageSource`; для Avalonia будет свой провайдер (под `.svg`).

## Тесты

`Philadelphus.Tests.Presentation` (net10.0, xUnit + FluentAssertions): тесты shared-логики конвертеров + `IconResolver` + `ChildCollectionTableBuilder`. Пустой `Philadelphus.Tests.Presentation.Wpf.UI` удалён; `InternalsVisibleTo` в `Core.Domain.csproj` переключён на `Philadelphus.Tests.Presentation`.

## Открытый тех-долг (можно делать до/во время Avalonia)

1. **Behaviors** — вынести чистую вычислительную логику крупных behaviors (`DiagramBehavior`, `FormulaBarTextBoxBehavior`, `DataGridColumnsBehavior`, `DataGridFormulaReferenceHighlightBehavior`, `DataGridAutoScrollBehavior`) в shared. В Avalonia behaviors переписываются — логику переиспользовать.
2. **`App.xaml.cs`** — вынести регистрацию shared-сервисов/VM/фабрик в shared-extension `AddPhiladelphusPresentation(IServiceCollection)`; платформенные (`WpfWindowService`, окна, `ImageSource`-реестр) оставить в каждом приложении. Снимет дублирование bootstrap между WPF и Avalonia.
3. **`appsettings.json`** — конфиг приложения, не презентационная логика. Решить при bootstrap: общий `appsettings.base.json` + per-app оверрайды или per-app полностью.
4. **`RepositoryExplorerPreviewConfigurator`** — мёртвый код (метод `ConfigureAsReadonly` без вызывающих). Удалить либо оставить как заготовку (решение отложено).
5. Naming `Reporter`/`Configurator` — оставлено как есть (решение пользователя: это валидные ролевые суффиксы).

## Грабли окружения

- **Глюк mount:** bash `git`/`grep`/`od` через mount иногда показывают фантомную порчу/«binary file matches»/устаревший индекс. Истину даёт инструмент **Read** (реальный диск Windows). Сборка/git — на стороне пользователя.
- **`PublishSingleFile`** + loose-иконки: иконки едут папкой рядом с exe (внутрь единого файла не попадают без `IncludeAllContentForSelfExtract`).
- DI-абстракции (`IServiceProvider`, `ActivatorUtilities`, `GetRequiredService`) доступны в shared через `Microsoft.Extensions.Hosting.Abstractions`; `IOptions`, AutoMapper, Serilog — пакеты shared уже подключены.

## Рабочие правила (соблюдались, сохранить)

1. Перенос файла и изменение его содержимого — разные коммиты. Смена `namespace`/`using`/видимости (`internal→public`) содержимым НЕ считается и может идти в перенос-коммите.
2. Порядок usings: алфавитный внутри групп; `Philadelphus.Presentation.*` до `Philadelphus.Presentation.Wpf.UI.*`; `System.*` в конце; alias — последними.
3. Правки — файловыми инструментами при закрытой Visual Studio. Сборка и git — на стороне пользователя; сборка = контрольная точка. Без `git reset`.
4. Сервисы — через обязательные параметры ctor (DI) с `ArgumentNullException.ThrowIfNull`.

## Формулировка для старта Avalonia-чата

> Продолжаем миграцию WPF → Avalonia проекта Philadelphus, ветка `feature/#65575392-avalonia`. Прочитай `docs/avalonia-migration/14-state-before-avalonia.md` — там финальное состояние: весь портируемый слой (ViewModels, оконный кластер, фабрики, логика конвертеров, AppIcon/ConverterColor, сервисы) уже в общем `Philadelphus.Presentation` (net10.0), в WPF остался только платформенный код. Задача: создать новый проект **Avalonia** (`Philadelphus.Presentation.Avalonia`, MVVM), переиспользующий shared `Philadelphus.Presentation`. Нужно: bootstrap (Host + DI, переиспользовать фабрики и VM), реализации платформенных абстракций (`IWindowService`, `IDialogService`, `IDispatcherService`, `IFileDialogService`, `IImportProgressReporter`, провайдер `AppIcon → Bitmap/SVG`), линковку иконок из `Assets/Icons` как `AvaloniaResource`, и перенос View (XAML Avalonia) с биндингами на существующие VM. Иконки в Avalonia могут быть `.svg`. Правки — файловыми инструментами при закрытой IDE; перенос и изменение содержимого — разными коммитами; сборка на моей стороне как контрольная точка.
