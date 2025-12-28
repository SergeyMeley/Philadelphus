using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface ITreeRepositoryService
    {
        #region [ Get + Load ]

        public IMainEntity GetEntityFromCollection(Guid guid);
        public IMainEntityModel GetModelFromCollection(Guid guid);
        public TreeRepositoryModel LoadMainEntityCollection(TreeRepositoryModel repository);
        public TreeRepositoryModel GetRepositoryContent(TreeRepositoryModel repository);
        public TreeRootModel GetRootContent(TreeRootModel root);
        public TreeNodeModel GetNodeContent(TreeNodeModel node);
        public TreeLeaveModel GetLeaveContent(TreeLeaveModel leave);
        public IEnumerable<ElementAttributeModel> GetPersonalAttributes(IAttributeOwnerModel attributeOwner);

    #endregion

        #region [ Save ]

        public long SaveChanges(TreeRepositoryModel treeRepository);
        public long SaveChanges(IEnumerable<TreeRootModel> treeRoots);
        public long SaveChanges(IEnumerable<TreeNodeModel> treeNodes);
        public long SaveChanges(IEnumerable<TreeLeaveModel> treeLeaves);

        public long SaveChanges(IEnumerable<ElementAttributeModel> elementAttributes);

        #endregion

        #region [ Create + Add ]

        public TreeRootModel CreateTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage);
        public TreeNodeModel CreateTreeNode(IParentModel parentElement);
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parentElement);
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        public bool SoftDeleteRepositoryMember(IChildrenModel element);

        #endregion
    }
}
