# 15. Тех. долг миграции на Avalonia

Сводка отложенных задач и ограничений, накопленных при переносе UI с WPF (`Philadelphus.Presentation.Wpf.UI`) на Avalonia (`Philadelphus.Presentation.Avalonia`). Ветка `feature/#65575392-avalonia`.

Общие правила, которых придерживались при переносе:
- Код Avalonia максимально близок к WPF; разовые «заодно»-улучшения не вносим, а фиксируем здесь.
- Комментарии автора и закомментированные блоки переносятся (с адаптацией под Avalonia).
- WPF выводится из эксплуатации: проект пока не удаляем (до полного переноса нужного на Avalonia), но новые правки в WPF не вносим и его код не рефакторим — синхронизация паритета больше не требуется.
- Предпочитаем дефолтные стили Avalonia вместо точечных WPF-микрозначений.
- Конструкции, вынужденные платформой (`global::Avalonia.*`, `value!`, `object?` и т.п.), — норма; в долг идёт только опционально-косметическое.

Итог финального прохода: активных пунктов миграционного техдолга в разделах A, D–M не осталось. Оставшиеся упоминания — это журнал принятых решений или будущие задачи вне рамок миграции (формулы, полный async-рефакторинг Core-уведомлений, удаление WPF-проекта, обновление библиотек расширений, нагрузочное тестирование инфраструктуры).

---

## A. Лента (Ribbon.Avalonia) — детальная настройка

Лента `MainWindow` переведена с самодельного `TabControl` на библиотеку `Ribbon.Avalonia` 0.9.0 (форк dmitryzarubin, pre-1.0; namespace `Ribbon.Avalonia`, вкладки в `Ribbon.Tabs`, окно — обычный `Window`).

Сделано:
- Иконки кнопок (`Icon`=ControlTemplate с Image), единые размеры (Size=Medium, MaxSize=Large, MinSize=Small с иконкой / Medium без), все обычные `Button` в группах → `RibbonButton`/`RibbonToggleButton`.
- Авто-раскладка кнопок: контролы — прямые элементы `RibbonGroupBox`, лента сама раскладывает `WrapPanel`'ом и переносит (группы со смешанным контентом «Пересчёт»/«Текущее расширение», «Данные» через `ItemsSource`).
- **Сворачивание ленты** — встроенный шеврон в шапке (привязан к `IsCollapsed`), работает из коробки.
- **Кнопка «?»** (`HelpButtonCommand`) → открывает окно «О программе» (`AboutWindow`/`AboutWindowVM`).
- Цвета — тема-зависимые (Light/Dark).

Закрыто решением:
- Дополнительные возможности `Ribbon.Avalonia` (`QuickAccessToolbar`, `Ribbon.Menu`/app-menu/backstage, `KeyTip`/Alt-навигация, контекстные группы вкладок, `RibbonGroupBox.Command`/диалог-лончер) не берём в миграционный долг: это будущие продуктовые улучшения, если они понадобятся.
- Popup свёрнутой ленты по клику на вкладку не реализуем в рамках миграции: в библиотеке 0.9.0 соответствующий код закомментирован, минимального штатного workaround не найден.
- Локализацию внутренних строк/tooltip'ов библиотеки не дорабатываем сейчас: простое подключение `ru` не сработало, а глубокая кастомизация `Ribbon.Avalonia` нецелесообразна для текущего этапа.
- Версия pre-1.0 запинена (0.9.0) — при будущих апдейтах сверять breaking changes.

> **Тема формул вынесена в отдельную задачу** (строка формул, подсветка синтаксиса, формульные
> behaviors ячеек, значение атрибута, пересчёт) и в этот документ не входит — см.
> `docs/attribute-value-formula/`. Прежние разделы B и C перенесены туда (нумерация ниже сохранена).

## D. Типизированные редакторы значения базового листа

**Сделано.** `SystemBaseValueEditorTemplateSelector` выбирает редактор по `SystemBaseType`: строка / число / время / дата / дата-время / файл. Для DATE/DATETIME/TIME используются `DatePicker`/`TimePicker`; рассогласование типов (`DatePicker.SelectedDate` — `DateTimeOffset?`, модель — `DateTime?`) решается через `nullableDateTimeToOffsetConverter`.

## E. Таблица атрибутов и наследников (RepositoryExplorer)

