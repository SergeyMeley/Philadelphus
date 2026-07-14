namespace Philadelphus.Presentation.Enums
{
    /// <summary>
    /// Идентификатор динамически выбираемой иконки (без привязки к файлу и UI-фреймворку).
    /// Сопоставление с конкретным файлом/ресурсом — на стороне платформенного провайдера.
    /// </summary>
    public enum AppIcon
    {
        Empty,
        StatusOk,
        StatusInfo,
        StatusWarning,
        StatusError,
        StatusAlarm,
        RepositoryLogo,
        Shrub,
        WorkingTree,
        TreeRoot,
        TreeNode,
        TreeLeaf,
        Attribute,
        Add,
        Open,
        Storage,
        Settings
    }
}
