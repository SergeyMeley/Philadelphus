# 09 — Хендофф миграции WPF → Avalonia и анализ переносимости классов

> Документ для продолжения работы в новом чате. Перед переходом к следующим этапам
> нужно принять решения по таблице переносимости (раздел 5).

## 1. Текущее состояние

- Ветка: `feature/#65575392-avalonia`, HEAD: `56be8c79`. **Сборка `dotnet build Philadelphus.sln` — зелёная (подтверждено).**
- Цель этапа 3 (по `04-roadmap.md`): перенос ViewModels/сервисов/логики в общий проект
  `Philadelphus.Presentation` (net10.0, без WPF), чтобы затем поднять Avalonia-клиент.
- Вся работа Этапа 3.1 СКВОШНУТА в один коммит `56be8c79`
  («Этап 3.1: перенос ViewModels и инфраструктуры в Philadelphus.Presentation») поверх `66dce630`.
  Включает: namespace-актуализацию, перенос ConfigurationService, вынос логики конвертеров,
  кластер сущностей/дерева (MainEntityBaseVM/ElementAttributeVM/IMainEntityVM/Tree*/INodeParent/ILeaveParent),
  Formula-типы, и фичу file-picker (`IFileDialogService.BrowseLocalFile`).
- Рабочее дерево ЧИСТОЕ (всё закоммичено); пустых/битых файлов нет, `TreeRootVM.cs`/`ILeaveParent.cs` целые.

### Уже в shared `Philadelphus.Presentation` (готово и проверено статикой)
- Базовое: `ViewModelBase`; `Infrastructure` (RelayCommand/AsyncRelayCommand + интерфейсы IRelayCommand/IAsyncRelayCommand);
- Интерфейсы сервисов: IDialogService, IDispatcherService, IWindowService, IConfigurationService,
  IFileDialogService, IMessageDialogService, IApplicationCommandsVM, IInfrastructureRepositoryFactory;
- ViewModels: NotificationsVMs (MessageLog/ModalWindow/PopUp), TabItemsVMs (4), сущности
  (DataStorageVM, DataStoragesCollectionVM, ExtensionInstanceVM, NotificationVM, ConfigurationFileVM,
  ConnectionStringsContainerVM), ImportExport (AdapterVM, OperationVM, ExcelImportSourceItemVM),
  ControlBaseVM, StorageCreationControlVM, IFormulaEditorIntelliSenseVM + FormulaIntelliSenseTypes;
- Кластер сущностей/дерева: MainEntityBaseVM, IMainEntityVM, ElementAttributeVM, TreeRootVM, TreeNodeVM,
  TreeLeaveVM, INodeParent, ILeaveParent;
- Сервисы/логика: ConfigurationService; Converters/Logic (EnumDisplay/LastLaunchToDaysAgo/UtcToLocalTime);
  Models/Tables (ChildCollectionTableColumn/Row).

## 2. ⚠️ Важные предупреждения (среда и состояние)

1. **NUL-«повреждения» — артефакт монтирования, а не реальная порча.** При открытой Visual Studio
   (file-watcher/Roslyn/сборка) или из-за кэша virtiofs файлы иногда читаются с NUL-байтами сразу
   после записи. Дисциплина: на время правок ЗАКРЫВАТЬ Visual Studio; при появлении NUL —
   перезапустить сессию Cowork (после рестарта видно реальное состояние диска, правки обычно целы).
2. **git-операции записи (reset/checkout/commit/clean) выполнять на Windows**, не через монтирование:
   индекс несколько раз повреждался (`fatal: Could not reset index file`). Чтение (status/show/log) — ок.
3. **Сборка/тесты — только на Windows** (в песочнице нет .NET SDK; WPF таргетит net10.0-windows).
   Контрольная точка после каждого батча: `dotnet build Philadelphus.sln`.
4. *(Исторически)* в прежнем коммите `67a016cb` `TreeRootVM.cs`/`ILeaveParent.cs` были закоммичены пустыми
   из-за артефакта монтирования — после сквоша в `56be8c79` это исправлено (файлы целые).

## 3. Последняя фича: IFileDialogService.BrowseLocalFile (file-picker)

- В `IFileDialogService` (shared) добавлен `string? BrowseLocalFile();`, реализован в WPF `FileDialogService`
  (Microsoft.Win32.OpenFileDialog: CheckFileExists/CheckPathExists/Multiselect=false).
