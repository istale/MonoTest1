using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MonoTest1.Startup))]
namespace MonoTest1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
