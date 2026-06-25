# 02 — Инвентаризация компонентов

> **Статус:** Аналитическая фаза. Дата: 2026-06-02.
>
> **Категории классификации:**
> - `Shared as-is` — можно переиспользовать без изменений
> - `Shared after refactor` — переносится после ограниченного рефакторинга
> - `WPF adapter` — остаётся в WPF-проекте (View или адаптер)
> - `Avalonia adapter` — нужна отдельная реализация для Avalonia
> - `View-specific` — переносится или переписывается на уровне UI
> - `Platform-specific` — требует платформенной изоляции
> - `Needs research` — недостаточно данных, требуется spike
> - `Do not migrate yet` — преждевременно из-за зависимостей или риска

---

## Инфраструктурные и доменные проекты

Все эти проекты таргетируют net10.0, не имеют WPF-зависимостей и **не требуют изменений для поддержки Avalonia**.

| Проект | Категория | Примечание |
|---|---|---|
| `Philadelphus.Core.Domain` | `Shared as-is` | Чистый доменный слой |
| `Philadelphus.Core.Domain.ExtensionSystem` | `Shared as-is` | |
| `Philadelphus.Core.Domain.FormulaEngine` | `Shared as-is` | |
| `Philadelphus.Core.Domain.ImportExport` | `Shared as-is` | |
| `Philadelphus.Core.Domain.Reports` | `Shared as-is` | |
| `Philadelphus.Core.Domain.TablesExport` | `Shared as-is` | |
| `Philadelphus.Core.Application` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.EF` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.EF.PostgreSQL` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.EF.SQLite` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.Json` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.ADO.MongoDB` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Persistence.ADO.PostgreSQL` | `Do not migrate yet` | **Мёртвый артефакт**: не в .sln, нигде не ссылается. Рекомендация: удалить директорию. |
| `Philadelphus.Infrastructure.Cache` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Cache.Redis` | `Shared as-is` | |
| `Philadelphus.Infrastructure.ImportExport.Excel` | `Shared as-is` | |
| `Philadelphus.Infrastructure.ImportExport.Phjson` | `Shared as-is` | |
| `Philadelphus.Infrastructure.Messaging.Kafka` | `Shared as-is` | |
| `Philadelphus.Infrastructure.AssemblyAdapters` | `Shared as-is` | |
| `Philadelphus.Infrastructure.AssemblyAdapters.CSharp` | `Shared as-is` | |
| `Philadelphus.Infrastructure.AssemblyAdapters.Python` | `Shared as-is` | |
| `Philadelphus.Infrastructure.AssemblyAdapters.JavaScript` | `Shared as-is` | |

---

## ViewModels

### ViewModelBase

| Файл | Категория | Примечание |
|---|---|---|
| `ViewModels/ViewModelBase.cs` | `Shared as-is` | Только `System.ComponentModel.INotifyPropertyChanged`. Чистый. |

### Application-level ViewModels

| Файл | Категория | WPF-зависимость | Действие |
|---|---|---|---|
| `ViewModels/ApplicationVM.cs` | `Shared after refactor` | `using System.Windows` — удалить, проверить использование | Убрать WPF using |
| `ViewModels/ApplicationCommandsVM.cs` | `Shared after refactor` | `using System.Windows` | Убрать WPF using |
| `ViewModels/ApplicationWindowsVM.cs` | `Shared after refactor` | Проверить наличие WPF-типов | Проверить |
| `ViewModels/MainWindowNotificationsVM.cs` | `Shared after refactor` | `using System.Windows` | Убрать WPF using |

### Control ViewModels (ControlsVMs/)

| Файл | Категория | WPF-зависимость | Действие |
|---|---|---|---|
| `ControlBaseVM.cs` | `Shared as-is` | Нет | — |
| `MainWindowVM.cs` | `Shared after refactor` | Создаёт `DetailsWindow` напрямую | Внедрить `IWindowService` |
| `LaunchWindowVM.cs` | `Shared after refactor` | Нет (чистый) | Минимальные изменения |
| `ApplicationSettingsControlVM.cs` | `Shared after refactor` | `System.Windows`, `System.Windows.Shapes`, `MessageBox.Show()` x5 | `IDialogService`, убрать WPF using |
| `ExtensionsControlVM.cs` | `Shared after refactor` | `System.Windows`, `MessageBox.Show()` x2 | `IDialogService` |
| `FormulaTestControlVM.cs` | `Shared after refactor` | Проверить | Проверить |
| `RepositoryExplorerControlVM.cs` | `Shared after refactor` | `System.Windows`, `Visibility` как тип возврата, `Dispatcher.Invoke` | `bool` вместо `Visibility`, `IDispatcherService` |
| `RepositoryFormulaBarVM.cs` | `Shared after refactor` | `System.Windows.Input` | Только `ICommand` — нормально, если не WPF-специфично |
| `ReportsControlVM.cs` | `Shared after refactor` | `System.Windows.Data`, `System.Windows.Input` | Проверить использование `CollectionView` |
| `StorageCreationControlVM.cs` | `Shared after refactor` | `System.Windows`, `MessageBox.Show()` x2 | `IDialogService` |
| `RepositoryCreationControlVM.cs` | `Shared after refactor` | `System.Windows`, `MessageBox.Show()` x2 | `IDialogService` |
| `ImportExportControlVM.cs` | `Shared after refactor` | `System.Windows`, `Dispatcher.Invoke` x2, `MessageBox.Show()` | `IDispatcherService`, `IDialogService` |

#### Notification ViewModels

| Файл | Категория | WPF-зависимость | Действие |
|---|---|---|---|
| `MessageLogControlVM.cs` | `Shared after refactor` | `System.Windows.Threading.DispatcherTimer`, `Dispatcher.BeginInvoke` | `ITimerService` или `System.Threading.PeriodicTimer` |
| `ModalWindowNotificationsControlVM.cs` | `Shared after refactor` | `System.Windows`, `MessageBox.Show()` | `IDialogService` |
| `PopUpNotificationsControlVM.cs` | `Shared after refactor` | Проверить | Проверить |

#### Tab ViewModels

| Файл | Категория |
|---|---|
| `TabItemControlBaseVM.cs` | `Shared as-is` |
| `ClosableTabItemControlBaseVM.cs` | `Shared as-is` |
| `ApplicationSettingsTabItemControlVM.cs` | `Shared as-is` |
| `LaunchWindowTabItemControlVM.cs` | `Shared as-is` |

### Entity ViewModels (EntitiesVMs/)

| Файл | Категория | Примечание |
|---|---|---|
| `DataStorageVM.cs` | `Shared after refactor` | Проверить WPF using |
| `DataStoragesCollectionVM.cs` | `Shared after refactor` | Проверить WPF using |
| `NotificationVM.cs` | `Shared as-is` | |
| `ConfigurationFileVM.cs` | `Shared as-is` | |
| `ConnectionStringsContainerVM.cs` | `Shared as-is` | |
| `ExtensionInstanceVM.cs` | `Shared as-is` | |
| `MainEntityBaseVM.cs` | `Shared after refactor` | Nullable warning — не WPF |
| `PhiladelphusRepositoryVM.cs` | `Shared after refactor` | Nullable warnings |
| `PhiladelphusRepositoryCollectionVM.cs` | `Shared after refactor` | Nullable warnings, Dispatcher? |
| `PhiladelphusRepositoryHeaderVM.cs` | `Shared as-is` | |
| `PhiladelphusRepositoryHeadersCollectionVM.cs` | `Shared after refactor` | `System.Windows.Data` (CollectionView?) |
| `ElementAttributeVM.cs` | `Shared after refactor` | `System.Windows`, `MessageBox.Show()` |
| `TreeRootVM.cs` | `Shared after refactor` | `System.Windows.Data` |
| `TreeNodeVM.cs` | `Shared after refactor` | `System.Windows.Data` |
| `TreeLeaveVM.cs` | `Shared as-is` | |
| `IMainEntityVM.cs` | `Shared as-is` | |
| `ILeaveParent.cs` | `Shared as-is` | |
| `INodeParent.cs` | `Shared as-is` | |

### Import/Export ViewModels

| Файл | Категория |
|---|---|
| `ExcelImportSourceItemViewModel.cs` | `Shared after refactor` |
| `ImportExportAdapterVM.cs` | `Shared after refactor` |
| `ImportExportControlVM.cs` | `Shared after refactor` — см. выше |
| `ImportExportOperationVM.cs` | `Shared after refactor` |
| `ImportFromExcelVM.cs` | `Shared after refactor` — `System.Windows.Input` |

---

## Команды (ICommand)

| Файл | Категория | WPF-зависимость | Действие |
|---|---|---|---|
| `Infrastructure/RelayCommand.cs` | `WPF adapter` | `CommandManager.RequerySuggested` (WPF-only) | В WPF-проекте. Создать `IRelayCommand` в shared Presentation + Avalonia-реализацию |
| `Infrastructure/AsyncRelayCommand.cs` | `WPF adapter` | `CommandManager.RequerySuggested` + `async void Execute()` (anti-pattern) | В WPF-проекте. Создать `IAsyncRelayCommand` + Avalonia-реализацию |

**Рекомендация для Avalonia:** использовать `ReactiveUI.ReactiveCommand` или написать собственную реализацию без `CommandManager`.

---

## Converters

| Файл | Категория | WPF-тип | Логика | Действие |
|---|---|---|---|---|
| `CanExecuteToColorConverter.cs` | `WPF adapter` / `Avalonia adapter` | `IValueConverter`, `System.Windows.Data`, `Brushes` | `bool → Brush` | Чистая логика: `bool → color-string`. Адаптер в каждом проекте. |
| `CanExecuteToColorMultiConverter.cs` | `WPF adapter` / `Avalonia adapter` | `IMultiValueConverter` | Multi-value | То же |
| `StateToColorConverter.cs` | `WPF adapter` / `Avalonia adapter` | `Brushes` (WPF) | `State enum → Brush` | Вынести маппинг State→color-name в shared; адаптер в каждом проекте |
| `IsAvailableToColorConverter.cs` | `WPF adapter` / `Avalonia adapter` | `Brushes` | `bool → Brush` | Аналогично |
| `CanExecuteToColorConverter.cs` | `WPF adapter` / `Avalonia adapter` | `Brushes` | `bool → Brush` | Аналогично |
| `MainEntityToIconConverter.cs` | `WPF adapter` / `Avalonia adapter` | `BitmapImage`, `pack://` URI | Entity type → Icon | Нельзя переиспользовать. Avalonia: `avares://` URI + `Bitmap` |
| `KeyToImageConverter.cs` | `WPF adapter` / `Avalonia adapter` | `Application.Current.Resources` | Resource key → Image | Нельзя переиспользовать. Avalonia: `IResourceDictionary` |
| `CriticalLevelToIconConverter.cs` | `WPF adapter` / `Avalonia adapter` | Зависит от проверки | CriticalLevel → Icon | Проверить |
| `EnumDisplayAttributeConverter.cs` | `Shared after refactor` | Только `CultureInfo`, reflection | Enum → display name | Чистая логика — вынести в shared |
| `LastLaunchToDaysAgoConverter.cs` | `Shared after refactor` | `CultureInfo`, `DateTime` | DateTime → "N дней назад" | Чистая логика — вынести в shared |
| `UtcToLocalTimeConverter.cs` | `Shared after refactor` | `CultureInfo`, `DateTime` | UTC → local | Чистая логика — вынести в shared |
| `RepresentationToDataGridVisibilityConverter.cs` | `WPF adapter` | `Visibility` | Enum → Visibility | `bool` в shared, `Visibility` — в адаптере |
| `RepresentationToStandardGridVisibilityConverter.cs` | `WPF adapter` | `Visibility` | Enum → Visibility | То же |
| `SelectedIndexConverter.cs` | `WPF adapter` / `Avalonia adapter` | `IValueConverter` | Index logic | Проверить логику |

