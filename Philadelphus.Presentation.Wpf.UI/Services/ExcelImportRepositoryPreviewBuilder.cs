using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Phjson;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public class ExcelImportRepositoryPreviewBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ExcelImportRepositoryPreviewBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RepositoryExplorerControlVM Build(string json, string? targetExistingRootName)
        {
            var repositoryService = _serviceProvider.GetRequiredService<IPhiladelphusRepositoryService>();
            var repositoryCollectionService = _serviceProvider.GetRequiredService<IPhiladelphusRepositoryCollectionService>();
            var dataStoragesCollectionVm = _serviceProvider.GetRequiredService<DataStoragesCollectionVM>();
            var jsonImportExportAdapter = _serviceProvider.GetRequiredService<JsonImportExportAdapter>();

            var previewStorage = dataStoragesCollectionVm.MainDataStorageVM?.Model
                ?? dataStoragesCollectionVm.DataStoragesVMs?.Select(x => x.Model).FirstOrDefault(x => x != null);

            if (previewStorage == null)
                throw new InvalidOperationException("Не удалось получить временное хранилище для предпросмотра.");

            var previewRepository = repositoryCollectionService.CreateNewPhiladelphusRepository(previewStorage, needAutoName: false);
            previewRepository.Name = "Предпросмотр импорта";
            previewRepository.Description = "Временный репозиторий для предпросмотра дерева из Excel";

            repositoryService.GetShrubContent(previewRepository);

            TreeRootModel? previewTargetRoot = null;
            if (string.IsNullOrWhiteSpace(targetExistingRootName) == false)
            {
                var previewWorkingTree = repositoryService.CreateWorkingTree(previewRepository, previewStorage, needAutoName: false, withoutInfoNotifications: true);
                previewWorkingTree.Name = $"Предпросмотр: {targetExistingRootName}";
                previewTargetRoot = repositoryService.CreateTreeRoot(previewWorkingTree, needAutoName: false, withoutInfoNotifications: true);
                previewTargetRoot.Name = targetExistingRootName;
            }

            var previewTree = jsonImportExportAdapter.ImportFromJson(json, repositoryService, previewRepository, _ => { }, (_, _) => { });
            var previewRepositoryVm = new PhiladelphusRepositoryVM(previewRepository, dataStoragesCollectionVm, repositoryService);
            previewRepositoryVm.Childs.Clear();

            var rootForPreview = previewTargetRoot ?? previewTree.ContentRoot;
            if (rootForPreview != null && rootForPreview.IsSystemBase == false)
            {
                previewRepositoryVm.Childs.Add(new TreeRootVM(rootForPreview, dataStoragesCollectionVm, repositoryService));
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
