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
        public InfrastructureTypes InfrastructureRepositoryTypes { get; } = InfrastructureTypes.WindowsDirectory;
        public MainEntitiesCollection GetMainEntitiesCollection()
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
        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes)
        {
            var list = new List<TreeRepository>();
            foreach (var item in pathes)
            {
                if (File.Exists(item))
                {
                    var repositoryXmlSerializer = new XmlSerializer(typeof(TreeRepository));
                    using (var repofs = new FileStream(item, FileMode.OpenOrCreate))
                    {
                        try
                        {
                            var repo = repositoryXmlSerializer.Deserialize(repofs) as TreeRepository;
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
        public IEnumerable<TreeRoot> SelectRoots()
        {
            return null;
        }
        public IEnumerable<TreeNode> SelectNodes()
        {
            return null;
        }
        public IEnumerable<TreeLeave> SelectLeaves()
        {
            return null;
        }
        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            return null;
        }
        public IEnumerable<AttributeEntry> SelectAttributeEntries()
        {
            return null;
        }
        public IEnumerable<AttributeValue> SelectAttributeValues()
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
        public long InsertRepositories(IEnumerable<TreeRepository> repositories)
        {
            foreach (var item in repositories)
            {
                // Проверяем указанный путь.Если его нет, пробуем создать.
                //if (!Directory.Exists(item.DirectoryFullPath))
                //{
                //    try
                //    {
                //        Directory.CreateDirectory(item.DirectoryFullPath);
                //    }
                //    catch (Exception)
                //    {
                //        throw new Exception("Ошибка создания директории, проверьте указанный путь");
                //    }
                //}
                //// Проверяем наличие файла. Если его нет, пробуем создать.
                //if (!File.Exists(item.ConfigPath))
                //{
                //    try
                //    {
                //        var repositoryXmlSerializer = new XmlSerializer(typeof(TreeRepository));
                //        using (var repofs = new FileStream(item.ConfigPath, FileMode.OpenOrCreate))
                //        {
                //            repositoryXmlSerializer.Serialize(repofs, item);
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        throw new Exception("Ошибка создания элемента, проверьте указанный путь");
                //    }
                //}
            }
            return repositories.Count();
        }
        public long InsertRoots(IEnumerable<TreeRoot> roots)
        {
            return roots.Count();
        }
        public long InsertNodes(IEnumerable<TreeNode> nodes)
        {
            return nodes.Count();
        }
        public long InsertLeaves(IEnumerable<TreeLeave> leaves)
        {
            return leaves.Count();
        }
        public long InsertAttributes(IEnumerable<ElementAttribute> attributes)
        {
            return attributes.Count();
        }
        public long InsertAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            return attributeEntries.Count();
        }
        public long InsertAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            return attributeValues.Count();
        }
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
        {
            return 0;
        }
        public long DeleteRoots(IEnumerable<TreeRoot> roots)
        {
            return 0;
        }
        public long DeleteNodes(IEnumerable<TreeNode> nodes)
        {
            return 0;
        }
        public long DeleteLeaves(IEnumerable<TreeLeave> leaves)
        {
            return 0;
        }
        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes)
        {
            return 0;
        }
        public long DeleteAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long DeleteAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<TreeRepository> repositories)
        {
            //foreach (var item in repositories)
            //{
            //    // Проверяем указанный путь.Если его нет, пробуем создать.
            //    if (!Directory.Exists(item.DirectoryFullPath))
            //    {
            //        try
            //        {
            //            Directory.CreateDirectory(item.DirectoryFullPath);
            //        }
            //        catch (Exception)
            //        {
            //            throw new Exception("Ошибка создания директории, проверьте указанный путь");
            //        }
            //        if (!File.Exists(item.ConfigPath))
            //        {
            //            try
            //            {
            //                File.Create(item.ConfigPath);
            //            }
            //            catch (Exception)
            //            {
            //                throw new Exception("Ошибка создания элемента, проверьте указанный путь");
            //            }
            //        }
            //    }
            //}
            return 0;
        }
        public long UpdateRoots(IEnumerable<TreeRoot> roots)
        {
            return 0;
        }
        public long UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            return 0;
        }
        public long UpdateLeaves(IEnumerable<TreeLeave> leaves)
        {
            return 0;
        }
        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes)
        {
            return 0;
        }
        public long UpdateAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long UpdateAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
    }
}