- `IFileDialogService` сделан ОБЯЗАТЕЛЬНЫМ параметром конструктора по всей цепочке создания entity-VM
  (MainEntityBaseVM хранит `protected readonly IFileDialogService _fileDialogService`; конструкторы
  ElementAttributeVM/TreeLeaveVM/TreeNodeVM/TreeRootVM/PhiladelphusRepositoryVM принимают и передают в base;
  корни PhiladelphusRepositoryCollectionVM/RepositoryCreationControlVM/ExcelImportRepositoryPreviewBuilder/
  RepositoryExplorerControlVM инъектируют через ctor (DI)). Везде `ArgumentNullException.ThrowIfNull`.
- `TreeLeaveVM.BrowseFileValue()` переведён с `OpenFileDialog` на `_fileDialogService.BrowseLocalFile()`.
- Отдельный временный `IFilePickerService` (промежуточный вариант) удалён.

## 4. Ключевые системные блокеры переноса (для принятия решений)

1. **Фабрика команд (риск R3).** Многие «хабовые» VM создают `new RelayCommand(...)` из WPF
   `Philadelphus.Presentation.Wpf.UI.Infrastructure` (реализация на `CommandManager.RequerySuggested` — WPF-only).
   Решение: ввести `IRelayCommandFactory`/`IAsyncRelayCommandFactory` в shared (WPF-реализация — с CommandManager,
   Avalonia — на shared RelayCommand), заменить `new RelayCommand` → `_commandFactory.Create(...)`.
   Это разблокирует ~9 VM одним приёмом без смены поведения.
2. **Навигация/окна.** Часть VM напрямую ссылается на `Wpf.UI.Views` (открывают окна/контролы).
   Решение: довести абстракцию через `IWindowService`/навигацию, убрать прямые ссылки на Views.
3. **Файловые диалоги (`Microsoft.Win32`).** Паттерн готов — абстрагировать через `IFileDialogService`
   (как сделано для TreeLeaveVM). Осталось в ApplicationSettingsControlVM, ImportExportControlVM.
4. **View-слой (code-behind `.xaml.cs`, Behaviors, Converters-обёртки).** Это и есть платформенный UI —
   в shared НЕ переносится. Для Avalonia пишутся свои реализации; из converters выносится только чистая логика.

## 5. Сводная таблица переносимости (.cs WPF-проекта — 116 файлов)

| Категория | ~Файлов | В shared? | Почему / блокер | Действие |
|---|---|---|---|---|
| Code-behind `*.xaml.cs` (Views, App) | ~55 | Нет | Partial-классы XAML, наследуют UserControl/Window/Application, System.Windows | Остаются платформенными; для Avalonia — свои View |
| Behaviors (`Behaviors/*`) | 9 | Нет | Microsoft.Xaml.Behaviors + System.Windows (attached behaviors WPF) | Остаются в WPF; Avalonia — свои |
| Converters (`Converters/*`) | 14 | Частично | Реализуют System.Windows.Data.IValueConverter; многие используют Brush/ImageSource/Visibility | Обёртка остаётся в WPF; ЧИСТУЮ логику выносить в shared Converters/Logic (сделано для 3) |
| WPF-адаптеры сервисов (WpfDialogService, WpfDispatcherService, WpfWindowService, FileDialogService, MessageDialogService) | 5 | Нет (намеренно) | Платформенные реализации shared-интерфейсов | Остаются; Avalonia — свои реализации |
| WPF Infrastructure (RelayCommand, AsyncRelayCommand) | 2 | Нет (намеренно) | CommandManager.RequerySuggested (WPF-only) | Остаются как WPF-реализация; shared имеет свои |
| WPF Factories (`Factories/*`) | 7 | Нет (большинство) | Создают WPF VM/контролы | Остаются/по-платформенные; интерфейсы можно в shared при необходимости |
| ExcelImport-сервисы (`Services/ExcelImport*`, RepositoryExplorerPreviewConfigurator, Services/Tables/ChildCollectionTableBuilder) | 7 | Ревизия | Ссылаются на `Wpf.UI.Services`/`System.Windows`/Microsoft.Win32 (ExcelImportUiServices) | Разобрать индивидуально: чистые билдеры (ExcelPreviewTableBuilder, FormulaDiagnosticsReporter, ChildCollectionTableBuilder) — кандидаты в shared после проверки транзитивных ссылок |
| App.xaml.cs, AssemblyInfo.cs | 2 | Нет | Точка композиции приложения, DI, атрибуты сборки | Остаются в WPF |

### 5.1. ViewModels — детально (главные кандидаты решений)

