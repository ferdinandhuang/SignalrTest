using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
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
        private static readonly ConcurrentDictionary<string, string> _guess = new ConcurrentDictionary<string, string>();
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            Cookie userNameCookie;
            if (request.Cookies.TryGetValue("userName", out userNameCookie) &&
                userNameCookie != null)
            {
                _clients[connectionId] = userNameCookie.Value;
                _users[userNameCookie.Value] = connectionId;
            }

            string user = GetUser(connectionId);

            return Connection.Send(connectionId, "Welcome!");
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            var message = JsonConvert.DeserializeObject<Message>(data);

            switch (message.Type)
            {
                case MessageType.Set:
                    _guess.AddOrUpdate(connectionId, message.Value, (key, oldValue) => oldValue = message.Value);
                    Connection.Broadcast(new
                    {
                        type = message.Type.ToString(),
                        fromUser = GetUser(connectionId),
                        fromClient = connectionId
                    });
                    break;
                case MessageType.Guess:
                    string answer = GetAnswer(connectionId);
                    Connection.Broadcast(new
                    {
                        type = message.Type.ToString(),
                        fromUser = GetUser(connectionId),
                        fromClient = connectionId,
                        data = message.Value,
                        correct = (message.Value == answer) ? "正确" : "错误"
                    });
                    break;
                default:
                    break;
            }

            return base.OnReceived(request, connectionId, data);
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
        /// 连接类型
        /// </summary>
        enum MessageType
        {
            Set,
            Guess
        }
        private string GetUser(string connectionId)
        {
            string user;
            if (!_clients.TryGetValue(connectionId, out user))
            {
                return connectionId;
            }
            return user;
        }

        private string GetClient(string user)
        {
            string connectionId;
            if (_users.TryGetValue(user, out connectionId))
            {
                return connectionId;
            }
            return null;
        }
        private string GetAnswer(string connectionId)
        {
            string answer="";

            foreach (var item in _guess)
            {
                if(item.Key!= connectionId)
                {
                    answer = item.Value;
                    break;
                }
            }

            return answer;
        }

        class Message
        {
            public MessageType Type { get; set; }
            public string Value { get; set; }
        }

        //private static string GetClientIP(IRequest request)
        //{
        //    return Get<string>(request.Environment, "server.RemoteIpAddress");
        //}

        //private static T Get<T>(IDictionary<string, object> env, string key)
        //{
        //    object value;
        //    return env.TryGetValue(key, out value) ? (T)value : default(T);
        //}
    }
}