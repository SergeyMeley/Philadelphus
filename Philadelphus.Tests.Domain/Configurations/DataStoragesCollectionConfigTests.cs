using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Tests.Domain.Configurations;

public class DataStoragesCollectionConfigTests
{
    [Fact]
    public void Deserialize_ConfigWithoutConnectionStrings_InitializesEmptyValues()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var json = $$"""
            {
              "DataStorages": [
                {
                  "Uuid": "{{storageUuid}}",
                  "Name": "Legacy storage",
                  "Description": "Legacy storage",
                  "InfrastructureType": "SQLiteEf"
                }
              ]
            }
            """;
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var config = JsonSerializer.Deserialize<DataStoragesCollectionConfig>(json, serializerOptions);

        // Assert
        config.Should().NotBeNull();
        var dataStorage = config!.DataStorages.Should().ContainSingle().Subject;
        dataStorage.ProviderName.Should().BeEmpty();
        dataStorage.ConnectionStrings.Should().NotBeNull().And.BeEmpty();
    }
}
