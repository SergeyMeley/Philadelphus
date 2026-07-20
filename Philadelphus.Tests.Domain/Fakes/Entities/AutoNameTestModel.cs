using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Tests.Common.Fakes.Entities;

namespace Philadelphus.Tests.Domain.Fakes.Entities;

/// <summary>
/// Минимальная доменная модель для изолированной проверки автоматического подбора имени.
/// </summary>
public sealed class AutoNameTestModel : MainEntityBaseModel<AutoNameTestModel>
{
    private readonly IDataStorageModel _dataStorage = new FakeDataStorageModel();
    private readonly string _defaultNamePrefix = $"Тестовое автоматическое имя {Guid.CreateVersion7()}";

    /// <inheritdoc />
    protected override string _defaultFixedPartOfName => _defaultNamePrefix;

    /// <inheritdoc />
    public override IDataStorageModel DataStorage => _dataStorage;

    /// <summary>
    /// Инициализирует тестовую модель с заданной политикой свойств.
    /// </summary>
    internal AutoNameTestModel(
        INotificationService notificationService,
        IPropertiesPolicy<AutoNameTestModel> propertiesPolicy)
        : base(Guid.CreateVersion7(), notificationService, propertiesPolicy)
    {
    }
}
