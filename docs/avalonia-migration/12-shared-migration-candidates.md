# 12 — Транзитивный анализ кандидатов на перенос в shared

> Обновляет таблицы 5.1/5.2 хендоффа после развязки хабов (фабрика команд, IWindowService,
> файловые диалоги, §8.D). Метод: проверка ctor-зависимостей + скан тел на WPF-резидентные типы
> (Wpf.UI.*, System.Windows не из BCL, Microsoft.Win32, Views, GetService<…Window>). Комментарии-TODO
> игнорируются (дают ложные срабатывания).

## Готовы к переносу в shared сейчас (транзитивно чисто)

| VM | Зависимости (все shared/Core/Infra/framework) |
|---|---|
| `MainWindowNotificationsVM` | MessageLog/PopUp/Modal NotificationsVMs (shared) + INotificationService |
| `ReportsControlVM` | IReportService, ITablesExportServiceFactory, DataStoragesCollectionVM, IApplicationCommandsVM, IRelayCommandFactory, IMapper/ILogger |
| `PhiladelphusRepositoryVM` | PhiladelphusRepositoryModel (Core), DataStoragesCollectionVM, IPhiladelphusRepositoryService, IFileDialogService; база MainEntityBaseVM (shared) |
| `PhiladelphusRepositoryHeaderVM` | PhiladelphusRepositoryHeaderModel (Core), IPhiladelphusRepositoryCollectionService, DataStorageVM, IConfigurationService, IOptions<…> |
| `PhiladelphusRepositoryHeadersCollectionVM` | IPhiladelphusRepositoryCollectionService, DataStoragesCollectionVM, IConfigurationService, IOptions<…> |
| `PhiladelphusRepositoryCollectionVM` | IPhiladelphusRepositoryCollectionService/Service, DataStoragesCollectionVM, IOptions<…>, IFileDialogService, IRelayCommandFactory, IServiceProvider* |
| `ViewModelsMappingProfile` (AutoMapper) | маппит Core-типы ↔ shared-VM (напр. ConnectionStringsContainer ↔ ConnectionStringsContainerVM) |

\* `PhiladelphusRepositoryCollectionVM` инъектит `IServiceProvider`, но в теле **нет** `GetService/GetRequiredService/ActivatorUtilities` с WPF-типами (скан чист). Перед переносом всё же глянуть `InitRepositoriesVMsCollection`.

Кластер `PhiladelphusRepository*VM` ссылается друг на друга — переносить вместе.

## Заблокированы переносом `RepositoryExplorerControlVM`

| VM | Блокер |
|---|---|
| `RepositoryFormulaBarVM` | ctor берёт `RepositoryExplorerControlVM` (WPF) |
| `FormulaTestControlVM` | ctor берёт `RepositoryExplorerControlVM` (WPF) |

## `RepositoryExplorerControlVM` (центральный хаб) — что держит

- `ChildCollectionTableBuilder` (WPF `Services/Tables`) — используется в теле (стр. ~727/731/816). Кандидат на вынос в shared, если билдер чистый (проверить транзитивно).
- Интерфейсы фабрик (`Wpf.UI.Factories.Interfaces`) — можно вынести интерфейсы в shared.
- `using …Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs` — разрешится переносом кластера `PhiladelphusRepository*VM`.

После выноса `ChildCollectionTableBuilder` + интерфейсов фабрик и переноса кластера — `RepositoryExplorerControlVM` можно переносить, что разблокирует `RepositoryFormulaBarVM`/`FormulaTestControlVM` и превью Excel-импорта (`ExcelImportPresentationPipeline`).

## Не найдены (вероятно уже в shared/удалены ранее)

`ParallelObservableCollectionVM`, `PropertyGridRepresentations`, `RepositoryExplorerRepresentations` — файлов по этим именам в проекте нет.

## Рекомендованный порядок переноса

1. **Кластер сущностей**: `PhiladelphusRepositoryVM` + `PhiladelphusRepositoryHeaderVM` + `PhiladelphusRepositoryHeadersCollectionVM` + `PhiladelphusRepositoryCollectionVM` (вместе, они взаимозависимы; чисты).
2. **Независимые чистые**: `MainWindowNotificationsVM`, `ReportsControlVM`, `ViewModelsMappingProfile`.
3. **Разблокировка explorer**: вынести `ChildCollectionTableBuilder` + интерфейсы фабрик в shared → перенести `RepositoryExplorerControlVM`.
4. **После explorer**: `RepositoryFormulaBarVM`, `FormulaTestControlVM`.

Правила переноса (раздел 7 хендоффа): файл переносится один-в-один (меняется только namespace), правки содержимого — отдельным коммитом; маленькими батчами с контрольной сборкой.
