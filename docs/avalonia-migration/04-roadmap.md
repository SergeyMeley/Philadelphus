# 04 — Roadmap миграции

> **Статус:** Аналитическая фаза. Дата: 2026-06-02.
>
> **Ветка для работы:** `feature/#65575392-avalonia`
>
> **Правило переноса (применяется ко всем этапам):** Файлы переносятся один-в-один, без изменений. Изменения содержимого — отдельный коммит.
>
> **Правило адаптации (Этапы 5–8):** При адаптации WPF-файлов под Avalonia весь закомментированный код, оригинальные комментарии и TODO сохраняются в адаптированном файле.

---

## Зависимости между этапами

```
Этап 0 → Этап 1 → Этап 2 → Этап 3 → Этап 4 → Этап 5 → Этап 6 → Этап 7 → ...
         (параллельно можно)
         Этап 1 + tech debt (удалить ADO.PostgreSQL dir, PropertyTools.Wpf)
```

---

## Этап 0: Baseline

**Цель:** Зафиксировать исходное состояние. Убедиться, что есть точка отсчёта.

**Предварительные условия:** Нет.

**Scope:**
- Зафиксировать результаты сборки и тестов
- Создать документацию `docs/avalonia-migration/` (текущий документ — часть этого этапа)

**Ожидаемые изменения:**
- Только `docs/avalonia-migration/` (Markdown-файлы)
- Никаких изменений в production-коде

**Результаты:**
- ✅ `dotnet build Philadelphus.sln` — 0 ошибок (347 nullable-предупреждений, pre-existing)
- ✅/❌ `dotnet test Philadelphus.Tests.Domain` — 256 пройдено / 24 упало (pre-existing, зафиксированы)
- Документация создана

**Out of scope:** Любые изменения кода.

**Acceptance criteria:**
- Документы `docs/avalonia-migration/00-08` созданы
- Baseline результаты зафиксированы
- Команда ознакомлена с планом

**Команды проверки:**
```powershell
dotnet build Philadelphus.sln --configuration Debug
dotnet test Philadelphus.Tests.Domain/Philadelphus.Tests.Domain.csproj --no-build
```

**Риск:** Низкий. **Откат:** N/A. **Размер:** S. **PR:** 1 PR (только docs).

---

## Этап 1: Создание `Philadelphus.Presentation` — Shared Presentation Layer

**Цель:** Создать пустой проект `Philadelphus.Presentation` и перенести в него инфраструктурный код ViewModels — без изменений содержимого файлов.

**Предварительные условия:** Этап 0 завершён.

**Scope:**

**PR 1.1 — Создать проект и перенести ViewModelBase:**
- Создать `Philadelphus.Presentation/Philadelphus.Presentation.csproj` (net10.0, без UseWPF)
- Добавить в `.sln`
- Перенести `ViewModelBase.cs` один-в-один (без изменений)
- Добавить `<ProjectReference>` в `Philadelphus.Presentation.Desktop.Wpf`

**PR 1.2 — Добавить интерфейсы сервисов:**
- Создать `IDialogService.cs`, `IDispatcherService.cs`, `IWindowService.cs`, `IRelayCommand.cs`, `IAsyncRelayCommand.cs` в `Philadelphus.Presentation/Services/Interfaces/` и `Infrastructure/`
- Перенести `IFileDialogService.cs`, `IMessageDialogService.cs`, `IConfigurationService.cs` из WPF-проекта
- WPF-проект: добавить `<ProjectReference>` на `Philadelphus.Presentation`; существующий код продолжает работать

**PR 1.3 — Перенести factory interfaces:**
- Перенести `Factories/Interfaces/*.cs` из WPF-проекта в `Philadelphus.Presentation`

**Ожидаемые изменения:**
- Новый проект `Philadelphus.Presentation/`
- `Philadelphus.sln`: добавление нового проекта
- `Philadelphus.Presentation.Desktop.Wpf.csproj`: новая `<ProjectReference>`

**Out of scope:** Перенос ViewModels, изменение содержимого файлов.

**Acceptance criteria:**
- `dotnet build Philadelphus.sln` — 0 ошибок
- WPF exe запускается
- `Philadelphus.Presentation.csproj` не содержит `<UseWPF>` и не ссылается на WPF-пакеты
- Проект компилируется без `System.Windows.*`

