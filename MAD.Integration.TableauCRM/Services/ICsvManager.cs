
namespace MAD.Integration.TableauCRM.Services
{
    public interface ICsvManager
    {
        string GenerateFile(string fileName, IEnumerable<dynamic> data);
        IEnumerable<byte[]> ReadFileChunks(string csvFilePath);
    }
}