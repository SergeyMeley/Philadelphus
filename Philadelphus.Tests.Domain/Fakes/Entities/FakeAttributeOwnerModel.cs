using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.Entities
{
    public class FakeAttributeOwnerModel : IAttributeOwnerModel, IPhiladelphusRepositoryMemberModel
    {
        public Guid Uuid { get; } = Guid.NewGuid();

        public IEnumerable<ElementAttributeModel>? Attributes => new List<ElementAttributeModel>();

        public bool HasAttributes => throw new NotImplementedException();

        public IReadOnlyList<ElementAttributeModel> PersonalAttributes => throw new NotImplementedException();

        public IReadOnlyList<ElementAttributeModel> ParentElementAttributes => throw new NotImplementedException();

        public ReadOnlyDictionary<Guid, IContentModel> Content => throw new NotImplementedException();

        public ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AuditInfoModel AuditInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsHidden { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public State State => throw new NotImplementedException();

        public IDataStorageModel DataStorage => new FakeDataStorageModel();

        public PhiladelphusRepositoryModel OwningRepository => new FakePhiladelphusRepositoryModel();

        public string Alias { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IOwnerModel Owner => new FakeWorkingTreeModel();

        public ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive => throw new NotImplementedException();

        IReadOnlyList<ElementAttributeModel> IAttributeOwnerModel.Attributes => throw new NotImplementedException();

        public void AddAttribute(ElementAttributeModel attr) { }

        public bool AddContent(IContentModel content)
        {
            throw new NotImplementedException();
        }

        public bool ChangeOwner(IOwnerModel newOwner)
        {
            throw new NotImplementedException();
        }

        public bool ClearAttributes()
        {
            throw new NotImplementedException();
        }

        public bool ClearContent()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttributeModel> GetVisibleAttributesRecursive(IWorkingTreeMemberModel? viewer)
        {
            throw new NotImplementedException();
        }

        public bool RemoveAttribute(ElementAttributeModel attribute)
        {
            throw new NotImplementedException();
        }

        public bool RemoveContent(IContentModel content)
        {
            throw new NotImplementedException();
        }

        bool IAttributeOwnerModel.AddAttribute(ElementAttributeModel attribute)
        {
            throw new NotImplementedException();
        }

        void IAttributeOwnerModel.MarkAsNeedRecalculateAttributesList()
        {
            throw new NotImplementedException();
        }
    }
}
