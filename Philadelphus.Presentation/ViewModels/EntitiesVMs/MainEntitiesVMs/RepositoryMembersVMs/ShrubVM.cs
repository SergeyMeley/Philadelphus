using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    /// <summary>
    /// Модель представления кустарника в визуальном дереве обозревателя.
    /// Кустарник остается доменным контейнером рабочих деревьев и транслирует состояние репозитория через модель.
    /// </summary>
    public class ShrubVM : MainEntityBaseVM<ShrubModel>
    {
        private readonly ObservableCollection<WorkingTreeVM> _workingTrees = new();

        public ObservableCollection<WorkingTreeVM> WorkingTrees => _workingTrees;

        /// <summary>
        /// Модель представления репозитория-владельца.
        /// </summary>
        public PhiladelphusRepositoryVM OwningRepositoryVM { get; }

        public override IEnumerable<IMainEntityVM> TreeChilds => _workingTrees;

        public override bool IsTreeExpandedByDefault => true;

        public string Alias
        {
            get => Model.Alias;
            set
            {
                Model.Alias = value;
                OnPropertyChanged(nameof(Alias));
                NotifyStateVisibilityPropertiesChanged();
            }
        }

        public string CustomCode
        {
            get => string.Empty;
            set { }
        }

        public ShrubVM(
            ShrubModel shrub,
            PhiladelphusRepositoryVM owningRepositoryVM,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService,
            IEnumerable<WorkingTreeModel>? workingTrees = null)
            : base(shrub, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(shrub);
            ArgumentNullException.ThrowIfNull(owningRepositoryVM);

            OwningRepositoryVM = owningRepositoryVM;

            foreach (var workingTree in workingTrees ?? shrub.ContentWorkingTrees)
            {
                _workingTrees.Add(new WorkingTreeVM(
                    workingTree,
                    dataStoragesCollectionVM,
                    service,
                    fileDialogService,
                    notificationService));
            }
        }

        public void NotifyChildsPropertyChangedRecursive()
        {
            NotifyStateVisibilityPropertiesChanged();
            OnPropertyChanged(nameof(Name));

            foreach (var workingTree in WorkingTrees)
            {
                workingTree.NotifyChildsPropertyChangedRecursive();
            }
        }
    }
}
