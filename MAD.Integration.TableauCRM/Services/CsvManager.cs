using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace MAD.Integration.TableauCRM.Services
{
    public class CsvManager : ICsvManager
    {
        private static readonly CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
        {
            ReadingExceptionOccurred = null,
            DetectDelimiter = true
        };

        public string GenerateFile(string fileName, ResultSet resultSet)
        {
            if (Directory.Exists("Temp") == false)
                Directory.CreateDirectory("Temp");

            // Create temporary csv file in application directory
            var outputFilePath = Path.Combine("Temp", $"{fileName}_{DateTime.Now:yyMMddHHmmss}.csv");

            using var writer = new StreamWriter(outputFilePath);
            using var csvWriter = new CsvWriter(writer, csvConfig);

            // Write header columns to csv file
            foreach (var schema in resultSet.Schema)
            {
                csvWriter.WriteField(schema.Name);
            }

            csvWriter.NextRecord();

            // Write rows to csv file
            foreach (var dict in resultSet.Results)
            {
                foreach (var pair in dict)
                {
                    csvWriter.WriteField(pair.Value);
                }

                csvWriter.NextRecord();
            }

            return outputFilePath;
        }

        public IEnumerable<byte[]> ReadFileChunks(string csvFilePath)
        {
            var bufferSize = 10 * 1024 * 1024;
            var buffer = new byte[bufferSize];

            using var fs = File.OpenRead(csvFilePath);
            using (var bs = new BufferedStream(fs))
            {
                while (bs.Read(buffer, 0, bufferSize) != 0)
                {
                    yield return buffer;
                }
            }
        }
    }
}
