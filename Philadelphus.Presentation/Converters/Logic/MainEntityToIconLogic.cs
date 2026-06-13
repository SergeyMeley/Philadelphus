using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера MainEntityToIconConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class MainEntityToIconLogic
    {
        /// <summary>
        /// Возвращает имя файла иконки (относительно папки Icons) для типа сущности рабочего дерева.
        /// </summary>
        public static string ResolveIconFileName(object? value)
            => value switch
            {
                PhiladelphusRepositoryVM => "philadelphus_logo_64.png",
                TreeRootVM => "root_64_1.png",
                TreeNodeVM => "node_64_3.png",
                TreeLeaveVM => "leave_64_3.png",
                _ => "without_a_license/Flaticon_icon_empty.png"
            };
    }
}
