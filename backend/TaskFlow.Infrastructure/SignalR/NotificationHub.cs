using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Infrastructure.SignalR;

/// <summary>The browser only listens: command handlers push "ReceiveNotification" messages to the user.</summary>
[Authorize]
public sealed class NotificationHub : Hub;
