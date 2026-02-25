using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Базовый класс для расширений
    /// </summary>
    public abstract class ExtensionBaseModel : IExtensionModel
    {
        public virtual IExtensionMetadataModel Metadata { get; protected set; }

        public virtual Task StartAsync() => Task.CompletedTask;
        public virtual Task StopAsync() => Task.CompletedTask;

        public abstract Task<CanExecuteResultModel> CanExecuteAsync(MainEntityBaseModel element);
        public abstract Task<MainEntityBaseModel> ExecuteAsync(MainEntityBaseModel element, IPhiladelphusRepositoryService service, CancellationToken cancellationToken = default);

        public virtual object GetRepositoryExplorerWidget() => null;
        public virtual object GetRibbonWidget() => null;
        public virtual object GetMainWindow() => null;
        public virtual void InitializeWidgets() { }
        public virtual void UninitializeWidgets() { }
    }

}
