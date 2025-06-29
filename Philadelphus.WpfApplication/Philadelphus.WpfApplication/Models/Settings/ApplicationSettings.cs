using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.WpfApplication.Models.Settings
{
    public static class ApplicationSettings
    {
        public static string DefaultRepositoryPath { get => @"C:\Users\MeleychukSV\Documents\Philadelphus\Repositories.xml"; }
        private static string _repositoryListConfigPath;
        public static string RepositoryListConfigPath 
        { 
            get
            {
                if (string.IsNullOrEmpty(_repositoryListConfigPath))
                    return DefaultRepositoryPath;
                return _repositoryListConfigPath;
            }
            set => _repositoryListConfigPath = value; 
        }
    }
}
