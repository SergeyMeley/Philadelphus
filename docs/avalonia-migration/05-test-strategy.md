# 05 — Стратегия тестирования

> **Статус:** Аналитическая фаза. Дата: 2026-06-02.

---

## Текущее состояние тестов (baseline)

| Проект | TFM | Фреймворк | Тестов | Статус |
|---|---|---|---|---|
| `Philadelphus.Tests.Domain` | net10.0 | xUnit + FluentAssertions + Moq | 280 | 256 ✅ / 24 ❌ (pre-existing) |
| `Philadelphus.Tests.Presentation.Wpf.UI` | net10.0-windows7.0 | xUnit + FluentAssertions + Moq | 1 файл | Не запускался авт. |
| `Philadelphus.Tests.Common` | net10.0 | Общие утилиты | — | — |

**Команды baseline-запуска:**
```powershell
dotnet build Philadelphus.sln --configuration Debug
dotnet test Philadelphus.Tests.Domain/Philadelphus.Tests.Domain.csproj
dotnet test Philadelphus.Tests.Presentation.Wpf.UI/Philadelphus.Tests.Presentation.Wpf.UI.csproj
```

**Known failing tests (24 шт.):**
- `SystemBaseTreeNodeModelTests.SystemBaseTreeLeave_NameChange_DoesNotChangeNameOrStringValue`
- `SystemBaseTreeNodeModelTests.CreateTreeLeave_SystemBaseNode_AppliesDefaultTypedValue` (8 вариантов)
- `PhiladelphusRepositoryModelTests.*` (3 теста)
- `ElementAttributePolicyTests.*`, `NonOwnAttributeRuleTests.*`, `NamePropertiesRuleTests.*` (11 тестов)

Эти тесты **не трогать в ходе миграции** — только фиксировать как pre-existing.

---

## Тестовая пирамида (целевая)

```
        ┌─────────────────┐
        │  E2E / Smoke    │  ← минимально: запуск обоих exe
        │    (2-3 тест)   │
        ├─────────────────┤
        │  Headless UI    │  ← Avalonia.Headless
        │  (per screen)   │
        ├─────────────────┤
        │  Integration    │  ← WPF/Avalonia adapter tests
        │   (adapters)    │
        ├─────────────────┤
        │   Unit Tests    │  ← ViewModels, Commands, Pure logic ← ОСНОВА
        │  (ViewModels,   │
        │   Converters)   │
        └─────────────────┘
```

---

## Тестовая матрица

### Tier 1: Domain Unit Tests (существующие)

| Область | Инструмент | Статус |
|---|---|---|
| Domain entities | xUnit + FluentAssertions | ✅ 256 проходят |
| Formula engine | xUnit | ✅ Проходят |
| Import/Export logic | xUnit | ✅ Проходят |
| AssemblyAdapters | xUnit | ✅ Проходят |

**Требование:** Domain-тесты должны проходить после каждого этапа миграции без изменений.

### Tier 2: Presentation Unit Tests (новые — `Philadelphus.Tests.Presentation`)

Создаётся в Этапе 3. Тестирует ViewModels без UI.

| Что тестировать | Инструмент | Когда добавлять |
|---|---|---|
| `ViewModelBase.SetProperty` / `OnPropertyChanged` | xUnit | Этап 1 |
| `IDialogService` через Mock | Moq | Этап 2 |
| `LaunchWindowVM.OpenMainWindowWithHeaderCommand.CanExecute` | xUnit + Moq | Этап 3 |
| `RepositoryExplorerControlVM.IsSystemBaseLeaveControlVisible` (после bool-рефакторинга) | xUnit | Этап 3 |
| `EnumDisplayAttributeLogic` (чистая логика) | xUnit | Этап 3 |
| `LastLaunchToDaysAgoLogic` | xUnit | Этап 3 |
| `ApplicationVM` инициализация (через DI mock) | xUnit + Moq | Этап 3 |

**Template теста:**
```csharp
// Philadelphus.Tests.Presentation/ViewModels/LaunchWindowVMTests.cs
public class LaunchWindowVMTests
{
    private readonly Mock<IDialogService> _dialogService = new();
    private readonly Mock<INavigationService> _navigationService = new();

    [Fact]
    public void OpenMainWindowCommand_WhenRepositoryAvailable_CanExecute()
    {
        var vm = new LaunchWindowVM(..., _dialogService.Object, ...);
        // ...
        vm.OpenMainWindowWithHeaderCommand.CanExecute(null).Should().BeTrue();
    }
}
```

### Tier 3: WPF Adapter Tests (`Philadelphus.Tests.Presentation.Desktop.Wpf`)

