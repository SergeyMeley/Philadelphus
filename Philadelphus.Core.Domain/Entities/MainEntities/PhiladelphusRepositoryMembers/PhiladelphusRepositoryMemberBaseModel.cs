using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers
{
    public abstract class PhiladelphusRepositoryMemberBaseModel : MainEntityBaseModel, IPhiladelphusRepositoryMemberModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый участник репозитория";

        private string _alias;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Псевдоним
        /// Используется в рамках локальной сессии пользователя для ускорения работы со ссылками
        /// Уникален в рамках репозитория
        /// </summary>
        public string Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                if (_alias != value)
                {
                    _alias = value;
                    UpdateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Владеющий репозиторий
        /// </summary>
        public PhiladelphusRepositoryModel OwningRepository { get; }

        /// <summary>
        /// Владелец
        /// </summary>
        public virtual IOwnerModel Owner 
        { 
            get => OwningRepository; 
        }

        /// <summary>
        /// Все владельцы (рекурсивно)
        /// </summary>
        public virtual ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        internal PhiladelphusRepositoryMemberBaseModel(
            Guid uuid,
            IMainEntity dbEntity,
            PhiladelphusRepositoryModel owner)
            : base(uuid, dbEntity)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            OwningRepository = owner;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя
        /// </summary>
        public bool ChangeOwner(IOwnerModel newOwner)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
