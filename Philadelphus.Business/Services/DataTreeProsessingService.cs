using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Philadelphus.Business.Services
{
    public class DataTreeRepositoryService
    {
        public List<TreeRepository> DataTreeRepositoryList { get; }
        List<TreeRepository> _repositories = new List<TreeRepository>();
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DbTreeRepository>));
        public List<TreeRepository> GetRepositoryList()
        {
            using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            {
                try
                {
                    var dbRepositories = xmlSerializer.Deserialize(fs) as List<DbTreeRepository>;
                    var service = new InfrastructureService();
                    foreach (var item in dbRepositories)
                    {
                        DataTreeRepositoryList.Add(service.DbToBusinessRepository(item));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return DataTreeRepositoryList;
            }
        }
        public void AddRepository(TreeRepository repository)
        {
            GetRepositoryList();
            if (!_repositories.Select(r => r.Path).Contains(repository.Path))
            {
                _repositories.Add(repository);
                using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
                {
                    try
                    {
                        xmlSerializer.Serialize(fs, _repositories);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private void SetPropertiesFromDb(IDbEntity dbEntity, ref IMainEntity businessEntity)
        {
            businessEntity.Id = dbEntity.Id;
            businessEntity.Path = dbEntity.Path;
            businessEntity.Sequence = dbEntity.Sequence;
            businessEntity.Name = dbEntity.Name;
            businessEntity.Alias = dbEntity.Alias;
            businessEntity.Code = dbEntity.Code;
            businessEntity.Description = dbEntity.Description;
            businessEntity.HasContent = dbEntity.HasContent;
            businessEntity.IsOriginal = dbEntity.IsOriginal;
            businessEntity.IsLegacy = dbEntity.IsLegacy;
            businessEntity.IsDeleted = dbEntity.IsDeleted;
            businessEntity.CreatedOn = dbEntity.CreatedOn;
            businessEntity.CreatedBy = dbEntity.CreatedBy;
            businessEntity.UpdatedOn = dbEntity.UpdatedOn;
            businessEntity.UpdatedBy = dbEntity.UpdatedBy;
            businessEntity.UpdatedContentOn = dbEntity.UpdatedContentOn;
            businessEntity.UpdatedContentBy = dbEntity.UpdatedContentBy;
            businessEntity.DeletedOn = dbEntity.DeletedOn;
            businessEntity.DeletedBy = dbEntity.DeletedBy;
        }
        //private void SetAttributeEntries(List<DbAttributeEntry> dbAttributeEntries, List<DbAttribute> dbAttributes, DbAttributeValue dbAttributeValues)
        //{
        //    IEnumerable<AttributeEntry> AttributeEntries = new List<AttributeEntry>();
        //    foreach (var dbAttributeEntry in dbAttributeEntries.Where(x => x.EntityTypeId == (long)this.EntityType && x.EntityId == this.Id))
        //    {
        //        AttributeEntry entry = new AttributeEntry();
        //        entry.Id = dbAttributeEntry.Id;
        //        entry.Sequence = dbAttributeEntry.Sequence;
        //        entry.Name = dbAttributeEntry.Name;
        //        entry.Alias = dbAttributeEntry.Alias;
        //        entry.Code = dbAttributeEntry.Code;
        //        entry.Description = dbAttributeEntry.Description;
        //        entry.HasContent = dbAttributeEntry.HasContent;
        //        entry.IsOriginal = dbAttributeEntry.IsOriginal;
        //        entry.IsLegacy = dbAttributeEntry.IsLegacy;
        //        entry.IsDeleted = dbAttributeEntry.IsDeleted;
        //        entry.CreatedOn = dbAttributeEntry.CreatedOn;
        //        entry.CreatedBy = dbAttributeEntry.CreatedBy;
        //        entry.UpdatedOn = dbAttributeEntry.UpdatedOn;
        //        entry.UpdatedBy = dbAttributeEntry.UpdatedBy;
        //        entry.UpdatedContentOn = dbAttributeEntry.UpdatedContentOn;
        //        entry.UpdatedContentBy = dbAttributeEntry.UpdatedContentBy;
        //        entry.DeletedOn = dbAttributeEntry.DeletedOn;
        //        entry.DeletedBy = dbAttributeEntry.DeletedBy;
        //        ((List<AttributeEntry>)AttributeEntries).Add(entry);
        //        Attribute att = new Attribute();
        //        DbAttribute dbAttribute = dbAttributes.Where(x => x.Id == dbAttributeEntry.AttributeId).First();
        //        att.Id = dbAttribute.Id;
        //        att.Sequence = dbAttribute.Sequence;
        //        att.Name = dbAttribute.Name;
        //        att.Alias = dbAttribute.Alias;
        //        att.Code = dbAttribute.Code;
        //        att.Description = dbAttribute.Description;
        //        att.HasContent = dbAttribute.HasContent;
        //        att.IsOriginal = dbAttribute.IsOriginal;
        //        att.IsLegacy = dbAttribute.IsLegacy;
        //        att.IsDeleted = dbAttribute.IsDeleted;
        //        att.CreatedOn = dbAttribute.CreatedOn;
        //        att.CreatedBy = dbAttribute.CreatedBy;
        //        att.UpdatedOn = dbAttribute.UpdatedOn;
        //        att.UpdatedBy = dbAttribute.UpdatedBy;
        //        att.UpdatedContentOn = dbAttribute.UpdatedContentOn;
        //        att.UpdatedContentBy = dbAttribute.UpdatedContentBy;
        //        att.DeletedOn = dbAttribute.DeletedOn;
        //        att.DeletedBy = dbAttribute.DeletedBy;
        //        entry.Attribute = att;
        //    }
        //}
        //private void SetChildTreeNodes(IEnumerable<DbTreeNode> dbChildTreeNodes)
        //{
        //    ChildTreeNodes = new List<TreeNode>();
        //    foreach (var node in dbChildTreeNodes.Where(x => x.ParentTreeRootId == this.Id))
        //    {
        //        TreeNode nod = new TreeNode();
        //        nod.Id = node.Id;
        //        nod.Sequence = node.Sequence;
        //        nod.Name = node.Name;
        //        nod.Alias = node.Alias;
        //        nod.Code = node.Code;
        //        nod.Description = node.Description;
        //        nod.HasContent = node.HasContent;
        //        nod.IsOriginal = node.IsOriginal;
        //        nod.IsLegacy = node.IsLegacy;
        //        nod.IsDeleted = node.IsDeleted;
        //        nod.CreatedOn = node.CreatedOn;
        //        nod.CreatedBy = node.CreatedBy;
        //        nod.UpdatedOn = node.UpdatedOn;
        //        nod.UpdatedBy = node.UpdatedBy;
        //        nod.UpdatedContentOn = node.UpdatedContentOn;
        //        nod.UpdatedContentBy = node.UpdatedContentBy;
        //        nod.DeletedOn = node.DeletedOn;
        //        nod.DeletedBy = node.DeletedBy;
        //        ((List<TreeNode>)ChildTreeNodes).Add(nod);
        //    }
        //}
    }
}
