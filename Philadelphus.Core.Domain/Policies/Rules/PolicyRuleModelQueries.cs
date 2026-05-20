using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Общие запросы к моделям для правил свойств.
    /// </summary>
    internal static class PolicyRuleModelQueries
    {
        /// <summary>
        /// Возвращает все атрибуты владельца: собственные и унаследованные.
        /// </summary>
        /// <remarks>
        /// Правила уникальности Name, Sequence и CustomCode для атрибутов работают с тем же списком,
        /// который пользователь видит у текущего владельца. Поэтому унаследованный атрибут должен
        /// блокировать создание собственного атрибута с таким же значением проверяемого свойства.
        /// </remarks>
        /// <param name="owner">Владелец атрибутов.</param>
        /// <returns>Полная коллекция атрибутов указанного владельца.</returns>
        public static IEnumerable<ElementAttributeModel> GetAttributes(IOwnerModel owner)
        {
            return (owner as IAttributeOwnerModel)?.Attributes ?? Enumerable.Empty<ElementAttributeModel>();
        }
    }
}
