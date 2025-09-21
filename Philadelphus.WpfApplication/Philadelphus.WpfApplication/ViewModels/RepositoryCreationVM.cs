using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCreationVM : ViewModelBase
    {
        private RepositoryCreationWindow _window;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; set => _dataStoragesSettingsVM = value; }
        public RepositoryCreationVM(RepositoryCreationWindow window, RelayCommand openDataStoragesSettingsWindowCommand)
        {
            _window = window;
            OpenDataStoragesSettingsWindowCommand = openDataStoragesSettingsWindowCommand;
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeProcessingService();
                    var result = service.CreateNewTreeRepository(_dataStoragesSettingsVM.SelectedDataStorage);
                    result.Name = _name;
                    result.Description = _description;
                    service.SaveChanges(result);
                    _window.Close();
                });
            }
        }
        public RelayCommand OpenDataStoragesSettingsWindowCommand;
    }
}
