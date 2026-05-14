using Microsoft.Extensions.Primitives;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
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
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(State));
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
            IPhiladelphusRepositoryService service) 
            : base(treeLeave, dataStoragesCollectionVM, service)
        {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentNullException.ThrowIfNull(service);

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

        #endregion
    }
}
