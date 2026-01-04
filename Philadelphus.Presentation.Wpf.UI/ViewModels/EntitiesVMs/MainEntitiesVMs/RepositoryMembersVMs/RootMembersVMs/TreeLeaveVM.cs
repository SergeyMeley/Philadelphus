using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    public class TreeLeaveVM : MainEntityBaseVM //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly ITreeRepositoryService _service;

        private readonly TreeLeaveModel _model;

        #endregion

        #region [ Construct ]

        public TreeLeaveVM(
            TreeLeaveModel treeLeave,
            ITreeRepositoryService service) 
            : base(treeLeave, service)
        {
            _service = service;
            _model = treeLeave;
        }

        #endregion

        #region [Commands]


        #endregion

        #region [ Methods ]

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            foreach (var item in PersonalAttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