| Что тестировать | Инструмент |
|---|---|
| `WpfDialogService.ShowError` вызывает `MessageBox.Show` | xUnit |
| `WpfDispatcherService` — проверить, что `Invoke` выполняется | xUnit |
| `ChildCollectionTableBuilder` (уже существует) | xUnit |

**Примечание:** WPF-тесты требуют STA thread. Использовать `[STAThread]` или `STATaskScheduler`.

### Tier 4: Avalonia Headless Tests (`Philadelphus.Tests.Presentation.Desktop.Avalonia`)

Создаётся в Этапе 4.

| Что тестировать | Инструмент | Когда |
|---|---|---|
| Avalonia bootstrap — приложение запускается | `Avalonia.Headless.XUnit` | Этап 4 |
| SplashWindow — control tree корректен | Headless | Этап 5 |
| SplashWindow — анимация запускается | Headless | Этап 5 |
| LaunchWindow — TabControl переключается | Headless | Этап 6 |
| Тема Light/Dark — ресурсы загружаются | Headless | Этап 9 |

**Пример headless-теста:**
```csharp
// Philadelphus.Tests.Presentation.Desktop.Avalonia/
[AvaloniaFact]
public async Task SplashWindow_WhenLoaded_ShowsTitle()
{
    var window = new SplashWindow();
    window.Show();
    await Task.Delay(100);
    
    var title = window.FindControl<TextBlock>("TitleText");
    title.Text.Should().Be("Чубушник");
}
```

**Пакет:** `Avalonia.Headless.XUnit` (добавить в Этапе 4).

---

## Characterization Tests (нужно добавить)

Characterization tests фиксируют текущее поведение WPF-приложения как "ground truth". Добавлять в `Philadelphus.Tests.Presentation.Desktop.Wpf`.

| Тест | Приоритет | Когда |
|---|---|---|
| `LaunchWindowVM` инициализируется без исключений | Высокий | До Этапа 2 |
| `MainWindowVM` инициализируется без исключений | Высокий | До Этапа 3 |
| `RepositoryExplorerControlVM.IsSystemBaseLeaveControlVisible = false` при старте | Средний | До Этапа 2 |
| `ApplicationVM` property chains корректны | Средний | До Этапа 3 |

---

## Test Gates по этапам

| Этап | Минимальный test gate |
|---|---|
| **0** | `dotnet build` — 0 ошибок; `Tests.Domain` — не новых падений |
| **1** | Всё выше + `Presentation.csproj` не ссылается на WPF/Avalonia |
| **2** | Всё выше + `grep System.Windows.Visibility ViewModels/` = 0 |
| **3** | Всё выше + `Tests.Presentation` — все ViewModel unit-тесты проходят |
| **4** | Всё выше + Avalonia exe запускается; `Tests.Desktop.Avalonia` — smoke-тест проходит |
| **5** | Всё выше + SplashWindow headless-тест |
| **6** | Всё выше + LaunchWindow headless-тест |
| **7** | Всё выше + MainWindow headless-тест (базовый workflow) |
| **8** | Всё выше + тест каждого перенесённого диалога |
| **9** | Всё выше + тесты переключения темы |
| **10** | Coverage > 60% для `Philadelphus.Presentation` |

---

## Регрессионная защита

Правила, которые должны выполняться после каждого этапа:

1. `dotnet build Philadelphus.sln` — 0 новых ошибок
2. `dotnet test Philadelphus.Tests.Domain` — не менее 256 проходит (pre-existing не считаются)
3. `dotnet test Philadelphus.Tests.Presentation.Desktop.Wpf` — все тесты проходят
4. Проверка: `Philadelphus.Presentation.csproj` не ссылается на WPF/Avalonia:
   ```powershell
   Select-String -Path "Philadelphus.Presentation\*.csproj" -Pattern "System.Windows|Avalonia"
   # Должно быть пусто
   ```
5. WPF exe запускается вручную (smoke-тест)

---

## Отсутствующие тесты (нужно добавить до Этапа 2)

| Тест | Приоритет |
|---|---|
| `ViewModelBase.SetProperty` не вызывает event если значение не изменилось | Высокий |
| `ViewModelBase.OnPropertyChangedRecursive` не зацикливается на circular reference | Высокий |
| `LaunchWindowVM` — все 5 вкладок инициализируются | Высокий |
| `MessageLogControlVM` — добавление уведомления отображается | Средний |
| `RepositoryExplorerControlVM.SelectedRepositoryMember = TreeLeaveVM` → `IsVisible = true` | Высокий |
| `AsyncRelayCommand.Execute` — `IsExecuting = true` во время выполнения | Средний |
| `AsyncRelayCommand.Execute` — исключение из async task не теряется | Высокий |
