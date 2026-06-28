# 15. Тех. долг миграции на Avalonia

Сводка отложенных задач и ограничений, накопленных при переносе UI с WPF (`Philadelphus.Presentation.Wpf.UI`) на Avalonia (`Philadelphus.Presentation.Avalonia`). Ветка `feature/#65575392-avalonia`.

Общие правила, которых придерживались при переносе:
- Код Avalonia максимально близок к WPF; разовые «заодно»-улучшения не вносим, а фиксируем здесь.
- Комментарии автора и закомментированные блоки переносятся (с адаптацией под Avalonia).
- WPF и Avalonia держим синхронно: правки паритета применяем к обоим проектам.
- Предпочитаем дефолтные стили Avalonia вместо точечных WPF-микрозначений.
- Конструкции, вынужденные платформой (`global::Avalonia.*`, `value!`, `object?` и т.п.), — норма; в долг идёт только опционально-косметическое.

---

## A. Лента (Ribbon.Avalonia) — детальная настройка

Лента `MainWindow` переведена с самодельного `TabControl` на библиотеку `Ribbon.Avalonia` 0.9.0 (форк dmitryzarubin, pre-1.0; namespace `Ribbon.Avalonia`, вкладки в `Ribbon.Tabs`, окно — обычный `Window`). Базовая структура рабочая. Осталось:

- Иконки кнопок: `RibbonButton` сейчас текстовые; добавить `LargeIcon`/`Icon` и осмысленные `MinSize`/`MaxSize`.
- Заменить обычные `Button`/`ToggleButton`/`ComboBox`/`ItemsControl` внутри групп на риббон-контролы (`RibbonButton`/`RibbonToggleButton`/`RibbonSplitButton`/`Gallery`), где уместно.
- Группы со смешанным контентом («Пересчёт», «Текущий репозиторий», «Расширения») — выверить вёрстку.
- `Ribbon.Menu` (app-menu/backstage) и QuickAccessToolbar — не задействованы; решить, нужны ли.
- `KeyTip`/навигация по Alt, `HelpButton` (`?`), сворачивание ленты, контекстные группы вкладок, `RibbonGroupBox.Command` (диалог-лончер) — восстановить при необходимости.
- Локализация (сейчас `Locale/en-ca.xaml`), проверка темы Light/Dark.
- Версия библиотеки pre-1.0 запинена (0.9.0); при апдейте сверять breaking changes.

## B. Формульные взаимодействия с таблицами (Avalonia DataGrid)

Строка формул (`FormulaBar`) и текст-бокс-behaviors (`FormulaBarTextBoxBehavior`, `FormulaSuggestionTextBoxBehavior`) перенесены и работают; отложенная пересборка по уходу фокуса (`DataGridLostKeyboardFocusCommandBehavior`) работает. НО три WPF-behavior'а, завязанных на ячейки `DataGrid`, **не реализуемы публичным API Avalonia** (`DataGridCell.OwningColumn`/`OwningRow`/`ColumnIndex` — internal; нет `CurrentCell`/`SelectedCells`/фокуса ячеек):

- `DataGridFormulaCellSelectionBehavior` — вставка ссылки на ячейку кликом (определение колонки/строки → `ChildFormulaCellSelection`).
- `DataGridFormulaReferenceHighlightBehavior` — подсветка выбранных ячеек-ссылок рамкой с фильтром «только атрибутные / колонка Значение».
- `DataGridCellFocusRequestBehavior` — возврат фокуса в конкретную ячейку колонки.

Варианты: (а) reflection к internal-членам (хрупко); (б) собственный контрол/тема ячейки с публичными хуками; (в) альтернативный UX вставки ссылок (через окно редактора формул). Пока формулы пишутся вручную в строке формул/редакторе.

## C. Подсветка синтаксиса в строке формул (FormulaBar)

Цветной оверлей сегментов формулы (прозрачный TextBox поверх раскрашенного `ItemsControl`) в `FormulaBar` **не перенесён** — приём требует жёстких цветов и ломал тёмную тему (текст становился невидимым). В редакторе формул (`FormulaTestControl`) подсветка есть (отдельной панелью, через конвертеры `formulaKindToBrushConverter`/`formulaKindToFontWeightConverter`/`boolToBrushConverter`). Долг: вернуть подсветку в `FormulaBar` тема-зависимой (цвета через ресурсы темы, а не хардкод).

## D. Типизированные редакторы значения базового листа

`SystemBaseValueEditorTemplateSelector` выбирает редактор по `SystemBaseType`: строка / число / время / файл. Типы **DATE и DATETIME пока редактируются строкой** (правка `StringValue`): у Avalonia `DatePicker.SelectedDate` тип `DateTimeOffset?`, что требует согласования с `DateValue`/`DateTimeDateValue` (`DateTime?`). Долг: вернуть `DatePicker`/типизированные редакторы.

## E. Таблица атрибутов и наследников (RepositoryExplorer)

- Редактируемый `ComboBox` значения с вводом формулы (`Text=FormulaValueText`) — в Avalonia `ComboBox` не editable; сейчас обычный выбор из списка. Долг: AutoCompleteBox или иной редактируемый вариант.
- Подсветка «переопределено» (Moccasin, `IsValueOverridden`/`AreValuesOverridden`/`ValueOverrideStates`) в таблицах «Атрибуты» и наследников — не перенесена (требует per-cell стиля по данным).

## F. Конструктор импорта Excel — вкладка «Диаграмма»

