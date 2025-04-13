using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeNode : RepositoryElementBase, IHavingChilds
    {
        public override EntityTypes EntityType { get => EntityTypes.Node; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public EntityElementType ElementType { get; set; }
        public IEnumerable<IHavingParent> Childs { get; set; }

        public TreeNode(Guid guid, IHavingChilds parent) : base(guid, parent)
        {
            //Parent = parent;
            Guid = guid;
            Initialize();
        }
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in Repository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            Name = NamingHelper.GetNewName(existNames, "Новый узел");
            Childs = new List<IHavingParent>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
