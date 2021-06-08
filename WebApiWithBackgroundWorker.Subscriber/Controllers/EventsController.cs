using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApiWithBackgroundWorker.Common.Messaging;
using WebApiWithBackgroundWorker.Subscriber.Messaging;
using WebApiWithBackgroundWorker.Subscriber.Providers.Events;

namespace WebApiWithBackgroundWorker.Subscriber.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IMessagesRepository _messageRepository;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger, IMessagesRepository messageRepository)
        {
            _logger = logger;
            _messageRepository = messageRepository;
        }

        /// <summary>
        /// Produce SSE
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Produces("text/event-stream")]
        [HttpGet]
        public async Task SubscribeEvents(CancellationToken cancellationToken)
        {
            SetServerSentEventHeaders();
            // On connect, welcome message ;)
            var data = new { Message = "connected!" };
            var jsonConnection = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            await Response.WriteAsync($"event:connection\n", cancellationToken);
            await Response.WriteAsync($"data: {jsonConnection}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            async void OnNotification(object? sender, MessageArgs eventArgs)
            {
                try
                {
                    // idea: https://stackoverflow.com/a/58565850/80527
                    var json = JsonSerializer.Serialize(eventArgs.Message, _jsonSerializerOptions);
                    await Response.WriteAsync($"id:{eventArgs.Message.Id}\n", cancellationToken);
                    await Response.WriteAsync("retry: 10000\n", cancellationToken);
                    await Response.WriteAsync($"event:snackbar\n", cancellationToken);
                    await Response.WriteAsync($"data:{json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception)
                {
                    _logger.LogError("Not able to send the notification");
                }
            }

            _messageRepository.MessagesEvent += OnNotification;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Spin until something break or stop...
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug("User most likely disconnected");
            }
            finally
            {
                _messageRepository.MessagesEvent -= OnNotification;
            }
        }

        private void SetServerSentEventHeaders()
        {
            Response.StatusCode = 200;
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
        }

        [HttpPost("broadcast")]
        public Task Broadcast([FromBody] string Text)
        {
            var notification = new Message();
            notification.Text = Text;
            notification.Id = Guid.NewGuid();
            notification.CreationDate = DateTime.UtcNow;
            _messageRepository.Broadcast(notification);
            return Task.CompletedTask;
        }
    }
}
