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
        public DataStorageBuilder SetGeneralParameters(string name, string description, Guid guid, InfrastructureTypes infrastructureType, bool isDisabled)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description)/* || guid == Guid.Empty*/)  //TODO: Исправить костыль
                throw new ArgumentException("Переданы некорректные параметры");
            _storageModel = new DataStorageModel(guid, name, description, infrastructureType, isDisabled);
            return this;
        }
        public DataStorageBuilder SetRepository(IInfrastructureRepository repository)
        {
            if (repository == null)
                return this;
            if (_storageModel.IsDisabled)
                return this;
            if (_storageModel == null)
                throw new ArgumentNullException("Сначала необходимо назначить основные параметры");
            if (_storageModel.InfrastructureRepositories.ContainsKey(repository.EntityGroup))
            {
                _storageModel.InfrastructureRepositories[repository.EntityGroup] = repository;
            }
            else
            {
                _storageModel.InfrastructureRepositories.Add(repository.EntityGroup, repository);
            }
            return this;
        }
        public IDataStorageModel Build()
        {
            if (_storageModel == null)
                return null;
            if (string.IsNullOrEmpty(_storageModel.Name) 
                || string.IsNullOrEmpty(_storageModel.Description)
                /*|| _storageModel.Guid == Guid.Empty*/)    //TODO: Исправить костыль
                return null;
            if (_storageModel.InfrastructureRepositories == null || _storageModel.InfrastructureRepositories.Count == 0)
                return null;
            return _storageModel;
        }
    }
}
