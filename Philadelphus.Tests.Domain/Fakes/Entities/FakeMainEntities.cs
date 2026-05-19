// 1. Фейковая реализация PhiladelphusRepository (из Infrastructure)
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Fakes.Entities
{
    // Тесты для PhiladelphusRepositoryModel
    public class PhiladelphusRepositoryModelTests
    {
        [Fact]
        public void Constructor_ValidArgs_SetsOwnDataStorageAndContentShrub()
        {
            // Arrange
            var uuid = Guid.CreateVersion7();
            var dataStorage = new FakeDataStorageModel();

            // Act
            var sut = new PhiladelphusRepositoryModelTestingFixture(uuid, dataStorage);

            // Assert
            sut.Uuid.Should().Be(uuid);
            sut.OwnDataStorage.Should().BeSameAs(dataStorage);
            sut.DataStorages.Should().Contain(dataStorage);
            sut.DataStorage.Should().Be(dataStorage);
            sut.ContentShrub.Should().NotBeNull();
            sut.Content.Should().HaveCount(1);
            sut.AliasesList.Should().NotBeNull();
            sut.State.Should().Be(State.Initialized);
        }

        [Fact]
        public void Constructor_NullDataStorage_ThrowsArgumentNullException()
        {
            // Arrange
            var act = () => new PhiladelphusRepositoryModelTestingFixture(Guid.CreateVersion7(), null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dataStorage");
        }

        // ... остальные тесты аналогично
    }

    internal class PhiladelphusRepositoryModelTestingFixture : PhiladelphusRepositoryModel
    {
        public PhiladelphusRepositoryModelTestingFixture(Guid uuid, IDataStorageModel dataStorage)
            : base(uuid, dataStorage, new FakeNotificationService(), new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(), new EmptyPropertiesPolicy<ShrubModel>()) { }

        // Переопределяем абстрактный DataStorage
        public override IDataStorageModel DataStorage => OwnDataStorage;
    }

    public class FakeAlwaysDenyRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        public bool CanWrite(ElementAttributeModel model, string prop, object value) => false;
        public bool CanRead(ElementAttributeModel model, string prop) => true;
        public object OnRead(ElementAttributeModel model, string prop, object value) => value;
        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue) { }
    }

    public class TestAttribute : MainEntityBaseModel<TestAttribute>
    {
        public TestAttribute(IPropertiesPolicy<TestAttribute> policy)
            : base(Guid.NewGuid(), new FakeNotificationService(), policy)
        {
        }

        private string _name;

        public string Name
        {
            get => GetValue(_name);
            set => SetValue(ref _name, value);
        }

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public bool TrySetName(string value)
            => SetValue(ref _name, value);
    }
}
