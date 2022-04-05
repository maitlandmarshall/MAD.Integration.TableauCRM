using Hangfire;
using MAD.API.Salesforce;
using MAD.Integration.TableauCRM.Data;
using MAD.Integration.TableauCRM.Jobs;
using MAD.Integration.TableauCRM.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MIFCore.Hangfire;
using MIFCore.Settings;
using SqlKata.Compilers;
using static MAD.API.Salesforce.SalesforceApiClientFactory;

namespace MAD.Integration.TableauCRM
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIntegrationSettings<AppConfig>()
                .AddDbContext<ConfigurationDbContext>((svc, builder) => builder.UseSqlServer(svc.GetRequiredService<AppConfig>().ConnectionString))
                .AddTransient(_ => new SqlServerCompiler())
                .AddTransient<IQueryFactoryFactory, QueryFactoryFactory>()
                .AddTransient<ISqlConnectionFactory, SqlConnectionFactory>()
                .AddTransient<SalesforceApiClientFactory>()
                .AddTransient<ICsvManager, CsvManager>()
                .AddTransient<IResultSetFactory, SqlResultSetFactory>()
                .AddTransient((svc) =>
                {
                    var config = svc.GetRequiredService<AppConfig>();

                    var options = new SalesforceApiOptions
                    {
                        ConsumerKey = config.ConsumerKey,
                        ConsumerSecret = config.ConsumerSecret
                    };

                    if (!string.IsNullOrEmpty(config.AuthEndpoint)) options.AuthEndpoint = config.AuthEndpoint;
                    if (!string.IsNullOrEmpty(config.InstanceEndpoint)) options.InstanceEndpoint = config.InstanceEndpoint;

                    return options;
                })
                .AddTransient<ApiClientProvider>()
                .AddScoped<JobManager>()
                .AddScoped<SourceTableConsumer>();
        }

        public void Configure(AppConfig config)
        {
            if (!this.IsConfigValid(config))
            {
                throw new Exception("Settings not configured.");
            }
        }

        public async Task PostConfigure(ConfigurationDbContext dbContext, IRecurringJobFactory recurringJobFactory, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            await dbContext.Database.MigrateAsync();

            // JobManager runs once a day to add new configurations and delete inactive ones
            recurringJobFactory.CreateRecurringJob<JobManager>("JobManager", y => y.CreateOrUpdateJobs(), Cron.Daily());
        }

        private bool IsConfigValid(AppConfig config)
        {
            if (string.IsNullOrEmpty(config.ConnectionString)
                || string.IsNullOrEmpty(config.ConsumerKey)
                || string.IsNullOrEmpty(config.ConsumerSecret))
            {
                return false;
            }

            return true;
        }
    }
}