using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    /// <summary>
    /// Маркерный интерфейс модели представления сущности рабочего дерева (без параметра модели).
    /// </summary>
    public interface IMainEntityVM
    {
        /// <summary>
        /// Признак раскрытия элемента в дереве обозревателя репозитория.
        /// </summary>
        bool IsTreeExpanded { get; set; }
    }

    /// <summary>
    /// Задает контракт для работы с IMainEntityVM.
    /// </summary>
    public interface IMainEntityVM<out T> : IMainEntityVM where T : IMainEntityModel
    {
        /// <summary>
        /// Модель.
        /// </summary>
        public T Model { get; }
        
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public Guid Uuid { get; }
       
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }
       
        /// <summary>
        /// Информация аудита.
        /// </summary>
        public AuditInfoModel AuditInfo { get; }
        
        /// <summary>
        /// Состояние.
        /// </summary>
        public State State { get; }
       
        /// <summary>
        /// Модели представления атрибутов.
        /// </summary>
        public ObservableCollection<ElementAttributeVM> AttributesVMs { get; }
        
        /// <summary>
        /// Выбранная модель представления атрибута.
        /// </summary>
        public ElementAttributeVM SelectedAttributeVM { get; set; }
       
        /// <summary>
        /// Модель представления хранилища данных.
        /// </summary>
        public DataStorageVM StorageVM { get; }
      
        /// <summary>
        /// Добавляет данные AddAttribute.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public ElementAttributeVM AddAttribute();
    }
}