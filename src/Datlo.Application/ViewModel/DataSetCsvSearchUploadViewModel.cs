using Datlo.Application.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Datlo.Application.ViewModel
{
    public class DataSetCsvSearchUploadViewModel
    {
        public int DataTypeId { get; set; }

        [AllowedExtensions(new string[] { ".csv" })]
        [DataType(DataType.Upload)]
        public IFormFile? File { get; set; }
    }
}
