using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace Acme.Parent;

[DependsOn(
    typeof(ParentApplicationModule),
    typeof(ParentHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
    )]
public class ParentHttpApiHostModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                {"Parent", "Parent API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Parent API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
                options.HideAbpEndpoints();
            });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "Parent";
            });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "Parent:";
        });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("Parent");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "Parent-Protection-Keys");
        }

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        context.Services.AddCap(option =>
        {
            //option.UseSqlServer(cfg =>
            //{
            //    cfg.ConnectionString = configuration["Cap:ConnectionString"];
            //});

            option.UseMongoDB(opt =>
            {
                opt.DatabaseName = configuration["CAP:DBName"];
                opt.DatabaseConnection = configuration["CAP:ConnectionString"];
            });

            option.UseKafka(opt =>
            {
                opt.Servers = configuration["CAP:Kafka:Connections:Default:BootstrapServers"].ToString();
                if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Protocol"]))
                {
                    opt.MainConfig.Add("security.protocol", configuration["CAP:Kafka:Protocol"].ToString());
                }
                if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Mechanism"]))
                {
                    opt.MainConfig.Add("sasl.mechanism", configuration["CAP:Kafka:Mechanism"].ToString());
                }
                if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Username"]))
                {
                    opt.MainConfig.Add("sasl.username", configuration["CAP:Kafka:Username"].ToString());
                }
                if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Password"]))
                {
                    opt.MainConfig.Add("sasl.password", configuration["CAP:Kafka:Password"].ToString());
                }
                opt.MainConfig.Add("allow.auto.create.topics", configuration["CAP:Kafka:AutoCreateTopics"]);
            });
            var failedRetryCount = int.Parse(configuration["Cap:FailedRetryCount"].ToString());
            var group = configuration["Cap:Group"].ToString();
            var succeedMessageExpiredAfter = int.Parse(configuration["Cap:SucceedMessageExpiredAfter"].ToString());
            var consumerThreadCount = int.Parse(configuration["Cap:ConsumerThreadCount"].ToString());
            var failedRetryInterval = int.Parse(configuration["Cap:FailedRetryInterval"].ToString());

            option.DefaultGroupName = configuration["Cap:DefaultGroupName"].ToString() ?? option.DefaultGroupName;
            option.ConsumerThreadCount = consumerThreadCount;
            option.FailedRetryInterval = failedRetryInterval;
            option.FailedRetryCount = failedRetryCount;
            option.SucceedMessageExpiredAfter = succeedMessageExpiredAfter;
            option.UseDashboard(o => o.PathMatch = "/cap");
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("Parent");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