| ViewModel | В shared? | Блокер | Что нужно для переноса |
|---|---|---|---|
| ApplicationCommandsVM | Нет | WPF RelayCommand + Factories | Фабрика команд; вынос фабрик/навигации |
| ApplicationWindowsVM | Нет | ссылки на Views (управление окнами) | Навигационная абстракция (по сути платформенный) |
| ApplicationVM | Ревизия | транзитивно: ApplicationCommandsVM/ApplicationWindowsVM/Views | После разблокировки хабов |
| MainWindowVM | Нет | Views + WPF RelayCommand + Factories | Фабрика команд + навигация |
| LaunchWindowVM | Нет | Views + WPF RelayCommand | Фабрика команд + навигация |
| RepositoryExplorerControlVM | Нет | Views + WPF RelayCommand + Factories + Wpf.UI.Services (Tables) | Фабрика команд + навигация + перенос Tables-сервисов |
| RepositoryFormulaBarVM | Нет | WPF RelayCommand | Фабрика команд |
| ApplicationSettingsControlVM | Нет | Views + WPF RelayCommand + Microsoft.Win32 | Фабрика команд + навигация + IFileDialogService |
| ExtensionsControlVM | Нет | System.Windows + WPF RelayCommand | Очистка System.Windows + фабрика команд |
| ImportExportControlVM | Нет | Views + WPF RelayCommand + Microsoft.Win32 | Фабрика команд + навигация + IFileDialogService |
| ImportFromExcelVM | Нет | System.Windows + WPF RelayCommand + Wpf.UI.Services | Очистка + фабрика команд |
| FormulaTestControlVM | Ревизия | использует shared RelayCommand; транзитивно ссылается на RepositoryExplorerControlVM | Проверить реальные (не комментарии) ссылки |
| ReportsControlVM | Ревизия | по маркерам чисто (shared RelayCommand) | Транзитивная проверка → вероятно переносим |
| RepositoryCreationControlVM | Ревизия | ссылается на PhiladelphusRepositoryCollectionVM (WPF) | После переноса коллекций |
| PhiladelphusRepositoryVM | Ревизия | по маркерам чисто; в WPF из-за коллекций/фабрик | Транзитивная проверка |
| PhiladelphusRepositoryCollectionVM | Ревизия | по маркерам чисто; IServiceProvider, InitRepositoriesVMsCollection | Транзитивная проверка |
| PhiladelphusRepositoryHeaderVM / HeadersCollectionVM | Ревизия | по маркерам чисто | Транзитивная проверка (AutoMapper, Config) |
| MainWindowNotificationsVM | Да (кандидат) | по маркерам чисто, наследует shared ViewModelBase | Перенос один-в-один после транзитивной проверки |

### 5.2. Прочее
- `Models/Entities/ParallelObservableCollectionVM.cs` — чистый (shared ViewModelBase) → кандидат в shared.
- `Models/Entities/Enums/PropertyGridRepresentations.cs`, `RepositoryExplorerRepresentations.cs` — чистые enum → кандидаты в shared.
- `Mapping/ViewModelsMappingProfile.cs` — AutoMapper-профиль; переносим, если все маппируемые типы в shared.

> ВАЖНО про метод анализа: маркерный скан НЕ ловит транзитивные ссылки на WPF-резидентные типы
> (например, ссылку на RepositoryExplorerControlVM). Поэтому «чисто по маркерам» ≠ «переносим»:
> для каждого кандидата нужен транзитивный проход (с учётом того, что упоминания в TODO-комментариях
> дают ложные срабатывания — это уже встречалось с MainEntityBaseVM).

## 6. Следующие шаги (предлагаемый порядок)

1. ✅ Сборка `56be8c79` зелёная (подтверждено). Baseline установлен.
2. **Очистка хабовых ViewModels от WPF** по детальному плану в разделе 8 (A→B→C→D).
3. Параллельно — транзитивный анализ кандидатов «ревизия/да» из таблицы 5.1/5.2.
4. Закрыть Этап 3 (+ создать Philadelphus.Tests.Presentation, 1–2 unit-теста). Затем Этап 4 — Avalonia bootstrap.

## 7. Рабочие правила (подтверждены пользователем)
- Файлы переносятся один-в-один (кроме актуализации namespace); изменения содержимого — отдельным коммитом.
- Коммиты делает пользователь; Claude пишет только текст коммита.
- Сервисы получать через конструктор (DI), параметры — обязательные, с ArgumentNullException.ThrowIfNull
  по аналогии с другими сервисами (не опциональные `= null`, не `GetService` в теле).
