using System;
using System.Collections.Generic;
using WebApiWithBackgroundWorker.Common.Messaging;
using WebApiWithBackgroundWorker.Subscriber.Providers.Events;

namespace WebApiWithBackgroundWorker.Subscriber.Messaging
{
    public class SSEMessagesRepository : IMessagesRepository
    {

        public event EventHandler<MessageArgs> MessagesEvent;

        public void Broadcast(Message message)
        {
            MessagesEvent?.Invoke(this, new MessageArgs(message));
        }
    }
}
