using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.Infrastructure
{
    public class DataStorageBuilder
    {
        private IDataStorageModel _storageModel;
        public DataStorageBuilder() 
        {

        }
        public DataStorageBuilder SetGeneralParameters(string name, string description, Guid guid, InfrastructureTypes infrastructureType)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || guid == Guid.Empty)
                throw new ArgumentException("Переданы некорректные параметры");
            _storageModel = new DataStorageModel(guid, name, description, infrastructureType);
            return this;
        }
        public DataStorageBuilder SetRepository(IDataStoragesInfrastructureRepository repository)
        {
            if (repository == null)
                return null;
            if (_storageModel == null)
                throw new ArgumentNullException("Сначала необходимо назначить основные параметры");
            _storageModel.DataStorageInfrastructureRepositoryRepository = repository;
            return this;
        }
        public DataStorageBuilder SetRepository(ITreeRepositoriesInfrastructureRepository repository)
        {
            if (repository == null)
                return null;
            if (_storageModel == null)
                throw new ArgumentNullException("Сначала необходимо назначить основные параметры");
            _storageModel.TreeRepositoryHeadersInfrastructureRepository = repository;
            return this;
        }
        public DataStorageBuilder SetRepository(IMainEntitiesInfrastructureRepository repository)
        {
            if (repository == null)
                return null;
            if (_storageModel == null)
                throw new ArgumentNullException("Сначала необходимо назначить основные параметры");
            _storageModel.MainEntitiesInfrastructureRepository = repository;
            return this;
        }
        public IDataStorageModel Build()
        {
            if (_storageModel == null)
                return null;
            if (string.IsNullOrEmpty(_storageModel.Name) 
                || string.IsNullOrEmpty(_storageModel.Description) 
                || _storageModel.Guid == Guid.Empty)
                return null;
            if (_storageModel.DataStorageInfrastructureRepositoryRepository == null
                && _storageModel.TreeRepositoryHeadersInfrastructureRepository == null
                && _storageModel.MainEntitiesInfrastructureRepository == null)
                return null;
            return _storageModel;
        }
    }
}
