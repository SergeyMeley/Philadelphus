# 15. Тех. долг миграции на Avalonia

Сводка отложенных задач и ограничений, накопленных при переносе UI с WPF (`Philadelphus.Presentation.Wpf.UI`) на Avalonia (`Philadelphus.Presentation.Avalonia`). Ветка `feature/#65575392-avalonia`.

Общие правила, которых придерживались при переносе:
- Код Avalonia максимально близок к WPF; разовые «заодно»-улучшения не вносим, а фиксируем здесь.
- Комментарии автора и закомментированные блоки переносятся (с адаптацией под Avalonia).
- WPF выводится из эксплуатации: проект пока не удаляем (до полного переноса нужного на Avalonia), но новые правки в WPF не вносим и его код не рефакторим — синхронизация паритета больше не требуется.
- Предпочитаем дефолтные стили Avalonia вместо точечных WPF-микрозначений.
- Конструкции, вынужденные платформой (`global::Avalonia.*`, `value!`, `object?` и т.п.), — норма; в долг идёт только опционально-косметическое.

---

## A. Лента (Ribbon.Avalonia) — детальная настройка

Лента `MainWindow` переведена с самодельного `TabControl` на библиотеку `Ribbon.Avalonia` 0.9.0 (форк dmitryzarubin, pre-1.0; namespace `Ribbon.Avalonia`, вкладки в `Ribbon.Tabs`, окно — обычный `Window`).

Сделано:
- Иконки кнопок (`Icon`=ControlTemplate с Image), единые размеры (Size=Medium, MaxSize=Large, MinSize=Small с иконкой / Medium без), все обычные `Button` в группах → `RibbonButton`/`RibbonToggleButton`.
- Авто-раскладка кнопок: контролы — прямые элементы `RibbonGroupBox`, лента сама раскладывает `WrapPanel`'ом и переносит (группы со смешанным контентом «Пересчёт»/«Текущее расширение», «Данные» через `ItemsSource`).
- **Сворачивание ленты** — встроенный шеврон в шапке (привязан к `IsCollapsed`), работает из коробки.
- **Кнопка «?»** (`HelpButtonCommand`) → открывает окно «О программе» (`AboutWindow`/`AboutWindowVM`).
- Цвета — тема-зависимые (Light/Dark).

Осталось (опционально):
- **QuickAccessToolbar** — в 0.9.0 штатно не поддержан: в шаблоне `Ribbon` нет места под QAT, код добавления через контекстное меню (`pinToQat`/`_rightClicked` в `Ribbon.cs`) закомментирован. Нужна кастомная реализация — отложено по решению пользователя.
- `Ribbon.Menu` (app-menu/backstage) — не задействован; отложено.
- `KeyTip`/навигация по Alt, контекстные группы вкладок, `RibbonGroupBox.Command` (диалог-лончер) — при необходимости.
- При свёрнутой ленте клик по вкладке не открывает popup (соответствующий код в библиотеке 0.9.0 закомментирован).
- Локализация (`Locale/en-ca.xaml`). Версия pre-1.0 запинена (0.9.0) — при апдейте сверять breaking changes.

## B. Формульные взаимодействия с таблицами (Avalonia DataGrid)

Строка формул (`FormulaBar`) и текст-бокс-behaviors (`FormulaBarTextBoxBehavior`, `FormulaSuggestionTextBoxBehavior`) перенесены и работают; отложенная пересборка по уходу фокуса (`DataGridLostKeyboardFocusCommandBehavior`) работает. НО три WPF-behavior'а, завязанных на ячейки `DataGrid`, **не реализуемы публичным API Avalonia** (`DataGridCell.OwningColumn`/`OwningRow`/`ColumnIndex` — internal; нет `CurrentCell`/`SelectedCells`/фокуса ячеек):

- `DataGridFormulaCellSelectionBehavior` — вставка ссылки на ячейку кликом (определение колонки/строки → `ChildFormulaCellSelection`).
- `DataGridFormulaReferenceHighlightBehavior` — подсветка выбранных ячеек-ссылок рамкой с фильтром «только атрибутные / колонка Значение».
- `DataGridCellFocusRequestBehavior` — возврат фокуса в конкретную ячейку колонки.