**Команды:**
```powershell
dotnet build Philadelphus.sln --configuration Debug
dotnet test Philadelphus.Tests.Domain/Philadelphus.Tests.Domain.csproj --no-build
dotnet test Philadelphus.Tests.Presentation.Wpf.UI/Philadelphus.Tests.Presentation.Wpf.UI.csproj --no-build
```

**Риск:** Низкий — только добавление, ничего не удаляется.
**Откат:** Удалить новый проект, убрать `<ProjectReference>`.
**Размер:** M. **PR:** 3 PR (по PR 1.1, 1.2, 1.3).

---

## Этап 2: Очистка ViewModels от WPF-зависимостей

**Цель:** Устранить MVVM-нарушения в WPF-проекте перед переносом в shared Presentation.

**Предварительные условия:** Этап 1 завершён (интерфейсы `IDialogService`, `IDispatcherService` созданы).

**Scope:**

**PR 2.1 — Реализации адаптеров в WPF:**
- `WpfDialogService.cs` — реализация `IDialogService` через `System.Windows.MessageBox`
- `WpfDispatcherService.cs` — реализация `IDispatcherService` через `Application.Current.Dispatcher`
- `WpfWindowService.cs` — реализация `IWindowService` (регистрация ViewModel→Window)
- Зарегистрировать в `App.xaml.cs` (DI)

**PR 2.2 — Заменить `System.Windows.Visibility` на `bool` (17 ViewModels):**
- `RepositoryExplorerControlVM.cs`: `SystemBaseLeaveControlVisibility` → `IsSystemBaseLeaveControlVisible` (bool)
- `ImportExportControlVM.cs` и другие — аналогично
- WPF Views: добавить `BooleanToVisibilityConverter` или использовать встроенный

**PR 2.3 — Заменить `MessageBox.Show()` на `IDialogService` (18+ мест):**
- Все ViewModels: внедрить `IDialogService`, заменить прямые вызовы

**PR 2.4 — Заменить `Dispatcher` на `IDispatcherService` (3 файла):**
- `MessageLogControlVM.cs`: `DispatcherTimer` → `System.Threading.PeriodicTimer` или `ITimerService`
- `ImportExportControlVM.cs`: `Dispatcher.Invoke` → `IDispatcherService.Invoke`

**PR 2.5 — Создать WPF-реализацию `IRelayCommand`:**
- `WpfRelayCommand.cs` — существующий `RelayCommand.cs` переименовывается/адаптируется
- `WpfAsyncRelayCommand.cs` — аналогично, исправить `async void Execute()` anti-pattern

**Ожидаемые изменения:**
- 17+ ViewModel-файлов в `Philadelphus.Presentation.Desktop.Wpf`
- 3+ новых адаптер-файлов
- `App.xaml.cs` (DI-регистрации)

**Out of scope:** Перенос ViewModels в shared Presentation (следующий этап).

**Acceptance criteria:**
- `dotnet build Philadelphus.sln` — 0 ошибок (или не больше pre-existing предупреждений)
- `dotnet test` — не новых падений
- `grep -r "System\.Windows\.Visibility" ViewModels/` — 0 результатов
- `grep -r "MessageBox\.Show" ViewModels/` — 0 результатов
- WPF exe запускается и функционирует

**Риск:** Средний — изменения логики visibility и dialogs могут нарушить UI.
**Откат:** `git revert` каждого PR отдельно.
**Размер:** L. **PR:** 5 PR (по группам изменений).

---

## Этап 3: Перенос ViewModels в `Philadelphus.Presentation`

**Цель:** После очистки перенести ViewModels, services и converters в shared Presentation.

**Предварительные условия:** Этап 2 завершён.

**Scope:**

**PR 3.1 — Перенести ViewModels (один-в-один):**
- Все очищенные ViewModel-файлы из `Presentation.Desktop.Wpf/ViewModels/` → `Presentation/ViewModels/`
- В WPF-проекте: заменить на `<ProjectReference>` к `Presentation`
- Namespace при переносе: оставить или адаптировать (отдельный коммит)

