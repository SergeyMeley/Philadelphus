# 10 — Рефакторинг ExcelImportDesignerWindow: вынос логики из code-behind в MVVM

> Документ-план. Закрывает последнее место Этапа 3.2 (B) — `ImportExportControlVM` ссылается на
> `ExcelImportDesignerWindow` и открывает его напрямую. Окно — это 1510 строк code-behind, поэтому
> простая абстракция открытия недостаточна: принято решение **полностью исключить code-behind** и
> перевести окно на MVVM, чтобы и `ImportExportControlVM`, и сам дизайнер стали переносимыми.

## 1. Контекст и цель

- Ветка `feature/#65575392-avalonia`. Предыдущие шаги B: `RepositoryExplorerControlVM` и
  `MainWindowVM` уже переведены на `IWindowService` (см. `09-handoff-portability.md`, раздел 8.B).
- Остаток шага B — `ImportExportControlVM.ImportFromExcelDesigner()`:
  `GetRequiredService<ExcelImportDesignerWindow>()` → `window.Initialize(4 арг.)` → `ShowDialog()`,
  плюс `using …Wpf.UI.Views.Windows`.
- Окно нельзя открыть через стандартный `IWindowService` (реестр VM→окно + `DataContext`), потому что
  это не MVVM-окно: `DataContext = this`, императивная пост-инициализация `Initialize(...)`,
  всё состояние и обработчики — в code-behind.
- **Решение пользователя:** не вводить промежуточный сервис-обёртку, а отрефакторить окно —
  исключить весь code-behind, вынеся логику в VM / `Infrastructure.ImportExport.Excel` /
  `Core.Domain.ImportExport`, в крайнем случае — в `AttachedBehavior`.

## 2. Текущее состояние окна

- `Views/Windows/ExcelImportDesignerWindow.xaml` — 333 строки.
- `Views/Windows/ExcelImportDesignerWindow.xaml.cs` — 1510 строк.
- Конструктор принимает через DI: `IServiceProvider`, `ConversionService`, `ExcelPreviewService`,
  `IExcelImportSchemaBuilder`, `IExcelImportSchemaTemplateStorage`, `ExcelImportPipeline`,
  `ExcelImportPresentationPipeline`. В ctor — `DataContext = this`.
- Пост-инициализация (рантайм-данные):
  `Initialize(ShrubModel shrub, PhiladelphusRepositoryModel repository, IPhiladelphusRepositoryService repositoryService, Action refreshRepositoryView)`.
- **Важно:** вся доменная логика уже вынесена в Infrastructure-хелперы и сервисы
  (`ExcelImportSchemaBuilder/Normalizer`, `ExcelImportProfileEditorHelper`, `ExcelPreviewService`/`ExcelPreviewTableBuilder`,
  `ExcelImportPipeline`, `ExcelImportPresentationPipeline`, `ExcelImportSchemaTemplateStorage`).
  Code-behind в основном **оркеструет** эти сервисы и **императивно** связывает данные с именованными
  контролами (`ItemsSource=`, `SelectedItem=`, `.Items.Refresh()`, `.Text=`), а также рисует диаграмму.

## 3. Классификация ответственностей (1510 строк → целевой слой)

| Кластер | Представители | Куда |
|---|---|---|
| Состояние | `_schema`, `_currentSheet`, `_selectedFilePath`, `_workbookPreview`, флаги `_isUpdating*`, `CompletedImport` | свойства **VM** |
| Оркестрация схемы | `LoadWorkbook/LoadSchema/LoadSchemaFromWorkbook`, `BindSchema`, `BindCurrentSheet`, `BindRelationEditor`, `RefreshRelationViews/ParentKeyOptions/Ui`, `*SelectionChanged`, `TxtSheetDisplayName_TextChanged`, `Apply/ClearSheetRelation`, `TryApplyRelation*`, `Btn(Add/Remove)Relation`, `ApplySheetEntityKind`, `Sync*Schema*`, `TrySyncSchemaFromImportParameters`, `TryGetImportParameters`, `BtnImport`, `BtnRefreshPreview`, `Btn(Load/Save)Template`, `ChkCreateNewRoot_*`, `GetSheet`, `ClearPreviewResult` | **VM** (делегирует в те же Infrastructure-сервисы) |
| Диалоги | `OpenFileDialog` (выбор файла, load/save шаблона), `MessageBox.Show` | `IFileDialogService` + `IMessageDialogService` (уже в shared) |
| **Диаграмма: визуал+ввод** | `RenderDiagram`, `ArrangeDiagramCardsByGrid`, `Add/UpdateRelationVisual(s)`, `Create/Update/RemoveRelationPreviewLine`, `CreateArrowHead(Points)`, `Get(Border/Element)Center`, `GetCanvas(Left/Top)`, `FindDiagramColumnDropTarget`, `IsPointInsideElement`, `IsInsideButton`, `BeginColumnRelationDrag`, `DiagramCard_Mouse*`, `DiagramScrollViewer_PreviewMouseWheel` (zoom), `ResetDiagramDragState` | **остаётся в View** (рисование на `Canvas`: `Line/Polygon/Border`, `Point`, mouse capture, `ScaleTransform`) — переносу в чистый VM не подлежит |

