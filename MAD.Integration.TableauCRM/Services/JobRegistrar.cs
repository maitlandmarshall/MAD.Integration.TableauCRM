using Hangfire;
using MAD.Integration.TableauCRM.Data;
using MAD.Integration.TableauCRM.Jobs;
using Microsoft.EntityFrameworkCore;
using MIFCore.Hangfire;

namespace MAD.Integration.TableauCRM.Services
{
    public class JobRegistrar : IJobRegistrar
    {
        private readonly ConfigurationDbContext dbContext;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IRecurringJobFactory recurringJobFactory;
        private readonly IBackgroundJobClient backgroundJobClient;        

        public JobRegistrar(ConfigurationDbContext dbContext, IRecurringJobManager recurringJobManager, IRecurringJobFactory recurringJobFactory, IBackgroundJobClient backgroundJobClient)
        {
            this.dbContext = dbContext;
            this.recurringJobManager = recurringJobManager;
            this.recurringJobFactory = recurringJobFactory;
            this.backgroundJobClient = backgroundJobClient;
        }

        public async Task RegisterOrDeleteJobsAsync()
        {
            var configs = await this.dbContext.Configuration.ToListAsync();

            // Delete inactive jobs
            foreach (var configuration in configs.Where(y => y.IsActive == false))
            {
                backgroundJobClient.Delete(configuration.DestinationTableName);
                recurringJobManager.RemoveIfExists(configuration.DestinationTableName);
            }

            // Register active jobs
            foreach (var configuration in configs.Where(y => y.IsActive))
            {
                recurringJobFactory.CreateRecurringJob<SourceTableConsumer>(configuration.DestinationTableName, y => y.ConsumeSourceTableAsync(configuration), Cron.Daily());
            }
        }
    }
}
