using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.FormulaEngine.TreeLeaves
{
    /// <summary>
    /// Ищет листья рабочего дерева для формул по поддерживаемым пользовательским идентификаторам.
    /// </summary>
    public interface ITreeLeaveResolver
    {
        /// <summary>
        /// Ищет лист по уникальному идентификатору.
        /// </summary>
        /// <param name="uuid">UUID листа рабочего дерева.</param>
        /// <returns>Результат поиска листа.</returns>
        TreeLeaveResolveResult ResolveByUuid(Guid uuid);

        /// <summary>
        /// Ищет лист по отображаемому наименованию.
        /// </summary>
        /// <param name="name">Наименование листа рабочего дерева.</param>
        /// <returns>Результат поиска листа.</returns>
        TreeLeaveResolveResult ResolveByName(string name);

        /// <summary>
        /// Ищет лист по пользовательскому коду.
        /// </summary>
        /// <param name="userCode">Пользовательский код листа.</param>
        /// <returns>Результат поиска листа или зарезервированная ошибка.</returns>
        TreeLeaveResolveResult ResolveByUserCode(string userCode);

        /// <summary>
        /// Ищет лист по псевдониму.
        /// </summary>
        /// <param name="alias">Псевдоним листа.</param>
        /// <returns>Результат поиска листа или зарезервированная ошибка.</returns>
        TreeLeaveResolveResult ResolveByAlias(string alias);
    }
}
