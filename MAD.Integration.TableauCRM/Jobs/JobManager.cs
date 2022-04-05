using Hangfire;
using MAD.Integration.TableauCRM.Data;

namespace MAD.Integration.TableauCRM.Jobs
{
    public class JobManager
    {
        private readonly ConfigurationDbContext dbContext;        
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;

        public JobManager(ConfigurationDbContext dbContext, IRecurringJobManager recurringJobManager, IBackgroundJobClient backgroundJobClient)
        {
            this.dbContext = dbContext;            
            this.recurringJobManager = recurringJobManager;
            this.backgroundJobClient = backgroundJobClient;
        }

        public async Task CreateOrUpdateJobs() => await Task.Run(() =>
        {
            // Delete inactive jobs
            foreach (var configuration in this.dbContext.Configuration.Where(y => y.IsActive == false))
            {
                backgroundJobClient.Delete(configuration.DestinationTableName);
                recurringJobManager.RemoveIfExists(configuration.DestinationTableName);
            }

            // Register active jobs
            foreach (var configuration in this.dbContext.Configuration.Where(y => y.IsActive))
            {
                recurringJobManager.AddOrUpdate<SourceTableConsumer>(configuration.DestinationTableName, y => y.ConsumeSourceTableAsync(configuration), Cron.Daily());
            }
        });
    }
}