**PR 3.2 — Перенести переносимые сервисы:**
- `ConfigurationService.cs` (после замены MessageBox) → `Presentation/Services/Implementations/`
- Остальные переносимые services

**PR 3.3 — Перенести чистые converters:**
- `EnumDisplayAttributeConverter.cs`, `LastLaunchToDaysAgoConverter.cs`, `UtcToLocalTimeConverter.cs` — логика в `Presentation/Converters/Logic/`
- WPF: оболочки `IValueConverter` остаются в `Presentation.Desktop.Wpf`

**Ожидаемые изменения:**
- `Presentation/` — наполнение файлами
- `Presentation.Desktop.Wpf/` — удаление перенесённых файлов, `<ProjectReference>` на `Presentation`
- `Philadelphus.Tests.Presentation.Desktop.Wpf.csproj` — добавить ссылку на `Presentation`

**Acceptance criteria:**
- `dotnet build Philadelphus.sln` — 0 ошибок
- `dotnet test` — нет новых падений
- `Presentation.csproj` — нет ссылок на WPF/Avalonia сборки
- WPF exe запускается
- Создать `Philadelphus.Tests.Presentation` и добавить 1-2 ViewModel unit-теста

**Риск:** Средний — рефакторинг namespace, ссылок.
**Откат:** `git revert`.
**Размер:** M. **PR:** 3 PR.

---

## Этап 4: Минимальный Avalonia Bootstrap

**Цель:** Убедиться, что Avalonia работает в данном решении. "Hello from Avalonia" — не больше.

**Предварительные условия:** Этап 3 завершён (shared Presentation готов).

**Scope:**

**PR 4.1:**
- Создать `Philadelphus.Presentation.Desktop.Avalonia/` (net10.0, Avalonia 11.x)
- Добавить в `.sln`
- `App.axaml.cs`: минимальный `AppBuilder` + DI регистрация `Presentation`
- Одно окно `MainWindow.axaml` с текстом "Philadelphus — Avalonia v{version}"
- Реализовать `AvaloniaRelayCommand`, `AvaloniaDialogService`, `AvaloniaDispatcherService`

**PR 4.2:**
- Добавить `Philadelphus.Tests.Presentation.Desktop.Avalonia` (headless тест запуска)
- Проверить, что `Presentation.Desktop.Avalonia.exe` запускается

**Ожидаемые изменения:**
- Новый проект `Philadelphus.Presentation.Desktop.Avalonia/`
- `.sln` обновлён

**Out of scope:** Перенос реальных экранов.

**Acceptance criteria:**
- `dotnet build Philadelphus.sln` — оба проекта собираются
- WPF exe — не сломан
- Avalonia exe — запускается и показывает окно
- Headless тест проходит

**Риск:** Низкий — новый проект, ничего не ломает.
**Откат:** Удалить новый проект.
**Размер:** S. **PR:** 2 PR.

---

## Этап 5: SplashWindow в Avalonia

**Цель:** Перенести первый реальный экран — SplashWindow.

**Предварительные условия:** Этап 4.

**Scope:**
- `SplashWindow.axaml` — Avalonia-аналог с анимациями
- Логику инициализации `InitializeApplicationAsync()` вынести в `IApplicationInitializationService`
- Анимации: `Avalonia.Animation` API (Transition, Animation, Keyframe)
- `DispatcherTimer` → `System.Threading.PeriodicTimer` или `DispatcherTimer` Avalonia
- Правило адаптации: весь закомментированный код WPF-версии сохранить в комментариях Avalonia-файла

**Acceptance criteria:**
- SplashWindow отображается в Avalonia с анимациями
- Инициализация запускается корректно
- WPF-версия не сломана
- WPF и Avalonia используют один и тот же `IApplicationInitializationService`

**Риск:** Средний — Avalonia animation API отличается от WPF Storyboard.
**Откат:** `git revert`.
**Размер:** M. **PR:** 1-2 PR.

---

## Этап 6: LaunchWindow в Avalonia

**Цель:** Перенести LaunchWindow с 5 вкладками.

**Предварительные условия:** Этап 5. `LaunchWindowVM` уже в shared Presentation.

