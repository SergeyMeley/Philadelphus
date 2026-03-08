using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    public interface IMainEntityVM<out T> where T : IMainEntityModel
    {
        public T Model { get; }
        public Guid Uuid { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AuditInfoModel AuditInfo { get; }
        public State State { get; }
        public ObservableCollection<ElementAttributeVM> AttributesVMs { get; }
        public ElementAttributeVM SelectedAttributeVM { get; set; }
        public DataStorageVM StorageVM { get; }
        public ElementAttributeVM AddAttribute();
    }
}
