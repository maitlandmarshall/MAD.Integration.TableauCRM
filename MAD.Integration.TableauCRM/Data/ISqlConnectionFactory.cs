using System.Data.SqlClient;

namespace MAD.Integration.TableauCRM.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection Create(string databaseName = "");
    }
}
