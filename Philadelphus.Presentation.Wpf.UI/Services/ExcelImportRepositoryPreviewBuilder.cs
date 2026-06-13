using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public class ExcelImportRepositoryPreviewBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileDialogService _fileDialogService;

        public ExcelImportRepositoryPreviewBuilder(
            IServiceProvider serviceProvider,
            IFileDialogService fileDialogService)
        {
            _serviceProvider = serviceProvider;
            _fileDialogService = fileDialogService;
        }

        public RepositoryExplorerControlVM Build(
            PhiladelphusRepositoryModel previewRepository,
            WorkingTreeModel previewTree,
            string? targetExistingRootName)
        {
            var repositoryService = _serviceProvider.GetRequiredService<IPhiladelphusRepositoryService>();
            var dataStoragesCollectionVm = _serviceProvider.GetRequiredService<DataStoragesCollectionVM>();

            TreeRootModel? previewTargetRoot = null;
            if (string.IsNullOrWhiteSpace(targetExistingRootName) == false)
            {
                var previewStorage = previewRepository.DataStorages?.FirstOrDefault(x => x.HasShrubMembersInfrastructureRepository)
                    ?? throw new InvalidOperationException("Не удалось получить временное хранилище для предпросмотра.");

                var previewWorkingTree = repositoryService.CreateWorkingTree(previewRepository, previewStorage, needAutoName: false, withoutInfoNotifications: true);
                previewWorkingTree.Name = $"Предпросмотр: {targetExistingRootName}";
                previewTargetRoot = repositoryService.CreateTreeRoot(previewWorkingTree, needAutoName: false, withoutInfoNotifications: true);
                previewTargetRoot.Name = targetExistingRootName;
            }

            var previewRepositoryVm = new PhiladelphusRepositoryVM(previewRepository, dataStoragesCollectionVm, repositoryService, _fileDialogService);
            previewRepositoryVm.Childs.Clear();

            var rootForPreview = previewTargetRoot ?? previewTree.ContentRoot;
            if (rootForPreview != null && rootForPreview.IsSystemBase == false)
            {
                previewRepositoryVm.Childs.Add(new TreeRootVM(rootForPreview, dataStoragesCollectionVm, repositoryService, _fileDialogService));
            }

            var previewExplorerVm = ActivatorUtilities.CreateInstance<RepositoryExplorerControlVM>(
                _serviceProvider,
                previewRepositoryVm,
                false);
            previewExplorerVm.SelectedRepositoryMember = previewRepositoryVm.Childs.FirstOrDefault();
            return previewExplorerVm;
        }

        public string BuildSummary(RepositoryExplorerControlVM previewViewModel)
        {
            var roots = previewViewModel.PhiladelphusRepositoryVM.Childs.Count;
            var nodes = previewViewModel.PhiladelphusRepositoryVM.Childs.Sum(CountNodes);
            var leaves = previewViewModel.PhiladelphusRepositoryVM.Childs.Sum(CountLeaves);
            return $"Предпросмотр дерева. Корней: {roots}. Узлов: {nodes}. Листьев: {leaves}.";
        }

        private static int CountNodes(TreeRootVM root)
        {
            return root.ChildNodes.Sum(node => 1 + CountChildNodes(node));
        }

        private static int CountChildNodes(TreeNodeVM node)
        {
            return node.ChildNodes.Sum(child => 1 + CountChildNodes(child));
        }

        private static int CountLeaves(TreeRootVM root)
        {
            return root.ChildNodes.Sum(CountLeaves);
        }

        private static int CountLeaves(TreeNodeVM node)
        {
            return node.ChildLeaves.Count + node.ChildNodes.Sum(CountLeaves);
        }
    }
}
