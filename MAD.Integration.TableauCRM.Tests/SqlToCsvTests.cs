using CsvHelper;
using FluentAssertions;
using MAD.Integration.TableauCRM.Data;
using MAD.Integration.TableauCRM.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Integration.TableauCRM.Tests
{
    [TestClass]
    public class SqlToCsvServiceTests
    {
        public static readonly List<IDictionary<string, object>> testRecords = new()
        {
            new Dictionary<string, object>
                {
                    { "Vendor Code", "DEMGAS" },
                    { "Vendor Unique Reference", "DEMGAS" },
                    { "Vendor Name", "De Martin & Gasparini Pty Ltd" },
                    { "Vendor Company Registration Number", "81 000 205 372" },
                    { "Vendor Country of Registration", "AU" },
                    { "Vendor Address", "Unit B, 7 Worth Street, Chullora, New South Wales, 2190" },
                    { "Vendor Type", "Formwork" },
                    { "Vendor Annual Spend", "12413098.22" },
                },
            new Dictionary<string, object>
                {
                    { "Vendor Code", "SPEMGAS" },
                    { "Vendor Unique Reference", "SPEMGAS" },
                    { "Vendor Name", "De Gasparini & Martin Pty Ltd" },
                    { "Vendor Company Registration Number", "81 000 205 372" },
                    { "Vendor Country of Registration", "AU" },
                    { "Vendor Address", "Unit B, 7 Worth Street, Chullora, New South Wales, 2190" },
                    { "Vendor Type", "Formwork" },
                    { "Vendor Annual Spend", "12413098.22" },
                }
        };

        [TestMethod]
        public async Task SqlToCsv_CreateCsvFileFromSql()
        {
            var configuration = new Configuration
            {
                DatabaseName = "TableauCrmDev",
                TableName = "SqlToCsvTest",
                DestinationTableName = "TestTable",
                IsActive = true
            };

            var config = TestConfigurationFactory.GetTestConfig();
            var sqlConFactory = new SqlConnectionFactory(config);
            var queryFactory = new QueryFactoryFactory(sqlConFactory, new SqlServerCompiler());
            var csvManager = new CsvManager();
            var resultSetFactory = new SqlResultSetFactory(queryFactory);            

            // Query source SQL table rows & column definitions
            var resultSet = await resultSetFactory.Create(configuration);
            resultSet.Should().NotBeNull();
            resultSet.Results.Should().NotBeEmpty();
            resultSet.Schema.Should().NotBeEmpty();

            // Generate the CSV file in the temp folder location
            var filePath = csvManager.GenerateFile(configuration.DestinationTableName, resultSet);
            filePath.Should().NotBeNullOrEmpty();

            // Double check the file exists
            var generatedFile = new FileInfo(filePath);
            generatedFile.Exists.Should().BeTrue();

            // Open the csv file and read the rows
            using var fs = File.OpenRead(generatedFile.FullName);
            using var sr = new StreamReader(fs);
            using (var flatFile = new CsvReader(sr, new(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = null,
                DetectDelimiter = true
            }))
            {

                // Use .ToList() here so that the all the records are retrieved from the file
                var fileRecords = flatFile.GetRecords<dynamic>().Cast<IDictionary<string, object>>().ToList();

                if (fileRecords.Any() == false)
                    return;

                // Convert file record values to list of dictionary <string, object>
                var entities = fileRecords.Select(x => x.ToDictionary(y => y.Key, y => y.Value)).ToList();
                entities.Should().HaveCount(2);

                for (int i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];
                    var testRecord = testRecords[i];

                    // Check entities and test records have the same elements
                    entity.Should().ContainKeys(testRecord.Keys);
                    entity.Should().ContainValues(testRecord.Values);
                }
            }

            generatedFile.Directory?.Delete(true);
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            var sqlConFactory = new SqlConnectionFactory(TestConfigurationFactory.GetTestConfig());
            var queryFactory = new QueryFactoryFactory(sqlConFactory, new SqlServerCompiler());

            // Delete test sql table
            using var db = queryFactory.Create();

            await db.StatementAsync("DROP TABLE [dbo].[SqlToCsvTest]");
        }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext testContext)
        {
            var sqlConFactory = new SqlConnectionFactory(TestConfigurationFactory.GetTestConfig());
            var queryFactory = new QueryFactoryFactory(sqlConFactory, new SqlServerCompiler());

            using var db = queryFactory.Create();

            // Create test sql table
            await db.StatementAsync("CREATE TABLE [dbo].[SqlToCsvTest] (" +
                "[Vendor Code] [nvarchar](50) NOT NULL, " +
                "[Vendor Unique Reference] [nvarchar] (50) NOT NULL, " +
                "[Vendor Name] [nvarchar] (50) NOT NULL, " +
                "[Vendor Company Registration Number] [nvarchar] (50) NOT NULL, " +
                "[Vendor Country of Registration] [nvarchar] (50) NOT NULL, " +
                "[Vendor Address] [nvarchar] (100) NOT NULL, " +
                "[Vendor Type] [nvarchar] (50) NOT NULL, " +
                "[Vendor Annual Spend] [float] NOT NULL) ON [PRIMARY]");

            // Insert test data
            var keys = testRecords.Select(x => x.Keys).First();
            var values = testRecords.Select(x => x.Values);

            await db.Query("SqlToCsvTest").InsertAsync(keys, values);
        }
    }
}