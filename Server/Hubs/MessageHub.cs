using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Hubs
{
    public class MessageHub : Hub<IMessageClient>
    {
        private readonly ILogger<MessageHub> logger;

        public MessageHub(ILogger<MessageHub> logger)
        {
            this.logger = logger;
        }

        public Task SendToOthers(Message message)
        {
            var messageForClient = NewMessage.Create(Context.Items["Name"] as string, message);
            return Clients.Others.Send(messageForClient);
        }

        public Task SetMyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.CompletedTask;

            if (Context.Items.ContainsKey("Name"))
                Context.Items["Name"] = name;
            else
                Context.Items.Add("Name", name);

            logger.LogInformation("Client with ConnectionId {ConnectionId} changed name to {Name}", Context.ConnectionId, name);

            return Task.CompletedTask;
        }

        public Task<string> GetMyName()
        {
            if (Context.Items.ContainsKey("Name"))
                return Task.FromResult(Context.Items["Name"] as string);

            return Task.FromResult("Anonymous");
        }
    }
}
