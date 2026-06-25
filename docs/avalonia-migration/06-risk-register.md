# 06 — Реестр рисков

> **Статус:** Аналитическая фаза. Дата: 2026-06-02.
>
> **Шкала вероятности:** Низкая / Средняя / Высокая
> **Шкала влияния:** Низкое / Среднее / Высокое
> **Момент повторной оценки:** указан для каждого риска

---

## R1 — WPF-зависимости в 17 ViewModels

| Поле | Значение |
|---|---|
| **Описание** | 17 файлов ViewModels содержат `using System.Windows.*`, возвращают `System.Windows.Visibility`, используют `MessageBox.Show()`. Очистка нетривиальна и затрагивает UI-логику. |
| **Доказательство** | `RepositoryExplorerControlVM.cs`, `ImportExportControlVM.cs`, `MessageLogControlVM.cs`, `ApplicationSettingsControlVM.cs` и 13 других |
| **Вероятность** | Высокая |
| **Влияние** | Высокое — блокирует создание shared Presentation |
| **Mitigation** | Поэтапная очистка (Этап 2), 5 отдельных PR, characterization tests до начала |
| **Owner** | Tech Lead |
| **Повторная оценка** | После PR 2.1 |
| **Blocker** | Нет |

---

## R2 — `RelayCommand` и `AsyncRelayCommand` — WPF-специфичный `CommandManager`

| Поле | Значение |
|---|---|
| **Описание** | Оба класса используют `CommandManager.RequerySuggested` для `CanExecuteChanged`. Этот механизм не существует в Avalonia. `AsyncRelayCommand.Execute` — `async void` (теряет исключения). |
| **Доказательство** | `Infrastructure/RelayCommand.cs:14-16`, `Infrastructure/AsyncRelayCommand.cs:19-21` |
| **Вероятность** | Высокая (факт, а не предположение) |
| **Влияние** | Высокое — команды не будут работать в Avalonia без замены |
| **Mitigation** | Создать `IRelayCommand` интерфейс в shared Presentation; WPF-реализация с `CommandManager`, Avalonia-реализация с ручным `RaiseCanExecuteChanged()` |
| **Owner** | Backend Dev |
| **Повторная оценка** | Этап 1 (при создании интерфейсов) |
| **Blocker** | Нет |

---

## R3 — `SplashWindow.xaml.cs` — 340 строк в code-behind

| Поле | Значение |
|---|---|
| **Описание** | SplashWindow содержит сложную логику: инициализация приложения, несколько параллельных анимаций (DispatcherTimer, Storyboard, BeginAnimation), Dispatcher.InvokeAsync с DispatcherPriority. Всё в code-behind. |
| **Доказательство** | `Views/Windows/SplashWindow.xaml.cs` — 340 строк |
| **Вероятность** | Высокая |
| **Влияние** | Среднее — SplashWindow изолирован, но его перенос потребует значительной работы |
| **Mitigation** | Вынести `InitializeApplicationAsync()` в `IApplicationInitializationService` (shared). Анимации — переписать под `Avalonia.Animation` API. |
| **Owner** | Frontend Dev |
| **Повторная оценка** | Начало Этапа 5 |
| **Blocker** | Нет |

---

## R4 — Отсутствие CI/CD

| Поле | Значение |
|---|---|
| **Описание** | Нет `.github/workflows`, `azure-pipelines.yml` или любого другого CI. Нет автоматических build/test при каждом PR. Регрессии можно пропустить. |
| **Доказательство** | Проверка репозитория — CI-файлы не найдены |
| **Вероятность** | Высокая (уже реализован риск) |
| **Влияние** | Высокое — любой этап миграции может незаметно сломать WPF |
| **Mitigation** | Добавить минимальный GitHub Actions workflow до Этапа 2: `dotnet build` + `dotnet test`. Можно сделать параллельно с Этапом 0/1. |
| **Owner** | DevOps / Tech Lead |
| **Повторная оценка** | Перед Этапом 2 |
| **Blocker** | Нет (но настоятельно рекомендуется) |

---

## R5 — 24 pre-existing падающих теста

| Поле | Значение |
|---|---|
| **Описание** | 24 теста в `Philadelphus.Tests.Domain` падают до начала миграции. Причина неизвестна без анализа. Риск: миграция маскирует проблемы, которые уже существуют. |
| **Доказательство** | `dotnet test Philadelphus.Tests.Domain` — 24 ❌ |
| **Вероятность** | Высокая (факт) |
| **Влияние** | Среднее — нет гарантии, что baseline чистый |
| **Mitigation** | Зафиксировать список падающих тестов до Этапа 1. Добавить `[Trait("Status", "KnownFailing")]` или issue в трекере. Не маскировать. |
| **Owner** | QA / Backend Dev |
| **Повторная оценка** | Этап 0 |
| **Blocker** | Нет |

---

## R6 — DataGrid Behaviors — высокая сложность

| Поле | Значение |
|---|---|
| **Описание** | 4 из 8 behaviors связаны с DataGrid: formula cell selection, formula reference highlighting, auto-scroll, column configuration. DataGrid в Avalonia имеет другую архитектуру Column/Cell. |
| **Доказательство** | `Behaviors/DataGridFormulaCellSelectionBehavior.cs`, `DataGridFormulaReferenceHighlightBehavior.cs` |
| **Вероятность** | Высокая |
| **Влияние** | Высокое — формулы являются центральной фичей приложения |
| **Mitigation** | Начать с Этапа 7 (MainWindow), behaviors переносить последними. Рассмотреть Avalonia DataGrid Community Toolkit. Зарезервировать spike. |
| **Owner** | Frontend Dev |
| **Повторная оценка** | Начало Этапа 7 |
| **Blocker** | Нет (но требует spike) |