---

## Behaviors

Все 8 behaviors используют `Microsoft.Xaml.Behaviors.Wpf` или WPF `DependencyProperty` API — они **не переносимы** напрямую.

| Файл | Категория | Риск | Действие |
|---|---|---|---|
| `DataGridAutoScrollBehavior.cs` | `Avalonia adapter` | Средний | Переписать через `Avalonia.Xaml.Interactions` |
| `DataGridCellFocusRequestBehavior.cs` | `Avalonia adapter` | Средний | Переписать |
| `DataGridColumnsBehavior.cs` | `Avalonia adapter` | Средний | Переписать |
| `DataGridFormulaCellSelectionBehavior.cs` | `Avalonia adapter` | Высокий | DataGrid behaviour в Avalonia отличается |
| `DataGridFormulaReferenceHighlightBehavior.cs` | `Avalonia adapter` | Высокий | Кастомный DataGrid — исследовать |
| `DataGridLostKeyboardFocusCommandBehavior.cs` | `Avalonia adapter` | Средний | Переписать |
| `FormulaBarTextBoxBehavior.cs` | `Avalonia adapter` | Средний | Переписать |
| `FormulaSuggestionTextBoxBehavior.cs` | `Avalonia adapter` | Средний | Переписать |

---

## Сервисы

