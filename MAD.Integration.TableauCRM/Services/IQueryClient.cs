
using MAD.Integration.TableauCRM.Data;

namespace MAD.Integration.TableauCRM.Services
{
    public interface IQueryClient
    {
        Task<IEnumerable<ColumnDefinition>> GetSourceTableColumns(Configuration configuration);
        Task<IEnumerable<dynamic>> QuerySourceTable(Configuration configuration);
    }
}