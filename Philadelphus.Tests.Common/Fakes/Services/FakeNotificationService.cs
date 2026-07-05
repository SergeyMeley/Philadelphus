using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Runtime.CompilerServices;

namespace Philadelphus.Tests.Common.Fakes.Services
{
#pragma warning disable CS0067
    public class FakeNotificationService : INotificationService
    {
        public List<string> Messages { get; } = new();

        public NotificationHandler TextMessageHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NotificationHandler ModalWindowHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NotificationHandler PopUpWindowHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NotificationHandler EmailHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NotificationHandler SmsHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NotificationHandler CallHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IReadOnlyList<Notification> NotificationsHistory => throw new NotImplementedException();

        public int HistoryCapacity { get; set; }

        public event Action<Notification>? HistoryUpdated;

        public bool SendCall<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendEmail<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendModalWindow<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendNotification<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel,
            NotificationTransmissionType transmissionType,
            NotificationTypesModel type = NotificationTypesModel.TextMessage,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendPopUpWindow<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendSms<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }

        public bool SendTextMessage<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null!,
            [CallerFilePath] string file = null!)
        {
            Messages.Add(text);
            return true;
        }
    }
#pragma warning restore CS0067
}
