using SqlKata.Execution;

namespace MAD.Integration.TableauCRM.Data
{
    public interface IQueryFactoryFactory
    {
        QueryFactory Create(string databaseName = "");
    }
}
