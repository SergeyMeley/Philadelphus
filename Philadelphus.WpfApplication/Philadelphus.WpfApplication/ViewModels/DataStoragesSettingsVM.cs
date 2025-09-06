using Philadelphus.Business.Entities.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class DataStoragesSettingsVM
    {
        public IEnumerable<IDataStorageModel>? DataStorages { get; set; }
    }
}
