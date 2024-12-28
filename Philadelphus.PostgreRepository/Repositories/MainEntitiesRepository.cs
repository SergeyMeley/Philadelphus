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
            collection.Layers = (List<DbLayer>)SelectLayers();
            collection.Nodes = (List<DbNode>)SelectProjects();
            return collection;
        }
        public IEnumerable<DbLayer> SelectLayers()
        {
            var dataCollection = new List<DbLayer>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbLayer(reader.GetInt32(0), reader.GetString(1));
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
        public IEnumerable<IDbEntity> SelectProjects()
        {
            var dataCollection = new List<DbProject>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbProject(reader.GetInt32(0), reader.GetString(1));
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
        #region Insert
        public IEnumerable<IDbEntity> InsertCatalogs()
        {
            throw new NotImplementedException();
        }

        public bool InsertProject(DbProject project)
        {
            try
            {
                using (var cmd = _context.CreateConnection().CreateCommand($""))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion
        #region Delete
        public IEnumerable<IDbEntity> DeleteCatalogs(int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDbEntity> DeleteProjects(int[] ids)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Update
        public IEnumerable<IDbEntity> UpdateCatalogs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDbEntity> UpdateProjects()
        {
            throw new NotImplementedException();
        }

        IEnumerable<DbLayer> IMainEntitiesRepository.SelectProjects()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> SelectNodes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> SelectElements()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> InsertProjects(IEnumerable<DbProject> projects)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> InsertLayers(IEnumerable<DbLayer> layers)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> InsertNodes(IEnumerable<DbNode> nodes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> InsertElements(IEnumerable<DbElement> elements)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> DeletetProjects(int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> DeleteLayers(int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> DeleteNodes(int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> DeleteElements(int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> UpdateProjects(IEnumerable<DbProject> projects)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbLayer> UpdateLayers(IEnumerable<DbLayer> layers)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> UpdateNodes(IEnumerable<DbNode> nodes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbNode> UpdateElements(IEnumerable<DbElement> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
