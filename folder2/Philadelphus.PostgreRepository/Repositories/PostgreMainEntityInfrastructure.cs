using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreInfrastructure.Repositories
{
    public class PostgreMainEntityInfrastructure : IMainEntitiesInfrastructureRepository
    {
        private readonly Context _context;
        public PostgreMainEntityInfrastructure()
        {
            _context = new Context();
        }
        public MainEntitiesCollection GetMainEntitiesCollection()
        {
            MainEntitiesCollection collection = new MainEntitiesCollection();
            //collection.DbTreeRoots = (List<DbTreeRoot>)SelectRoots();
            collection.DbTreeNodes = (List<TreeNode>)SelectNodes();
            return collection;
        }
        #region [Select]
        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes)
        {
            var dataCollection = new List<TreeRepository>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new TreeRepository();
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
        public IEnumerable<TreeElementAttribute> SelectAttributeEntries()
        {
            var result = new List<TreeElementAttribute>();
            return result;
        }
        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues()
        {
            var result = new List<TreeElementAttributeValue>();
            return result;
        }
        #endregion
        #region [Insert]
        public int InsertRepositories(IEnumerable<TreeRepository> projects)
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
        public int UpdateRepositories(IEnumerable<TreeRepository> projects)
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

        public IEnumerable<TreeRepository> SelectRepositories(string configPath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttribute> SelectAttributes(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeElementAttribute> SelectAttributeEntries(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues(TreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
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

        public long DeleteAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesInfrastructureRepository.InsertRoots(IEnumerable<TreeRoot> roots)
        {
            return InsertRoots(roots);
        }

        long IMainEntitiesInfrastructureRepository.InsertNodes(IEnumerable<TreeNode> nodes)
        {
            return InsertNodes(nodes);
        }

        long IMainEntitiesInfrastructureRepository.InsertLeaves(IEnumerable<TreeLeave> leaves)
        {
            return InsertLeaves(leaves);
        }

        long IMainEntitiesInfrastructureRepository.UpdateRoots(IEnumerable<TreeRoot> roots)
        {
            return UpdateRoots(roots);
        }

        long IMainEntitiesInfrastructureRepository.UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            return UpdateNodes(nodes);
        }

        long IMainEntitiesInfrastructureRepository.UpdateLeaves(IEnumerable<TreeLeave> leaves)
        {
            return UpdateLeaves(leaves);
        }

        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