| Файл | Категория | WPF-зависимость | Действие |
|---|---|---|---|
| `Services/Implementations/ConfigurationService.cs` | `Shared after refactor` | `MessageBox.Show()` (предупреждение при dev mode) | `IDialogService` |
| `Services/Interfaces/IConfigurationService.cs` | `Shared as-is` | Нет | — |
| `Services/ExcelImportUiServices.cs` | `WPF adapter` | `Microsoft.Win32.OpenFileDialog`, `SaveFileDialog`, `MessageBox.Show()` x3 | WPF-реализация `IFileDialogService`. Создать Avalonia-реализацию |
| `Services/IFileDialogService.cs` | `Shared as-is` | Нет | Уже изолирован |
| `Services/IMessageDialogService.cs` | `Shared as-is` | Нет | Уже изолирован |
| `Services/ExcelImportPresentationPipeline.cs` | `Shared after refactor` | Проверить | |
| `Services/ExcelImportPresentationSessionState.cs` | `Shared after refactor` | | |
| `Services/ExcelImportRepositoryPreviewBuilder.cs` | `Shared after refactor` | | |
| `Services/ExcelPreviewTableBuilder.cs` | `Shared after refactor` | | |
| `Services/FormulaDiagnosticsReporter.cs` | `Shared after refactor` | | |
| `Services/RepositoryExplorerPreviewConfigurator.cs` | `Shared after refactor` | | |
| `Services/Tables/ChildCollectionTableBuilder.cs` | `Shared after refactor` | Может иметь WPF-типы | Проверить |

