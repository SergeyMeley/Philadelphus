using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Infrastructure.Persistence.ADO.PostgreSQL.Repositories
{
    public class PostgreMainEntityInfrastructure : IPhiladelphusRepositoriesMembersInfrastructureRepository
    {
        private readonly Context _context;

        public InfrastructureEntityGroups EntityGroup => throw new NotImplementedException();

        public PostgreMainEntityInfrastructure()
        {
            _context = new Context();
        }
        #region [Select]
        public IEnumerable<PhiladelphusRepository> SelectRepositories(List<string> pathes)
        {
            var dataCollection = new List<PhiladelphusRepository>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new PhiladelphusRepository();
                    record.Name = reader.GetString(1);
                    if (!reader.IsDBNull(3))
                    {
                        record.Description = reader.GetString(3);
                    }
                    dataCollection.Add(record);
                }
            }
            return dataCollection;
        }
        public IEnumerable<TreeRoot> SelectRoots()
        {
            var dataCollection = new List<TreeRoot>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new TreeRoot();
                    record.Name = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                    {
                        record.Description = reader.GetString(2);
                    }
                    dataCollection.Add(record);
                }
            }
            return dataCollection;
        }
        public IEnumerable<TreeNode> SelectNodes()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<TreeLeave> SelectLeaves()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            var result = new List<ElementAttribute>();
            return result;
        }
        #endregion
        #region [Insert]
        public int InsertRepositories(IEnumerable<PhiladelphusRepository> projects)
        {
            try
            {
                using (var cmd = _context.CreateConnection().CreateCommand($""))
                {
                    cmd.ExecuteNonQuery();
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public int InsertRoots(IEnumerable<TreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int InsertNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int InsertLeaves(IEnumerable<TreeLeave> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Delete]
        public int DeleteRepositories(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteRoots(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteNodes(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteLeaves(int[] ids)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Update]
        public int UpdateRepositories(IEnumerable<PhiladelphusRepository> projects)
        {
            throw new NotImplementedException();
        }
        public int UpdateRoots(IEnumerable<TreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int UpdateLeaves(IEnumerable<TreeLeave> elements)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PhiladelphusRepository> SelectRepositories(string configPath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots(PhiladelphusRepository dbPhiladelphusRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes(PhiladelphusRepository dbPhiladelphusRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves(PhiladelphusRepository dbPhiladelphusRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttribute> SelectAttributes(PhiladelphusRepository dbPhiladelphusRepository)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long DeleteRepositories(IEnumerable<PhiladelphusRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long DeleteRoots(IEnumerable<TreeRoot> roots)
        {
            throw new NotImplementedException();
        }

        public long DeleteNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        public long DeleteLeaves(IEnumerable<TreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.InsertRoots(IEnumerable<TreeRoot> roots)
        {
            return InsertRoots(roots);
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.InsertNodes(IEnumerable<TreeNode> nodes)
        {
            return InsertNodes(nodes);
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.InsertLeaves(IEnumerable<TreeLeave> leaves)
        {
            return InsertLeaves(leaves);
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.UpdateRoots(IEnumerable<TreeRoot> roots)
        {
            return UpdateRoots(roots);
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            return UpdateNodes(nodes);
        }

        long IPhiladelphusRepositoriesMembersInfrastructureRepository.UpdateLeaves(IEnumerable<TreeLeave> leaves)
        {
            return UpdateLeaves(leaves);
        }

        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots(Guid[] uuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