`ExcelImportDesignerWindow`: вкладки Листы/Связи/Предпросмотр перенесены. Вкладка **«Диаграмма»** переносится поэтапно (Avalonia `DiagramBehavior` — attached-property на `ScrollViewer`, `Content` = `LayoutTransformControl`+`Canvas`+`ScaleTransform`):

- **Этап 1 — сделано:** отрисовка карточек листов (заголовок, родитель, колонки), линии связей со стрелками, кнопка удаления связи (`ClearSheetRelation`), выбор листа кликом, зум по Ctrl+колесо с якорением под курсором.
- **Этап 2 — в работе:** перетаскивание карточек по `Canvas` (pointer capture, сохранение `CanvasX/Y`, live-обновление линий).
- **Этап 3 — в работе:** создание/переназначение связи перетягиванием колонки-источника на колонку-цель (preview-линия, hit-test визуального дерева Avalonia, `VM.ApplyRelationFromColumns`).
- Цвета карточек пока жёсткие (как в WPF) — тематизация под Dark вместе с общим долгом темы.

## G. Журнал сообщений (MessageLog)

Автопрокрутка к новым сообщениям и подсветка новой строки (WPF `DataGridAutoScrollBehavior`, ~356 строк) — **не перенесены**. Долг: реализовать автоскролл Avalonia `DataGrid` к последнему элементу + временную подсветку.

## H. Инфраструктура показа окон / sync-over-async

- `UiSync.RunSync` переведён на вложенный цикл диспетчера (`DispatcherFrame` + `Dispatcher.PushFrame`) вместо busy-loop `RunJobs()` — корректно. Но это всё ещё «синхронно поверх асинхронного». Долг-кандидат: перевести `IFileDialogService`/`IDialogService`/`IWindowService.ShowDialog` на async-интерфейсы и убрать мост.
- `AvaloniaWindowService.ShowDialog` показывает окно модально, но без синхронного ожидания закрытия (Avalonia `ShowDialog` асинхронный; результат `bool?` сейчас не используется). Если понадобится результат диалога — переводить вызов в async.

## I. Динамические колонки DataGrid из DataTable/DataView

`DataGridDataTableBehavior` строит колонки и привязывает ячейки через конвертер (имя колонки в параметре), т.к. Avalonia DataGrid не умеет авто-колонки из `DataTable`/`DataView`, а привязка по пути `[имя]` ломается на заголовках со спецсимволами. Это рабочее решение; долг — при желании сделать редактируемые ячейки/типизацию (сейчас только чтение строкой).

## J. Косметика кода (опционально, отложено по договорённости)

При переносе НЕ вносили «заодно»-изменения стиля: добавление `sealed`, замена `NotImplementedException`→`NotSupportedException`, переход на `?`/`!`/`=>`-стиль вместо `{}` и т.п. Платформенно-вынужденные конструкции оставлены как есть. При желании — отдельный проход по стилю (синхронно с WPF).

## K. Прочее

- `KafkaConsumer.Dispose`: убран двойной `Close` (graceful Close делается в `ExecuteAsync.finally`) — зафиксировано как осознанное изменение, проверить при нагрузочном тесте.
- Тема: **реализовано** — переключатель Светлая/Тёмная/Как в системе (вкладка ленты «Вид» → группа «Тема»), дефолт «Как в системе»; применение `RequestedThemeVariant`, сохранение в `AppearanceConfig:ThemeString` (presentation-конфиг, не Core). Жёсткие светлые цвета (фоны/рамки/вторичный текст) переведены на `DynamicResource` Fluent. Оставшийся долг: подсветка синтаксиса в `FormulaBar` тема-зависимой (см. C); полный визуальный QA всех экранов в Dark; спайк иконок PNG→SVG (иконки сейчас растровые, в Dark не инвертируются).

## L. Лента — DataContext групп vs RibbonTab

`Ribbon.Avalonia` выносит группы выбранной вкладки в свой контент, поэтому `DataContext`, заданный на `RibbonTab`, **до групп не наследуется** — биндинги внутри `RibbonGroupBox` резолвятся против `DataContext` ленты. Рабочий приём: задавать `DataContext` на самой `RibbonGroupBox` (как во вкладках «Формулы»→«Пересчет», «Вид»→«Тема», «Расширения»). **Исправлено** для всех затронутых вкладок, включая «Расширения» (`DataContext="{Binding ExtensionsControlVM}"` перенесён с `RibbonTab` на обе группы). На текущий момент список расширений всё равно пуст — библиотеки расширений устарели после рефакторинга; раскрытие проверить после их обновления.

## M. Два списка на один SelectedItem (стартовое окно)

В `FavoriteAndLastRepositoryHeaders` два `ListBox` («Избранные» и «Недавние») привязаны TwoWay к одному `RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM`. При выборе элемента в одном списке второй снимает свой выбор (элемента в нём нет) и пишет обратно `null`, затирая только что выбранное значение → кнопка «Выбрать и начать работу!» (её `CanExecute` смотрит на `IsPhiladelphusRepositoryAvailable` выбранного) становилась неактивной для неизбранных заголовков. **Исправлено** ранним выходом в сеттере `SelectedPhiladelphusRepositoryHeaderVM` при `value == null` (паразитный сброс игнорируется; правка в shared, действует и в WPF — там безвредна). Побочный эффект: выбор нельзя обнулить через UI (для стартового окна приемлемо, никто на это не полагается). Чистая альтернатива на будущее — отдельный `SelectedItem` на каждый список + производное «эффективное выделение».