- **Сделано:** подсветка «переопределено» (Moccasin) для «Значение» (`IsValueOverridden`) и «Присвоенные значения» (`AreValuesOverridden`) — через `boolToBrushConverter` на фоне ячейки + чёрный текст для читаемости в Dark (пустая часть параметра конвертера → `UnsetValue`, т.е. дефолт темы). Moccasin — статусный цвет (исключение из тематизации).
- **Исправлено: «слетали» значения «Область видимости»/«Переопределение» (баг #66708083).** В Avalonia колонки перенесли как `ComboBox` в `DataGridTemplateColumn` с `SelectedValue`+`SelectedValueBinding` (выбор по значению). При **рециклинге/виртуализации ячеек DataGrid** (добавление/прокрутка строк) выбор-по-значению не пере-резолвился против `ItemsSource` новой строки: при подмене `DataContext` биндинги (`ItemsSource`/`SelectedValue`/`SelectedValueBinding`) переоцениваются без гарантированного порядка, отложенный поиск элемента по значению не находит совпадения → `SelectedIndex=-1` → ComboBox пуст; внутренний маппинг `SelectedValue↔SelectedItem` кэширует «не найдено» и не перезапускает поиск до следующего рециклинга → значение «восстанавливалось». Данные в модели при этом не терялись (null в non-nullable enum не пишется) — рассинхрон только отображения. Соседняя колонка «Тип данных» багу не подвержена, т.к. выбирает **по ссылке** (`SelectedItem`): сравнение по идентичности переживает подмену `DataContext`.
  - **Решение (консолидировано, без дублей).** Введены свойства VM `SelectedVisibilityScope`/`SelectedOverrideType` (`ElementAttributeVM`) типа элемента списка (`VisibilityScopeItem`/`OverrideTypeItem`); геттер отдаёт элемент из списка по ссылке (стабильное выделение после рециклинга), сеттер игнорирует `null`-write при рециклинге (приём из M) и держит проверку состояния/нотификацию для области видимости. Старые enum-свойства `Visibility`/`Override` **удалены** из общего VM. Обе вьюхи переведены на выбор по ссылке: Avalonia — `ComboBox.SelectedItem`; WPF — `DataGridComboBoxColumn.SelectedItemBinding` (вместо `SelectedValueBinding`/`SelectedValuePath`). **Исключение из правила «WPF не трогаем»** — внесено по явному решению пользователя, чтобы убрать enum-свойства из shared-проекта; WPF-семантика сохранена (item-based выбор, как уже было у колонки «Тип данных»).
  - **Правило:** `SelectedValue`+`SelectedValueBinding` на `ComboBox` внутри рециклируемых ячеек Avalonia DataGrid использовать не следует — только item-based `SelectedItem`.
  - **Проверено и исправлено:** `ExcelImportDesignerWindow.axaml` больше не использует `SelectedValueBinding="{Binding Value}"` для скрытого выбора вида сущности; Avalonia-вьюха переведена на `SelectedItem="{Binding SelectedSheetEntityKindItem}"` с item-based фасадом в `ExcelImportDesignerVM`. Старое enum-свойство `SelectedSheetEntityKind` оставлено для WPF/совместимости.
- **Скрытые таблицы наследников — перенесены как заглушки (паритет с WPF).** «Перекрестная таблица значений», «Плоская таблица допустимых значений», «Перекрестная таблица допустимых значений» добавлены во вкладку «Наследники» как `IsVisible="False"`. Это **не отложенная миграция, а нереализованная фича**: в WPF они тоже `Visibility="Collapsed"` и пусты — у «перекрёстной таблицы значений» лишь статичный макет (три пустых `ComboBox` для осей + пустой `DataGrid` без привязок), две «допустимых значений» полностью пустые; в `RepositoryExplorerControlVM` нет ни источника данных, ни pivot-логики. Реальная реализация (pivot значений по осям + допустимые значения) — отдельная фича вне рамок миграции.

## F. Конструктор импорта Excel — вкладка «Диаграмма»

`ExcelImportDesignerWindow`: вкладки Листы/Связи/Предпросмотр перенесены. Вкладка **«Диаграмма»** переносится поэтапно (Avalonia `DiagramBehavior` — attached-property на `ScrollViewer`, `Content` = `LayoutTransformControl`+`Canvas`+`ScaleTransform`):

- **Этап 1 — сделано:** отрисовка карточек листов (заголовок, родитель, колонки), линии связей со стрелками, кнопка удаления связи (`ClearSheetRelation`), выбор листа кликом, зум по Ctrl+колесо с якорением под курсором.
- **Этап 2 — сделано:** перетаскивание карточек по `Canvas` (pointer capture, сохранение `CanvasX/Y`, live-обновление линий связей). Подсветка выбора обновляется «на месте», без ререндера (иначе пересоздание карточек ломает захват указателя).
- **Этап 3 — сделано:** создание/переназначение связи перетягиванием колонки-источника (родитель) на колонку-цель (ребёнок) — preview-линия, поиск цели по канва-rect колонок, `VM.ApplyRelationFromColumns`. Удаление связи — кнопкой на карточке дочернего листа.
- Фон диаграммы — на `ScrollViewer` (`#FAFAFA`), `Canvas` прозрачный (чтобы не было видимой «рамки» канвы внутри области).
- **Тематизация — сделано:** фон диаграммы и нейтральные цвета карточек/колонок/текста берутся из ресурсов Fluent (`SystemControlBackgroundChromeMediumLow/AltHigh`, `SystemControlForegroundBaseHigh/Medium/Low`) через `ThemeBrush(...)` с запасными кистями; акценты (связь — SeaGreen, выбор/preview — SteelBlue) и полупрозрачная заливка колонки-связи оставлены как статусные. Перекраска при смене темы — при следующей перерисовке диаграммы (не «вживую»).

## G. Журнал сообщений (MessageLog)

**Сделано.** Автопрокрутка к новым сообщениям и кратковременная подсветка новых строк перенесены — Avalonia `DataGridAutoScrollBehavior` (attached-property `IsEnabled`/`HighlightBrush`).

## H. Инфраструктура показа окон / sync-over-async

**Сделано.** Мост `UiSync` удалён:
- В интерфейсы `IFileDialogService`/`IMessageDialogService`/`IDialogService` добавлены `*Async`-методы (дефолт — обёртка поверх синхронных через default-interface-methods, чтобы WPF не править); Avalonia-реализации переопределяют их нативно-асинхронно.
- Вызыватели переведены на `await …Async` + `IAsyncRelayCommandFactory`/`AsyncRelayCommand`: `ExcelImportDesignerVM`, `ImportFromExcelVM`, `ImportExportControlVM`, `TreeLeaveVM`, `ApplicationSettingsControlVM`.
- Оставшиеся сообщения из синхронных границ (`INotificationService.ModalWindowHandler`, `ConfigurationService.UpdateConfigFile<T>`) больше не блокируют UI: они запускают async-показ сообщений без синхронного ожидания.
- Синхронные методы Avalonia-реализаций `IFileDialogService`/`IMessageDialogService`/`IDialogService` больше не имитируют блокирующую семантику поверх Avalonia async API и явно требуют использовать `*Async`.
- Smoke QA по H пройден вручную: файловые диалоги, Excel import-сценарии и модальные уведомления проверены после удаления моста.

**Остаток не считается миграционным долгом.** `INotificationService.ModalWindowHandler` и `NotificationService.ProcessNotification` остаются синхронными Core-контрактами (`delegate bool NotificationHandler(Notification)`). Полный перевод уведомлений на `Func<Notification, Task<bool>>` и async-цепочку отправителей — отдельный архитектурный рефакторинг Core/тестов, не требующийся для завершения Avalonia-миграции.

- `AvaloniaWindowService.ShowDialog` показывает окно модально, но без синхронного ожидания закрытия (Avalonia `ShowDialog` асинхронный; результат `bool?` сейчас не используется). Если понадобится результат диалога — переводить вызов в async.

## I. Динамические колонки DataGrid

Два независимых механизма:

- **`DataGridDataTableBehavior`** (источник — `DataTable`/`DataView`): строит колонки и привязывает ячейки через конвертер (имя колонки в параметре), т.к. Avalonia DataGrid не умеет авто-колонки из `DataTable`/`DataView`, а привязка по пути `[имя]` ломается на заголовках со спецсимволами. **Редактирование/типизация не нужны:** все потребители — read-only (`ExcelImportDesignerWindow` → `SheetPreview`, `ImportFromExcelWindow` → `PreviewView`, `ReportProcessor` → `ReportResult`), обратной записи в `DataTable` нет. Долгом не считаем.

- **`DataGridColumnsBehavior`** (источник — дескрипторы `ChildCollectionTableColumn`, плоская таблица наследников): ячейки уже редактируемые (combo/text TwoWay) и частично типизированы (даты `{0:yyyy-MM-dd HH:mm}`, цветная колонка состояния).
  - **Сделано (п.1):** подсветка «переопределено» (Moccasin) + тултип на атрибутных ячейках — `WrapWithOverride` биндит фон на `ValueOverrideStates[bindingKey]` (через `boolToBrushConverter`), тултип на `ValueOverrideToolTips[bindingKey]`, текст чёрным при переопределении. Оформление атрибутных ячеек унифицировано с таблицей атрибутов (E2): в покое — `Border` (Moccasin) + `TextBlock` с чёрным текстом; редактор появляется только в `CellEditingTemplate` (текст → `TextBox`, значения со списком → `ComboBox`). Это устранило плохой контраст в Dark, который был, когда combo показывался постоянно.
  - **Редактируемый ввод значения атрибута (формула/значение)** — вынесено в отдельную задачу, см. `docs/attribute-value-formula/`.

## J. Косметика кода

**Закрыто для миграции.** Конвертеры и behaviors помечены `sealed`; в `ConvertBack` конвертеров `NotImplementedException`→`NotSupportedException` (семантически «не поддерживается»). `NotImplementedException` оставлен только там, где это реальный «ещё не реализовано» (напр. `InfrastructureRepositoryFactory`). Дополнительная стилистическая полировка (`?`/`!`/`=>`-стиль и прочие мелочи по VM/сервисам) не считается миграционным долгом и может выполняться только как обычный opportunistic cleanup.

## K. Прочее

- `KafkaConsumer.Dispose`: убран двойной `Close` (graceful Close делается в `ExecuteAsync.finally`) — зафиксировано как осознанное изменение; нагрузочный тест Kafka относится к инфраструктурной проверке, а не к миграционному долгу Avalonia.
- Тема: **реализовано** — переключатель Светлая/Тёмная/Как в системе (вкладка ленты «Вид» → группа «Тема»), дефолт «Как в системе»; применение `RequestedThemeVariant`, сохранение в `AppearanceConfig:ThemeString` (presentation-конфиг, не Core). Жёсткие светлые цвета (фоны/рамки/вторичный текст) переведены на `DynamicResource` Fluent. **Dark QA пройден вручную по экранам.**
- Иконки: **SVG spike расширен на используемые Avalonia-иконки, визуальный QA пройден вручную** — не-логотипные PNG-иконки в Ribbon, MessageLog, LaunchWindow, RepositoryExplorer/Attributes/FormulaBar и дереве репозитория заменены на минималистичные SVG из `Assets/Icons/svg/light` и `Assets/Icons/svg/dark`; цвет задаётся прямо в SVG-файлах, `ThemedSvg` строит путь по `Icon` и переключает его при смене `ActualThemeVariant`, динамические `AppIcon`-привязки идут через `IconNameConverter`. Логотип приложения (`philadelphus_logo_64.png`/`.ico`) остаётся PNG. Миграционный долг по PNG→SVG закрыт; дальнейшая замена временных минималистичных SVG на финальные дизайнерские версии — отдельная продуктовая полировка.

## L. Лента — DataContext групп vs RibbonTab

`Ribbon.Avalonia` выносит группы выбранной вкладки в свой контент, поэтому `DataContext`, заданный на `RibbonTab`, **до групп не наследуется** — биндинги внутри `RibbonGroupBox` резолвятся против `DataContext` ленты. Рабочий приём: задавать `DataContext` на самой `RibbonGroupBox` (как во вкладках «Формулы»→«Пересчет», «Вид»→«Тема», «Расширения»). **Исправлено** для всех затронутых вкладок, включая «Расширения» (`DataContext="{Binding ExtensionsControlVM}"` перенесён с `RibbonTab` на обе группы). На текущий момент список расширений пуст, потому что библиотеки расширений устарели после рефакторинга; их обновление и повторная проверка раскрытия — отдельная задача вне Avalonia-миграции.

## M. Два списка на один SelectedItem (стартовое окно)

В `FavoriteAndLastRepositoryHeaders` два `ListBox` («Избранные» и «Недавние») привязаны TwoWay к одному `RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM`. При выборе элемента в одном списке второй снимает свой выбор (элемента в нём нет) и пишет обратно `null`, затирая только что выбранное значение → кнопка «Выбрать и начать работу!» (её `CanExecute` смотрит на `IsPhiladelphusRepositoryAvailable` выбранного) становилась неактивной для неизбранных заголовков. **Исправлено** ранним выходом в сеттере `SelectedPhiladelphusRepositoryHeaderVM` при `value == null` (паразитный сброс игнорируется; правка в shared, действует и в WPF — там безвредна). Побочный эффект: выбор нельзя обнулить через UI (для стартового окна приемлемо, никто на это не полагается). Чистая альтернатива на будущее — отдельный `SelectedItem` на каждый список + производное «эффективное выделение».
