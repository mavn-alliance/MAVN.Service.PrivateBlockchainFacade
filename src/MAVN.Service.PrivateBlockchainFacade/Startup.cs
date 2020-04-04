using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.PrivateBlockchainFacade.Auth;
using MAVN.Service.PrivateBlockchainFacade.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MAVN.Service.PrivateBlockchainFacade
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "PrivateBlockchainFacade API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "PrivateBlockchainFacadeLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.PrivateBlockchainFacadeService.Db.LogsConnString;
                };

                options.Extend = (sc, settings) =>
                {
                    sc
                        .Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
                        {
                            apiBehaviorOptions.SuppressModelStateInvalidFilter = true;
                        })
                        .AddAuthentication(KeyAuthOptions.AuthenticationScheme)
                        .AddScheme<KeyAuthOptions, KeyAuthHandler>(KeyAuthOptions.AuthenticationScheme, "", opts => { });

                    sc.AddAutoMapper(typeof(AutoMapperProfile));
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x =>
                {
                    x.UseAuthentication();
                };
            });

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
