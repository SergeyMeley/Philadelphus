using Philadelphus.Core.Domain.Identity.Entities;

namespace Philadelphus.Core.Domain.Identity.Services.Interfaces
{
    /// <summary>
    /// Сервис данных текущего пользователя.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        User CurrentUser { get; }
    }
}
