using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs;
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
        public bool SelectAnotherConfigFile(ConfigurationFileVM configurationFileVM, FileInfo newFile)
        {
            var originPath = configurationFileVM.FileInfo;
            var result = configurationFileVM.ChangeFile(newFile);
            if (result)
            {
                UpdateConfigPath(originPath, newFile);
            }
            return result;
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
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
            if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("В режиме разработки изменения не могут быть сохранены. Повторите попытку в режиме Продакшн.");
                return false;
            }

            var jsonSection = newConfigObject.Value.GetType().Name;

            var json = System.IO.File.ReadAllText(configFile.FullName);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            var root = JsonSerializer.Deserialize<JsonElement>(json);

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

            writer.WriteStartObject();

            foreach (var property in root.EnumerateObject())
            {
                if (property.Name == jsonSection)
                {
                    writer.WritePropertyName(jsonSection);
                    var newSectionJson = JsonSerializer.Serialize(newConfigObject.Value, options);
                    using var newSectionDoc = JsonDocument.Parse(newSectionJson);
                    newSectionDoc.RootElement.WriteTo(writer);
                }
                else
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
            writer.Flush();

            var resultText = Encoding.UTF8.GetString(stream.ToArray());

            System.IO.File.WriteAllText(configFile.FullName, resultText);

            return true;
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
