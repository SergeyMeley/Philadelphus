using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreRepository.Repositories
{
    public class MainEntitiesRepository : IMainEntitiesRepository
    {
        private readonly Context _context;
        public MainEntitiesRepository()
        {
            _context = new Context();
        }
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            DbMainEntitiesCollection collection = new DbMainEntitiesCollection();
            collection.TreeRoots = (List<DbTreeRoot>)SelectLayers();
            collection.TreeNodes = (List<DbTreeNode>)SelectNodes();
            return collection;
        }
        #region [Select]
        IEnumerable<DbTreeRepository> IMainEntitiesRepository.SelectProjects()
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
        public IEnumerable<DbTreeRoot> SelectLayers()
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
        public IEnumerable<DbTreeLeave> SelectElements()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Insert]
        public int InsertProjects(IEnumerable<DbTreeRepository> projects)
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
        public int InsertLayers(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int InsertElements(IEnumerable<DbTreeLeave> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Delete]
        public int DeleteProjects(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteLayers(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteNodes(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteElements(int[] ids)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Update]
        public int UpdateProjects(IEnumerable<DbTreeRepository> projects)
        {
            throw new NotImplementedException();
        }
        public int UpdateLayers(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int UpdateElements(IEnumerable<DbTreeLeave> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
