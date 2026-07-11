using FluentAssertions;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

public class SqliteConnectionStringVMTests
{
    [Fact]
    public void Constructor_ParsesDataSource()
    {
        // Act
        var sut = new SqliteConnectionStringVM("Data Source=C:\\data\\repository.db");

        // Assert
        sut.DataSource.Should().Be("C:\\data\\repository.db");
    }

    [Fact]
    public void ConnectionString_UsesSqliteBuilderEscaping()
    {
        // Arrange
        var sut = new SqliteConnectionStringVM();

        // Act
        sut.DataSource = "C:\\data\\repository;main.db";

        // Assert
        sut.ConnectionString.Should().Be("Data Source=\"C:\\data\\repository;main.db\"");
    }

    [Fact]
    public void Constructor_ParsesAdditionalParameters()
    {
        // Act
        var sut = new SqliteConnectionStringVM("Data Source=repository.db;Mode=ReadOnly;Cache=Shared");

        // Assert
        sut.AdditionalParameters.Should().Contain(x => x.Key == "Mode" && x.Value == "ReadOnly");
        sut.AdditionalParameters.Should().Contain(x => x.Key == "Cache" && x.Value == "Shared");
    }

    [Fact]
    public void AdditionalParameterChange_RebuildsConnectionString()
    {
        // Arrange
        var sut = new SqliteConnectionStringVM("Data Source=repository.db;Mode=ReadOnly");
        var mode = sut.AdditionalParameters.Single(x => x.Key == "Mode");

        // Act
        mode.Value = "ReadWriteCreate";

        // Assert
        sut.ConnectionString.Should().Contain("Mode=ReadWriteCreate");
    }
}
