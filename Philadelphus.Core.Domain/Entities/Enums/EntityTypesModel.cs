namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Тип сущности
    /// </summary>
    public enum EntityTypesModel
    {
        /// <summary>
        /// Нет
        /// </summary>
        //[EnumDescription("-")]
        None = -1,
        /// <summary>
        /// Репозиторий
        /// </summary>
        //[EnumDescription("Репозиторий")]
        Repository,
        /// <summary>
        /// Корень
        /// </summary>
        //[EnumDescription("Корень")]
        Root,
        /// <summary>
        /// Узел
        /// </summary>
        //[EnumDescription("Узел")]
        Node,
        /// <summary>
        /// Лист
        /// </summary>
        //[EnumDescription("Лист")]
        Leave,
        /// <summary>
        /// Атрибут
        /// </summary>
        //[EnumDescription("Атрибут")]
        Attribute,
        /// <summary>
        /// Тип элемента (устар.)
        /// </summary>
        //[EnumDescription("Тип элемента")]
        RepositoryElementType,
    }
}
