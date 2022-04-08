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
        private readonly IBackgroundJobClient backgroundJobClient;        

        public JobRegistrar(ConfigurationDbContext dbContext, IRecurringJobManager recurringJobManager, IBackgroundJobClient backgroundJobClient)
        {
            this.dbContext = dbContext;
            this.recurringJobManager = recurringJobManager;            
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
                recurringJobManager.CreateRecurringJob<SourceTableConsumer>(configuration.DestinationTableName, y => y.ConsumeSourceTableAsync(configuration), Cron.Daily());
            }
        }
    }
}
