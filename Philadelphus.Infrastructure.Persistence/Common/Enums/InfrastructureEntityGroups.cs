using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Infrastructure.Persistence.Common.Enums
{
    /// <summary>
    /// Перечисляет варианты хранимых сущностей.
    /// </summary>
    public enum InfrastructureEntityGroups
    {
        [Display(Name = "Репозитории")]
        PhiladelphusRepositories,
        [Display(Name = "Элементы кустарника")]
        ShrubMembers,
        [Display(Name = "Отчёты")]
        Reports,
    }
}
