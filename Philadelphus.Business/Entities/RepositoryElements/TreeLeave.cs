using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeLeave : RepositoryElementBase, IHavingParent
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public TreeLeave(Guid guid, IHavingChilds parent) : base(guid, parent)
        {
            Parent = parent;
            ParentRepository = ((RepositoryElementBase)Parent).ParentRepository;
            ParentRoot = ((RepositoryElementBase)Parent).ParentRoot;
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
            Name = NamingHelper.GetNewName(existNames, "Новый лист");
            //Childs = new ObservableCollection<IHavingParent>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
