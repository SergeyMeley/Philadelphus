using Microsoft.Extensions.Primitives;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    public class TreeLeaveVM : MainEntityBaseVM //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        public string Alias
        {
            get
            {
                if (_model is TreeLeaveModel m)
                {
                    return m.Alias;
                }
                return string.Empty;
            }
            set
            {
                if (_model is TreeLeaveModel m)
                {
                    m.Alias = value;
                    OnPropertyChanged(nameof(Alias));
                    OnPropertyChanged(nameof(State));
                }
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

        public TreeLeaveVM(
            TreeLeaveModel treeLeave,
            IPhiladelphusRepositoryService service) 
            : base(treeLeave, service)
        {
            _service = service;
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
