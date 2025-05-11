using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Philadelphus.Business.Helpers;
using System.Collections.ObjectModel;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRoot : RepositoryElementBase, IHavingOwnStorage, IHavingParent, IHavingChilds
    {
        public override EntityTypes EntityType { get => EntityTypes.Root; }
        public InfrastructureTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public EntityElementType ElementType { get; set; }
        public IHavingChilds Parent {  get; private set; }
        public IEnumerable<IHavingParent> Childs { get; set; }
        public TreeRoot(Guid guid, IHavingChilds parent) : base(guid, parent)
        {
            Parent = parent;
            ParentRepository = (TreeRepository)Parent;
            ParentRoot = this;
            Guid = guid;
            Initialize();
        }
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in ParentRepository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, "Новый корень");
            Childs = new ObservableCollection<IHavingParent>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
