using System.Threading;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;

namespace SignalrTest.App_Code
{
    [HubName("MyHub")]
    public class MyHub : Hub
    {

        #region Data structures

        public class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Line
        {
            public Point From { get; set; }
            public Point To { get; set; }
            public string Color { get; set; }
        }
        #endregion

        private static long _id;
        // defines some colors
        private readonly static string[] colors = new string[]{
            "red", "green", "blue", "orange", "navy", "silver", "black", "lime"
        };

        public void Join()
        {
            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
        }

        // A user has drawed a line ...
        [HubMethodName("Draw")]
        public void Draw(Line data)
        {
            // ... propagate it to all users
            Clients.Others.draw(data);
        }
    }
}