- `using` добавлять сразу в отсортированную позицию (сортировка по namespace, System-семейство по общему
  правилу, alias-using в конце), одна пустая строка перед `namespace`.
- `ModalWindowNotificationsControlVM` использует `IDialogService` напрямую — это правильно.
- Не делать `git reset`; если нужно откатить — оформлять как новую правку.


## 8. План очистки ViewModels от WPF (детально)

Анализ фактического WPF-API в хабовых VM показал: поверхность УЗКАЯ. `System.Windows.Input` — это
`ICommand` из BCL (в shared компилируется), `Visibility` в VM фактически нет. Реальные блокеры — 4 штуки,
под каждый есть чистая абстракция. Порядок: A → B → C → D.

### A. Фабрика команд (главный разблокировщик, ~9 VM)
Блокер: VM создают `new RelayCommand(...)` из `Wpf.UI.Infrastructure` (на `CommandManager.RequerySuggested` — WPF-only).
```csharp
// shared: Philadelphus.Presentation/Infrastructure/
public interface IRelayCommandFactory {
    IRelayCommand Create(Action<object?> execute, Predicate<object?>? canExecute = null);
}
public interface IAsyncRelayCommandFactory {
    IAsyncRelayCommand Create(Func<object?, Task> execute, Predicate<object?>? canExecute = null);
}
```
- WPF-реализация фабрики создаёт текущий WPF `RelayCommand`/`AsyncRelayCommand` (с CommandManager) — поведение не меняется.
- Avalonia-реализация (позже) — на shared `RelayCommand` (без CommandManager).
- В VM: `new RelayCommand(...)` → `_commandFactory.Create(...)`, фабрика инъектируется обязательным ctor-параметром (DI), `ArgumentNullException.ThrowIfNull`.
- Зарегистрировать фабрики в `App.xaml.cs`.
- Разблокирует: ApplicationCommandsVM, RepositoryFormulaBarVM, ExtensionsControlVM, MainWindowVM, LaunchWindowVM,
  ApplicationSettingsControlVM, ImportExportControlVM, ImportFromExcelVM, RepositoryExplorerControlVM.

### B. Открытие окон через `IWindowService` (уже есть в shared)
`IWindowService` (Show<TVM>/ShowDialog<TVM>/Close/Hide/ShowOrActivate + реестр VM→Window) уже в shared.
Заменить прямое создание окон:
- `MainWindowVM:183` `new DetailsWindow(elementVM); .Show()` → `_windowService.Show(elementVM)` (+ регистрация ElementVM→DetailsWindow в WpfWindowService).
- `RepositoryExplorerControlVM:380` `GetRequiredService<AttributeValuesCollectionWindow>().Show()` → `_windowService.Show(attrValuesVM)`.
- `ImportExportControlVM:245` `GetRequiredService<ExcelImportDesignerWindow>().ShowDialog()` → `_windowService.ShowDialog(designerVM)`.
- `ApplicationWindowsVM` — по его TODO «Удалить и брать окна из DI»: убрать класс, окна вести через IWindowService.

### C. Файловые диалоги через `IFileDialogService` (паттерн готов — `BrowseLocalFile`)
Добавить общие методы:
```csharp
string? OpenFile(string? filter = null);
string? SaveFile(string? filter = null, string? defaultName = null);
```
Реализовать в WPF `FileDialogService` (Open/SaveFileDialog). Заменить `Microsoft.Win32` в:
- `ApplicationSettingsControlVM` (OpenFileDialog, ~стр.235);
- `ImportExportControlVM` (Save/Open, стр.175/209/265/276).

### D. Структурный рефакторинг `LaunchWindowVM` (сложнее, после A–C)
Анти-MVVM: VM создаёт View-контролы и проверяет их тип:
```csharp
tab.Content = new LaunchWindowMainTabControl() { DataContext = this };   // VM создаёт View
... x.Content is LaunchWindowStoragesTabControl ...                       // проверка типа View
```
Сделать `Content` вкладок — VM (а не UserControl), маппинг VM→контрол — через XAML `DataTemplate`.
Тогда `LaunchWindowVM` перестаёт знать о View. Правка затрагивает View+VM.

### Итог
После A–D хабовые VM становятся WPF-free и переносятся в shared один-в-один (+ актуализация namespace).
Рекомендуемый старт: **A (фабрика команд)** — добавить интерфейсы/WPF-реализацию/регистрацию, заменить
`new RelayCommand` в одном VM как образец, проверить сборку, затем прокатить по остальным.
