using Microsoft.Extensions.Configuration;
using Philadelphus.Core.Domain.Config;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface IDataStoragesService
    {
        #region [ Props ]

        #endregion

        #region [ Get + Load ]

        public IEnumerable<IDataStorageModel> GetStoragesModels(ConnectionStringsCollection connectionStrings);

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

        public IDataStorageModel CreateMainDataStorageModel(FileInfo storagesConfigFullPath, FileInfo repositoryHeadersConfigFullPath);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
