using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    /// <summary>
    /// Модель представления рабочего дерева в визуальном дереве обозревателя.
    /// </summary>
    public class WorkingTreeVM : MainEntityBaseVM<WorkingTreeModel>
    {
        private readonly ObservableCollection<TreeRootVM> _roots = new();

        public ObservableCollection<TreeRootVM> Roots => _roots;

        public override IEnumerable<IMainEntityVM> TreeChilds => _roots;

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

        public WorkingTreeVM(
            WorkingTreeModel workingTree,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService)
            : base(workingTree, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(workingTree);

            if (workingTree.ContentRoot != null)
            {
                _roots.Add(new TreeRootVM(
                    workingTree.ContentRoot,
                    dataStoragesCollectionVM,
                    service,
                    fileDialogService,
                    notificationService));
            }
        }

        public void NotifyChildsPropertyChangedRecursive()
        {
            NotifyStateVisibilityPropertiesChanged();
            foreach (var root in Roots)
            {
                root.NotifyChildsPropertyChangedRecursive();
            }
        }
    }
}
