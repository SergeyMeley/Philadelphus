using Philadelphus.Business.Entities.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Models.StorageConfig
{
    public interface IStorageConfigService
    {
        StorageConfig LoadConfig(string filePath = "storage-config.json");
        IDataStorageModel? CreateStorageModel(Guid modelGuid);
        IDataStorageModel? GetDefaultStorageModel();
        List<IDataStorageModel> GetEnabledStorageModels();
    }
}
