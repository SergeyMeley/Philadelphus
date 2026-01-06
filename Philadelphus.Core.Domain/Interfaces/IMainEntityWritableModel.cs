using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Основная сущность редактируемая
    /// </summary>
    internal interface IMainEntityWritableModel : IMainEntityModel
    {
        /// <summary>
        /// Изменить статус
        /// </summary>
        /// <param name="newState">Новый статус</param>
        /// <returns></returns>
        internal bool SetState(State newState);
    }
}
