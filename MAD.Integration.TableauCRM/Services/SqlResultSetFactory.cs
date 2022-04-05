using MAD.Integration.TableauCRM.Data;
using SqlKata.Execution;

namespace MAD.Integration.TableauCRM.Services
{
    public class SqlResultSetFactory : IResultSetFactory
    {
        private readonly IQueryFactoryFactory queryFactory;

        public SqlResultSetFactory(IQueryFactoryFactory queryFactory)
        {
            this.queryFactory = queryFactory;
        }

        public async Task<ResultSet> Create(Configuration configuration)
        {
            // Execute the query using the configuration.TableName property
            // return results as a dictionary
            using var db = this.queryFactory.Create(configuration.DatabaseName);

            var schema = await this.GetResultSetSchema(db, configuration);           
            var results = await db.Query(configuration.TableName).GetAsync<dynamic>();

            return new ResultSet
            {
                Results = results.Cast<IDictionary<string, object>>().ToList(),
                Schema = schema
            };
        }

        public async Task<IEnumerable<ResultSetSchema>> GetResultSetSchema(QueryFactory db, Configuration configuration)
        {            
            var rawSql = db.Compiler.Compile(db.Query(configuration.TableName)).RawSql;

            return await db.SelectAsync<ResultSetSchema>("exec sp_describe_first_result_set @tsql", new { tsql = rawSql });
        }
    }
}
