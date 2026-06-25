# 08 — Первый implementation-task

> **Статус:** Аналитическая фаза. Дата: 2026-06-02.
>
> Это описание первого PR в рамках Этапа 1 миграции. Задача минимальна по scope, не затрагивает WPF-код и создаёт фундамент для всех последующих этапов.

---

## Задача: PR 1.1 — Создать `Philadelphus.Presentation` и перенести `ViewModelBase`

### Цель

Создать новый проект `Philadelphus.Presentation` (net10.0, без WPF), добавить его в решение, перенести в него `ViewModelBase.cs` один-в-один (без изменений), и добавить `<ProjectReference>` из WPF-проекта.

После этого PR WPF-приложение продолжает работать и ссылается на `ViewModelBase` из shared Presentation. Это первый шаг к разделению общего и платформенного кода.

### Точный scope

**Создать:**
1. `Philadelphus.Presentation/Philadelphus.Presentation.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
     </PropertyGroup>
     <ItemGroup>
       <!-- Project references to Core.Domain etc. — только те, что нужны ViewModelBase -->
     </ItemGroup>
   </Project>
   ```
   `ViewModelBase.cs` — нет зависимостей на Core. Достаточно пустого csproj.

2. `Philadelphus.Presentation/ViewModels/ViewModelBase.cs` — перенести один-в-один из `Philadelphus.Presentation.Wpf.UI/ViewModels/ViewModelBase.cs`.

**Изменить:**
3. `Philadelphus.sln` — добавить новый проект в секцию Presentation.
4. `Philadelphus.Presentation.Wpf.UI/Philadelphus.Presentation.Wpf.UI.csproj` — добавить:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\Philadelphus.Presentation\Philadelphus.Presentation.csproj" />
   </ItemGroup>
   ```
5. `Philadelphus.Presentation.Wpf.UI/ViewModels/ViewModelBase.cs` — удалить файл (или добавить `using`-перенаправление если namespace меняется).

**Не изменять:**
- Содержимое `ViewModelBase.cs`
- Никаких других файлов WPF-проекта
- Никаких других .csproj файлов

### Затрагиваемые файлы и директории

```
Создать:
  Philadelphus.Presentation/
  Philadelphus.Presentation/Philadelphus.Presentation.csproj
  Philadelphus.Presentation/ViewModels/
  Philadelphus.Presentation/ViewModels/ViewModelBase.cs  (копия без изменений)

Изменить:
  Philadelphus.sln                                        (добавить проект)
  Philadelphus.Presentation.Wpf.UI/
    Philadelphus.Presentation.Wpf.UI.csproj              (добавить ProjectReference)
    ViewModels/ViewModelBase.cs                           (удалить — используется из Presentation)
```

### Namespace

`ViewModelBase.cs` в текущем WPF-проекте имеет namespace `Philadelphus.Presentation.Wpf.UI.ViewModels`.

**Вопрос:** После переноса namespace нужно будет либо изменить (в отдельном коммите), либо оставить как есть и добавить `global using Philadelphus.Presentation.Wpf.UI.ViewModels = Philadelphus.Presentation.ViewModels` в WPF-проекте.

**Рекомендуемый подход для PR 1.1:** Оставить namespace без изменений при переносе. Изменение namespace — отдельный коммит PR 1.2 (чтобы diff был читаем).

### Критерии готовности (DoD)

- [ ] `dotnet build Philadelphus.sln --configuration Debug` — 0 ошибок
- [ ] `dotnet build Philadelphus.Presentation/Philadelphus.Presentation.csproj` — 0 ошибок
- [ ] `Philadelphus.Presentation.csproj` не содержит `<UseWPF>`, `WindowsBase`, `PresentationCore`, `PresentationFramework`
- [ ] `grep -r "System.Windows" Philadelphus.Presentation/` — 0 результатов
- [ ] `dotnet test Philadelphus.Tests.Domain` — не меньше 256 проходит
- [ ] `dotnet test Philadelphus.Tests.Presentation.Wpf.UI` — все проходят
- [ ] WPF exe компилируется (проверить локально)

### Команды проверки

```powershell
# 1. Сборка всего решения
dotnet build Philadelphus.sln --configuration Debug

