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
    /// </summary>
    public class ShrubVM : MainEntityBaseVM<ShrubModel>
    {
        private readonly ObservableCollection<WorkingTreeVM> _workingTrees = new();

        public ObservableCollection<WorkingTreeVM> WorkingTrees => _workingTrees;

        public override IEnumerable<IMainEntityVM> TreeChilds => _workingTrees;

        public override bool IsTreeExpandedByDefault => true;

        public ShrubVM(
            ShrubModel shrub,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService,
            IEnumerable<WorkingTreeModel>? workingTrees = null)
            : base(shrub, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(shrub);

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
            foreach (var workingTree in WorkingTrees)
            {
                workingTree.NotifyChildsPropertyChangedRecursive();
            }
        }
    }
}
