﻿using System.Threading.Tasks;

namespace Codeworx.Identity.Notification
{
    public interface INotificationProcessor
    {
        Task SendNotificationAsync(INotification notification);
    }
}
