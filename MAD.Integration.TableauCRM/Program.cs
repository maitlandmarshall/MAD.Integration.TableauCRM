using MAD.Integration.TableauCRM;
using Microsoft.Extensions.Hosting;
using MIFCore;
using MIFCore.Http;

IntegrationHost.CreateDefaultBuilder()
    .UseStartup<Startup>()
    .UseAspNetCore()
    .Build()
    .Run();