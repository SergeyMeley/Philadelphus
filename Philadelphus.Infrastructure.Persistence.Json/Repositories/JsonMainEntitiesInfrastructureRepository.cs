using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Json.Repositories
{
    public class JsonMainEntitiesInfrastructureRepository : IShrubMembersInfrastructureRepository
    {
        private DirectoryInfo _baseDirectory;
        public JsonMainEntitiesInfrastructureRepository(DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }
        public InfrastructureEntityGroups EntityGroup => InfrastructureEntityGroups.ShrubMembers;

        public bool CheckAvailability()
        {
            return _baseDirectory.Exists;
        }

        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items)
        {
            throw new NotImplementedException();
        }

        public long SoftDeleteNodes(IEnumerable<TreeNode> items)
        {
            throw new NotImplementedException();
        }

        public long SoftDeleteRoots(IEnumerable<TreeRoot> items)
        {
            throw new NotImplementedException();
        }

        public long SoftDeleteTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long InsertLeaves(IEnumerable<TreeLeave> items)
        {
            throw new NotImplementedException();
        }

        public long InsertNodes(IEnumerable<TreeNode> items)
        {
            throw new NotImplementedException();
        }

        public long InsertRoots(IEnumerable<TreeRoot> items)
        {
            throw new NotImplementedException();
        }

        public long InsertTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingTree> SelectTreeAggregates(Guid[]? uuids = null)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateLeaves(IEnumerable<TreeLeave> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateNodes(IEnumerable<TreeNode> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateRoots(IEnumerable<TreeRoot> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }

    }
}