---

## Windows (View-specific)

| Файл | Категория | Code-behind сложность | Приоритет миграции |
|---|---|---|---|
| `SplashWindow.xaml` + `.cs` | `View-specific` + `Avalonia adapter` | Высокая (340 строк, анимации, Dispatcher) | Этап 5 |
| `LaunchWindow.xaml` + `.cs` | `View-specific` + `Avalonia adapter` | Средняя (36 строк, Hide-логика) | Этап 6 |
| `MainWindow.xaml` + `.cs` | `View-specific` + `Avalonia adapter` | Низкая (42 строки, multi-instance shutdown) | Этап 7 |
| `FormulaEditorWindow.xaml` + `.cs` | `View-specific` | Проверить | Этап 8 |
| `ImportFromExcelWindow.xaml` + `.cs` | `View-specific` | Средняя | Этап 8 |
| `ExcelImportDesignerWindow.xaml` + `.cs` | `View-specific` | Высокая (Excel preview, 14+ MessageBox) | Этап 8 |
| `ImportProgressWindow.xaml` + `.cs` | `View-specific` | Низкая | Этап 8 |
| `AttributeValuesCollectionWindow.xaml` + `.cs` | `View-specific` | Низкая | Этап 8 |
| `DetailsWindow.xaml` + `.cs` | `View-specific` | Низкая | Этап 8 |
| `EmptyWindow.xaml` + `.cs` | `View-specific` | Минимальная | Этап 8 |

---

## UserControls (View-specific, ~25 штук)

Все UserControls — `View-specific`. Переносятся как AXAML-файлы на Avalonia. Порядок переноса определяется зависимостями: сначала leaf-controls (без дочерних вложений), затем составные.

