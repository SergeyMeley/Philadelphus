using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Philadelphus.WindowsFileSystemRepository.Repositories
{
    public class WindowsMainEntityRepository : IMainEntitiesInfrastructure
    {
        public InfrastructureTypes InftastructureRepositoryTypes { get; } = InfrastructureTypes.WindowsDirectory;
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            return null;
        }
        # region [ Select ]
        public IEnumerable<string> SelectRepositoryPathes(string configPath)
        {
            var list = new List<string>();
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var fs = new FileStream(configPath, FileMode.OpenOrCreate))
            {
                try
                {
                    list = serializer.Deserialize(fs) as List<string>;
                }
                catch (Exception ex)
                {
                }
            }
            return list;
        }
        public IEnumerable<DbTreeRepository> SelectRepositories(List<string> pathes)
        {
            var list = new List<DbTreeRepository>();
            foreach (var item in pathes)
            {
                if (File.Exists(item))
                {
                    var repositoryXmlSerializer = new XmlSerializer(typeof(DbTreeRepository));
                    using (var repofs = new FileStream(item, FileMode.OpenOrCreate))
                    {
                        try
                        {
                            var repo = repositoryXmlSerializer.Deserialize(repofs) as DbTreeRepository;
                            list.Add(repo);
                        }
                        catch (Exception)
                        {
                            File.Delete(item);
                        }
                    }
                }
            }
            return list;
        }
        public IEnumerable<DbTreeRoot> SelectRoots()
        {
            return null;
        }
        public IEnumerable<DbTreeNode> SelectNodes()
        {
            return null;
        }
        public IEnumerable<DbTreeLeave> SelectLeaves()
        {
            return null;
        }
        public IEnumerable<DbEntityAttribute> SelectAttributes()
        {
            return null;
        }
        public IEnumerable<DbAttributeEntry> SelectAttributeEntries()
        {
            return null;
        }
        public IEnumerable<DbAttributeValue> SelectAttributeValues()
        {
            return null;
        }
        #endregion
        #region [ Insert ]
        public long InsertRepositoryPathes(string configPath, List<string> inputPathes)
        {
            var listXmlSerializer = new XmlSerializer(typeof(List<string>));
            using (var fs = new FileStream(configPath, FileMode.OpenOrCreate))
            {
                listXmlSerializer.Serialize(fs, inputPathes);
            }
            return 0;
        }
        public long InsertRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            foreach (var item in repositories)
            {
                // Проверяем указанный путь.Если его нет, пробуем создать.
                if (!Directory.Exists(item.DirectoryFullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(item.DirectoryFullPath);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Ошибка создания директории, проверьте указанный путь");
                    }
                }
                // Проверяем наличие файла. Если его нет, пробуем создать.
                if (!File.Exists(item.ConfigPath))
                {
                    try
                    {
                        var repositoryXmlSerializer = new XmlSerializer(typeof(DbTreeRepository));
                        using (var repofs = new FileStream(item.ConfigPath, FileMode.OpenOrCreate))
                        {
                            repositoryXmlSerializer.Serialize(repofs, item);
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Ошибка создания элемента, проверьте указанный путь");
                    }
                }
            }
            return repositories.Count();
        }
        public long InsertRoots(IEnumerable<DbTreeRoot> roots)
        {
            return roots.Count();
        }
        public long InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            return nodes.Count();
        }
        public long InsertLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return leaves.Count();
        }
        public long InsertAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            return attributes.Count();
        }
        public long InsertAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return attributeEntries.Count();
        }
        public long InsertAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return attributeValues.Count();
        }
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            return 0;
        }
        public long DeleteRoots(IEnumerable<DbTreeRoot> roots)
        {
            return 0;
        }
        public long DeleteNodes(IEnumerable<DbTreeNode> nodes)
        {
            return 0;
        }
        public long DeleteLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return 0;
        }
        public long DeleteAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            return 0;
        }
        public long DeleteAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long DeleteAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            foreach (var item in repositories)
            {
                // Проверяем указанный путь.Если его нет, пробуем создать.
                if (!Directory.Exists(item.DirectoryFullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(item.DirectoryFullPath);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Ошибка создания директории, проверьте указанный путь");
                    }
                    if (!File.Exists(item.ConfigPath))
                    {
                        try
                        {
                            File.Create(item.ConfigPath);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Ошибка создания элемента, проверьте указанный путь");
                        }
                    }
                }
            }
            return 0;
        }
        public long UpdateRoots(IEnumerable<DbTreeRoot> roots)
        {
            return 0;
        }
        public long UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            return 0;
        }
        public long UpdateLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return 0;
        }
        public long UpdateAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            return 0;
        }
        public long UpdateAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long UpdateAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
    }
}
