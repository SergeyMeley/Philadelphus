//using Remotion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Enums
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
        //[EnumDescription("Вхождение атрибута")]
        AttributeEntry,
        //[EnumDescription("Значение атрибута")]
        AttributeValue,
        //[EnumDescription("Тип элемента")]
        RepositoryElementType,
    }
}
