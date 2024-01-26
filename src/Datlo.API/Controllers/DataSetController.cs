using Datlo.API.Configuration;
using Datlo.Application.Interfaces;
using Datlo.Application.ViewModel;
using Datlo.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Datlo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSetController : MainController
    {
        private readonly IDataTypeService _dataTypeService;

        public DataSetController(IDataTypeService dataTypeService, INotifier notifier) : base(notifier)
        {
            _dataTypeService = dataTypeService;
        }


        [HttpPost]
        public async Task<IActionResult> PostFile([FromForm] DataTypeCsvUploadViewModel model)
        {
            
            return Ok(await _dataTypeService.UploadAndSaveDataFromCsvAsync(model.File, model));
        }

        [HttpPost("search")]
        public async Task<IActionResult> PostSearchFile([FromForm] DataSetCsvSearchUploadViewModel model)
        {

            return Ok(await _dataTypeService.UploadAndSearchDataFromCsvAsync(model.File, model));
        }

    }
}
