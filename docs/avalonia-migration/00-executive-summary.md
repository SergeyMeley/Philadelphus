# 00 — Краткое резюме миграции WPF → Avalonia

> **Статус документа:** Черновик аналитической фазы. Дата: 2026-06-02.
> **Ветка:** `feature/#65575392-avalonia`. Рабочее дерево: чистое.

---

## Текущее состояние

Philadelphus — desktop-приложение на WPF + .NET 10, с чистым доменным и инфраструктурным слоями (26 проектов) и единственным WPF-специфичным проектом `Philadelphus.Presentation.Wpf.UI`.

| Параметр | Значение |
|---|---|
| Фреймворк | net10.0-windows7.0 (WPF), net10.0 (остальные) |
| Окна | 10 Windows |
| UserControls | 25+ |
| ViewModels | 47 |
| Converters | 13 |
| Behaviors | 8 |
| Покрытие тестами | 256/280 domain-тестов проходят (24 падают — pre-existing) |
| CI/CD | Отсутствует |
| Состояние сборки | ✅ 0 ошибок, 347 nullable-предупреждений |

**Baseline (зафиксирован 2026-06-02):**
- `dotnet build Philadelphus.sln` — успешно, 0 ошибок
- `dotnet test Philadelphus.Tests.Domain` — 256 пройдено / 24 упало (pre-existing)
- `Philadelphus.Tests.Presentation.Wpf.UI` — требует отдельного запуска с UI

---

## Рекомендуемая стратегия

**Параллельное добавление, а не Big Bang замена.** WPF-клиент остаётся рабочим на всём протяжении миграции.

Ключевой принцип — **разделение на три проекта:**

```
Philadelphus.Presentation                  ← shared ViewModel layer (NEW, net10.0, без WPF)
Philadelphus.Presentation.Desktop.Wpf      ← переименованный WPF-клиент
Philadelphus.Presentation.Desktop.Avalonia ← новый Avalonia-клиент (NEW)
```

**Правило переноса:** файлы переносятся один-в-один без изменений; изменения содержимого — отдельный коммит.

**Правило адаптации:** при адаптации WPF-файлов под Avalonia весь закомментированный код и комментарии сохраняются в адаптированном файле.

### Последовательность этапов

| Этап | Описание | Размер | Риск |
|---|---|---|---|
| 0 | Baseline: сборка, тесты, smoke | S | Низкий |
| 1 | Shared Presentation: интерфейсы, ViewModelBase | M | Низкий |
| 2 | Очистка ViewModels (Visibility, MessageBox, Dispatcher) | L | Средний |
| 3 | Перенос ViewModels в shared Presentation | M | Средний |
| 4 | Минимальный Avalonia bootstrap | S | Низкий |
| 5 | SplashWindow в Avalonia | M | Средний |
| 6 | LaunchWindow в Avalonia | L | Средний |
| 7 | MainWindow в Avalonia | XL | Высокий |
| 8 | Оставшиеся окна и диалоги | XL | Высокий |
| 9 | Темы, стили, иконки | M | Средний |
| 10 | Тесты и coverage | M | Низкий |
| 11 | Решение: отказ от WPF | — | Командное |

---

## Главные риски

| № | Риск | Вероятность | Влияние | Blocker |
|---|---|---|---|---|
| R1 | 17 ViewModels зависят от `System.Windows` — очистка нетривиальна | Высокая | Высокое | Нет |
| R2 | `SplashWindow.xaml.cs` (340 строк) — сложные анимации на WPF Dispatcher/Storyboard | Высокая | Среднее | Нет |
| R3 | `RelayCommand` / `AsyncRelayCommand` используют `CommandManager.RequerySuggested` (WPF-only) | Высокая | Высокое | Нет |
| R4 | 18+ вызовов `MessageBox.Show()` в ViewModels — требуют `IDialogService` | Высокая | Среднее | Нет |
| R5 | Отсутствие CI/CD — нет автоматической защиты от регрессий | Высокая | Высокое | Нет (но усложняет) |

---

## Технический долг (не связанный с миграцией, выявлен в ходе анализа)

- `PropertyTools.Wpf` подключён в .csproj, но не используется ни в одном файле — удалить пакет.
- Директория `Philadelphus.Infrastructure.Persistence.ADO.PostgreSQL/` существует, но проект не включён в `.sln` и нигде не ссылается — мёртвый артефакт, удалить.
- 24 падающих domain-теста — pre-existing, до начала миграции зафиксировать как известные.

---

## Рекомендуемый первый этап

**Этап 1: Создание `Philadelphus.Presentation` — shared ViewModel layer.**

Scope: создать пустой net10.0 проект без `<UseWPF>`, перенести `ViewModelBase.cs` один-в-один, добавить интерфейсы `IRelayCommand`, `IDialogService`, `IDispatcherService`, `IWindowService`. Сделать WPF-адаптеры в `Philadelphus.Presentation.Desktop.Wpf`. Проверить сборку и тесты.

Это нулевой риск для WPF и создаёт фундамент для всех последующих этапов.

> Подробности: [04-roadmap.md](04-roadmap.md), [08-first-implementation-task.md](08-first-implementation-task.md).
