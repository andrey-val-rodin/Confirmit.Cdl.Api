//==== DO NOT MODIFY THIS FILE ====
using System;
using Confirmit.IPLockdown;
using Confirmit.NetCore.Api.Extensions;
using Confirmit.NetCore.Authorization;
using Confirmit.NetCore.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Confirmit.Cdl.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddControllers(config =>
                    {
                        config.ConfigureConfirmitMvcOptions(Configuration);
                        config.ConfigureLocalMvcOptions();
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    .AddNewtonsoftJson(config =>
                    {
                        config.SerializerSettings.AdjustToConfirmitSettings();
                        config.ConfigureLocalMvcJsonOptions();
                    })
                    .AddLocalMvc(Configuration);

                services.AddApiServices(Configuration.GetValue<string>("Confirmit:ApplicationName"));
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = ConfirmitAuthorization.CreateDefaultPolicy(Configuration);
                    options.InvokeHandlersAfterFailure = false;
                });
                services.AddLocalServices(Configuration);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder builder, IWebHostEnvironment env)
        {
            try
            {
                builder.UseMiddleware<IPLockdownMiddleware>();
                builder.UseRouting();
                builder.UseApiConfiguration("api/cdl", Configuration.GetValue<string>("Confirmit:ApplicationName"));
                builder.UseAuthorization();
                builder.UseLocalConfiguration(env);
                if (env.IsDevelopment())
                    builder.UseDeveloperExceptionPage();
                builder.UseEndpoints(config =>
                {
                    config.MapControllers().RequireAuthorization();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
