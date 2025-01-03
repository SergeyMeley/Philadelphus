using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRoot : MainEntityBase
    {
        public override EntityTypes entityType { get => EntityTypes.Root; }
        public string DirectoryPath { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; }
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; }
        public TreeRoot(DbTreeRoot dbTreeRoot, List<DbAttributeEntry> dbAttributeEntries, List<DbAttribute> dbAttributes, DbAttributeValue dbAttributeValues, IEnumerable<DbTreeNode> dbTreeNodes)
        {
            SetProperties(dbTreeRoot);
            SetAttributeEntries(dbAttributeEntries, dbAttributes, dbAttributeValues);
            SetChildTreeNodes(dbTreeNodes);
        }
        private void SetProperties(DbTreeRoot dbTreeRoot)
        {
            Id = dbTreeRoot.Id;
            Sequence = dbTreeRoot.Sequence;
            Name = dbTreeRoot.Name;
            Alias = dbTreeRoot.Alias;
            Code = dbTreeRoot.Code;
            Description = dbTreeRoot.Description;
            HasContent = dbTreeRoot.HasContent;
            IsOriginal = dbTreeRoot.IsOriginal;
            IsLegacy = dbTreeRoot.IsLegacy;
            IsDeleted = dbTreeRoot.IsDeleted;
            CreatedOn = dbTreeRoot.CreatedOn;
            CreatedBy = dbTreeRoot.CreatedBy;
            UpdatedOn = dbTreeRoot.UpdatedOn;
            UpdatedBy = dbTreeRoot.UpdatedBy;
            UpdatedContentOn = dbTreeRoot.UpdatedContentOn;
            UpdatedContentBy = dbTreeRoot.UpdatedContentBy;
            DeletedOn = dbTreeRoot.DeletedOn;
            DeletedBy = dbTreeRoot.DeletedBy;
        }
        private void SetAttributeEntries(List<DbAttributeEntry> dbAttributeEntries, List<DbAttribute> dbAttributes, DbAttributeValue dbAttributeValues)
        {
            AttributeEntries = new List<AttributeEntry>();
            foreach (var dbAttributeEntry in dbAttributeEntries.Where(x => x.EntityTypeId == (long)this.entityType && x.EntityId == this.Id))
            {
                AttributeEntry entry = new AttributeEntry();
                entry.Id = dbAttributeEntry.Id;
                entry.Sequence = dbAttributeEntry.Sequence;
                entry.Name = dbAttributeEntry.Name;
                entry.Alias = dbAttributeEntry.Alias;
                entry.Code = dbAttributeEntry.Code;
                entry.Description = dbAttributeEntry.Description;
                entry.HasContent = dbAttributeEntry.HasContent;
                entry.IsOriginal = dbAttributeEntry.IsOriginal;
                entry.IsLegacy = dbAttributeEntry.IsLegacy;
                entry.IsDeleted = dbAttributeEntry.IsDeleted;
                entry.CreatedOn = dbAttributeEntry.CreatedOn;
                entry.CreatedBy = dbAttributeEntry.CreatedBy;
                entry.UpdatedOn = dbAttributeEntry.UpdatedOn;
                entry.UpdatedBy = dbAttributeEntry.UpdatedBy;
                entry.UpdatedContentOn = dbAttributeEntry.UpdatedContentOn;
                entry.UpdatedContentBy = dbAttributeEntry.UpdatedContentBy;
                entry.DeletedOn = dbAttributeEntry.DeletedOn;
                entry.DeletedBy = dbAttributeEntry.DeletedBy;
                ((List<AttributeEntry>)AttributeEntries).Add(entry);
                Attribute att = new Attribute();
                DbAttribute dbAttribute = dbAttributes.Where(x => x.Id == dbAttributeEntry.AttributeId).First();
                att.Id = dbAttribute.Id;
                att.Sequence = dbAttribute.Sequence;
                att.Name = dbAttribute.Name;
                att.Alias = dbAttribute.Alias;
                att.Code = dbAttribute.Code;
                att.Description = dbAttribute.Description;
                att.HasContent = dbAttribute.HasContent;
                att.IsOriginal = dbAttribute.IsOriginal;
                att.IsLegacy = dbAttribute.IsLegacy;
                att.IsDeleted = dbAttribute.IsDeleted;
                att.CreatedOn = dbAttribute.CreatedOn;
                att.CreatedBy = dbAttribute.CreatedBy;
                att.UpdatedOn = dbAttribute.UpdatedOn;
                att.UpdatedBy = dbAttribute.UpdatedBy;
                att.UpdatedContentOn = dbAttribute.UpdatedContentOn;
                att.UpdatedContentBy = dbAttribute.UpdatedContentBy;
                att.DeletedOn = dbAttribute.DeletedOn;
                att.DeletedBy = dbAttribute.DeletedBy;
                entry.Attribute = att;
            }
        }
        private void SetChildTreeNodes(IEnumerable<DbTreeNode> dbChildTreeNodes)
        {
            ChildTreeNodes = new List<TreeNode>();
            foreach (var node in dbChildTreeNodes.Where(x => x.ParentTreeRootId == this.Id))
            {
                TreeNode nod = new TreeNode();
                nod.Id = node.Id;
                nod.Sequence = node.Sequence;
                nod.Name = node.Name;
                nod.Alias = node.Alias;
                nod.Code = node.Code;
                nod.Description = node.Description;
                nod.HasContent = node.HasContent;
                nod.IsOriginal = node.IsOriginal;
                nod.IsLegacy = node.IsLegacy;
                nod.IsDeleted = node.IsDeleted;
                nod.CreatedOn = node.CreatedOn;
                nod.CreatedBy = node.CreatedBy;
                nod.UpdatedOn = node.UpdatedOn;
                nod.UpdatedBy = node.UpdatedBy;
                nod.UpdatedContentOn = node.UpdatedContentOn;
                nod.UpdatedContentBy = node.UpdatedContentBy;
                nod.DeletedOn = node.DeletedOn;
                nod.DeletedBy = node.DeletedBy;
                ((List<TreeNode>)ChildTreeNodes).Add(nod);
            }
        }
    }
}
