using MAD.Integration.TableauCRM.Data;
using MAD.Integration.TableauCRM.Services;
using Newtonsoft.Json;
using Salesforce.Common.Models.Xml;
using System.Text;

namespace MAD.Integration.TableauCRM.Jobs
{
    public class SourceTableConsumer
    {
        private readonly IResultSetFactory resultSetFactory;
        private readonly ICsvManager csvManager;
        private readonly ApiClientProvider apiClientProvider;

        public SourceTableConsumer(IResultSetFactory resultSetFactory, ICsvManager csvManager, ApiClientProvider apiClientProvider)
        {
            this.resultSetFactory = resultSetFactory;
            this.csvManager = csvManager;
            this.apiClientProvider = apiClientProvider;
        }

        public async Task ConsumeSourceTableAsync(Configuration configuration)
        {
            var apiClient = await this.apiClientProvider.Get();

            // Get the rows and column definitions for the input table
            var resultSet = await this.resultSetFactory.Create(configuration);

            // Results and column definitions are required for the integration to work
            // Exit the job if either are null
            if (resultSet.Results.Any() == false || resultSet.Schema.Any() == false)
                return;

            // Create the metadata JSON object using the column definitions 
            // Serialize the results into a string and get the bytes 
            var metadata = this.GenerateMetadata(configuration, resultSet.Schema);
            var serializedMetadata = JsonConvert.SerializeObject(metadata);
            var metadataBytes = Encoding.UTF8.GetBytes(serializedMetadata);

            // Create the Salesforce Object header
            // Metadata JSON must be sent as a base64 string
            var headerObject = new SObject
            {
                { "Format", "Csv" },
                { "EdgemartAlias", configuration.DestinationTableName },
                { "MetadataJson", Convert.ToBase64String(metadataBytes) },
                { "Operation", "Overwrite" },
                { "Action", "None" }
            };

            var headerResponse = await apiClient.Api.CreateAsync("InsightsExternalData", headerObject);

            if (headerResponse.Success == false)
                throw new Exception($"Error occurred while creating InsightsExternalData Header Object: {JsonConvert.SerializeObject(headerResponse.Errors)}");

            // Generate a temp CSV file using the result set retrieved from the input table
            var csvFilePath = this.csvManager.GenerateFile(configuration.DestinationTableName, resultSet);

            // Break down the CSV file into 10MB chunks
            // This is the maximum limit Salesforce will accept when uploading data
            var fileChunks = this.csvManager.ReadFileChunks(csvFilePath);
            var chunkIndex = 1;

            foreach (var chunk in fileChunks)
            {
                var dataObject = new SObject
                {
                    { "DataFile", chunk },
                    { "InsightsExternalDataId", headerResponse.Id },
                    { "PartNumber", chunkIndex }
                };

                var dataResponse = await apiClient.Api.CreateAsync("InsightsExternalDataPart", dataObject);

                if (dataResponse.Success == false)
                    throw new Exception($"Error occurred while creating InsightsExternalDataPart {chunkIndex}: {JsonConvert.SerializeObject(dataResponse.Errors)}");

                chunkIndex++;
            }

            // Update the action for the Salesforce object to "Process" to start the data upload to the new dataset
            var updateObject = new SObject
            {
                //{ "Id", headerResponse.Id },
                { "Action", "Process" }
            };

            var updateResponse = await apiClient.Api.UpdateAsync("InsightsExternalData", headerResponse.Id, updateObject);

            if (updateResponse.Success == false)
                throw new Exception($"Error occurred while updating InsightsExternalData Object: {JsonConvert.SerializeObject(updateResponse.Errors)}");

            // Delete the temp CSV file
            var fileInfo = new FileInfo(csvFilePath);

            if (fileInfo.Exists)
                fileInfo.Delete();
        }

        private Metadata GenerateMetadata(Configuration configuration, IEnumerable<ResultSetSchema> resultSetSchema) => new()
        {
            FileFormat = new FileFormat
            {
                NumberOfLinesToIgnore = 1
            },
            Objects = new List<ObjectInfo>
            {
                new ObjectInfo
                {
                    Connector = "MAD.Integration.TableauCRM",
                    FullyQualifiedName = configuration.DestinationTableName,
                    Label = configuration.DestinationTableName,
                    Name = configuration.DestinationTableName,
                    Fields = resultSetSchema.Select(x =>
                    {
                        var dataType = this.GetSalesforceDataType(x.System_Type_Name);
                        var name = x.Name.Replace(" ", "");

                        var fieldInfo = new FieldInfo
                        {
                            CanTruncate = false,
                            Type = dataType,
                            Format = dataType == "Date" ? "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'" : null,
                            FullyQualifiedName = name,
                            Precision = Math.Min(18, x.Precision),
                            Scale = x.Scale,
                            Name = name,
                            Label = x.Name
                        };

                        if (dataType == "Numeric")
                        {
                            fieldInfo.DefaultValue = "0";
                        }

                        return fieldInfo;
                    }).ToList()
                }
            }
        };

        private string GetSalesforceDataType(string sqlDataType)
        {
            // Convert the SQL data type to the required type for Salesforce
            // Salesforce types are Text, Numeric & Date
            var result = string.Empty;

            if (string.IsNullOrEmpty(sqlDataType))
                return result;

            // Strip (xx) characters from sql type name
            // e.g. varchar(50) will become varchar
            sqlDataType = sqlDataType.Contains('(') 
                ? sqlDataType[..sqlDataType.IndexOf("(")] 
                : sqlDataType;

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
                case "numeric":
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
