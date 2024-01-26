using AutoMapper;
using Datlo.Application.Interfaces;
using Datlo.Application.Services.Base;
using Datlo.Application.ViewModel;
using Datlo.Domain.Entities;
using Datlo.Domain.Helpers;
using Datlo.Domain.Interfaces;
using Datlo.Domain.Notifications;
using Microsoft.AspNetCore.Http;

namespace Datlo.Application.Services
{
    public class DataTypeService : BaseService, IDataTypeService
    {
        private readonly IMapper _mapper;
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly ICsvUploaderService _csvUploaderService;

        public DataTypeService(IMapper mapper, IDataTypeRepository dataTypepository, ICsvUploaderService csvUploaderService, INotifier notifier) : base(notifier)
        {
            _mapper = mapper;
            _dataTypeRepository = dataTypepository;
            _csvUploaderService = csvUploaderService;
        }

        public async Task<bool> UploadAndSaveDataFromCsvAsync(IFormFile file, DataTypeCsvUploadViewModel model)
        {
            var csvContent = await _csvUploaderService.ReadAndSaveLocallyAsync(file, model.Name);

            if (csvContent == null)
                return false;

            if (csvContent?.Data == null)
                return false;

            if (csvContent?.type == null)
                return false;

            var dataType = csvContent?.type;
            dataType.Name = model.Name;

            return  await SaveDataAsync(csvContent?.type, csvContent?.Data);

        }

        public async Task<dynamic> UploadAndSearchDataFromCsvAsync(IFormFile file, DataSetCsvSearchUploadViewModel model)
        {
            var csvContent = await _csvUploaderService.ReadAndSaveSearchLocallyAsync(file, DateTime.Now.ToFileTimeUtc().ToString());

            if (csvContent == null)
                return false;

            if (csvContent?.Data == null)
                return false;

            if (csvContent?.columns == null)
                return false;

            var column = csvContent?.columns.FirstOrDefault();
       
           var searchParams = new List<string>();


            foreach (var record in csvContent?.Data)
            {
                var r = ((IDictionary<string, object>)record).FirstOrDefault();
                searchParams.Add(r.Value.ToString());
            }

            var items = await _dataTypeRepository.SearchDataSetByColumnValues("dataset_pokemon_39013580", column, searchParams);
            return items;

        }

        public async Task<bool> SaveDataAsync(DataType type, List<dynamic> records)
        {
            //Busca datatype com as mesmas colunas e tipos
            var dataTypeCheck = await _dataTypeRepository.GetByColumnsAsync(type!.ColumnsMetadata);

            //Caso este datatype ja exista, apenas insere os dados
            /*
             *   Em um cenario real, onde talvez, esta seja um API utilizada por usuarios com chaves eg,
             *   poderiamos adicionar mais uma validação, de usuario/chave, para nao misturar os dados, ou apenas adicionar mais uma coluna com a referencia do usuario,
             *   para quando ele buscar, trazer apenas seus dados
            */
            if (dataTypeCheck != null)
            {
                await _dataTypeRepository.InsertDataRangeAsync($"{dataTypeCheck!.TableReferenceName!}", type.ColumnsMetadata, records);
                return true;
            }

            var dataType = type;
            //Gera o nome da tabela, utilizo o GetRandom8Digits() por conta de poder ter conjutos de dados com o mesmo nome
            dataType!.TableReferenceName = $"dataset_{type.Name}_{StringHelper.GetRandom8Digits()}";

            //Adiciona o DataType
            await _dataTypeRepository.InsertAsync(dataType);

            //Cria a tabela
            /*
             *   Caso estivessemos utilizando a abordagem 2 mostrada na pesquisa, nao precisariamos ficar gerando tabelas
             *   Apenas fariamos o split/mitigação dos dados com regras criadas anteriormente, seja por categoria/tipo, região etc
             */
            await _dataTypeRepository.CreateTableAsync(dataType!.TableReferenceName, dataType!.ColumnsMetadata);

            //Adiciona os dados e retorna
            await _dataTypeRepository.InsertDataRangeAsync(dataType.TableReferenceName, dataType.ColumnsMetadata, records);
            return true;
        }



        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
