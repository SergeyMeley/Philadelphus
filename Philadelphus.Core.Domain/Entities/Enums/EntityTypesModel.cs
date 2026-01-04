namespace Philadelphus.Core.Domain.Entities.Enums
{
    public enum EntityTypesModel
    {
        //[EnumDescription("-")]
        None = -1,
        //[EnumDescription("Репозиторий")]
        Repository,
        //[EnumDescription("Корень")]
        Root,
        //[EnumDescription("Узел")]
        Node,
        //[EnumDescription("Лист")]
        Leave,
        //[EnumDescription("Атрибут")]
        Attribute,
        //[EnumDescription("Тип элемента")]
        RepositoryElementType,
    }
}
