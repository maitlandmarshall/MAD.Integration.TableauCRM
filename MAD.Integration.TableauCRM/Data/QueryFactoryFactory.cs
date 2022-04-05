using SqlKata.Compilers;
using SqlKata.Execution;

namespace MAD.Integration.TableauCRM.Data
{
    public class QueryFactoryFactory : IQueryFactoryFactory
    {
        private readonly ISqlConnectionFactory sqlConnectionFactory;
        private readonly SqlServerCompiler compiler;

        public QueryFactoryFactory(ISqlConnectionFactory sqlConnectionFactory, SqlServerCompiler compiler)
        {
            this.sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        }

        public QueryFactory Create(string databaseName = "") => new(sqlConnectionFactory.Create(databaseName), compiler);
    }
}
