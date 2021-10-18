using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Accounts.Clients;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Authorization.Clients;
using Confirmit.Cdl.Api.Database;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.NetCore.Authorization;
using Confirmit.NetCore.Client;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using IMetadata = Confirmit.Cdl.Api.Authorization.Clients.IMetadata;

namespace Confirmit.Cdl.Api
{
    public partial class StartupExtensions
    {
        static partial void AddLocalServicesImpl(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfirmitAuthorizationServices(); // deny not authorized users.
            services.AddConfirmitAuthorization(options =>
            {
                options.AddPolicy("DefaultAccessPolicy", ConfirmitAuthorization.CreateDefaultPolicy(configuration));
            });

            services.AddMvc(options => { options.Filters.Add(typeof(RefreshUserFilter)); });

            services.AddSingleton<ICmlStorageDatabase, CmlStorageDatabase>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddDbContext<CdlDbContext>();
            services.AddAutoMapper(typeof(MapperProfile));

            services.AddControllers(mvcOptions => mvcOptions.EnableEndpointRouting = false);
            services.AddOData();

            services.AddConfirmitClient<ISmartHub>("smartHub");
            services.AddConfirmitClient<IMetadata>("metaData");
            services.AddConfirmitClient<Accounts.Clients.IMetadata>("metaData");
            services.AddConfirmitClient<IUsers>("users");
            services.AddConfirmitClient<IEndusers>("endusers");

            services.AddScoped<Factory>();
            services.AddScoped<IAccountLoader, AccountLoader>();
            services.AddScoped<HubPermissionReader>();
            services.AddScoped<SurveyPermissionReader>();
            services.AddScoped<AliasService>();
            services.AddScoped<EventService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<CommitService>();
            services.AddScoped<RevisionService>();
            services.AddScoped<UserPermissionService>();
            services.AddScoped<EnduserPermissionService>();

            services.AddSingleton<Cleanup>();

            services.Configure<CleanupConfig>(configuration.GetSection("Confirmit").GetSection("Cleanup"));

            RemoveODataFormatters(services);
            services.AddMvc()
                .AddNewtonsoftJson(opts =>
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // ReSharper disable once UnusedParameterInPartialMethod
        static partial void UseLocalConfigurationImpl(this IApplicationBuilder builder, IWebHostEnvironment environment)
        {
            builder.UseMiddleware<ExceptionMiddleware>();
            builder.UseEndpoints(config =>
            {
                config.EnableDependencyInjection();
            });

            var serviceProvider = builder.ApplicationServices;
            var cleanup = serviceProvider.GetService<Cleanup>();
            cleanup.Start();
        }


        /// <summary>
        /// Removes all input and output OData formatters. We have to do this because otherwise Swagger
        /// throws InvalidOperationException with message "Add at least one media type to the list of supported media types".
        /// This means that some OData formatters have unsupported media types.
        /// The service uses only native parameters and doesn't respond with OData, so we can safely remove all OData formatters.
        /// </summary>
        private static void RemoveODataFormatters(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                var inputFormattersToRemove = options.InputFormatters.OfType<ODataInputFormatter>();
                foreach (var formatter in inputFormattersToRemove)
                {
                    options.InputFormatters.Remove(formatter);
                }

                var outputFormattersToRemove = options.OutputFormatters.OfType<ODataOutputFormatter>();
                foreach (var formatter in outputFormattersToRemove)
                {
                    options.OutputFormatters.Remove(formatter);
                }
            });
        }
    }
}
