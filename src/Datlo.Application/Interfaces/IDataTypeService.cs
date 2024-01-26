using Datlo.Application.ViewModel;
using Microsoft.AspNetCore.Http;

namespace Datlo.Application.Interfaces
{
    public interface IDataTypeService : IDisposable
    {
        Task<bool> UploadAndSaveDataFromCsvAsync(IFormFile file, DataTypeCsvUploadViewModel model);
        Task<dynamic> UploadAndSearchDataFromCsvAsync(IFormFile file, DataSetCsvSearchUploadViewModel model);
    }
}