Варианты: (а) reflection к internal-членам (хрупко); (б) собственный контрол/тема ячейки с публичными хуками; (в) альтернативный UX вставки ссылок (через окно редактора формул). Пока формулы пишутся вручную в строке формул/редакторе.

## C. Подсветка синтаксиса в строке формул (FormulaBar)

**Сделано.** Подсветка наложена **прямо в поле ввода** строки формул: раскрашенный `TextBlock` (инлайны через `FormulaHighlightOverlayBehavior.Segments`, цветные `Run` по сегментам `FormulaHighlightSegments`) лежит ПОД редактируемым `TextBox`, у которого текст прозрачный при активной подсветке (`Foreground` через `boolToBrushConverter` c параметром `Transparent|` от `IsFormulaHighlightOpen`; каретка — `CaretBrush`). Старый WPF-оверлей ломал тёмную тему из-за жёстких цветов — здесь цвета тема-зависимы. Данные сегментов VM (`RepositoryFormulaBarVM`) уже строил.

Совпадение переносов оверлея и `TextBox` обеспечивается одинаковыми шрифтом/размером/`Padding` и **единым весом шрифта** (вес по виду сегмента в оверлее намеренно не варьируется — иначе ширины символов разойдутся и переносы поедут).

Цвета — **тема-зависимы**: `FormulaHighlightKindToBrushConverter` держит две палитры (светлая «VS» / тёмная «VS Code Dark+»), выбор по `Application.Current.ActualThemeVariant` (общий статический `GetBrush`, используется и конвертером, и behavior'ом). Перекраска применяется при следующей пересборке сегментов (на ввод/смену курсора), «вживую» при смене темы не перекрашивается — приемлемо (тот же приём, что в диаграмме F).

Поле формулы сделано **многострочным с авто-переносом** (`TextWrapping=Wrap`, `AcceptsReturn=False` — `Enter` применяет формулу, `Escape` отменяет; высота растёт по содержимому). Подсказка по сигнатуре функции вынесена в **сквозную строку на всю ширину** (`Grid.Row=1`, `ColumnSpan=6`), раньше висела под кнопкой «открыть редактор» и распирала её колонку.

Парная скобка: новый `FormulaMatchingParenthesisToBrushConverter` (светлая `#D7ECFF` / тёмная `#3D5A80`) используется в редакторе формул (`FormulaTestControl`, отдельная панель сегментов). В самом `FormulaBar` фон у инлайнового `Run` недоступен, поэтому парная скобка под курсором помечается **подчёркиванием** (`TextDecorations.Underline` в `FormulaHighlightOverlayBehavior`) — ширину символов не меняет, переносы не сдвигаются.

Нюансы оверлея:
- Фон поля `TextBox` переопределён на `Transparent` во ВСЕХ состояниях (`TextControlBackground`/`PointerOver`/`Focused`/`Disabled` в `TextBox.Resources`): иначе при фокусе Fluent подменяет фон внутренней рамки на непрозрачный `TextControlBackgroundFocused` и перекрывает оверлей (текст «исчезал» в Dark при установке курсора, проявлялся при потере фокуса окном).
- Автодополнение (`UpdateFormulaSuggestions` в `RepositoryFormulaBarVM`) показывается только при длине набираемого префикса имени формулы ≥ 2 символов — пустой/однобуквенный префикс открывал полный список и перекрывал подсказку по аргументам функции. Подсказка по сигнатуре от порога не зависит.
- Автодополнение подавляется внутри строкового литерала (`IsCaretInsideStringLiteral` через `FormulaTokenizer`) — в текстовой части формулы имена формул не предлагаются (незавершённый литерал считается «внутри» до конца текста).

## D. Типизированные редакторы значения базового листа

**Сделано.** `SystemBaseValueEditorTemplateSelector` выбирает редактор по `SystemBaseType`: строка / число / время / дата / дата-время / файл. Для DATE/DATETIME/TIME используются `DatePicker`/`TimePicker`; рассогласование типов (`DatePicker.SelectedDate` — `DateTimeOffset?`, модель — `DateTime?`) решается через `nullableDateTimeToOffsetConverter`.

## E. Таблица атрибутов и наследников (RepositoryExplorer)

- **Сделано:** редактируемый ввод значения — штатный editable `ComboBox` (Avalonia 11.3: `IsEditable`+`Text`). Всё идёт через `Text=FormulaValueText` (TwoWay, `UpdateSourceTrigger=LostFocus`), без `SelectedItem`: связка `SelectedItem`+`Text` сбрасывала значение в null при ручном вводе. Выбор из списка пишет в текст «[uuid]» через `TextSearch.TextBinding="{Binding Uuid, StringFormat='[{0}]'}"`, а сеттер `FormulaValueText` присваивает значение по ссылке (`TryGetLeafUuidReference`); ввод формул/значений — тоже через `Text`. (`AutoCompleteBox` отверг — нет выпадашки.)
- **Сделано:** подсветка «переопределено» (Moccasin) для «Значение» (`IsValueOverridden`) и «Присвоенные значения» (`AreValuesOverridden`) — через `boolToBrushConverter` на фоне ячейки + чёрный текст для читаемости в Dark (пустая часть параметра конвертера → `UnsetValue`, т.е. дефолт темы). Moccasin — статусный цвет (исключение из тематизации).
- **Исправлено: «слетали» значения «Область видимости»/«Переопределение» (баг #66708083).** В Avalonia колонки перенесли как `ComboBox` в `DataGridTemplateColumn` с `SelectedValue`+`SelectedValueBinding` (выбор по значению). При **рециклинге/виртуализации ячеек DataGrid** (добавление/прокрутка строк) выбор-по-значению не пере-резолвился против `ItemsSource` новой строки: при подмене `DataContext` биндинги (`ItemsSource`/`SelectedValue`/`SelectedValueBinding`) переоцениваются без гарантированного порядка, отложенный поиск элемента по значению не находит совпадения → `SelectedIndex=-1` → ComboBox пуст; внутренний маппинг `SelectedValue↔SelectedItem` кэширует «не найдено» и не перезапускает поиск до следующего рециклинга → значение «восстанавливалось». Данные в модели при этом не терялись (null в non-nullable enum не пишется) — рассинхрон только отображения. Соседняя колонка «Тип данных» багу не подвержена, т.к. выбирает **по ссылке** (`SelectedItem`): сравнение по идентичности переживает подмену `DataContext`.
  - **Решение (консолидировано, без дублей).** Введены свойства VM `SelectedVisibilityScope`/`SelectedOverrideType` (`ElementAttributeVM`) типа элемента списка (`VisibilityScopeItem`/`OverrideTypeItem`); геттер отдаёт элемент из списка по ссылке (стабильное выделение после рециклинга), сеттер игнорирует `null`-write при рециклинге (приём из M) и держит проверку состояния/нотификацию для области видимости. Старые enum-свойства `Visibility`/`Override` **удалены** из общего VM. Обе вьюхи переведены на выбор по ссылке: Avalonia — `ComboBox.SelectedItem`; WPF — `DataGridComboBoxColumn.SelectedItemBinding` (вместо `SelectedValueBinding`/`SelectedValuePath`). **Исключение из правила «WPF не трогаем»** — внесено по явному решению пользователя, чтобы убрать enum-свойства из shared-проекта; WPF-семантика сохранена (item-based выбор, как уже было у колонки «Тип данных»).
  - **Правило:** `SelectedValue`+`SelectedValueBinding` на `ComboBox` внутри рециклируемых ячеек Avalonia DataGrid использовать не следует — только item-based `SelectedItem`. Кандидат на проверку тем же дефектом: `ExcelImportDesignerWindow.axaml` (`SelectedValueBinding="{Binding Value}"` в combo внутри DataGrid) — вне рамок этой карточки.
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

**Сделано (частично).** Все файловые диалоги переведены на true-async и больше не используют `UiSync`:
- В интерфейсы `IFileDialogService`/`IMessageDialogService`/`IDialogService` добавлены `*Async`-методы (дефолт — обёртка поверх синхронных через default-interface-methods, чтобы WPF не править); Avalonia-реализации переопределяют их нативно-асинхронно (без моста).
- Вызыватели переведены на `await …Async` + `IAsyncRelayCommandFactory`/`AsyncRelayCommand`: `ExcelImportDesignerVM`, `ImportFromExcelVM`, `ImportExportControlVM`, `TreeLeaveVM`, `ApplicationSettingsControlVM`.

**Осознанно оставлено (мост `UiSync` сохранён).** Полностью удалить `UiSync` нельзя без правок контрактов Core. Окна сообщений вызываются с синхронных границ:
- `INotificationService.ModalWindowHandler` — Core-делегат `delegate bool NotificationHandler(Notification)`, синхронный, общий для 6 типов обработчиков, вызывается синхронно в `NotificationService.ProcessNotification`; `SendModalWindow/SendTextMessage` дёргаются синхронно из множества мест. `ModalWindowNotificationsControlVM.ShowModal` назначается этим обработчиком.
- `ConfigurationService.UpdateConfigFile<T>(...)` — синхронный сервисный метод (`bool`) в цепочке сохранения конфигов; использует `_dialogService.ShowWarning(...)`.

Поэтому синхронные методы сервисов диалогов и `UiSync.RunSync` оставлены работать на этих границах. `UiSync.RunSync` корректен (вложенный цикл диспетчера `DispatcherFrame` + `Dispatcher.PushFrame`, не busy-loop). Дальнейшее удаление моста вынесено в отдельную карточку тех-долга (далёкое будущее) — см. ниже.

**Долг (отдельная карточка).** Сделать `NotificationHandler` асинхронным (`Func<Notification, Task<bool>>` или эквивалент), перевести `NotificationService.ProcessNotification` и всех синхронных отправителей уведомлений на async, перевести `ConfigurationService.UpdateConfigFile` на async, мигрировать оставшиеся `ShowError/ShowWarning/ShowInformation` на `*Async`, после чего удалить синхронные методы сервисов диалогов и `UiSync`. Затрагивает доменный слой и тесты — высокий риск, требует отдельной проработки.

- `AvaloniaWindowService.ShowDialog` показывает окно модально, но без синхронного ожидания закрытия (Avalonia `ShowDialog` асинхронный; результат `bool?` сейчас не используется). Если понадобится результат диалога — переводить вызов в async.

## I. Динамические колонки DataGrid

Два независимых механизма:

- **`DataGridDataTableBehavior`** (источник — `DataTable`/`DataView`): строит колонки и привязывает ячейки через конвертер (имя колонки в параметре), т.к. Avalonia DataGrid не умеет авто-колонки из `DataTable`/`DataView`, а привязка по пути `[имя]` ломается на заголовках со спецсимволами. **Редактирование/типизация не нужны:** все потребители — read-only (`ExcelImportDesignerWindow` → `SheetPreview`, `ImportFromExcelWindow` → `PreviewView`, `ReportProcessor` → `ReportResult`), обратной записи в `DataTable` нет. Долгом не считаем.

- **`DataGridColumnsBehavior`** (источник — дескрипторы `ChildCollectionTableColumn`, плоская таблица наследников): ячейки уже редактируемые (combo/text TwoWay) и частично типизированы (даты `{0:yyyy-MM-dd HH:mm}`, цветная колонка состояния).
  - **Сделано (п.1):** подсветка «переопределено» (Moccasin) + тултип на атрибутных ячейках — `WrapWithOverride` биндит фон на `ValueOverrideStates[bindingKey]` (через `boolToBrushConverter`), тултип на `ValueOverrideToolTips[bindingKey]`, текст чёрным при переопределении. Оформление атрибутных ячеек унифицировано с таблицей атрибутов (E2): в покое — `Border` (Moccasin) + `TextBlock` с чёрным текстом; редактор появляется только в `CellEditingTemplate` (текст → `TextBox`, значения со списком → `ComboBox`). Это устранило плохой контраст в Dark, который был, когда combo показывался постоянно.
  - **Сделано (п.2):** ячейка значения атрибута работает ПОЛНОСТЬЮ идентично таблице атрибутов — ввод формулы (`=…`), ссылки на лист (выбор из списка пишет «[uuid]» через `TextSearch.TextBinding`), системные значения строкой, отображение результата формулы/кода ошибки. Общая логика вынесена в `AttributeValueText` (Presentation): `GetDisplayText`/`GetFormulaText`/`SetFormulaText` над `ElementAttributeModel`; `ElementAttributeVM.DisplayedValueText`/`FormulaValueText` теперь делегируют туда же (одна логика на обе таблицы, без дублей). В `ChildCollectionTableRow` добавлены два канала: `DisplayTexts[bindingKey]` (просмотр) и `EditText[bindingKey]` (TwoWay-редактирование через `EditTextAccessor`); правка через `EditText` запускает тот же каскад пересчёта зависимых ячеек/подсветки/подсказок, что и обычная правка ячейки. `DataGridColumnsBehavior` (Avalonia): в покое — `TextBlock=DisplayTexts[...]` на подсветке, при редактировании — editable `ComboBox` (`IsEditable`, `Text=EditText[...]`, `ItemsSource=ValueOptions[...]`, `TextSearch.TextBinding=[{Uuid}]`), как в `Attributes.axaml`. Каналы наполняются только для одиночных атрибутных колонок (`HasValueOptions`); коллекционные остаются read-only текстом. **WPF не трогали** — его плоская таблица использует прежний канал `[bindingKey]`+`ValueOptions` и продолжает работать (новые каналы он игнорирует); довести WPF до идентичности — отдельная задача, если потребуется (WPF выводится из эксплуатации).

## J. Косметика кода (опционально, отложено по договорённости)

**Частично сделано (Avalonia-проект; WPF больше не трогаем).** Конвертеры и behaviors помечены `sealed`; в `ConvertBack` конвертеров `NotImplementedException`→`NotSupportedException` (семантически «не поддерживается»). `NotImplementedException` оставлен только там, где это реальный «ещё не реализовано» (напр. `InfrastructureRepositoryFactory`).

Остаток (по желанию): переход на `?`/`!`/`=>`-стиль вместо `{}`, прочие стилевые мелочи по остальным классам (VM/сервисы). Низкий приоритет.

## K. Прочее

- `KafkaConsumer.Dispose`: убран двойной `Close` (graceful Close делается в `ExecuteAsync.finally`) — зафиксировано как осознанное изменение, проверить при нагрузочном тесте.
- Тема: **реализовано** — переключатель Светлая/Тёмная/Как в системе (вкладка ленты «Вид» → группа «Тема»), дефолт «Как в системе»; применение `RequestedThemeVariant`, сохранение в `AppearanceConfig:ThemeString` (presentation-конфиг, не Core). Жёсткие светлые цвета (фоны/рамки/вторичный текст) переведены на `DynamicResource` Fluent. Оставшийся долг: подсветка синтаксиса в `FormulaBar` тема-зависимой (см. C); полный визуальный QA всех экранов в Dark; спайк иконок PNG→SVG (иконки сейчас растровые, в Dark не инвертируются).

## L. Лента — DataContext групп vs RibbonTab

`Ribbon.Avalonia` выносит группы выбранной вкладки в свой контент, поэтому `DataContext`, заданный на `RibbonTab`, **до групп не наследуется** — биндинги внутри `RibbonGroupBox` резолвятся против `DataContext` ленты. Рабочий приём: задавать `DataContext` на самой `RibbonGroupBox` (как во вкладках «Формулы»→«Пересчет», «Вид»→«Тема», «Расширения»). **Исправлено** для всех затронутых вкладок, включая «Расширения» (`DataContext="{Binding ExtensionsControlVM}"` перенесён с `RibbonTab` на обе группы). На текущий момент список расширений всё равно пуст — библиотеки расширений устарели после рефакторинга; раскрытие проверить после их обновления.

## M. Два списка на один SelectedItem (стартовое окно)

В `FavoriteAndLastRepositoryHeaders` два `ListBox` («Избранные» и «Недавние») привязаны TwoWay к одному `RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM`. При выборе элемента в одном списке второй снимает свой выбор (элемента в нём нет) и пишет обратно `null`, затирая только что выбранное значение → кнопка «Выбрать и начать работу!» (её `CanExecute` смотрит на `IsPhiladelphusRepositoryAvailable` выбранного) становилась неактивной для неизбранных заголовков. **Исправлено** ранним выходом в сеттере `SelectedPhiladelphusRepositoryHeaderVM` при `value == null` (паразитный сброс игнорируется; правка в shared, действует и в WPF — там безвредна). Побочный эффект: выбор нельзя обнулить через UI (для стартового окна приемлемо, никто на это не полагается). Чистая альтернатива на будущее — отдельный `SelectedItem` на каждый список + производное «эффективное выделение».
