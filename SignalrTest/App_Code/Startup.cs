using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Test2.App_Code;

[assembly: OwinStartup(typeof(SignalrTest.App_Code.Startup))]

namespace SignalrTest.App_Code
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR<MyConnection>("/my-connection");
        }
    }
}
