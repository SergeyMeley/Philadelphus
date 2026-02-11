using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs
{
    public class ElementAttributeVM : MainEntityBaseVM
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        private readonly ElementAttributeModel _model;

        public IOwnerModel Owner { get => _model.Owner; }
        public IDataStorageModel DataStorage { get => _model.DataStorage; }
        public TreeNodeModel ValueType 
        { 
            get
            {
                return _model.ValueType;
            }
            set
            {
                _model.ValueType = value;
                OnPropertyChanged(nameof(ValueType));
            }
        }
        public IEnumerable<TreeNodeModel>? ValueTypesList { get => _model.ValueTypesList; }
        public TreeLeaveModel Value
        {
            get
            {
                return _model.Value;
            }
            set
            {
                _model.Value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        public IEnumerable<TreeLeaveModel>? ValuesList { get => _model.ValuesList; }

        #endregion

        #region [ Construct ]

        public ElementAttributeVM(
            ElementAttributeModel elementAttribute,
            IPhiladelphusRepositoryService service) 
            : base(elementAttribute, service)
        {
            _service = service;

            _model = elementAttribute;
        }

        #endregion

        #region [Commands]



        #endregion

        #region [ Methods ]

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
        }

        #endregion
    }
}
