
namespace MAD.Integration.TableauCRM.Services
{
    public interface ICsvManager
    {
        string GenerateFile(string fileName, ResultSet resultSet);
        IEnumerable<byte[]> ReadFileChunks(string csvFilePath);
    }
}