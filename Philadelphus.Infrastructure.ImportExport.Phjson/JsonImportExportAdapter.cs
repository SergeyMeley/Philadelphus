using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Infrastructure.ImportExport.Phjson
{
    /// <summary>
    /// Адаптер импорта и экспорта рабочих деревьев в формате PHJSON.
    /// </summary>
    public class JsonImportExportAdapter : ImportExportAdapterBase
    {
        /// <summary>
        /// Расширение файлов PHJSON.
        /// </summary>
        public const string PhjsonFileFormat = ".phjson";

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="JsonImportExportAdapter" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public JsonImportExportAdapter(INotificationService notificationService)
            : base(notificationService, PhjsonFileFormat, "Базовый")
        {
        }

        /// <summary>
        /// Сериализует DTO рабочего дерева в файл PHJSON.
        /// </summary>
        /// <param name="dto">DTO рабочего дерева.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        public override void Serialize(WorkingTreeExportDTO dto, string filePath)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            File.WriteAllText(filePath, JsonImportExportHelper.GetJson(dto));
        }

        /// <summary>
        /// Читает файл PHJSON и преобразует его в DTO рабочего дерева.
        /// </summary>
        /// <param name="filePath">Путь к исходному файлу.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public override WorkingTreeExportDTO Parse(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (File.Exists(filePath) == false)
            {
                throw new FileNotFoundException("Файл импорта не найден.", filePath);
            }

            var json = File.ReadAllText(filePath);

            return JsonImportExportHelper.ParseDtoFromJson(json);
        }

        /// <summary>
        /// Импортирует рабочее дерево из JSON-строки формата PHJSON в доменную модель.
        /// </summary>
        /// <param name="json">JSON-строка.</param>
        /// <param name="service">Доменный сервис репозитория.</param>
        /// <param name="repository">Репозиторий Чубушника.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        /// <returns>Импортированное рабочее дерево.</returns>
        public WorkingTreeModel ImportFromJson(
            string json,
            IPhiladelphusRepositoryService service,
            PhiladelphusRepositoryModel repository,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress)
        {
            return JsonImportExportHelper.ParseJson(json, service, repository, refreshProcess, refreshProgress);
        }
    }
}
