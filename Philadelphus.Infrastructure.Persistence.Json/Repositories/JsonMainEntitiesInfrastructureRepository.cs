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
    public class JsonMainEntitiesInfrastructureRepository : IPhiladelphusRepositoriesMembersInfrastructureRepository
    {
        private DirectoryInfo _baseDirectory;
        public JsonMainEntitiesInfrastructureRepository(DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }
        public InfrastructureEntityGroups EntityGroup => InfrastructureEntityGroups.MainEntities;

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

        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots(Guid[] uuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingTree> SelectTrees()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingTree> SelectTrees(Guid[] uuids)
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