Именованные контролы, к которым обращается code-behind (станут биндингами/командами):
`TxtRootName/FilePath/SheetDisplayName/CurrentSheetSource/SheetPreviewInfo/PreviewSummary`,
`LstSchemaSheets`, `DgSheetColumns/DgSheetPreview/DgRelations`,
`CmbExistingRoots/SheetKeyColumn/SheetEntityKind/RelationChildSheet/RelationParentSheet/RelationParentKey/RelationChildKey`,
`ChkCreateNewRoot`, `Btn*`, `DiagramCanvas/DiagramScrollViewer/DiagramCanvasScaleTransform`.

## 4. Целевая архитектура

- Новый **`ExcelImportDesignerVM`** (в `Philadelphus.Presentation`, shared): держит состояние, свойства
  (наблюдаемые коллекции листов/связей/колонок, выбранные элементы, тексты), команды
  (`SelectFile`, `Add/RemoveRelation`, `RefreshPreview`, `Import`, `Load/SaveTemplate`, `Close`),
  делегирует в Infrastructure-сервисы (те же, что в окне) + `IFileDialogService`/`IMessageDialogService`.
- Рантайм-данные (`shrub`, `repository`, `repositoryService`, `refreshRepositoryView`) и DI-сервисы
  смешаны → создавать VM через **фабрику** `IExcelImportDesignerVMFactory.Create(...)`
  (паттерн как `IMainWindowVMFactory`).
- **Диаграмма** — единственная часть, что остаётся во View. VM владеет *моделью* диаграммы
  (карточки с координатами X/Y, связи «родительская колонка → дочерняя»); рисование и обработка
  мыши/зума — либо `AttachedBehavior`, либо data-templated `Canvas`. **Решение отложено до Фазы 3**
  (см. раздел 6).
- Открытие: окно становится VM-driven → `Register<ExcelImportDesignerVM, ExcelImportDesignerWindow>`,
  `ImportExportControlVM` открывает через `_windowService.ShowDialog(factory.Create(...))`; прямая
  ссылка на `Views.Windows` уходит.

## 5. Фазы (каждая — отдельный коммит, отдельная контрольная сборка)

**Фаза 1 — каркас VM (без подключения).** Создать `ExcelImportDesignerVM` в shared: состояние,
свойства, команды; перенести非-визуальные методы (оркестрация + диалоги) с делегированием в
Infrastructure-сервисы (через обязательные ctor-параметры, `ArgumentNullException.ThrowIfNull`).
XAML/окно пока не трогаем. Результат: shared компилируется, VM ещё не используется.
Параллельно — `IExcelImportDesignerVMFactory` + WPF-реализация + регистрация в DI.

**Фаза 2 — перевод XAML на VM.** `DataContext` окна = `ExcelImportDesignerVM`; заменить весь
императив (`ItemsSource/SelectedItem/Text/.Items.Refresh()`, `Btn*_Click`) на биндинги и команды;
удалить перенесённый code-behind. После фазы в code-behind остаётся только кластер диаграммы.
Тестируемо: всё, кроме диаграммы, работает по MVVM.

**Фаза 3 — диаграмма.** По выбранному в разделе 6 подходу вынести рисование + drag/zoom; удалить
остаток code-behind. В окне остаётся `InitializeComponent()` (пустой ctor).

**Фаза 4 — открытие через IWindowService.** `Register<ExcelImportDesignerVM, ExcelImportDesignerWindow>`;
`ImportExportControlVM`: инъекция `IWindowService` + `IExcelImportDesignerVMFactory`, тело
`ImportFromExcelDesigner()` → `_windowService.ShowDialog(factory.Create(shrub, repo, service, refresh))`;
убрать `using …Views.Windows`. **Исходная цель шага B закрыта.**

## 6. Открытое решение (перед Фазой 3): реализация диаграммы

- **AttachedBehavior** — инкапсулировать текущее рисование/drag/zoom в behavior, управляемый
  состоянием/командами VM. Ближе к существующему императивному коду, ниже риск регрессий.
- **Data-templated Canvas** — `ItemsControl` поверх `Canvas` + `DataTemplate` для карточек,
  координаты/связи через биндинги. Максимальный MVVM, но больше переписывания XAML и риска.

Зафиксировать выбор здесь перед началом Фазы 3.

## 7. Рабочие правила (напоминание)

- Записи — при **закрытой Visual Studio** (иначе NUL-порча/обрезка через монтирование; уже ловили дважды,
  лечится перезапуском сессии Cowork — после рестарта реальное состояние диска цело).
- Сборка/git-запись — на **Windows**. `git reset` не делать; откат — новой правкой. Коммиты делает
  пользователь; Claude пишет текст коммита.
- Маленькие батчи, контрольная сборка пользователя после каждого. Сервисы — обязательные ctor-параметры
  (DI) с `ArgumentNullException.ThrowIfNull`. `using` — сразу в отсортированную позицию, alias-using в конце.

## 8. Прогресс

- [ ] Фаза 1 — `ExcelImportDesignerVM` (каркас) + фабрика
- [ ] Фаза 2 — XAML на VM, удалить перенесённый code-behind
- [ ] Решение по диаграмме (раздел 6)
- [ ] Фаза 3 — диаграмма, удалить остаток code-behind
- [ ] Фаза 4 — открытие через IWindowService, очистить ImportExportControlVM
