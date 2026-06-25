using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления окна детальной информации о доменной модели.
    /// </summary>
    public class DetailsWindowVM : ViewModelBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DetailsWindowVM" />.
        /// </summary>
        /// <param name="content">Отображаемый элемент доменной модели.</param>
        public DetailsWindowVM(IMainEntityVM<IMainEntityModel> content)
        {
            ArgumentNullException.ThrowIfNull(content);

            Content = content;
        }

        /// <summary>
        /// Отображаемый элемент доменной модели.
        /// </summary>
        public IMainEntityVM<IMainEntityModel> Content { get; }
    }
}
