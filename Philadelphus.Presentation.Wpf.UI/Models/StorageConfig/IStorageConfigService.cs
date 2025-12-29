using Philadelphus.Core.Domain.Entities.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Models.StorageConfig
{
    public interface IStorageConfigService
    {
        StorageConfig LoadConfig(string filePath = "storage-config.json");
        IDataStorageModel? CreateStorageModel(Guid modelUuid);
        //IDataStorageModel? GetDefaultStorageModel();
        //List<IDataStorageModel> GetEnabledStorageModels();
    }
}
