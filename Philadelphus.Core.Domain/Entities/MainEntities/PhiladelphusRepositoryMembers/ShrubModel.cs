using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers
{
    /// <summary>
    /// Доменная модель кустарника рабочих деревьев.
    /// </summary>
    public class ShrubModel : PhiladelphusRepositoryMemberBaseModel<ShrubModel>, IOwnerModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый рабочий кустарник";

        private string _alias;
        private string _customCode;
        private List<Guid> _contentTreesUuids = new List<Guid>();

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]



        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Владеющий репозиторий
        /// </summary>
        [Display(Name = "Репозиторий", Description = "Владеющий репозиторий")]
        public PhiladelphusRepositoryModel OwningRepository { get; }

        /// <summary>
        /// Содержащиеся рабочие деревья
        /// </summary>
        [Display(Name = "Рабочие деревья", Description = "Содержащиеся рабочие деревья")]
        public List<WorkingTreeModel> ContentWorkingTrees { get; }

        /// <summary>
        /// Системное рабочее дерево
        /// </summary>
        [Display(Name = "Системное дерево", Description = "Системное рабочее дерево")]
        public WorkingTreeModel SystemBaseWorkingTree { get => ContentWorkingTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseUuid); }

        /// <summary>
        /// Содержимое
        /// </summary>
        [Display(Name = "Содержимое", Description = "Содержимое")]
        public ReadOnlyDictionary<Guid, IContentModel> Content 
        {
            get
            {
                var result = new Dictionary<Guid, IContentModel>();

                foreach (var item in ContentWorkingTrees)
                {
                    result.Add(item.Uuid, item);
                }

                return result.AsReadOnly();
            } 
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        [Display(Name = "Все содержимое", Description = "Все содержимое")]
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => throw new NotImplementedException(); 
        }


        /// <summary>
        /// Уникальные идентификаторы содержащихся деревьев
        /// </summary>
        [Display(Name = "UUID'ы деревьев", Description = "Уникальные идентификаторы содержащихся рабочих деревьев")]
        public List<Guid> ContentWorkingTreesUuids 
        { 
            get
            {
                return _contentTreesUuids;
            }
            set
            {
                if (_contentTreesUuids != value)
                {
                    _contentTreesUuids = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Хранилище данных
        /// </summary>
        [Display(Name = "Хранилище", Description = "Хранилище данных")]
        public override IDataStorageModel DataStorage 
        { 
            get => OwningRepository.DataStorage;
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Рабочий кустарник
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="owner">Владелец</param>
        internal ShrubModel(
            Guid uuid,
            PhiladelphusRepositoryModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<ShrubModel> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(owner);

            OwningRepository = owner;

            ContentWorkingTrees = new List<WorkingTreeModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool AddContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content is WorkingTreeModel wt 
                && ContentWorkingTrees.Any(x => x.Uuid == content.Uuid) == false)
            {
                ContentWorkingTrees.Add(wt);
                ContentWorkingTreesUuids.Add(wt.Uuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool RemoveContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content is WorkingTreeModel wt
                && ContentWorkingTrees.Any(x => x.Uuid == content.Uuid))
            {
                var remItem = ContentWorkingTrees.First(x => x.Uuid == content.Uuid);
                ContentWorkingTrees.Remove(remItem);
                ContentWorkingTreesUuids.Remove(remItem.Uuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ClearContent()
        {
            ContentWorkingTrees.Clear();
            ContentWorkingTreesUuids.Clear();
            return true;
        }

        #endregion
    }
}
