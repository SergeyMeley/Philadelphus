using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.TreeRepositoryElementsVMs.RepositoryMembersVMs.RootMembersVMs
{
    public class TreeLeaveVM : MainEntityBaseVM
    {
        #region [ Props ]

        private readonly TreeLeaveModel _model;

        #endregion

        #region [ Construct ]

        public TreeLeaveVM(TreeLeaveModel treeLeave, TreeRepositoryService service) : base(treeLeave, service)
        {
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
