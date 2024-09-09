using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DonationWall.Startup))]
namespace DonationWall
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
           
        }
    }
}
