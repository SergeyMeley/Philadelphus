using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.JsonRepository.Repositories;
using Philadelphus.InfrastructureConverters.Converters;
using Philadelphus.InfrastructureEntities.Enums;

namespace Philadelphus.WpfApplicationTests;

[TestClass]
public class Test1
{
    [TestMethod]
    public void InsertDataStorages()
    {
        var repository = new JsonDataStorageAndTreeRepositoryInfrastructureRepository();
        var storages = new List<DataStorage>()
        {
            new DataStorageBuilder()
                .SetGeneralParameters("qwe", "dfbdfzbz", Guid.NewGuid(), InfrastructureTypes.JsonDocument)
                .SetRepository((IDataStorageInfrastructureRepository)repository)
                .Build().BusinessToDbEntity(),
            new DataStorageBuilder()
                .SetGeneralParameters("qwe2", "qwewgfd", Guid.NewGuid(), InfrastructureTypes.JsonDocument)
                .SetRepository((IDataStorageInfrastructureRepository)repository)
                .Build().BusinessToDbEntity()
        };
        repository.InsertDataStorages(storages);
    }

    [TestMethod]
    public void SelectDataStorages() 
    {

    }
}
