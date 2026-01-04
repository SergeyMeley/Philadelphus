using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Interfaces
{
    internal interface IMainEntityWritableModel : IMainEntityModel
    {
        internal bool SetState(State newState);
    }
}
