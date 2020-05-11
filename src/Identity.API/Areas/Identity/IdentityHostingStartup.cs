using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Identity.API.Areas.Identity.IdentityHostingStartup))]
namespace Identity.API.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { });
        }
    }
}