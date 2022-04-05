using MAD.Integration.TableauCRM.Data;
using SqlKata.Execution;

namespace MAD.Integration.TableauCRM.Services
{
    public interface IResultSetFactory
    {
        Task<ResultSet> Create(Configuration configuration);
        Task<IEnumerable<ResultSetSchema>> GetResultSetSchema(QueryFactory db, Configuration configuration);
    }
}
