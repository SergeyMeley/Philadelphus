using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Builders;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Serilog;
using System.Diagnostics;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Основной сервис для работы с репозиторием и его элементами
    /// </summary>
    public class PhiladelphusRepositoryService : IPhiladelphusRepositoryService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основной сервис для работы с репозиторием и его элементами
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);

            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с содержимым</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="InvalidOperationException">Если операция недопустима для текущего состояния объекта.</exception>
        public PhiladelphusRepositoryModel GetShrubContent(PhiladelphusRepositoryModel repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            var shrub = repository.ContentShrub
                ?? throw new InvalidOperationException("Рабочий кустарник не инициализирован");

            repository.ContentShrub.ContentWorkingTrees.Clear();

            var sw = Stopwatch.StartNew();

            var result = GetShrubContentForce(repository);

            sw.Stop();

            SendContentLoadNotification("Содержимое репозитория", repository.Name, "БД", sw.Elapsed);

            InitSystemWorkingTree(repository.ContentShrub);

            return result;
        }

        /// <summary>
        /// Получить коллекцию элементов репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Репозиторий с содержимым</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public Task<PhiladelphusRepositoryModel> GetShrubContentAsync(
            PhiladelphusRepositoryModel repository,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(repository);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return GetShrubContent(repository);
            }, cancellationToken);
        }

        #endregion



        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        /// <exception cref="InvalidOperationException">Если операция недопустима для текущего состояния объекта.</exception>
        public long SaveChanges(ref PhiladelphusRepositoryModel repository, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (repository == null)
                return 0;

            var originalState = repository.State;

            try
            {
                // Уведомление
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Начало сохранения репозитория. Репозиторий: '{repository.Name}'",
                    criticalLevel: NotificationCriticalLevelModel.Info);

                // Преобразование в сущность БД
                long result = 0;
                var infrastructure = repository.OwnDataStorage?.PhiladelphusRepositoriesInfrastructureRepository
                    ?? throw new InvalidOperationException("Инфраструктура хранилища не инициализирована");

                var entity = _mapper.Map<PhiladelphusRepository>(repository);
                var storage = repository.OwnDataStorage;

                // Сохранение изменений
                switch (repository.State)
                {
                    case State.Initialized:
                        result += infrastructure.InsertRepository(entity);
                        break;
                    case State.Changed:
                        result += infrastructure.UpdateRepository(entity);
                        break;
                    case State.ForSoftDelete:
                        result += infrastructure.SoftDeleteRepository(entity);
                        break;
                    default:
                        _logger.Warning($"Неизвестное состояние репозитория: {repository.State}");
                        break;
                }

                // Возвращение данных, генерируемых инфраструктурой
                repository.AuditInfo = _mapper.Map<AuditInfoModel>(entity.AuditInfo);

                // Актуализация статуса
                SetModelState(repository, State.SavedOrLoaded);

                // Сохранение содержимого
                if (saveMode == SaveMode.WithContent ||
                    saveMode == SaveMode.WithContentAndMembers)
                {
                }

                // Сохранение участников
                if (saveMode == SaveMode.WithContentAndMembers)
                {
                    result += SaveChanges(repository.ContentShrub.ContentWorkingTrees, saveMode);
                }

                // Уведомление
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Сохранение репозитория успешно выполнено. Сохранено {result} элементов.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);

                return result;
            }
            catch (Exception ex)
            {
                SetModelState(repository, originalState);
                _logger.Error(ex, $"Ошибка сохранения репозитория '{repository.Name}'");
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка сохранения изменений в БД. Подробнее:\r\n{ex.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
                throw;
            }
        }

        /// <summary>
        /// Сохранить изменения (рабочие деревья)
        /// </summary>
        /// <param name="workingTrees">Коллекция рабочих деревьев</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<WorkingTreeModel> workingTrees, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (workingTrees == null)
                return 0;

            var workingTreesList = workingTrees.ToList();
            if (workingTreesList.Count == 0)
                return 0;

            EnsureSystemWorkingTreeUsesMainDataStorage(workingTreesList);
            EnsureShrubMembersStorageSupport(workingTreesList.Select(x => x.DataStorage));

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения рабочих деревьев. Рабочие деревья: {string.Join(", ", workingTreesList.Select(x => $"'{x.Name}' [{x.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in workingTreesList.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = workingTreesList.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.ForHardDelete,
                    dbCollection => deletedCount = infrastructure.HardDeleteTrees(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(workingTreesList);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(workingTreesList.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(workingTreesList.Select(x => x.ContentRoot), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение рабочих деревьев успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (корни)
        /// </summary>
        /// <param name="treeRoots">Коллекция корней</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeRootModel> treeRoots, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeRoots == null)
                return 0;

            var treeRootsList = treeRoots
                .Where(x => x != null)
                .ToList();
            if (treeRootsList.Count == 0)
                return 0;

            EnsureShrubMembersStorageSupport(treeRootsList.Select(x => x.DataStorage));

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения корней. Рабочие деревья: {string.Join(", ", treeRootsList.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeRootsList.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeRootsList.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.ForHardDelete,
                    dbCollection => deletedCount = infrastructure.HardDeleteRoots(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeRootsList);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeRootsList.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeRootsList.SelectMany(x => x.ChildNodes), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение корней успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (узлы)
        /// </summary>
        /// <param name="treeNodes">Коллекция узлов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeNodeModel> treeNodes, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeNodes == null || treeNodes.Count() == 0)
                return 0;

            EnsureShrubMembersStorageSupport(treeNodes.Select(x => x.DataStorage));

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения узлов. Рабочие деревья: {string.Join(", ", treeNodes.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeNodes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeNodes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.ForHardDelete,
                    dbCollection => deletedCount = infrastructure.HardDeleteNodes(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeNodes);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeNodes.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildNodes), saveMode);
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildLeaves), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение узлов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (листы)
        /// </summary>
        /// <param name="treeLeaves">Коллекция листов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeLeaveModel> treeLeaves, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeLeaves == null || treeLeaves.Count() == 0)
                return 0;

            EnsureShrubMembersStorageSupport(treeLeaves.Select(x => x.DataStorage));

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения листов. Рабочие деревья: {string.Join(", ", treeLeaves.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeLeaves.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeLeaves.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.ForHardDelete,
                    dbCollection => deletedCount = infrastructure.HardDeleteLeaves(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeLeaves);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeLeaves.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение листов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (атрибуты элемента)
        /// </summary>
        /// <param name="elementAttributes">Коллекция атрибутов</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveContentChanges(IEnumerable<ElementAttributeModel> elementAttributes)
        {
            // Runtime-атрибуты вычисляются заново и не имеют представления в хранилище.
            var persistentAttributes = elementAttributes?
                .Where(x => x is { IsRuntime: false })
                .ToList() ?? [];

            // Проверка исходных данных
            if (persistentAttributes.Count == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения атрибутов. Рабочие деревья: {string.Join(", ", persistentAttributes.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in persistentAttributes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = persistentAttributes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.ForHardDelete,
                    dbCollection => deletedCount = infrastructure.HardDeleteAttributes(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(persistentAttributes);

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение атрибутов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return result;
        }

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать корень и добавить родителю
        /// </summary>
        /// <param name="owner">Родитель</param>
        /// <param name="dataStorage">Хранилище</param>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        /// <param name="withoutInfoNotifications">Признак отключения информационных уведомлений.</param>
        /// <returns>Корень</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public WorkingTreeModel CreateWorkingTree(
            PhiladelphusRepositoryModel owner, 
            IDataStorageModel dataStorage, 
            bool needAutoName = true,
            bool withoutInfoNotifications = false)
        {
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentNullException.ThrowIfNull(dataStorage);

            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания рабочего дерева. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new WorkingTreeModel(
                    Guid.CreateVersion7(),
                    dataStorage,
                    owner.ContentShrub,
                    _notificationService,
                    PropertiesPolicyBuilder.CreateWorkingTreeDefault(_notificationService));

                if (needAutoName)
                {
                    NewEntityAutoNameAssignmentHelper.TryAssign(result, _notificationService);
                    result.AssignAutoSequence(owner.ContentShrub.ContentWorkingTrees.Select(x => x.Sequence));
                }

                owner.ContentShrub.ContentWorkingTrees.Add(result);
                owner.ContentShrub.ContentWorkingTreesUuids.Add(result.Uuid);

                SetModelState(result, State.Initialized);
                SetModelState(owner, State.Changed);


                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание рабочего дерева '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания рабочего дерева.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания рабочего дерева. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        private static string? GetDefaultSystemBaseLeaveStringValue(SystemBaseType type)
        {
            return type switch
            {
                _ => null,
                //SystemBaseType.STRING => TreeLeaveModel.EmptyStringValue,
                //SystemBaseType.INTEGER => "0",
                //SystemBaseType.NUMERIC or SystemBaseType.FLOAT or SystemBaseType.MONEY => "0.0",
                //SystemBaseType.BOOL => "false",
                //SystemBaseType.DATETIME => "1970-01-01T00:00:00+00:00",
                //SystemBaseType.DATE => "1970-01-01",
                //SystemBaseType.TIME => "00:00:00",
                //SystemBaseType.OBJECT => TreeLeaveModel.EmptyStringValue,
                //_ => TreeLeaveModel.EmptyStringValue,
            };
        }

        /// <summary>
        /// Создать корень и добавить родителю
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <param name="dataStorage">Хранилище</param>
        /// <param name="owner">Владелец.</param>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        /// <param name="withoutInfoNotifications">Признак отключения информационных уведомлений.</param>
        /// <returns>Корень</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeRootModel CreateTreeRoot(WorkingTreeModel owner, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            ArgumentNullException.ThrowIfNull(owner);

            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания корня. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new TreeRootModel(
                    Guid.CreateVersion7(),
                    owner,
                    _notificationService,
                    PropertiesPolicyBuilder.CreateTreeRootDefault(_notificationService));

                if (needAutoName)
                {
                    NewEntityAutoNameAssignmentHelper.TryAssign(result, _notificationService);
                    result.AssignAutoSequence(owner.ContentRoot != null
                        ? new[] { owner.ContentRoot.Sequence }
                        : Enumerable.Empty<long>());
                }

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание корня '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания корня.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания корня. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать узел и добавить родителю
        /// </summary>
        /// <param name="parent">Родитель</param>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        /// <param name="withoutInfoNotifications">Признак отключения информационных уведомлений.</param>
        /// <returns>Узел</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeNodeModel CreateTreeNode(IParentModel parent, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            ArgumentNullException.ThrowIfNull(parent);

            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания узла. Родитель - '{(parent as IMainEntityModel).Name}' [{(parent as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new TreeNodeModel(
                    Guid.CreateVersion7(),
                    parent,
                    (parent as IWorkingTreeMemberModel)?.OwningWorkingTree,
                    _notificationService,
                    PropertiesPolicyBuilder.CreateTreeNodeDefault(_notificationService));

                if (needAutoName)
                {
                    NewEntityAutoNameAssignmentHelper.TryAssign(result, _notificationService);
                    result.AssignAutoSequence(parent.Childs.Values
                        .OfType<TreeNodeModel>()
                        .Select(x => x.Sequence));
                }

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание узла '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания узла.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания узла. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать лист и добавить родителю
        /// </summary>
        /// <param name="parent">Родитель</param>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        /// <param name="withoutInfoNotifications">Признак отключения информационных уведомлений.</param>
        /// <returns>Лист</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parent, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            ArgumentNullException.ThrowIfNull(parent);

            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания листа. Родитель - '{(parent as IMainEntityModel).Name}' [{(parent as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                // Узел BOOL обслуживается как системный справочник с двумя предопределенными листьями:
                // "Истина" и "Ложь". Пользовательское расширение этого набора нарушит семантику bool.
                if (parent is SystemBaseTreeNodeModel { SystemBaseType: SystemBaseType.BOOL })
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание листа в узле логических значений запрещено. Для типа BOOL допустимы только предопределенные значения 'Истина' и 'Ложь'.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return null;
                }

                TreeLeaveModel result = null;
                if (parent is SystemBaseTreeNodeModel sbn)
                {
                    result = new SystemBaseTreeLeaveModel(
                        Guid.CreateVersion7(),
                        sbn,
                        sbn.OwningWorkingTree,
                        sbn.SystemBaseType,
                        _notificationService,
                        PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));
                }
                else
                {
                    result = new TreeLeaveModel(
                        Guid.CreateVersion7(),
                        parent,
                        parent.OwningWorkingTree,
                        _notificationService,
                        PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));
                }

                if (result is SystemBaseTreeLeaveModel systemBaseLeave)
                {
                    // У системного листа валидируемым значением является StringValue, а не автоимя.
                    // Поэтому при создании сразу задаем корректный дефолт для его SystemBaseType, если такой
                    // дефолт существует без обращения к пользовательскому ресурсу. Например, для FILE нельзя
                    // безопасно выбрать универсальный путь, поэтому значение остается пустым до выбора файла.
                    var defaultValue = GetDefaultSystemBaseLeaveStringValue(systemBaseLeave.SystemBaseType);
                    if (defaultValue != null
                        && SystemBaseStringValueValidator.IsValid(systemBaseLeave.SystemBaseType, defaultValue, out _))
                    {
                        systemBaseLeave.StringValue = defaultValue;
                    }
                }
                else if (needAutoName)
                {
                    NewEntityAutoNameAssignmentHelper.TryAssign(result, _notificationService);
                }

                if (needAutoName)
                {
                    result.AssignAutoSequence(parent.ChildLeaves.Select(x => x.Sequence));
                }

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                       $"Создание листа '{result.Name}' успешно выполнено.",
                       criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания листа.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания листа. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать атрибут и добавить владельцу
        /// </summary>
        /// <param name="owner">Владелец</param>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        /// <param name="withoutInfoNotifications">Признак отключения информационных уведомлений.</param>
        /// <returns>Атрибут</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            ArgumentNullException.ThrowIfNull(owner);

            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания атрибута. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                if (owner is TreeLeaveModel)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Ошибка создания атрибута. Изменение перечня атрибутов листов не допускается.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return null;
                }

                var uuid = Guid.CreateVersion7();

                if (owner is IWorkingTreeMemberModel wtm)
                {
                    var result = new ElementAttributeModel(
                        uuid,
                        owner,
                        uuid,
                        owner,
                        wtm.OwningWorkingTree,
                        _notificationService,
                        AttributePolicyBuilder.CreateDefault(_notificationService))
                    {
                        ValueType = wtm.OwningShrub.SystemBaseWorkingTree.GetAllNodesRecursive().SingleOrDefault(x => x is SystemBaseTreeNodeModel sbn && sbn.SystemBaseType == SystemBaseType.STRING)
                    };

                    if (needAutoName)
                    {
                        NewEntityAutoNameAssignmentHelper.TryAssign(result, _notificationService);
                        result.AssignAutoSequence(owner.Attributes.Select(x => x.Sequence));
                    }

                    SetModelState(result, State.Initialized);

                    if (withoutInfoNotifications == false)
                    {
                        _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                            $"Создание атрибута '{result.Name}' успешно выполнено.",
                            criticalLevel: NotificationCriticalLevelModel.Ok);
                    }

                    return result;
                }
                _logger.Warning("Попытка добавления атрибута элементу, НЕ являющумяся участником рабочего дерева.");
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания атрибута. Атрибут можно добавить только участнику рабочего дерева.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания атрибута.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания атрибута. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        /// <summary>
        /// Мягкое удаление элемента репозитория
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool SoftDeleteShrubMember(IContentModel element)
            => DeleteShrubMember(element, State.ForSoftDelete);

        /// <summary>
        /// Пометить элемент репозитория для физического удаления из хранилища данных.
        /// </summary>
        public bool HardDeleteShrubMember(IContentModel element)
            => DeleteShrubMember(element, State.ForHardDelete);

        /// <summary>
        /// Пометить элемент репозитория для удаления.
        /// </summary>
        /// <param name="element">Удаляемый элемент.</param>
        /// <param name="deletionState">Вид удаления.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если элемент не задан.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Если передано состояние, не являющееся видом удаления.</exception>
        private bool DeleteShrubMember(
            IContentModel element,
            State deletionState)
        {
            ArgumentNullException.ThrowIfNull(element);
            if (deletionState is not State.ForSoftDelete and not State.ForHardDelete)
                throw new ArgumentOutOfRangeException(nameof(deletionState));

            var hardDelete = deletionState == State.ForHardDelete;

            try
            {
                var mainEntity = (IMainEntityModel)element;
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    hardDelete
                        ? $"Начало безвозвратного удаления элемента. Элемент - '{mainEntity.Name}' [{mainEntity.Uuid}]."
                        : $"Начало удаления элемента. Элемент - '{mainEntity.Name}' [{mainEntity.Uuid}].",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                if (element is ElementAttributeModel ea && ea.Owner is TreeLeaveModel)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        "Удаление элемента невозможно. Изменение перечня атрибутов листов не допускается.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                // TODO #65206730: Тех. долг. Проанализировать корректный сценарий удаления атрибута,
                // если от него уже созданы унаследованные атрибуты у дочерних элементов.
                if (element is ElementAttributeModel attribute
                    && HasInheritedAttributesInDescendants(attribute))
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Удаление атрибута '{attribute.Name}' невозможно. Атрибут уже унаследован дочерними элементами. Требуется отдельный анализ правил удаления унаследованных атрибутов.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                // Логические системные листья нельзя удалять: на них могут ссылаться атрибуты,
                // а сам набор значений должен оставаться полным и предсказуемым.
                if (element is SystemBaseTreeLeaveModel { SystemBaseType: SystemBaseType.BOOL } boolLeave)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Удаление логического значения '{boolLeave.StringValue}' запрещено. Для типа BOOL допустимы только предопределенные значения 'Истина' и 'Ложь'.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                if (element is WorkingTreeModel { IsSystemBase: true }
                    || element is TreeRootModel { IsSystemBase: true }
                    || element is SystemBaseTreeNodeModel
                    || element is SystemBaseTreeLeaveModel)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Удаление системного элемента '{mainEntity.Name}' [{mainEntity.Uuid}] запрещено.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                if (element is WorkingTreeModel or TreeRootModel)
                {
                    var workingTree = element is WorkingTreeModel tree
                        ? tree
                        : ((TreeRootModel)element).OwningWorkingTree;

                    SetModelState(workingTree, deletionState);
                    if (workingTree.ContentRoot != null)
                    {
                        SetModelState(workingTree.ContentRoot, deletionState);
                    }

                    workingTree.OwningShrub.ContentWorkingTreesUuids.Remove(workingTree.Uuid);
                    SetModelState(workingTree.OwningShrub.OwningRepository, State.Changed);
                }
                else if (element is IMainEntityWritableModel writableModel)
                {
                    SetModelState(writableModel, deletionState);
                }

                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    hardDelete
                        ? $"Элемент '{mainEntity.Name}' [{mainEntity.Uuid}] помечен для безвозвратного удаления. Изменения применяются после сохранения, если удаление не требуется - выполните обновление данных из хранилища."
                        : $"Элемент '{mainEntity.Name}' [{mainEntity.Uuid}] помечен для мягкого удаления. Изменения применяются после сохранения, если удаление не требуется - выполните обновление данных из хранилища.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);
                return true;
            }
            catch (Exception ex)
            {
                if (hardDelete)
                {
                    _logger.Error(ex, "Ошибка безвозвратного удаления элемента.");
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Ошибка безвозвратного удаления элемента. Подробнее: {ex.Message}",
                        criticalLevel: NotificationCriticalLevelModel.Error);
                }
                else
                {
                    _logger.Error(ex, "Ошибка удаления элемента.");
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Ошибка удаления элемента. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                }
                throw;
            }
        }

        #endregion

        #region [ Temp ]



        #endregion

        #region [ Private methods ]

        /// <summary>
        /// Отправить уведомление о получении содержимого
        /// </summary>
        /// <param name="contentName">Название содержимого</param>
        /// <param name="entityName">Название сущности</param>
        /// <param name="sourceName">Название источника данных</param>
        /// <param name="elapsed">Время выполнения операции</param>
        private void SendContentLoadNotification(
            string contentName,
            string entityName,
            string sourceName,
            TimeSpan elapsed)
        {
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"{contentName} '{entityName}' получено. Источник - {sourceName}, время операции - {elapsed.TotalMilliseconds:0.###} мс.",
                criticalLevel: NotificationCriticalLevelModel.Info);
        }

        /// <summary>
        /// Изменить статус элемента
        /// </summary>
        /// <param name="model">Элемент</param>
        /// <param name="newState">Новый статус</param>
        private void SetModelState(
            IMainEntityWritableModel model,
            State newState)
        {
            model.SetState(newState);
        }

        /// <summary>
        /// Инициализировать системное рабочее дерево базовых типов
        /// </summary>
        /// <param name="shrub">Кустарник</param>
        /// <returns>Признак инициализации системного рабочего дерева</returns>
        private bool InitSystemWorkingTree(
            ShrubModel shrub)
        {
            var mainDataStorage = GetMainDataStorage(shrub);
            var existTree = shrub.ContentWorkingTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseUuid);
            if (existTree != null)
            {
                existTree.ChangeDataStorage(mainDataStorage);
                return EnsureSystemBaseTypes(existTree);
            }

            // TODO: ех. долг #61187115

            var tree = new WorkingTreeModel(
                uuid: WorkingTreeModel.SystemBaseUuid,
                dataStorage: mainDataStorage,
                owner: shrub,
                notificationService: _notificationService,
                new EmptyPropertiesPolicy<WorkingTreeModel>())
            {
                Name = "Системное рабочее дерево"
            };

            var root = new TreeRootModel(
                uuid: TreeRootModel.SystemBaseUuid,
                owner: tree,
                notificationService: _notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>())
            {
                Name = "Базовые типы данных"
            };

            tree.ContentRoot = root;

            EnsureSystemBaseTypes(tree);

            shrub.ContentWorkingTrees.Add(tree);
            if (shrub.ContentWorkingTreesUuids.Contains(tree.Uuid) == false)
                shrub.ContentWorkingTreesUuids.Add(tree.Uuid);
            return true;
        }

        /// <summary>
        /// Инициализировать недостающие системные базовые типы и их предопределенные значения.
        /// </summary>
        /// <param name="tree">Системное рабочее дерево.</param>
        /// <returns>Признак изменения дерева.</returns>
        private bool EnsureSystemBaseTypes(
            WorkingTreeModel tree)
        {
            var nodesChanged = EnsureSystemBaseNodes(tree);
            var leavesChanged = EnsureSystemBaseLeaves(tree);
            return nodesChanged || leavesChanged;
        }

        /// <summary>
        /// Инициализирует недостающие узлы системных базовых типов.
        /// </summary>
        private bool EnsureSystemBaseNodes(WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            var root = tree.ContentRoot
                ?? throw new InvalidOperationException("Системное рабочее дерево должно иметь корень.");

            var changed = false;

            var obj = GetOrCreateSystemBaseNode(tree, root, SystemBaseType.OBJECT, ref changed);

            GetOrCreateSystemBaseNode(tree, obj, SystemBaseType.STRING, ref changed);
            GetOrCreateSystemBaseNode(tree, obj, SystemBaseType.BOOL, ref changed);
            GetOrCreateSystemBaseNode(tree, obj, SystemBaseType.FILE, ref changed);

            var num = GetOrCreateSystemBaseNode(tree, obj, SystemBaseType.NUMERIC, ref changed);

            GetOrCreateSystemBaseNode(tree, num, SystemBaseType.INTEGER, ref changed);
            GetOrCreateSystemBaseNode(tree, num, SystemBaseType.FLOAT, ref changed);
            GetOrCreateSystemBaseNode(tree, num, SystemBaseType.MONEY, ref changed);

            var dateTime = GetOrCreateSystemBaseNode(tree, obj, SystemBaseType.DATETIME, ref changed);

            GetOrCreateSystemBaseNode(tree, dateTime, SystemBaseType.DATE, ref changed);
            GetOrCreateSystemBaseNode(tree, dateTime, SystemBaseType.TIME, ref changed);

            return changed;
        }

        /// <summary>
        /// Инициализирует недостающие предопределённые листья системных типов.
        /// </summary>
        private bool EnsureSystemBaseLeaves(WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            var boolean = tree.ContentNodes
                .OfType<SystemBaseTreeNodeModel>()
                .Single(x => x.SystemBaseType == SystemBaseType.BOOL);
            var changed = false;

            // Значения логического типа хранятся системными листьями под узлом BOOL.
            foreach (var value in SystemBaseTreeLeaveModel.GetValuesByType(SystemBaseType.BOOL))
            {
                GetOrCreateSystemBaseLeave(tree, boolean, value, ref changed);
            }

            return changed;
        }

        /// <summary>
        /// Получить существующий системный узел или создать его с предопределенными свойствами.
        /// </summary>
        /// <param name="tree">Системное рабочее дерево.</param>
        /// <param name="parent">Родительский элемент системного узла.</param>
        /// <param name="type">Системный базовый тип.</param>
        /// <param name="changed">Признак изменения дерева.</param>
        /// <returns>Системный узел.</returns>
        private SystemBaseTreeNodeModel GetOrCreateSystemBaseNode(
            WorkingTreeModel tree,
            IParentModel parent,
            SystemBaseType type,
            ref bool changed)
        {
            var existing = tree.ContentNodes
                .OfType<SystemBaseTreeNodeModel>()
                .SingleOrDefault(x => x.SystemBaseType == type);

            if (existing != null)
            {
                return existing;
            }

            changed = true;
            return new SystemBaseTreeNodeModel(
                parent,
                tree,
                type,
                _notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());
        }

        /// <summary>
        /// Получить существующий системный лист или создать его с предопределенными свойствами.
        /// </summary>
        /// <param name="tree">Системное рабочее дерево.</param>
        /// <param name="parent">Родительский системный узел.</param>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <param name="changed">Признак изменения дерева.</param>
        /// <returns>Системный лист.</returns>
        private SystemBaseTreeLeaveModel GetOrCreateSystemBaseLeave(
            WorkingTreeModel tree,
            SystemBaseTreeNodeModel parent,
            string value,
            ref bool changed)
        {
            // Ищем лист по UUID, чтобы сохранить те же правила идемпотентной инициализации, что и для узлов.
            var uuid = SystemBaseTreeLeaveModel.GetUuidByValue(parent.SystemBaseType, value);
            var existing = tree.ContentLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .SingleOrDefault(x => x.Uuid == uuid);

            if (existing != null)
            {
                existing.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));
                return existing;
            }

            changed = true;
            var result = new SystemBaseTreeLeaveModel(
                parent,
                tree,
                value,
                _notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
            result.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));

            return result;
        }

        /// <summary>
        /// Сохранить изменения и вернуть данные аудита в доменные модели
        /// </summary>
        /// <typeparam name="TMainEntityModel">Тип доменной модели</typeparam>
        /// <typeparam name="TMainEntity">Тип сущности хранилища данных</typeparam>
        /// <param name="fullCollection">Полная коллекция доменных моделей</param>
        /// <param name="state">Статус сохраняемых моделей</param>
        /// <param name="persister">Делегат сохранения сущностей в хранилище данных</param>
        /// <param name="result">Счетчик сохраненных изменений</param>
        private void SaveAndReturnAuditInfo<TMainEntityModel, TMainEntity>(
            IEnumerable<TMainEntityModel> fullCollection,
            State state,
            Func<IEnumerable<TMainEntity>, long> persister,
            ref long result)
            where TMainEntityModel : IMainEntityWritableModel
            where TMainEntity : IMainEntity
        {
            // Проверки
            ArgumentOutOfRangeException.ThrowIfEqual(state, State.SavedOrLoaded);

            // Подготовка
            var collection = fullCollection?
                .Where(x => x != null && x.State == state)
                .ToList() ?? new List<TMainEntityModel>();

            if (collection.Count == 0)
                return;

            var dbCollection = _mapper.Map<List<TMainEntity>>(collection);

            // Сохранение в хранилище
            result += persister(dbCollection);

            // Возвращение полей аудита их хранилища
            foreach (var (src, dest) in collection.Zip(dbCollection, (src, dest) => (src, dest)))
            {
                src.AuditInfo = _mapper.Map<AuditInfoModel>(dest.AuditInfo);
            }
        }

        /// <summary>
        /// Постобработка сохраненных элементов
        /// </summary>
        /// <param name="collection">Коллекция элементов</param>
        private void PostProcessSavedEntities(
            IEnumerable<IMainEntityWritableModel> collection)
        {
            var savedItems = collection
                .Where(x => x != null)
                .ToList();

            // Постобработка удаленных элементов
            var remItems = savedItems
                .Where(x => x.State == State.ForSoftDelete
                    || x.State == State.ForHardDelete
                    || x.State == State.SoftDeleted)
                .ToList();

            foreach (var item in remItems)
            {
                if (item is IContentModel c)
                {
                    c.Owner.RemoveContent(c);
                }
                if (item is IChildrenModel ch)
                {
                    ch.Parent.RemoveChild(ch);
                }
            }

            // Актуализация статуса остальных
            foreach (var item in savedItems.Where(x =>
                x.State == State.Initialized
                || x.State == State.Changed))
            {
                SetModelState(item, State.SavedOrLoaded);
            }
        }

        /// <summary>
        /// Принудительно получить содержимое репозитория из хранилища данных
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с содержимым</returns>
        private PhiladelphusRepositoryModel GetShrubContentForce(
            PhiladelphusRepositoryModel repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repository.ContentShrub);

            repository.ContentShrub.ContentWorkingTrees.Clear();
            var loadedTreeAggregates = new List<(WorkingTreeModel tree, WorkingTree dbTree)>();
            var systemTreeAggregates = new List<(IDataStorageModel storage, WorkingTree dbTree)>();
            var userTreeAggregates = new List<(IDataStorageModel storage, WorkingTree dbTree)>();

            foreach (var dataStorage in repository.DataStorages ?? Enumerable.Empty<IDataStorageModel>())
            {
                if (dataStorage.IsAvailable)
                {
                    var treesUuids = repository.ContentShrub.ContentWorkingTreesUuids?.ToArray();
                    if (treesUuids != null
                        && treesUuids.Contains(WorkingTreeModel.SystemBaseUuid) == false)
                    {
                        treesUuids = treesUuids
                            .Append(WorkingTreeModel.SystemBaseUuid)
                            .ToArray();
                    }

                    var dbTrees = dataStorage.ShrubMembersInfrastructureRepository
                        .SelectTreeAggregates(treesUuids)
                        .ToList();
                    systemTreeAggregates.AddRange(dbTrees
                        .Where(x => x.Uuid == WorkingTreeModel.SystemBaseUuid)
                        .Select(x => (dataStorage, x)));
                    userTreeAggregates.AddRange(dbTrees
                        .Where(x => x.Uuid != WorkingTreeModel.SystemBaseUuid)
                        .Select(x => (dataStorage, x)));
                }
            }

            // Системное дерево восстанавливается первым: его узлы и листья используются
            // как типы и значения при загрузке атрибутов пользовательских деревьев.
            LoadSystemWorkingTreeAggregate(
                repository,
                systemTreeAggregates,
                loadedTreeAggregates);

            // Пользовательские деревья при чтении из БД сначала создаются с пустой политикой.
            // Рабочая политика назначается сразу после добавления модели в доменный граф.
            var selectedUserTreeAggregates = userTreeAggregates
                .GroupBy(x => x.dbTree.Uuid)
                .Select(group => group.FirstOrDefault(x => x.storage.Uuid == x.dbTree.OwnDataStorageUuid).dbTree
                    ?? group.First().dbTree)
                .ToList();
            var userTrees = _mapper.MapWorkingTrees(
                selectedUserTreeAggregates,
                repository.DataStorages,
                repository.ContentShrub,
                _notificationService,
                new EmptyPropertiesPolicy<WorkingTreeModel>());
            var userDbTreesByUuid = selectedUserTreeAggregates.ToDictionary(x => x.Uuid);

            foreach (var tree in userTrees)
            {
                repository.ContentShrub.ContentWorkingTrees.Add(tree);
                tree.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateWorkingTreeDefault(_notificationService));

                if (userDbTreesByUuid.TryGetValue(tree.Uuid, out var dbTree))
                {
                    var dbRoots = dbTree.ContentRoot == null
                        ? Array.Empty<TreeRoot>()
                        : new[] { dbTree.ContentRoot };
                    DistributeWorkingTreeContentWithoutAttributes(
                        tree,
                        dbRoots,
                        dbTree.ContentNodes.ToList(),
                        dbTree.ContentLeaves.ToList());
                    loadedTreeAggregates.Add((tree, dbTree));
                }
            }

            // Получение атрибутов всех элементов деревьев из БД
            var allShrubNodesByUuid = repository.ContentShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentNodes ?? Enumerable.Empty<TreeNodeModel>())
                .ToDictionary(x => x.Uuid);
            var allShrubLeavesByUuid = repository.ContentShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentLeaves ?? Enumerable.Empty<TreeLeaveModel>())
                .ToDictionary(x => x.Uuid);

            foreach (var (tree, dbTree) in loadedTreeAggregates)
            {
                DistributeWorkingTreeAttributes(
                    tree,
                    dbTree,
                    allShrubNodesByUuid,
                    allShrubLeavesByUuid);
            }

            InitSystemWorkingTree(repository.ContentShrub);

            SetModelState(repository, State.SavedOrLoaded);

            return repository;
        }

        /// <summary>
        /// Загружает единый системный каркас из первого найденного агрегата и объединяет системные листья из всех хранилищ.
        /// </summary>
        private void LoadSystemWorkingTreeAggregate(
            PhiladelphusRepositoryModel repository,
            IReadOnlyCollection<(IDataStorageModel storage, WorkingTree dbTree)> systemTreeAggregates,
            ICollection<(WorkingTreeModel tree, WorkingTree dbTree)> loadedTreeAggregates)
        {
            if (systemTreeAggregates.Count == 0)
                return;

            var mainDataStorage = GetMainDataStorage(repository.ContentShrub);
            var firstAggregate = systemTreeAggregates.First().dbTree;
            var mainAggregate = systemTreeAggregates
                .FirstOrDefault(x => x.storage.Uuid == mainDataStorage.Uuid)
                .dbTree;
            var mainNodeUuids = mainAggregate?.ContentNodes
                .Select(x => x.Uuid)
                .ToHashSet() ?? new HashSet<Guid>();
            var allSystemLeaves = systemTreeAggregates
                .SelectMany(x => x.dbTree.ContentLeaves.Select(leave => (x.storage, leave)))
                .DistinctBy(x => x.leave.Uuid)
                .ToList();
            var loadedSystemLeaveUuids = allSystemLeaves
                .Select(x => x.leave.Uuid)
                .ToHashSet();

            // Дерево, корень и узлы берутся только из первой найденной копии.
            // Листья добавляются из остальных хранилищ после восстановления системных узлов.
            firstAggregate.OwnDataStorageUuid = mainDataStorage.Uuid;

            var systemTree = _mapper.MapWorkingTrees(
                    new[] { firstAggregate },
                    repository.DataStorages,
                    repository.ContentShrub,
                    _notificationService,
                    new EmptyPropertiesPolicy<WorkingTreeModel>())
                .Single();
            repository.ContentShrub.ContentWorkingTrees.Add(systemTree);
            if (repository.ContentShrub.ContentWorkingTreesUuids.Contains(systemTree.Uuid) == false)
                repository.ContentShrub.ContentWorkingTreesUuids.Add(systemTree.Uuid);
            systemTree.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateWorkingTreeDefault(_notificationService));

            var dbRoots = firstAggregate.ContentRoot == null
                ? Array.Empty<TreeRoot>()
                : new[] { firstAggregate.ContentRoot };
            DistributeWorkingTreeContentWithoutAttributes(
                systemTree,
                dbRoots,
                firstAggregate.ContentNodes.ToList(),
                Array.Empty<TreeLeave>());
            EnsureSystemBaseNodes(systemTree);

            foreach (var (storage, dbLeave) in allSystemLeaves)
            {
                var loadedLeave = _mapper.MapTreeLeaves(
                        new[] { dbLeave },
                        systemTree.ContentNodes,
                        systemTree,
                        _notificationService,
                        new EmptyPropertiesPolicy<TreeLeaveModel>(),
                        storage)
                    .SingleOrDefault();
                if (loadedLeave == null)
                    continue;

                loadedLeave.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));
                SetModelState(loadedLeave, State.SavedOrLoaded);
            }
            EnsureSystemBaseLeaves(systemTree);
            firstAggregate.ContentLeaves = allSystemLeaves.Select(x => x.leave).ToList();

            if (mainAggregate == null)
            {
                SetSystemWorkingTreeState(systemTree, State.Initialized);
                foreach (var leave in systemTree.ContentLeaves.Where(x => loadedSystemLeaveUuids.Contains(x.Uuid)))
                    SetModelState(leave, State.SavedOrLoaded);
            }
            else
            {
                if (systemTree.ContentRoot != null)
                {
                    SetModelState(
                        systemTree.ContentRoot,
                        mainAggregate.ContentRoot == null ? State.Initialized : State.SavedOrLoaded);
                }
                foreach (var node in systemTree.ContentNodes)
                {
                    SetModelState(
                        node,
                        mainNodeUuids.Contains(node.Uuid) ? State.SavedOrLoaded : State.Initialized);
                }
                foreach (var leave in systemTree.ContentLeaves)
                {
                    SetModelState(
                        leave,
                        loadedSystemLeaveUuids.Contains(leave.Uuid) ? State.SavedOrLoaded : State.Initialized);
                }
            }

            loadedTreeAggregates.Add((systemTree, firstAggregate));
        }

        /// <summary>
        /// Распределить содержимое рабочего дерева без атрибутов по доменной модели
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="dbRoots">Сущности корней рабочего дерева</param>
        /// <param name="dbNodes">Сущности узлов рабочего дерева</param>
        /// <param name="dbLeaves">Сущности листьев рабочего дерева</param>
        private void DistributeWorkingTreeContentWithoutAttributes(
            WorkingTreeModel tree,
            IReadOnlyCollection<TreeRoot> dbRoots,
            IReadOnlyCollection<TreeNode> dbNodes,
            IReadOnlyCollection<TreeLeave> dbLeaves)
        {
            if (dbRoots.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Рабочее дерево '{tree.Name}' [{tree.Uuid}] должно иметь ровно один корень. Получено: {dbRoots.Count}.");
            }

            var dbRoot = dbRoots.Single();
            // Корень, узлы и листья также мапятся с EmptyPropertiesPolicy. Иначе новые правила могли бы
            // заблокировать присвоение сохраненных значений во время восстановления объекта из БД.
            var root = _mapper.MapTreeRoot(dbRoot, tree, _notificationService, new EmptyPropertiesPolicy<TreeRootModel>());

            if (root == null)
                return;

            // С этого момента корень уже находится в графе дерева, поэтому можно возвращать рабочую политику.
            root.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeRootDefault(_notificationService));

            var nodes = _mapper.MapTreeNodes(dbNodes, new[] { tree.ContentRoot }, tree.ContentRoot.OwningWorkingTree, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            var allNodes = new List<TreeNodeModel>();
            while (nodes.Any())
            {
                // Узлы загружаются уровнями: сначала дети корня, затем дети уже найденных узлов.
                // Политика назначается каждому загруженному уровню до перехода к следующему.
                nodes.ToList().ForEach(x => x.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeNodeDefault(_notificationService)));
                allNodes.AddRange(nodes);
                nodes = _mapper.MapTreeNodes(dbNodes, nodes, tree.ContentRoot.OwningWorkingTree, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            }

            var allLeaves = _mapper.MapTreeLeaves(
                dbLeaves,
                allNodes,
                tree,
                _notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>(),
                tree.DataStorage);
            // Листья загружаются после всех узлов, поэтому к моменту назначения политики их родители уже известны.
            allLeaves.ToList().ForEach(x => x.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService)));

            // Обновление статусов
            SetModelState(tree, State.SavedOrLoaded);
            SetModelState(tree.ContentRoot, State.SavedOrLoaded);
            tree.ContentNodes.ToList().ForEach(x => SetModelState(x, State.SavedOrLoaded));
            tree.ContentLeaves.ToList().ForEach(x => SetModelState(x, State.SavedOrLoaded));

            // Регистрация имен
            tree.UnavailableNames.Add(tree.Name);
            tree.UnavailableNames.Add(tree.ContentRoot.Name);
            tree.ContentNodes.ToList().ForEach(x => tree.UnavailableNames.Add(x.Name));
            tree.ContentLeaves.ToList().ForEach(x => tree.UnavailableNames.Add(x.Name));
        }

        /// <summary>
        /// Распределить атрибуты рабочего дерева из агрегата.
        /// </summary>
        /// <param name="tree">Рабочее дерево.</param>
        /// <param name="dbTree">Агрегат рабочего дерева из хранилища данных.</param>
        private void DistributeWorkingTreeAttributes(
            WorkingTreeModel tree,
            WorkingTree dbTree,
            IReadOnlyDictionary<Guid, TreeNodeModel> allShrubNodesByUuid,
            IReadOnlyDictionary<Guid, TreeLeaveModel> allShrubLeavesByUuid)
        {
            // Формирование списка владельцев атрибутов
            var owners = new List<IAttributeOwnerModel>();
            owners.Add(tree);
            if (tree.ContentRoot != null)
            {
                owners.Add(tree.ContentRoot);
            }

            owners.AddRange(tree.ContentNodes ?? Enumerable.Empty<TreeNodeModel>());
            owners.AddRange(tree.ContentLeaves ?? Enumerable.Empty<TreeLeaveModel>());

            owners.ForEach(x => x.SuspendAttributesListRecalculation());
            try
            {
                var allAttributes = _mapper.MapAttributes(dbTree.ContentAttributes.ToList(), owners, allShrubNodesByUuid, tree, _notificationService, AttributePolicyBuilder.CreateDefault(_notificationService));
            }
            finally
            {
                foreach (var owner in owners.AsEnumerable().Reverse())
                {
                    owner.ResumeAttributesListRecalculation();
                }
            }

            // Обновление статусов
            tree.ContentAttributes.ToList().ForEach(x => SetModelState(x, State.SavedOrLoaded));

            // Регистрация имен
            tree.ContentAttributes.ToList().ForEach(x => tree.UnavailableNames.Add(x.Name));
        }

        /// <summary>
        /// Проверить, есть ли у атрибута унаследованные копии у дочерних элементов.
        /// </summary>
        private bool HasInheritedAttributesInDescendants(ElementAttributeModel attribute)
        {
            if (attribute.Owner is not IParentModel parent)
                return false;

            return parent.Childs.Values.Any(x => HasInheritedAttributeInBranch(x, attribute.DeclaringUuid));
        }

        /// <summary>
        /// Проверить ветку наследования на наличие унаследованной копии атрибута.
        /// </summary>
        private bool HasInheritedAttributeInBranch(IChildrenModel child, Guid declaringUuid)
        {
            if (child is IAttributeOwnerModel attributeOwner
                && attributeOwner.Attributes.Any(x => x.IsOwn == false && x.DeclaringUuid == declaringUuid))
            {
                return true;
            }

            return child is IParentModel parent
                && parent.Childs.Values.Any(x => HasInheritedAttributeInBranch(x, declaringUuid));
        }

        /// <summary>
        /// Проверяет поддержку сущностей кустарника всеми указанными хранилищами.
        /// </summary>
        /// <exception cref="InvalidOperationException">Если хранилище не поддерживает сущности кустарника.</exception>
        private static void EnsureShrubMembersStorageSupport(IEnumerable<IDataStorageModel> dataStorages)
        {
            var unsupportedStorage = dataStorages
                .DistinctBy(x => x.Uuid)
                .FirstOrDefault(x => x.HasShrubMembersInfrastructureRepository == false);
            if (unsupportedStorage == null)
                return;

            throw new InvalidOperationException(
                $"Хранилище '{unsupportedStorage.Name}' [{unsupportedStorage.Uuid}] не поддерживает сохранение элементов кустарника.");
        }

        /// <summary>
        /// Назначает основное хранилище всем системным рабочим деревьям перед сохранением.
        /// </summary>
        private static void EnsureSystemWorkingTreeUsesMainDataStorage(
            IEnumerable<WorkingTreeModel> workingTrees)
        {
            foreach (var systemTree in workingTrees.Where(x => x.Uuid == WorkingTreeModel.SystemBaseUuid))
            {
                systemTree.ChangeDataStorage(GetMainDataStorage(systemTree.OwningShrub));
            }
        }

        /// <summary>
        /// Возвращает основное хранилище приложения из списка доступных хранилищ репозитория.
        /// </summary>
        private static IDataStorageModel GetMainDataStorage(ShrubModel shrub)
        {
            return shrub.OwningRepository.DataStorages
                .SingleOrDefault(x => x.Uuid == DataStorageModel.MainDataStorageUuid)
                ?? throw new InvalidOperationException(
                    "Основное хранилище данных не входит в список доступных хранилищ репозитория.");
        }

        /// <summary>
        /// Назначает состояние всему системному дереву, включая корень, узлы, листья и атрибуты.
        /// </summary>
        private void SetSystemWorkingTreeState(WorkingTreeModel tree, State state)
        {
            SetModelState(tree, state);
            if (tree.ContentRoot != null)
                SetModelState(tree.ContentRoot, state);
            tree.ContentNodes.ToList().ForEach(x => SetModelState(x, state));
            tree.ContentLeaves.ToList().ForEach(x => SetModelState(x, state));
            tree.ContentAttributes.ToList().ForEach(x => SetModelState(x, state));
        }

        #endregion
    }
}
