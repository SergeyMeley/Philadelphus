using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers
{
    /// <summary>
    /// Участник репозитория
    /// </summary>
    public interface IPhiladelphusRepositoryMemberModel : IMainEntityModel, IContentModel
    {
        #region [ Properties ]

        /// <summary>ы
        /// Владеющий репозиторий
        /// </summary>
        public PhiladelphusRepositoryModel OwningRepository { get; }

        /// <summary>
        /// Псевдоним
        /// Используется в рамках локальной сессии пользователя для ускорения работы со ссылками
        /// Уникален в рамках репозитория
        /// </summary>
        public string Alias { get; set; }

        #endregion

        #region [ Methods ]



        #endregion
    }
}
