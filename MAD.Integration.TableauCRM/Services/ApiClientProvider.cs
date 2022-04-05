using MAD.API.Salesforce;
using static MAD.API.Salesforce.SalesforceApiClientFactory;

namespace MAD.Integration.TableauCRM.Services
{
    public class ApiClientProvider
    {
        private readonly SalesforceApiClientFactory salesforceApiClientFactory;
        private readonly AppConfig appConfig;
        private readonly SalesforceApiOptions salesforceApiOptions;
        private SemaphoreSlim apiAsyncLock = new SemaphoreSlim(1);
        private SalesforceApiClient apiClient;

        public ApiClientProvider(SalesforceApiClientFactory salesforceApiClientFactory, AppConfig appConfig, SalesforceApiOptions salesforceApiOptions)
        {
            this.salesforceApiClientFactory = salesforceApiClientFactory;
            this.appConfig = appConfig;
            this.salesforceApiOptions = salesforceApiOptions;
        }

        public async Task<SalesforceApiClient> Get()
        {
            await this.apiAsyncLock.WaitAsync();

            try
            {
                if (this.apiClient is null)                
                    this.apiClient = await this.salesforceApiClientFactory.CreateApiClient(this.salesforceApiOptions, this.appConfig.Username, this.appConfig.Password);                

                return this.apiClient;
            }
            finally
            {
                this.apiAsyncLock.Release();
            }
        }
    }
}
