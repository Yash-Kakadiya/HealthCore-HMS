using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HMS.Hubs // Make sure this namespace is correct
{
    public class ChatHub : Hub
    {
        // A static (shared) list to track online users.
        // Key: ConnectionId, Value: UserName
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        /// <summary>
        /// Called by the client right after connecting to register their username.
        /// </summary>
        public async Task RegisterUser(string username)
        {
            // Add the new user to our tracking list
            OnlineUsers.TryAdd(Context.ConnectionId, username);

            // Send a "system message" to everyone that a new user joined
            await Clients.All.SendAsync(
                "ReceiveSystemMessage",
                $"{username} has joined the chat.",
                "joined"
            );

            // Send the updated list of all online users to EVERYONE
            await SendUserListUpdate();
        }

        /// <summary>
        /// Sends a chat message from a user to all clients.
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync(
                "ReceiveMessage",
                user,
                message,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// This automatically runs when a user disconnects (e.g., closes tab).
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Try to remove the user from our tracking list
            if (OnlineUsers.TryRemove(Context.ConnectionId, out string? username))
            {
                // Send a "system message" that the user left
                await Clients.All.SendAsync(
                    "ReceiveSystemMessage",
                    $"{username} has left the chat.",
                    "left"
                );

                // Send the newly updated user list to everyone
                await SendUserListUpdate();
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Helper method to broadcast the current online user list to all clients.
        /// </summary>
        private Task SendUserListUpdate()
        {
            var userList = OnlineUsers.Values.Distinct().OrderBy(u => u);
            return Clients.All.SendAsync("UpdateUserList", userList);
        }
        public async Task NotifyStartTyping(string username)
        {
            // Notify all clients EXCEPT the one who is typing
            await Clients.Others.SendAsync("UserIsTyping", username);
        }

        public async Task NotifyStopTyping(string username)
        {
            // Notify all clients EXCEPT the one who stopped typing
            await Clients.Others.SendAsync("UserStoppedTyping", username);
        }
    }
}