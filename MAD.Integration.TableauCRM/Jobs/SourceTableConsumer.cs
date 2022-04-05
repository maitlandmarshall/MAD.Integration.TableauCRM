using MAD.Integration.TableauCRM.Data;
using MAD.Integration.TableauCRM.Services;
using Newtonsoft.Json;
using Salesforce.Common.Models.Xml;
using System.Text;

namespace MAD.Integration.TableauCRM.Jobs
{
    public class SourceTableConsumer
    {
        private readonly ConfigurationDbContext dbContext;
        private readonly IQueryClient queryClient;
        private readonly ICsvManager csvManager;
        private readonly ApiClientProvider apiClientProvider;

        public SourceTableConsumer(ConfigurationDbContext dbContext, IQueryClient queryClient, ICsvManager csvManager, ApiClientProvider apiClientProvider)
        {
            this.dbContext = dbContext;
            this.queryClient = queryClient;
            this.csvManager = csvManager;
            this.apiClientProvider = apiClientProvider;
        }

        public async Task ConsumeSourceTableAsync(Configuration configuration)
        {
            var apiClient = await this.apiClientProvider.Get();

            // Get the rows and column definitions for the input table
            var queryResults = await this.queryClient.QuerySourceTable(configuration);
            var columnResults = await this.queryClient.GetSourceTableColumns(configuration);

            // Results and column definitions are required for the integration to work
            // Exit the job if either are null
            if (queryResults.Any() == false || columnResults.Any() == false)
                return;

            // Create the metadata JSON object using the column definitions 
            // Serialize the results into a string and get the bytes 
            var metadata = this.GenerateMetadata(configuration, columnResults);
            var serializedMetadata = JsonConvert.SerializeObject(metadata);
            var metadataBytes = Encoding.UTF8.GetBytes(serializedMetadata);

            // Create the Salesforce Object header
            // Metadata JSON must be sent as a base64 string
            var headerObject = new SObject
            {                
                { "Format", "Csv" },
                { "EdgemartAlias", configuration.DestinationTableName },
                { "MetadataJson", Convert.ToBase64String(metadataBytes) },
                { "Operation", "Upsert" },
                { "Action", "None" }
            };

            var headerResponse = await apiClient.Api.CreateAsync("InsightsExternalData", headerObject);

            if (headerResponse.Success == false)
                throw new Exception($"Error occurred while creating InsightsExternalData Header Object: {JsonConvert.SerializeObject(headerResponse.Errors)}");

            // Generate a temp CSV file using the rows & columns retrieved from the input table
            var csvFilePath = this.csvManager.GenerateFile(configuration.DestinationTableName, queryResults);

            var chunkIndex = 1;

            // Break down the CSV file into 10MB chunks
            // This is the maximum limit Salesforce will accept when uploading data
            foreach (var chunk in this.csvManager.ReadFileChunks(csvFilePath))
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
                { "parentId", headerResponse.Id },
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

        private Metadata GenerateMetadata(Configuration configuration, IEnumerable<ColumnDefinition> columnResults) => new()
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
                    Fields = columnResults.Select(x => new FieldInfo
                    {
                        CanTruncate = false,
                        Type = x.DataType,
                        Format = x.DataType == "Date" ? "dd/MM/yyyy HH:mm:ss" : "",
                        FullyQualifiedName = x.ColumnName,
                        Precision = x.Precision ?? 0,
                        Scale = x.Scale ?? 0,
                        Name = x.ColumnName,
                        Label = x.ColumnName
                    }).ToList()
                }
            }
        };
    }
}
