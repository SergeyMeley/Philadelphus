using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
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
    /// <summary>
    /// Доменная модель репозитория Чубушника.
    /// </summary>
    public abstract class PhiladelphusRepositoryMemberBaseModel<T> : MainEntityBaseModel<T>, IPhiladelphusRepositoryMemberModel, IContentModel
        where T : PhiladelphusRepositoryMemberBaseModel<T>
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
                    UpdateStateStateAfterChange();
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
        public IOwnerModel Owner { get; }

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
            IOwnerModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<T> propertiesPolicy)
            : base(uuid, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(owner);

            Owner = owner;

            if (owner is PhiladelphusRepositoryModel r)
            {
                OwningRepository = r;
            }
            else if (owner is IPhiladelphusRepositoryMemberModel rm)
            {
                OwningRepository = rm.OwningRepository;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя
        /// </summary>
        /// <param name="newOwner">Новый владелец.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public bool ChangeOwner(IOwnerModel newOwner)
        {
            ArgumentNullException.ThrowIfNull(newOwner);

            throw new NotImplementedException();
        }

        #endregion
    }
}
