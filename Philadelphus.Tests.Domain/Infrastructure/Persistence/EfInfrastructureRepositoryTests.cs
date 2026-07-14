using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using Serilog;

namespace Philadelphus.Tests.Domain.Infrastructure.Persistence;

public class EfInfrastructureRepositoryTests
{
    [Fact]
    public void UpdateAndSoftDelete_MissingEntity_SkipDatabaseChanges()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"philadelphus-{Guid.CreateVersion7()}.sqlite");
        var connectionString = $"Data Source={databasePath};Pooling=False";

        try
        {
            var repository = new SqliteEfShrubMembersInfrastructureRepository(
                Mock.Of<ILogger>(),
                connectionString,
                "test-user");
            var missingRoot = new TreeRoot
            {
                Uuid = Guid.CreateVersion7(),
                Name = "Несохраненный корень",
                OwningWorkingTreeUuid = Guid.CreateVersion7(),
                AuditInfo = new AuditInfo
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "test-user"
                }
            };

            var update = () => repository.UpdateRoots(new[] { missingRoot });
            var softDelete = () => repository.SoftDeleteRoots(new[] { missingRoot });

            update.Should().NotThrow().Which.Should().Be(0);
            softDelete.Should().NotThrow().Which.Should().Be(0);
            using var context = new SqliteEfShrubMembersContext(connectionString);
            context.TreeRoots
                .AsNoTracking()
                .Any(x => x.Uuid == missingRoot.Uuid)
                .Should().BeFalse();
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public void UpdateAndSoftDelete_MixedEntities_ProcessOnlyExistingEntity()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"philadelphus-{Guid.CreateVersion7()}.sqlite");
        var connectionString = $"Data Source={databasePath};Pooling=False";

        try
        {
            var repository = new SqliteEfShrubMembersInfrastructureRepository(
                Mock.Of<ILogger>(),
                connectionString,
                "test-user");
            var existingTree = CreateWorkingTree("Существующее дерево");
            var missingTree = CreateWorkingTree("Отсутствующее дерево");
            repository.InsertTrees(new[] { existingTree }).Should().BeGreaterThan(0);
            existingTree.Name = "Измененное дерево";

            var update = () => repository.UpdateTrees(new[] { existingTree, missingTree });
            var softDelete = () => repository.SoftDeleteTrees(new[] { existingTree, missingTree });

            update.Should().NotThrow().Which.Should().BeGreaterThan(0);
            softDelete.Should().NotThrow().Which.Should().BeGreaterThan(0);
            using var context = new SqliteEfShrubMembersContext(connectionString);
            var savedTree = context.WorkingTrees
                .AsNoTracking()
                .Single(x => x.Uuid == existingTree.Uuid);
            savedTree.Name.Should().Be("Измененное дерево");
            savedTree.AuditInfo.IsDeleted.Should().BeTrue();
            context.WorkingTrees
                .AsNoTracking()
                .Any(x => x.Uuid == missingTree.Uuid)
                .Should().BeFalse();
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    private static WorkingTree CreateWorkingTree(string name)
    {
        return new WorkingTree
        {
            Uuid = Guid.CreateVersion7(),
            Name = name,
            OwnDataStorageUuid = Guid.CreateVersion7(),
            AuditInfo = new AuditInfo
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test-user"
            }
        };
    }
}
