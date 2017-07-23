using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalrTest.App_Code
{
    public class MyConnection : PersistentConnection
    {
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> _clients = new ConcurrentDictionary<string, string>();
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            Cookie userNameCookie;
            if (request.Cookies.TryGetValue("user", out userNameCookie) &&
                userNameCookie != null)
            {
                _clients[connectionId] = userNameCookie.Value;
                _users[userNameCookie.Value] = connectionId;
            }

            string clientIp = GetClientIP(request);

            string user = GetUser(connectionId);

            return Connection.Send(connectionId, "Welcome!");
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Send(connectionId, data);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            Debug.WriteLine("OnDisconnected");
            return base.OnDisconnected(request, connectionId, stopCalled);
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            Debug.WriteLine("OnReconnected");
            return base.OnReconnected(request, connectionId);
        }

        /// <summary>
        /// 通过connectionId获取用户名
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private string GetUser(string connectionId)
        {
            string user;
            if (!_clients.TryGetValue(connectionId, out user))
            {
                return connectionId;
            }
            return user;
        }

        /// <summary>
        /// 通过user获取connectionId
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GetClient(string user)
        {
            string connectionId;
            if (_users.TryGetValue(user, out connectionId))
            {
                return connectionId;
            }
            return null;
        }

        /// <summary>
        /// 连接类型
        /// </summary>
        enum MessageType
        {
            Send,
            Broadcast,
            Join,
            PrivateMessage,
            AddToGroup,
            RemoveFromGroup,
            SendToGroup,
            BroadcastExceptMe,
        }
        class Message
        {
            public MessageType Type { get; set; }
            public string Value { get; set; }
        }

        private static string GetClientIP(IRequest request)
        {
            return Get<string>(request.Environment, "server.RemoteIpAddress");
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}