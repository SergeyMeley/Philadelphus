using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Policies.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Attributes.Builders
{
    /// <summary>
    /// Строитель политик атрибутов.
    /// </summary>
    public static class AttributePolicyBuilder
    {
        /// <summary>
        /// Создает политики атрибутов.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <returns>Созданный объект.</returns>
        public static IAttributePropertiesPolicy CreateDefault(INotificationService notificationService)
        {
            return new CompositeAttributePropertiesPolicy(notificationService, new IAttributePropertiesRule<ElementAttributeModel>[]
            {
                new NonOwnAttributePropertiesRule(notificationService),
                new ParentOverrideForbiddenPropertiesRule(notificationService),
                new OverrideVisibilityPropertiesRule(notificationService),
                new RequiredOverrideValuePropertiesRule(notificationService),
                new SystemBaseAttributeValuePropertiesRule(notificationService),
                new RequiredNamePropertiesRule<ElementAttributeModel>(notificationService),
                new ValidNamePropertiesRule<ElementAttributeModel>(notificationService, NameUniquenessStrategy.ElementAttribute()),
                new SequencePropertiesRule<ElementAttributeModel>(notificationService, SequenceUniquenessStrategy.ElementAttribute()),
                new CustomCodePropertiesRule<ElementAttributeModel>(notificationService, CustomCodeUniquenessStrategy.ElementAttribute()),
            });
        }
    }
}
