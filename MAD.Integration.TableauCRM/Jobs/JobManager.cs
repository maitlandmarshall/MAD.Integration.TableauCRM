using MAD.Integration.TableauCRM.Services;

namespace MAD.Integration.TableauCRM.Jobs
{
    public class JobManager
    {        
        private readonly IJobRegistrar jobRegistrar;

        public JobManager(IJobRegistrar jobRegistrar)
        {            
            this.jobRegistrar = jobRegistrar;
        }

        public async Task UpdateJobsAsync() => await this.jobRegistrar.RegisterOrDeleteJobsAsync();
    }
}
