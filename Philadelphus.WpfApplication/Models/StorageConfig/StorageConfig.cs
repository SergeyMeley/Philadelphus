using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Models.StorageConfig
{
    public class StorageConfig
    {
        public List<StorageModelConfig> DataStorageModels { get; set; } = new();
        //public Guid DefaultStorageGuid { get; set; }
        //public int CacheTimeoutMinutes { get; set; } = 30;
        //public int MaxRetryAttempts { get; set; } = 3;
    }
}
