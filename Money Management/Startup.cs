using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Money_Management.Startup))]
namespace Money_Management
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
