using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BankingEnterpriseSystem.Startup))]
namespace BankingEnterpriseSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