# 2. Проверка отсутствия WPF-зависимостей в новом проекте
Select-String -Path "Philadelphus.Presentation\*.csproj" -Pattern "System.Windows|Avalonia|UseWPF"
# Ожидается: пустой вывод

# 3. Проверка отсутствия WPF using в ViewModelBase
Select-String -Path "Philadelphus.Presentation\ViewModels\ViewModelBase.cs" -Pattern "using System.Windows"
# Ожидается: пустой вывод

# 4. Тесты Domain
dotnet test Philadelphus.Tests.Domain/Philadelphus.Tests.Domain.csproj --no-build

# 5. Тесты WPF Presentation
dotnet test Philadelphus.Tests.Presentation.Wpf.UI/Philadelphus.Tests.Presentation.Wpf.UI.csproj --no-build
```

### Out of scope (явно)

- Перенос других ViewModels (следующие PR)
- Изменение содержимого ViewModelBase.cs
- Создание интерфейсов IDialogService и т.д. (PR 1.2)
- Изменение namespace (отдельный коммит)
- Avalonia-проект
- Какие-либо изменения в Views, Controls, XAML

---

## Следующий промпт для Codex / Claude Code

Для передачи следующему исполнителю или агенту можно использовать следующий промпт:

```
Задача: Создать проект Philadelphus.Presentation и перенести ViewModelBase.

Репозиторий: d:\MelSV_Projects\Philadelphus.General
Ветка: feature/#65575392-avalonia

Что нужно сделать:
1. Создать директорию Philadelphus.Presentation/
2. Создать Philadelphus.Presentation/Philadelphus.Presentation.csproj с:
   - TargetFramework: net10.0
   - Nullable: enable
   - ImplicitUsings: enable
   - НЕТ UseWPF, НЕТ ссылок на WPF-сборки
3. Создать Philadelphus.Presentation/ViewModels/ и скопировать туда ViewModelBase.cs из 
   Philadelphus.Presentation.Wpf.UI/ViewModels/ViewModelBase.cs БЕЗ ИЗМЕНЕНИЙ
4. Добавить проект в Philadelphus.sln (в папку Presentation)
5. В Philadelphus.Presentation.Wpf.UI.csproj добавить ProjectReference на новый проект
6. Из Philadelphus.Presentation.Wpf.UI/ViewModels/ удалить ViewModelBase.cs 
   (он теперь в Philadelphus.Presentation)

Правила:
- Файл ViewModelBase.cs переносится один-в-один, без изменений содержимого
- Не трогать никакой другой production-код
- Не коммитить изменения

Проверка:
- dotnet build Philadelphus.sln — должно быть 0 ошибок
- grep "System.Windows" в Philadelphus.Presentation/ — должно быть 0 результатов
- dotnet test Philadelphus.Tests.Domain — не меньше 256 проходит
```

---

## Контекст для ревьюера PR

**Почему именно ViewModelBase первым?**

`ViewModelBase.cs` — единственный файл в ViewModel-слое, который уже сейчас чист: зависит только от `System.ComponentModel.INotifyPropertyChanged` (не WPF-специфичен). Все другие ViewModels содержат `using System.Windows.*` и требуют очистки (Этап 2) перед переносом. Перенос ViewModelBase создаёт фундамент и доказывает, что механизм (ProjectReference, namespace) работает правильно.

**Почему это безопасно?**

- Новый проект только добавляется — ничего не удаляется из существующих проектов, кроме одного файла
- ViewModelBase.cs не имеет WPF-зависимостей — компиляция нового проекта гарантированно чиста
- WPF-проект получает `<ProjectReference>` — это стандартный механизм

**Признаки успеха:**

После этого PR `Philadelphus.Presentation.csproj` компилируется как net10.0-библиотека без WPF, и WPF-проект ссылается на неё. Это первый шаг к разделению.
