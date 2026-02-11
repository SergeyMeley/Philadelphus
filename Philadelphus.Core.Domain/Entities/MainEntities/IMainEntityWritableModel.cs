using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Основная сущность редактируемая
    /// </summary>
    internal interface IMainEntityWritableModel : IMainEntityModel
    {
        #region [ Properties ]



        #endregion

        #region [ Methods ]

        /// <summary>
        /// Изменить статус
        /// </summary>
        /// <param name="newState">Новый статус</param>
        /// <returns></returns>
        internal bool SetState(State newState);

        #endregion
    }
}
