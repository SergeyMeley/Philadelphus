using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.JsonRepository.Repositories;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.Business.Helpers.InfrastructureConverters;

namespace Philadelphus.WpfApplicationTests;

[TestClass]
public class Test1
{
    [TestMethod]
    public void InsertDataStorages()
    {
        //var repository = new JsonDataStoragesCollectionInfrastructureRepository();
        //var storages = new List<DataStorage>()
        //{
        //    new DataStorageBuilder()
        //        .SetGeneralParameters("qwe", "dfbdfzbz", Guid.NewGuid(), InfrastructureTypes.JsonDocument)
        //        .SetRepository((IDataStoragesInfrastructureRepository)repository)
        //        .Build().ToDbEntity(),
        //    new DataStorageBuilder()
        //        .SetGeneralParameters("qwe2", "qwewgfd", Guid.NewGuid(), InfrastructureTypes.JsonDocument)
        //        .SetRepository((IDataStoragesInfrastructureRepository)repository)
        //        .Build().ToDbEntity()
        //};
        //repository.InsertDataStorages(storages);
    }

    [TestMethod]
    public void SelectDataStorages() 
    {

    }
}
