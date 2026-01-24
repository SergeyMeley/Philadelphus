using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace Philadelphus.Presentation.Wpf.UI.Services.Implementations
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollectionConfig;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollectionConfig;
        private readonly IOptions<TreeRepositoryHeadersCollectionConfig> _treeRepositoryHeadersCollectionConfig;
        public ConfigurationService(
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollectionConfig,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollectionConfig,
            IOptions<TreeRepositoryHeadersCollectionConfig> treeRepositoryHeadersCollectionConfig)
        {
            _appConfig = appConfig;
            _connectionStringsCollectionConfig = connectionStringsCollectionConfig;
            _dataStoragesCollectionConfig = dataStoragesCollectionConfig;
            _treeRepositoryHeadersCollectionConfig = treeRepositoryHeadersCollectionConfig;
        }

        public bool MoveConfigFile(FileInfo configFile, DirectoryInfo newDirectory)
        {
            if (configFile.DirectoryName == newDirectory.FullName)
                return true;
            if (newDirectory.Exists == false)
            {
                try
                {
                    newDirectory.Create();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            var oldFullPath = new FileInfo(configFile.FullName);
            var newFullPath = new FileInfo(Path.Combine(newDirectory.FullName, configFile.Name));

            configFile.MoveTo(newFullPath.FullName);
            UpdateConfigPath(oldFullPath, newFullPath);

            return true;
        }

        private bool UpdateConfigPath(FileInfo oldFullPath, FileInfo newFullPath)
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var text = System.IO.File.ReadAllText(configPath);
            text = text.Replace(oldFullPath.FullName, newFullPath.FullName);
            var oldContract = ContractEnvironmentVariables(oldFullPath.FullName);
            var newContract = ContractEnvironmentVariables(newFullPath.FullName);
            text = text.Replace(oldContract, newContract);

            System.IO.File.WriteAllText(configPath, text);
            return true;
        }

        public bool UpdateConfigFile<T>(FileInfo configFile, IOptions<T> newConfigObject) where T : class
        {
            //var jsonSection = newConfigObject.Value.GetType().Name;

            //var json = System.IO.File.ReadAllText(configFile.FullName);
            //var options = new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    WriteIndented = true,
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //};

            //var root = JsonSerializer.Deserialize<JsonElement>(json);

            //T configContent = null;

            //if (root.TryGetProperty(jsonSection, out var collectionNode))
            //{
            //    configContent = JsonSerializer.Deserialize<T>(collectionNode, options);
            //}

            //if (configContent == null)
            //    throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");

            //var json2 = json.Replace(
            //    "\"ApplicationSettings\": {",
            //    $"\"ApplicationSettings\": {JsonSerializer.Serialize(configContent, options)}");

            //MessageBox.Show(json);
            ////File.WriteAllText(_file.FullName, json);

            return false;
        }

        public static string ContractEnvironmentVariables(string expandedPath)
        {
            // Основные переменные среды из вашего конфига
            var envVars = new Dictionary<string, string>
            {
                ["%USERPROFILE%"] = Environment.GetEnvironmentVariable("USERPROFILE"),
                ["%LOCALAPPDATA%"] = Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                ["%APPDATA%"] = Environment.GetEnvironmentVariable("APPDATA")
            };

            string result = expandedPath;

            foreach (var kvp in envVars)
            {
                if (!string.IsNullOrEmpty(kvp.Value) && result.Contains(kvp.Value, StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Replace(kvp.Value, kvp.Key, StringComparison.OrdinalIgnoreCase);
                }
            }

            return result.Replace("\\", "\\\\");
        }
    }
}
