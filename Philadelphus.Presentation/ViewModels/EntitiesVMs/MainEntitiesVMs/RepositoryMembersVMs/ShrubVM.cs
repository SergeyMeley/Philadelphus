using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Services.StateVisibility;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    /// <summary>
    /// Нередактируемый узел-группа для визуального дерева обозревателя.
    /// Кустарник здесь только объединяет рабочие деревья внутри репозитория.
    /// </summary>
    public class ShrubVM : ViewModelBase, IMainEntityVM
    {
        private readonly ShrubModel _shrub;
        private readonly ObservableCollection<WorkingTreeVM> _workingTrees = new();

        public ObservableCollection<WorkingTreeVM> WorkingTrees => _workingTrees;

        public IEnumerable<IMainEntityVM> TreeChilds => _workingTrees;

        public bool IsTreeExpandedByDefault => true;

        public Guid Uuid => _shrub.Uuid;

        public string Name => _shrub.Name;

        public State State => _shrub.State;

        public State ChildContentAggregateState => StateVisibilityInfoBuilder.Build(_shrub).ChildContentState ?? State.SavedOrLoaded;

        public string StateVisibilityToolTip
            => $"Репозиторий: {State.GetDisplayDescription()}{Environment.NewLine}"
             + $"Содержимое: {ChildContentAggregateState.GetDisplayDescription()}";

        public ShrubVM(
            ShrubModel shrub,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService,
            IEnumerable<WorkingTreeModel>? workingTrees = null)
        {
            ArgumentNullException.ThrowIfNull(shrub);

            _shrub = shrub;

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
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(ChildContentAggregateState));
            OnPropertyChanged(nameof(StateVisibilityToolTip));

            foreach (var workingTree in WorkingTrees)
            {
                workingTree.NotifyChildsPropertyChangedRecursive();
            }
        }
    }
}