---

## R7 — `KeyToImageConverter` — `Application.Current.Resources`

| Поле | Значение |
|---|---|
| **Описание** | `KeyToImageConverter.Convert()` обращается к `Application.Current.Resources[key]` — runtime-зависимость от WPF Application. Avalonia использует другую resource lookup систему. |
| **Доказательство** | `Converters/KeyToImageConverter.cs` |
| **Вероятность** | Средняя |
| **Влияние** | Среднее — иконки не будут отображаться при неправильном переносе |
| **Mitigation** | Создать `IResourceService` или использовать `avares://` URI + `AssetLoader`. |
| **Owner** | Frontend Dev |
| **Повторная оценка** | Этап 5 (первый экран с иконками) |
| **Blocker** | Нет |

---

## R8 — Namespace при переименовании проектов

| Поле | Значение |
|---|---|
| **Описание** | Переименование `Philadelphus.Presentation.Wpf.UI` → `Philadelphus.Presentation.Desktop.Wpf` изменяет default namespace. Все внутренние ссылки, XAML x:Class, `InternalsVisibleTo` в тестах потребуют обновления. |
| **Доказательство** | XAML-файлы: `x:Class="Philadelphus.Presentation.Wpf.UI.Views.Windows.MainWindow"` |
| **Вероятность** | Высокая (при решении переименовать) |
| **Влияние** | Среднее — широкий scope, легко пропустить |
| **Mitigation** | Переименование — отдельный PR до начала активной миграции. Использовать IDE refactoring, затем проверить компиляцию. Изменение namespace — отдельный коммит от переноса файлов. |
| **Owner** | Tech Lead |
| **Повторная оценка** | Этап 1 (решение о переименовании) |
| **Blocker** | Нет |

---

## R9 — `ExcelImportDesignerWindow` — высокая сложность code-behind

| Поле | Значение |
|---|---|
| **Описание** | `ExcelImportDesignerWindow.xaml.cs` содержит 14+ вызовов `MessageBox.Show()` и логику предпросмотра Excel. Наибольшая сложность среди диалогов. |
| **Доказательство** | Анализ code-behind файла |
| **Вероятность** | Средняя |
| **Влияние** | Среднее — отдельный workflow (Excel import), можно отложить |
| **Mitigation** | Перенести в последний момент (конец Этапа 8). Все MessageBox заменить на `IDialogService` в Этапе 2. |
| **Owner** | Frontend Dev |
| **Повторная оценка** | Начало Этапа 8 |
| **Blocker** | Нет |

---

## R10 — `AllocConsole`/`FreeConsole` (Windows-only P/Invoke)

| Поле | Значение |
|---|---|
| **Описание** | `App.xaml.cs` использует `DllImport("kernel32.dll")` для `AllocConsole` и `FreeConsole`. Это Windows-специфичный код. Для Avalonia (если будет кросс-платформенность) — blocker. |
| **Доказательство** | `App.xaml.cs`, строки с `[DllImport("kernel32.dll")]` |
| **Вероятность** | Низкая (если цель — только Windows) |
| **Влияние** | Среднее (если цель — кросс-платформенность) |
| **Mitigation** | В Avalonia-startup не использовать console allocation. Serilog Console sink достаточен. |
| **Owner** | Backend Dev |
| **Повторная оценка** | Этап 4 (Avalonia bootstrap) |
| **Blocker** | Нет для Windows-only |

---

## R11 — Подпись сборки (`SignAssembly`)

| Поле | Значение |
|---|---|
| **Описание** | WPF-проект и ряд других проектов подписаны (`<SignAssembly>True`). Ключ `philadelphus.snk`. Новые проекты (Presentation shared, Avalonia) потребуют решения о подписи. |
| **Доказательство** | `Philadelphus.Presentation.Wpf.UI.csproj`: `<SignAssembly>True</SignAssembly>` |
| **Вероятность** | Средняя |
| **Влияние** | Среднее — может блокировать публикацию |
| **Mitigation** | Добавить подпись в `Philadelphus.Presentation.csproj` и `Philadelphus.Presentation.Desktop.Avalonia.csproj` при их создании. |
| **Owner** | DevOps |
| **Повторная оценка** | Этап 1 (создание Presentation project) |
| **Blocker** | Нет |

---

## Сводная таблица рисков

| ID | Риск | Вероятность | Влияние | Blocker | Этап обнаружения |
|---|---|---|---|---|---|
| R1 | WPF-зависимости в ViewModels | Высокая | Высокое | Нет | 0 |
| R2 | CommandManager в RelayCommand | Высокая | Высокое | Нет | 0 |
| R3 | SplashWindow code-behind сложность | Высокая | Среднее | Нет | 0 |
| R4 | Отсутствие CI/CD | Высокая | Высокое | Нет | 0 |
| R5 | 24 pre-existing failing tests | Высокая | Среднее | Нет | 0 |
| R6 | DataGrid Behaviors сложность | Высокая | Высокое | Нет | 0 |
| R7 | KeyToImageConverter resource lookup | Средняя | Среднее | Нет | 0 |
| R8 | Namespace при переименовании | Высокая | Среднее | Нет | 1 |
| R9 | ExcelImportDesigner code-behind | Средняя | Среднее | Нет | 0 |
| R10 | AllocConsole P/Invoke | Низкая | Среднее | Нет (Windows-only) | 4 |
| R11 | SignAssembly для новых проектов | Средняя | Среднее | Нет | 1 |
