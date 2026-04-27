using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;
using Philadelphus.Tests.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.Attributes
{
    public class ElementAttributeModelTests
    {
        [Fact]
        public void Should_Not_Set_Value_When_Policy_Blocks()
        {
            var policy = new BlockWritePolicy<ElementAttributeModel>();

            var model = EntitiesCreationHelper.CreateAttribute(policy);

            var oldValue = model.Name;

            model.Name = "New Name";

            Assert.Equal(oldValue, model.Name);
        }
    }
}
