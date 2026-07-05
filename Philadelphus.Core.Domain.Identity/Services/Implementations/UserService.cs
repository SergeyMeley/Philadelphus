using Philadelphus.Core.Domain.Identity.Entities;
using Philadelphus.Core.Domain.Identity.Services.Interfaces;

namespace Philadelphus.Core.Domain.Identity.Services.Implementations
{
    /// <summary>
    /// Сервис данных текущего пользователя.
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public User CurrentUser { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserService" />.
        /// </summary>
        /// <param name="manualUserName">Имя пользователя, заданное вручную.</param>
        public UserService(string? manualUserName)
        {
            CurrentUser = new User(
                Guid.CreateVersion7(),
                manualUserName,
                GetAutomaticUserName());
        }

        private static string GetAutomaticUserName()
        {
            return string.IsNullOrWhiteSpace(Environment.UserDomainName)
                ? Environment.UserName
                : $"{Environment.UserDomainName}\\{Environment.UserName}";
        }
    }
}
