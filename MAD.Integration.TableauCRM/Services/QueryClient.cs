using MAD.Integration.TableauCRM.Data;
using SqlKata.Execution;

namespace MAD.Integration.TableauCRM.Services
{
    public class QueryClient : IQueryClient
    {
        private readonly IQueryFactoryFactory queryFactoryFactory;

        public QueryClient(IQueryFactoryFactory queryFactoryFactory)
        {
            this.queryFactoryFactory = queryFactoryFactory;
        }

        public async Task<IEnumerable<dynamic>> QuerySourceTable(Configuration configuration)
        {
            using var db = this.queryFactoryFactory.Create(configuration.DatabaseName);

            // Query the configured database/table name and return the results
            return await db.Query(configuration.TableName).GetAsync<dynamic>();
        }

        public async Task<IEnumerable<ColumnDefinition>> GetSourceTableColumns(Configuration configuration)
        {
            using var db = this.queryFactoryFactory.Create(configuration.DatabaseName);

            //Retrieve the columns with their type, precision and scale for the input table
            var rawQuery = $@"
SELECT col.[name] AS [ColumnName],
       [sys].[systypes].[name] AS [DataType],
       [sys].[systypes].[prec] AS [Precision],
       [sys].[systypes].[scale] AS [Scale]
FROM [sys].[columns] col
    INNER JOIN [sys].[systypes]
        ON [sys].[systypes].[xusertype] = col.[system_type_id]
WHERE EXISTS
(
    SELECT 1
    FROM [sys].[tables]
    WHERE [object_id] = col.[object_id]
          AND [name] = @TableName
)";

            var queryResult = await db.SelectAsync(rawQuery, new { configuration.TableName });

            return queryResult.Select(cd =>
            {
                return new ColumnDefinition
                {
                    ColumnName = cd.ColumnName,
                    DataType = this.GetColumnDataType(cd.DataType),
                    Precision = cd.Precision,
                    Scale = cd.Scale
                };
            });
        }

        private string GetColumnDataType(string sqlDataType)
        {
            //Convert the SQL data type to the required type for Salesforce
            // Salesforce types are Text, Number & Date
            var result = string.Empty;

            if (string.IsNullOrEmpty(sqlDataType))
                return result;

            switch (sqlDataType.ToLower())
            {
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "uniqueidentifier":
                case "varchar":
                case "variant":
                case "bit":
                case "time":
                    result = "Text";
                    break;
                case "bigint":
                case "decimal":
                case "float":
                case "int":
                case "money":
                case "real":
                case "smallint":
                case "smallmoney":
                case "tinyint":
                    result = "Numeric";
                    break;
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "datetimeoffset":
                    result = "Date";
                    break;
            }

            return result;
        }
    }
}
