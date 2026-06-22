using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Модель представления для листа рабочего дерева.
    /// </summary>
    public class TreeLeaveVM : MainEntityBaseVM<TreeLeaveModel> //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        /// <summary>
        /// Родительский элемент.
        /// </summary>
        public TreeNodeVM Parent { get; }

        /// <summary>
        /// Псевдоним листа.
        /// </summary>
        public string Alias
        {
            get
            {
                return _model.Alias;
            }
            set
            {
                _model.Alias = value;
                OnPropertyChanged(nameof(Alias));
                OnPropertyChanged(nameof(State));
            }
        }

        /// <summary>
        /// Строковое значение системного листа.
        /// </summary>
        /// <remarks>
        /// Для системных листов это основное редактируемое значение. Для обычных листов свойство
        /// возвращает <see cref="MainEntityBaseVM{T}.Name" />, чтобы старый UI-сценарий оставался совместимым.
        /// </remarks>
        public string StringValue
        {
            get
            {
                if (_model is SystemBaseTreeLeaveModel m)
                { 
                    return m.StringValue; 
                }
                return Name;
            }
            set
            {
                if (_model is SystemBaseTreeLeaveModel m)
                {
                    m.StringValue = value;
                }
                OnPropertyChanged(nameof(StringValue));
                NotifyTypedValuePropertiesChanged();
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(State));
            }
        }

        /// <summary>
        /// Системный базовый тип листа, используемый XAML-шаблонами для выбора редактора значения.
        /// </summary>
        public SystemBaseType SystemBaseType => _model.SystemBaseType;

        /// <summary>
        /// Представление <see cref="StringValue" /> как даты для редактора DATE.
        /// </summary>
        /// <remarks>
        /// В домене дата хранится строкой в invariant culture. Это свойство является только UI-адаптером
        /// для <see cref="System.Windows.Controls.DatePicker" /> и не вводит отдельного источника истины.
        /// </remarks>
        public DateTime? DateValue
        {
            get => TryGetDate(StringValue);
            set
            {
                if (value.HasValue)
                {
                    StringValue = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Дата из составного значения DATETIME.
        /// </summary>
        /// <remarks>
        /// При изменении даты собирает новый ISO-подобный <see cref="StringValue" />, сохраняя текущие
        /// значения времени и смещения, если они уже доступны.
        /// </remarks>
        public DateTime? DateTimeDateValue
        {
            get => TryGetDate(StringValue);
            set
            {
                if (value.HasValue)
                {
                    StringValue = BuildDateTimeValue(value.Value.Date, DateTimeTimeValue, DateTimeOffsetValue);
                }
            }
        }

        /// <summary>
        /// Представление <see cref="StringValue" /> как времени для редактора TIME.
        /// </summary>
        /// <remarks>
        /// Корректное время нормализуется к формату HH:mm:ss. Некорректная промежуточная строка
        /// передается в домен как есть, чтобы штатная валидация могла отклонить ее без скрытой подмены.
        /// </remarks>
        public string TimeValue
        {
            get => TryGetTime(StringValue) ?? StringValue;
            set
            {
                if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    StringValue = time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    StringValue = value;
                }
            }
        }

        /// <summary>
        /// Временная часть составного значения DATETIME.
        /// </summary>
        /// <remarks>
        /// Используется отдельным текстовым полем, потому что штатный WPF DatePicker редактирует только дату.
        /// </remarks>
        public string DateTimeTimeValue
        {
            get => TryGetTime(StringValue) ?? "00:00:00";
            set => StringValue = BuildDateTimeValue(DateTimeDateValue ?? DateTime.Today, value, DateTimeOffsetValue);
        }

        /// <summary>
        /// Смещение часового пояса составного значения DATETIME.
        /// </summary>
        /// <remarks>
        /// Хранится в формате +HH:mm или -HH:mm. Это позволяет сохранить смысл значений вроде
        /// 2026-05-21T10:15:30+03:00 при редактировании через UI.
        /// </remarks>
        public string DateTimeOffsetValue
        {
            get => TryGetOffset(StringValue) ?? FormatOffset(DateTimeOffset.Now.Offset);
            set => StringValue = BuildDateTimeValue(DateTimeDateValue ?? DateTime.Today, DateTimeTimeValue, value);
        }

        /// <summary>
        /// Часы:минуты значения TIME для Avalonia TimePicker (минутная точность; секунды — в
        /// <see cref="TimeSecondsText" />). Avalonia-адаптер поверх <see cref="TimeValue" />, не вводит
        /// отдельного источника истины. В WPF не используется (там TimePicker'а нет).
        /// </summary>
        public TimeSpan? TimeHoursMinutesValue
        {
            get => ParseHoursMinutes(TimeValue);
            set => TimeValue = FormatHms(value, ParseSeconds(TimeValue));
        }

        /// <summary>
        /// Секунды значения TIME (00..59) для отдельного поля рядом с TimePicker.
        /// </summary>
        public string TimeSecondsText
        {
            get => ParseSeconds(TimeValue).ToString("D2", CultureInfo.InvariantCulture);
            set => TimeValue = FormatHms(ParseHoursMinutes(TimeValue), ClampSeconds(value));
        }

        /// <summary>
        /// Часы:минуты временной части DATETIME для Avalonia TimePicker (секунды — в
        /// <see cref="DateTimeSecondsText" />). Адаптер поверх <see cref="DateTimeTimeValue" />.
        /// </summary>
        public TimeSpan? DateTimeHoursMinutesValue
        {
            get => ParseHoursMinutes(DateTimeTimeValue);
            set => DateTimeTimeValue = FormatHms(value, ParseSeconds(DateTimeTimeValue));
        }

        /// <summary>
        /// Секунды временной части DATETIME (00..59) для отдельного поля рядом с TimePicker.
        /// </summary>
        public string DateTimeSecondsText
        {
            get => ParseSeconds(DateTimeTimeValue).ToString("D2", CultureInfo.InvariantCulture);
            set => DateTimeTimeValue = FormatHms(ParseHoursMinutes(DateTimeTimeValue), ClampSeconds(value));
        }

        /// <summary>
        /// Команда выбора локального файла для значения системного типа FILE.
        /// </summary>
        /// <remarks>
        /// Сейчас UI выбирает файл из локальной файловой системы. При появлении MinIO рядом можно добавить
        /// отдельную команду/диалог выбора объекта внешнего хранилища, сохранив общий StringValue-контракт.
        /// </remarks>
        public RelayCommand BrowseFileValueCommand
        {
            get
            {
                return new RelayCommand(
                    _ => BrowseFileValue(),
                    _ => SystemBaseType == SystemBaseType.FILE);
            }
        }

        /// <summary>
        /// Команда открытия локального файла, указанного в значении системного типа FILE.
        /// </summary>
        public RelayCommand OpenFileValueCommand
        {
            get
            {
                return new RelayCommand(
                    _ => OpenFileValue(),
                    _ => SystemBaseType == SystemBaseType.FILE && IsLocalFileAvailable(StringValue));
            }
        }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveVM" />.
        /// </summary>
        /// <param name="parent">Родительский элемент.</param>
        /// <param name="treeLeave">Лист рабочего дерева.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeLeaveVM(
            TreeNodeVM parent,
            TreeLeaveModel treeLeave,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService) 
            : base(treeLeave, dataStoragesCollectionVM, service, fileDialogService)
        {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);

            _service = service;

            Parent = parent;
        }

        #endregion

        #region [Commands]


        #endregion

        #region [ Methods ]

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            foreach (var item in AttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        /// <summary>
        /// Уведомляет UI, что типизированные представления строкового значения могли измениться.
        /// </summary>
        /// <remarks>
        /// Все типизированные свойства вычисляются из <see cref="StringValue" />, поэтому после изменения
        /// строки нужно обновить и дату, и время, и offset.
        /// </remarks>
        private void NotifyTypedValuePropertiesChanged()
        {
            OnPropertyChanged(nameof(DateValue));
            OnPropertyChanged(nameof(DateTimeDateValue));
            OnPropertyChanged(nameof(TimeValue));
            OnPropertyChanged(nameof(DateTimeTimeValue));
            OnPropertyChanged(nameof(DateTimeOffsetValue));
            OnPropertyChanged(nameof(TimeHoursMinutesValue));
            OnPropertyChanged(nameof(TimeSecondsText));
            OnPropertyChanged(nameof(DateTimeHoursMinutesValue));
            OnPropertyChanged(nameof(DateTimeSecondsText));
        }

        /// <summary>Часы:минуты из строки времени (HH:mm:ss) как TimeSpan для TimePicker; null если не распознано.</summary>
        private static TimeSpan? ParseHoursMinutes(string? time)
        {
            return TimeOnly.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? new TimeSpan(parsed.Hour, parsed.Minute, 0)
                : null;
        }

        /// <summary>Секунды (0..59) из строки времени (HH:mm:ss); 0 если не распознано.</summary>
        private static int ParseSeconds(string? time)
        {
            return TimeOnly.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.Second
                : 0;
        }

        /// <summary>Парсит секунды из текста поля и ограничивает диапазоном 0..59.</summary>
        private static int ClampSeconds(string? text)
        {
            return int.TryParse(text, out var seconds) ? Math.Clamp(seconds, 0, 59) : 0;
        }

        /// <summary>Собирает строку времени HH:mm:ss из часов:минут (TimeSpan) и секунд.</summary>
        private static string FormatHms(TimeSpan? hoursMinutes, int seconds)
        {
            var hm = hoursMinutes ?? TimeSpan.Zero;
            return $"{hm.Hours:D2}:{hm.Minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// Пытается извлечь дату из строкового значения DATE или DATETIME.
        /// </summary>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <returns>Дата в виде <see cref="DateTime" /> для WPF DatePicker; null, если строка не распознана.</returns>
        private static DateTime? TryGetDate(string value)
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
            {
                return dateTimeOffset.Date;
            }

            if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date.ToDateTime(TimeOnly.MinValue);
            }

            return null;
        }

        /// <summary>
        /// Пытается извлечь время из строкового значения TIME или DATETIME.
        /// </summary>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <returns>Время в формате HH:mm:ss; null, если строка не распознана.</returns>
        private static string? TryGetTime(string value)
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
            {
                return dateTimeOffset.TimeOfDay.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            }

            if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return null;
        }

        /// <summary>
        /// Пытается извлечь смещение часового пояса из строкового значения DATETIME.
        /// </summary>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <returns>Смещение в формате +HH:mm или -HH:mm; null, если строка не распознана.</returns>
        private static string? TryGetOffset(string value)
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
            {
                return FormatOffset(dateTimeOffset.Offset);
            }

            return null;
        }

        /// <summary>
        /// Собирает строковое значение DATETIME из отдельных UI-полей даты, времени и смещения.
        /// </summary>
        /// <remarks>
        /// Если время или offset сейчас набраны не полностью, метод сохраняет введенный текст как есть.
        /// Это важно для UX с UpdateSourceTrigger=PropertyChanged: доменная валидация увидит промежуточное
        /// значение и отклонит его, но ViewModel не будет молча заменять пользовательский ввод дефолтом.
        /// </remarks>
        /// <param name="date">Дата DATETIME.</param>
        /// <param name="timeValue">Строковая часть времени.</param>
        /// <param name="offsetValue">Строковая часть смещения.</param>
        /// <returns>Строка DATETIME для записи в <see cref="StringValue" />.</returns>
        private static string BuildDateTimeValue(DateTime date, string timeValue, string offsetValue)
        {
            var timeSegment = TimeOnly.TryParse(timeValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime)
                ? parsedTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                : timeValue;
            var offsetSegment = TryParseOffset(offsetValue, out var parsedOffset)
                ? FormatOffset(parsedOffset)
                : offsetValue;

            return $"{date:yyyy-MM-dd}T{timeSegment}{offsetSegment}";
        }

        /// <summary>
        /// Пытается распознать смещение часового пояса в формате HH:mm, +HH:mm или -HH:mm.
        /// </summary>
        /// <param name="value">Строковое значение смещения.</param>
        /// <param name="offset">Распознанное смещение.</param>
        /// <returns>true, если смещение распознано; иначе false.</returns>
        private static bool TryParseOffset(string value, out TimeSpan offset)
        {
            if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out offset))
            {
                return true;
            }

            if (value.Length > 1
                && (value[0] == '+' || value[0] == '-')
                && TimeSpan.TryParseExact(value[1..], @"hh\:mm", CultureInfo.InvariantCulture, out offset))
            {
                if (value[0] == '-')
                {
                    offset = -offset;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Форматирует смещение часового пояса в invariant-формате +HH:mm или -HH:mm.
        /// </summary>
        /// <param name="offset">Смещение часового пояса.</param>
        /// <returns>Строковое представление смещения.</returns>
        private static string FormatOffset(TimeSpan offset)
        {
            var sign = offset < TimeSpan.Zero ? "-" : "+";
            offset = offset.Duration();
            return string.Create(CultureInfo.InvariantCulture, $"{sign}{offset:hh\\:mm}");
        }

        /// <summary>
        /// Открывает стандартный диалог выбора локального файла и записывает выбранный путь в <see cref="StringValue" />.
        /// </summary>
        private void BrowseFileValue()
        {
            var path = _fileDialogService.BrowseLocalFile();
            if (path != null)
            {
                StringValue = path;
            }
        }

        /// <summary>
        /// Открывает локальный файл, указанный в <see cref="StringValue" />, через ассоциации Windows.
        /// </summary>
        private void OpenFileValue()
        {
            var filePath = ResolveLocalFilePath(StringValue);

            if (filePath is null || IsLocalFileAvailable(filePath) == false)
            {
                return;
            }

            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true,
            });
        }

        /// <summary>
        /// Преобразует строковое значение локального файла в путь, который можно передать shell.
        /// </summary>
        /// <param name="value">Путь к файлу или file:// URI.</param>
        /// <returns>Локальный путь к файлу или null, если значение не похоже на локальный файл.</returns>
        private static string? ResolveLocalFilePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                ? (uri.IsFile ? uri.LocalPath : null)
                : value;
        }

        /// <summary>
        /// Проверяет, что строковое значение указывает на существующий локальный файл.
        /// </summary>
        /// <remarks>
        /// UI пока открывает только локальные файлы. Значения будущих внешних хранилищ, например MinIO,
        /// должны будут открываться через отдельный провайдер, а не через shell.
        /// </remarks>
        /// <param name="value">Путь к файлу или file:// URI.</param>
        /// <returns>true, если файл существует; иначе false.</returns>
        private static bool IsLocalFileAvailable(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    return uri.IsFile && File.Exists(uri.LocalPath);
                }

                return File.Exists(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        #endregion
    }
}
