using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Signum.Entities.Basics;
using Signum.React.Alerts;
using Signum.React.Authorization;
using System.Threading.Tasks;

namespace Signum.React.Alerts;

public interface IAlertsClient
{
    Task AlertsChanged();
}

public class AlertsHub : Hub<IAlertsClient>
{
    public Task Login(string tokenString)
    {
        var token = AuthTokenServer.DeserializeToken(tokenString);

        AlertsServer.Connections.Add(token.User.ToLite(), Context.ConnectionId);

        return Task.CompletedTask;
    }

    public Task Logout(string tokenString)
    {
        AlertsServer.Connections.Remove(Context.ConnectionId);

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        AlertsServer.Connections.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

}

public class ConnectionMapping<T> where T : class
{
    private readonly Dictionary<T, HashSet<string>> userToConnection = new Dictionary<T, HashSet<string>>();
    private readonly Dictionary<string, T> connectionToUser = new Dictionary<string, T>();

    public int Count => userToConnection.Count;

    public void Add(T key, string connectionId)
    {
        lock (this)
        {
            HashSet<string>? connections;
            if (!userToConnection.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                userToConnection.Add(key, connections);
            }

            connections.Add(connectionId);

            connectionToUser.Add(connectionId, key);
        }
    }

    public IEnumerable<string> GetConnections(T key) => userToConnection.TryGetC(key) ?? Enumerable.Empty<string>();

    public void Remove(string connectionId)
    {
        lock (this)
        {
            var user = connectionToUser.TryGetC(connectionId);
            if (user != null)
            {
                HashSet<string>? connections = userToConnection.TryGetC(user);
                if (connections != null)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                        userToConnection.Remove(user);
                }

                connectionToUser.Remove(connectionId);
            }
        }
    }
}