**Высокий риск (сложная логика/behaviors):**
- `RepositoryExplorer/RepositoryExplorer.xaml` — центральный контрол с формулами, деревом, DataGrid
- `RepositoryExplorer/Attributes.xaml` — DataGrid с formula behaviors
- `RepositoryExplorer/FormulaBar.xaml` — кастомный TextBox с behaviors
- `RepositoryExplorer/SystemBaseValueEditors/` — кастомные редакторы значений

**Средний риск:**
- `DetailedInformationGrids/` (8 штук)
- `TabItemsControls/` (8 штук)
- `CollectionControls/ListBoxes/` (3 штуки)

**Низкий риск:**
- `About.xaml`, `ApplicationSettings.xaml`, `MessageLog.xaml` и др.

---

## NuGet-пакеты

### WPF-специфичные (требуют замены)

| Пакет | Версия | Используется | Замена для Avalonia |
|---|---|---|---|
| `Microsoft.Xaml.Behaviors.Wpf` | 1.1.142 | Да (все behaviors) | `Avalonia.Xaml.Behaviors` |
| `PropertyTools.Wpf` | 3.1.0 | **Нет** (проверено: 0 using) | Удалить пакет |
| `System.Drawing.Common` | 9.0.8 | Частично | Avalonia: `Avalonia.Media.Imaging.Bitmap` |

### Переносимые (используются без изменений)

| Пакет | Переносимость |
|---|---|
| AutoMapper 16.1.1 | ✅ |
| Serilog (все sinks) | ✅ |
| Microsoft.Extensions.Hosting | ✅ |
| StackExchange.Redis | ✅ |
| Confluent.Kafka | ✅ |
| ClosedXML, DocumentFormat.OpenXml | ✅ |
| Npgsql, EF Core | ✅ |
| MongoDB.Driver | ✅ |

### Требуют дополнительного исследования

| Пакет | Вопрос |
|---|---|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Переносимый, но зарегистрирован в App.xaml.cs — нужен общий DI bootstrap |

---

## Platform-specific зависимости

| Зависимость | Файл | Категория | Действие |
|---|---|---|---|
| `AllocConsole`/`FreeConsole` (kernel32) | `App.xaml.cs` | `Platform-specific` | В Avalonia-startup не нужен. Изолировать или убрать. |
| `Microsoft.Win32.OpenFileDialog` | `ExcelImportUiServices.cs` | `WPF adapter` | Реализация `IFileDialogService`. Avalonia: `StorageProvider.OpenFilePickerAsync()` |
| `System.Windows.MessageBox` | 18+ мест | `WPF adapter` | Реализация `IDialogService`. Avalonia: `DialogHost` или кастомный диалог |
| `Application.Current.Dispatcher` | 3 ViewModels | `WPF adapter` | `IDispatcherService`. Avalonia: `Dispatcher.UIThread` |
| `DispatcherTimer` | `MessageLogControlVM.cs` | `WPF adapter` | `System.Threading.PeriodicTimer` (переносимый) или `ITimerService` |
| `pack://application:,,,/` URI | `MainEntityToIconConverter.cs`, `App.xaml` | `WPF adapter` | Avalonia: `avares://ProjectName/` |
| `CommandManager.RequerySuggested` | `RelayCommand.cs`, `AsyncRelayCommand.cs` | `WPF adapter` | Нет аналога. Avalonia: ручной `RaiseCanExecuteChanged()` |
| `Application.Current.Resources` | `KeyToImageConverter.cs` | `WPF adapter` | Avalonia: `IResourceDictionary` через DI или `Application.Current.Resources` |

---

## Технический долг (выявлен, не связан с Avalonia)

| Элемент | Описание | Действие |
|---|---|---|
| `PropertyTools.Wpf` в .csproj | Подключён, не используется | Удалить `<PackageReference>` |
| `ADO.PostgreSQL` директория | Проект не в .sln, нигде не ссылается | Удалить директорию |
| 24 падающих теста | Pre-existing, до начала миграции | Зафиксировать, создать issues |
| 347 nullable-предупреждений | Pre-existing в WPF-проекте | Не блокирует миграцию |
| `async void Execute()` в AsyncRelayCommand | Anti-pattern, исключения теряются | Исправить при создании Avalonia-реализации |
