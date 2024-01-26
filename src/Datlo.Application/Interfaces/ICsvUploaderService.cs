using Datlo.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Datlo.Application.Interfaces
{
    public interface ICsvUploaderService : IDisposable
    {
        Task<(DataType type, List<dynamic> Data)?> ReadCsvAsync(string filePath, string datasetName);

        Task<(DataType type, List<dynamic> Data)?> ReadAndSaveLocallyAsync(IFormFile csvFile, string datasetName);

        Task<(string[] columns, List<dynamic> Data)?> ReadAndSaveSearchLocallyAsync(IFormFile csvFile, string datasetName);
    }
}
