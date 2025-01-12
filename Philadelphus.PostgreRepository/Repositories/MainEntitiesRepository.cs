using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreRepository.Repositories
{
    public class MainEntitiesRepository //: IMainEntitiesRepository
    {
        private readonly Context _context;
        public MainEntitiesRepository()
        {
            _context = new Context();
        }
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            DbMainEntitiesCollection collection = new DbMainEntitiesCollection();
            collection.DbTreeRoots = (List<DbTreeRoot>)SelectRoots();
            collection.DbTreeNodes = (List<DbTreeNode>)SelectNodes();
            return collection;
        }
        #region [Select]
        IEnumerable<DbTreeRepository> SelectProjects()
        {
            var dataCollection = new List<DbTreeRepository>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbTreeRepository(reader.GetInt32(0), reader.GetString(1));
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
        public IEnumerable<DbTreeRoot> SelectRoots()
        {
            var dataCollection = new List<DbTreeRoot>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbTreeRoot(reader.GetInt32(0), reader.GetString(1));
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
        public IEnumerable<DbTreeNode> SelectNodes()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<DbTreeLeave> SelectLeaves()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Insert]
        public int InsertRepositories(IEnumerable<DbTreeRepository> projects)
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
        public int InsertRoots(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int InsertLeaves(IEnumerable<DbTreeLeave> elements)
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
        public int UpdateRepositories(IEnumerable<DbTreeRepository> projects)
        {
            throw new NotImplementedException();
        }
        public int UpdateRoots(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int UpdateLeaves(IEnumerable<DbTreeLeave> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
