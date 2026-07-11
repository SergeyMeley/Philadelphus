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
}
