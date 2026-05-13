using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Задает контракт для политик атрибутов.
    /// </summary>
    public interface IAttributePropertiesPolicy : IPropertiesPolicy<ElementAttributeModel>
    {
    }
}
