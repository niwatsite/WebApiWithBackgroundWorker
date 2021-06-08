using System;
using System.Collections.Generic;
using WebApiWithBackgroundWorker.Common.Messaging;
using WebApiWithBackgroundWorker.Subscriber.Providers.Events;

namespace WebApiWithBackgroundWorker.Subscriber.Messaging
{
    public interface IMessagesRepository
    {
        event EventHandler<MessageArgs> MessagesEvent;
        void Broadcast(Message message);
    }
}