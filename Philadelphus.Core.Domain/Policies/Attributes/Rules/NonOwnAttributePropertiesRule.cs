using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее изменение значений некоторых свойств для унаследованных атрибутов
    /// </summary>
    public class NonOwnAttributePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private static readonly HashSet<string> _mustBeInherited =
        [
            nameof(ElementAttributeModel.Name),
            nameof(ElementAttributeModel.Description),
            nameof(ElementAttributeModel.ValueType),
            nameof(ElementAttributeModel.IsCollectionValue),
            nameof(ElementAttributeModel.Visibility)
        ];

        private static readonly HashSet<string> _canBeInherited =
        [
            nameof(ElementAttributeModel.Name),
            nameof(ElementAttributeModel.Description),
            nameof(ElementAttributeModel.ValueType),
            nameof(ElementAttributeModel.IsCollectionValue),
            nameof(ElementAttributeModel.Value),
            nameof(ElementAttributeModel.Values),
            nameof(ElementAttributeModel.Visibility)
        ];

        public bool CanRead(ElementAttributeModel attr, string prop)
        {
            return true;
        }

        public bool CanWrite(ElementAttributeModel attr, string prop, object value)
        {
            if (prop == nameof(attr.IsOwn))
                return true; 
            
            return attr.IsOwn || (_mustBeInherited.Contains(prop) == false);
        }

        public object OnRead(ElementAttributeModel attr, string prop, object value)
        {
            if (prop == nameof(attr.IsOwn))
                return value;

            // Если атрибут НЕ собственный и значение может или обязано быть унаследовано
            if (attr.IsOwn == false && (_canBeInherited.Contains(prop) || _mustBeInherited.Contains(prop)))
            {

                // Если значение свойства обязано быть унаследовано ИЛИ еще не заполнено у текущего атрибута, берем с родителя
                if (_mustBeInherited.Contains(prop) || value == default)
                {
                    var parentAttribute = attr.GetInheritedAttributeFromParent();

                    var q1 = parentAttribute?.GetType().GetProperty(prop);
                    var q2 = q1?.GetValue(parentAttribute);
                    var q3 = parentAttribute.Name;

                    return parentAttribute?.GetType().GetProperty(prop)?.GetValue(parentAttribute);
                }
            }

            return value;
        }

        public void OnWrite(ElementAttributeModel attr, string prop, object oldValue, object newValue) 
        { 
        }
    }
}