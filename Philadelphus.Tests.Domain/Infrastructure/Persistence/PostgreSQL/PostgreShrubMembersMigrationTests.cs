using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;

namespace Philadelphus.Tests.Domain.Infrastructure.Persistence.PostgreSQL
{
    public class PostgreShrubMembersMigrationTests
    {
        [Fact]
        public void GetMigrations_IncludesTreeLeavesStringValueMigration()
        {
            using var context = new PostgreEfShrubMembersContext(
                "Host=localhost;Database=philadelphus_test;Username=philadelphus;Password=philadelphus");

            context.Database.GetMigrations().Should().Contain("20260522090000_AddTreeLeaveStringValue");
        }
    }
}
