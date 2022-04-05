using System.Data.SqlClient;

namespace MAD.Integration.TableauCRM.Data
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly AppConfig appConfig;

        public SqlConnectionFactory(AppConfig appConfig)
        {
            this.appConfig = appConfig;
        }

        public SqlConnection Create(string databaseName = "")
        {
            // Update InitialCatalog if databaseName is passed through from configuration item
            var builder = new SqlConnectionStringBuilder(appConfig.ConnectionString);

            if (string.IsNullOrEmpty(databaseName) == false)            
                builder.InitialCatalog = databaseName;
            
            return new(builder.ConnectionString);
        }
    }
}
