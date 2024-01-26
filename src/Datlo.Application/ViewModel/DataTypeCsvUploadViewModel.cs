using Datlo.Application.Attributes;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Datlo.Application.ViewModel
{
    [JsonObject(Title = "dataType")]
    public class DataTypeCsvUploadViewModel
    {
        public string Name { get; set; }

        [AllowedExtensions(new string[] { ".csv" })]
        [DataType(DataType.Upload)]
        public IFormFile? File { get; set; }
    }
}