**Scope:**
- `LaunchWindow.axaml` с 5 TabControl вкладками
- Все `LaunchWindowTabItemsControls/*.axaml` (5 контролов)
- Логика `OnClosing → Hide` → через `IWindowService`

**Acceptance criteria:**
- LaunchWindow открывается, вкладки работают
- Переключение вкладок через shared `LaunchWindowVM`
- WPF не сломан

**Риск:** Средний. **Размер:** L. **PR:** 2-3 PR.

---

## Этап 7: MainWindow в Avalonia

**Цель:** Перенести MainWindow с дочерними контролами.

**Предварительные условия:** Этап 6.

**Scope — порядок по зависимостям:**
1. Leaf-контролы (без дочерних вложений): `About`, `MessageLog`, `RepositoryCreation`, `StoragesCreation`
2. Средние: `ApplicationSettings`, `DataExchange`, `DetailedInformationGrids/*`
3. Сложные: `RepositoryExplorer/*` (DataGrid + behaviors — наибольший риск)
4. `MainWindow.axaml` как оболочка

**Out of scope:** Полный feature parity (часть функциональности может быть заглушкой).

**Acceptance criteria:**
- MainWindow открывается из LaunchWindow (Avalonia)
- Базовый workflow работает (открыть репозиторий, просмотр)
- WPF не сломан

**Риск:** Высокий (DataGrid behaviors, formula bar). **Размер:** XL. **PR:** 5-8 PR по контролам.

---

## Этап 8: Оставшиеся окна и диалоги

**Предварительные условия:** Этап 7.

**Окна для переноса (по приоритету):**
1. `DetailsWindow` — низкая сложность
2. `ImportProgressWindow` — низкая сложность
3. `FormulaEditorWindow` — средняя (связан с formula engine)
4. `ImportFromExcelWindow` — средняя
5. `ExcelImportDesignerWindow` — высокая (14+ MessageBox в code-behind)
6. `AttributeValuesCollectionWindow`
7. `EmptyWindow`

**Каждое окно — отдельный PR.**

**Риск:** Высокий (ExcelImportDesigner). **Размер:** XL суммарно. **PR:** 7 PR.

---

## Этап 9: Темы и стили

**Цель:** Внедрить систему Light/Dark/System тем. Перенести ресурсы иконок.

**Scope:**
- `LightTheme.axaml`, `DarkTheme.axaml`, `CommonTheme.axaml`
- Иконки: переход с `pack://application:,,,/Icons/` на `avares://...Icons/`
- Spike: оценить переход PNG → SVG (отдельная задача)
- `ThemeVariant` switching: System/Light/Dark

**Acceptance criteria:**
- Avalonia-приложение переключает темы
- Системная тема следует ОС
- Иконки отображаются через `avares://`

**Риск:** Средний. **Размер:** M. **PR:** 2-3 PR.

---

## Этап 10: Тесты и покрытие

**Scope:**
- Unit-тесты ViewModels в `Philadelphus.Tests.Presentation`
- Headless UI-тесты в `Philadelphus.Tests.Presentation.Desktop.Avalonia`
- Smoke-тест запуска Avalonia-приложения

**Риск:** Низкий. **Размер:** M. **PR:** 2 PR.

---

## Этап 11: Решение о WPF (командное)

**Предварительные условия:** Feature parity Avalonia ≥ WPF.

**Вопросы для команды:**
- Все ли сценарии работают в Avalonia?
- Есть ли Windows-only фичи, которые не перенесены?
- Каков план для существующих пользователей WPF-версии?

**Риск:** Высокий (организационный). **Размер:** XL (если принято решение удалить WPF). **PR:** 1 PR (удаление WPF-проекта и обновление .sln).

---

## Технический долг до начала миграции (рекомендуется сделать в Этапе 0 или 1)

| Задача | Описание | PR |
|---|---|---|
| Удалить `PropertyTools.Wpf` | Пакет не используется, убрать из .csproj | Отдельный PR |
| Удалить директорию `ADO.PostgreSQL` | Мёртвый артефакт, не в .sln | Отдельный PR |
| Зафиксировать 24 pre-existing теста | Создать issues или добавить `[Trait("Known", "Failing")]` | До Этапа 2 |
