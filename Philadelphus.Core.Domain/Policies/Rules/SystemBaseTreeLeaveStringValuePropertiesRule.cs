using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    internal class SystemBaseTreeLeaveStringValuePropertiesRule : IPropertiesRule<TreeLeaveModel>
    {
        private readonly INotificationService _notificationService;

        public SystemBaseTreeLeaveStringValuePropertiesRule(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public bool CanRead(TreeLeaveModel model, string prop)
        {
            return true;
        }

        public bool CanWrite(TreeLeaveModel model, string prop, object value)
        {
            if (prop != nameof(SystemBaseTreeLeaveModel.StringValue)
                || model is not SystemBaseTreeLeaveModel systemBaseLeave)
            {
                return true;
            }

            var stringValue = value as string;
            var isValid = SystemBaseStringValueValidator.IsValid(systemBaseLeave.SystemBaseType, stringValue, out var expectedFormat);

            if (isValid)
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseTreeLeaveStringValuePropertiesRule>(
                $"Для системного листа '{model.Name}' [{model.Uuid}] значение '{stringValue ?? "<null>"}' " +
                $"не соответствует системному типу '{systemBaseLeave.SystemBaseType}'. Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        public object OnRead(TreeLeaveModel model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(TreeLeaveModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
