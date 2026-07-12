using FluentAssertions;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

public class PostgreSqlConnectionStringVMTests
{
    [Fact]
    public void Constructor_ParsesFixedAndAdditionalParameters()
    {
        var sut = new PostgreSqlConnectionStringVM(
            "Host=db.local;Port=5433;Database=philadelphus;Username=user;Password=secret;Pooling=false");

        sut.Host.Should().Be("db.local");
        sut.Port.Should().Be(5433);
        sut.Database.Should().Be("philadelphus");
        sut.Username.Should().Be("user");
        sut.Password.Should().Be("secret");
        sut.AdditionalParameters.Should().ContainSingle(x => x.Key == "Pooling" && x.Value == "False");
        sut.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FixedFieldChange_RebuildsConnectionString()
    {
        var sut = new PostgreSqlConnectionStringVM(
            "Host=db.local;Database=philadelphus;Username=user");

        sut.Database = "reports";

        sut.ConnectionString.Should().Contain("Database=reports");
    }

    [Fact]
    public void InvalidOrDuplicateAdditionalParameter_IsInvalid()
    {
        var sut = new PostgreSqlConnectionStringVM(
            "Host=db.local;Database=philadelphus;Username=user");
        sut.AdditionalParameters.Add(new("Pooling", "true"));
        sut.AdditionalParameters.Add(new("pooling", "false"));

        sut.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddAndRemoveCommands_UpdateParameters()
    {
        var sut = new PostgreSqlConnectionStringVM();
        sut.AddAdditionalParameterCommand.Execute(null);
        var parameter = sut.AdditionalParameters.Single();

        sut.RemoveAdditionalParameterCommand.Execute(parameter);

        sut.AdditionalParameters.Should().BeEmpty();
    }
}
