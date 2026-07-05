using Philadelphus.Core.Domain.Identity.Services.Interfaces;
using Philadelphus.Core.Domain.Identity.Entities;

namespace Philadelphus.Tests.Common.Fakes.Services
{
    public class FakeUserService : IUserService
    {
        public User CurrentUser { get; set; } = new(
            Guid.CreateVersion7(),
            "test-user",
            "TEST\\test-user");
    }
}
