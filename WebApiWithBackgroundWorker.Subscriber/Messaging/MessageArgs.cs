using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiWithBackgroundWorker.Common.Messaging;

namespace WebApiWithBackgroundWorker.Subscriber.Providers.Events
{
    public class MessageArgs : EventArgs
    {
        public Message Message { get; }

        public MessageArgs(Message message)
        {
            Message = message;
        }
    }

}
