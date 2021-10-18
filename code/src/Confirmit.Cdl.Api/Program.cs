//==== DO NOT MODIFY THIS FILE ====
using System;
using System.Threading.Tasks;
using Confirmit.NetCore.Api.Extensions;
using Confirmit.NetCore.Client.ServiceResolving;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Confirmit.Cdl.Api
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .MinimumLevel.Override("Confirmit", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                DatabaseConfiguration.ConfigureDatabase();
                Log.Information("Starting web host");
                var uriResolver = await UriResolverFactory.CreateServiceUriResolver("Confirmit.Cdl.Api");
                Host
                    .CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(configure => configure.UseApiHostConfiguration<Startup>(uriResolver))
                    .Build()
                    .Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
