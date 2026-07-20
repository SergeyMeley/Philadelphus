using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;
using Philadelphus.Tests.Domain.Fakes.Entities;

namespace Philadelphus.Tests.Domain.Entities.MainEntities;

/// <summary>
/// Проверяет повторный подбор автоматически сгенерированного имени.
/// </summary>
public sealed class AutoNameAssignmentTests
{
    /// <summary>
    /// После отклонения кандидата политикой модель должна пробовать следующее имя.
    /// </summary>
    [Fact]
    public void TryAssign_RejectedCandidates_RetriesUntilAccepted()
    {
        var attempts = 0;
        var notificationService = new FakeNotificationService();
        var policy = CreatePolicy((_, property, _) =>
            property != nameof(AutoNameTestModel.Name) || ++attempts == 3);
        var model = new AutoNameTestModel(notificationService, policy);

        var result = NewEntityAutoNameAssignmentHelper.TryAssign(
            model,
            notificationService);

        attempts.Should().Be(3);
        result.Should().BeTrue();
        model.Name.Should().NotBeNullOrWhiteSpace();
        notificationService.Messages.Should().HaveCount(2);
    }

    /// <summary>
    /// После тысячи отклонённых кандидатов подбор должен завершиться одним предупреждением.
    /// </summary>
    [Fact]
    public void TryAssign_AttemptLimitReached_NotifiesAndStops()
    {
        var attempts = 0;
        var notificationService = new FakeNotificationService();
        var policy = CreatePolicy((_, property, _) =>
        {
            if (property == nameof(AutoNameTestModel.Name))
                attempts++;
            return false;
        });
        var model = new AutoNameTestModel(notificationService, policy);

        var result = NewEntityAutoNameAssignmentHelper.TryAssign(
            model,
            notificationService);

        attempts.Should().Be(1000);
        result.Should().BeFalse();
        model.Name.Should().BeNullOrEmpty();
        notificationService.Messages.Should().HaveCount(1001)
            .And.Contain(message => message.Contains("1000 попыток"));
    }

    /// <summary>
    /// Параллельный подбор должен возвращать уникальные имена без ошибок общей коллекции.
    /// </summary>
    [Fact]
    public void GetNewName_ParallelCalls_ReturnUniqueNames()
    {
        const int namesCount = 100;
        var fixedPart = $"Параллельное имя {Guid.CreateVersion7()}";

        var names = Enumerable.Range(0, namesCount)
            .AsParallel()
            .Select(_ => NamingHelper.GetNewName(fixedPart))
            .ToArray();

        names.Should().HaveCount(namesCount)
            .And.OnlyHaveUniqueItems();
    }

    /// <summary>
    /// Создаёт политику с управляемым результатом проверки записи.
    /// </summary>
    private static IPropertiesPolicy<AutoNameTestModel> CreatePolicy(
        Func<AutoNameTestModel, string, object, bool> canWrite)
    {
        var policy = new Mock<IPropertiesPolicy<AutoNameTestModel>>();
        policy.Setup(x => x.CanRead(It.IsAny<AutoNameTestModel>(), It.IsAny<string>()))
            .Returns(true);
        policy.Setup(x => x.OnRead(
                It.IsAny<AutoNameTestModel>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns((AutoNameTestModel _, string _, object value) => value);
        policy.Setup(x => x.PrepareWriteValue(
                It.IsAny<AutoNameTestModel>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns((AutoNameTestModel _, string _, object value) => value);
        policy.Setup(x => x.CanWrite(
                It.IsAny<AutoNameTestModel>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns(canWrite);
        return policy.Object;
    }
}
