using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs.ElementsContentVMs
{
    public class ElementAttributeVM : MainEntityBaseVM
    {
        #region [ Props ]

        private readonly TreeRepositoryService _service;

        private readonly ElementAttributeModel _model;

        public EntityTypesModel EntityType { get => _model.EntityType; }
        public IAttributeOwnerModel Owner { get => _model.Owner; }
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

        public ElementAttributeVM(ElementAttributeModel elementAttribute, TreeRepositoryService service) : base(elementAttribute, service)
        {
            _model = elementAttribute;
            _service = service;
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
