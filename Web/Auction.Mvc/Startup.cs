using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Auction.Mvc.Startup))]
namespace Auction.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
