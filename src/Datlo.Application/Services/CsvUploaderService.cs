using CsvHelper;
using CsvHelper.Configuration;
using Datlo.Application.Interfaces;
using Datlo.Application.Services.Base;
using Datlo.Domain.Entities;
using Datlo.Domain.Helpers;
using Datlo.Domain.Notifications;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Globalization;

namespace Datlo.Application.Services
{
    public class CsvUploaderService : BaseService, ICsvUploaderService
    {
        public CsvUploaderService(INotifier notifier) : base(notifier)
        {
        }

        public async Task<(DataType type, List<dynamic> Data)?> ReadCsvAsync(string filePath, string datasetName)
        {
            using (var reader = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

                // Lê as colunas do CSV
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
                var headers = csvReader.HeaderRecord.ToList();
                headers.Add("JsonData");
                var columnsNotNormalized = headers.ToArray();
                var columns = new List<string>();
                foreach (var item in columnsNotNormalized)
                {
                    columns.Add(DataTypeColumHelper.GetColumnName(item));
                }


                // Lê os dados
                var records = (csvReader.GetRecordsAsync<dynamic>()).ToBlockingEnumerable().ToList();

                foreach (var record in records)
                {    
                    record.JsonData = ConvertCsvLineToJsonObject(columns.ToArray(), record);
                }

                // Obtem os tipos de dados das colunas
                var columnTypes = DataTypeColumHelper.GetColumnTypes(columnsNotNormalized, records);


                // Cria o DataType com as colunas e tipo de dados
                var metadata = new DataType
                {
                    Name = datasetName,
                    ColumnsMetadata = columns.ToDictionary(column => column, column => columnTypes[column])
                };

                return (metadata, records);
            }
        }


        public async Task<(string[] columns, List<dynamic> Data)?> ReadSearchCsvAsync(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

                // Lê as colunas do CSV
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
                var headers = csvReader.HeaderRecord.ToList();
                
                var columnsNotNormalized = headers.ToArray();
                var columns = new List<string>();
                foreach (var item in columnsNotNormalized)
                {
                    columns.Add(DataTypeColumHelper.GetColumnName(item));
                }

                // Lê os dados
                var records = (csvReader.GetRecordsAsync<dynamic>()).ToBlockingEnumerable().ToList();

                return (columns.ToArray(), records);
            }
        }

        public async Task<(DataType type, List<dynamic> Data)?> ReadAndSaveLocallyAsync(IFormFile csvFile, string datasetName)
        {
            try
            {
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                {
                    var csvData = await streamReader.ReadToEndAsync();

                    // Salva o CSV localmente
                    var localFilePath = await SaveCsvLocallyAsync(csvData, datasetName);

                    if (!string.IsNullOrEmpty(localFilePath))
                    {
                        return await ReadCsvAsync(localFilePath, datasetName);
                    }
                    else
                    {
                        Notify("Falha ao salvar o CSV localmente.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Notify($"Erro ao processar o arquivo CSV: {ex.Message}");
                return null;
            }
        }

        public async Task<(string[] columns, List<dynamic> Data)?> ReadAndSaveSearchLocallyAsync(IFormFile csvFile, string datasetName)
        {
            try
            {
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                {
                    var csvData = await streamReader.ReadToEndAsync();

                    // Salva o CSV localmente
                    var localFilePath = await SaveCsvLocallyAsync(csvData, datasetName);

                    if (!string.IsNullOrEmpty(localFilePath))
                    {
                        return await ReadSearchCsvAsync(localFilePath);
                    }
                    else
                    {
                        Notify("Falha ao salvar o CSV localmente.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Notify($"Erro ao processar o arquivo CSV: {ex.Message}");
                return null;
            }
        }

        public async Task<string> ReadCsvFromApiAsync(string apiUrl)
        {
            using (var webClient = new HttpClient())
            {
                try
                {
                    var csvData = await webClient.GetStringAsync(apiUrl);
                    return csvData;
                }
                catch (Exception ex)
                {
                    Notify($"Erro ao baixar o CSV da API: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<string> SaveCsvLocallyAsync(string csvData, string datasetName)
        {
            try
            {
                // Diretório onde o projeto está localizado
                var projectDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Diretório onde o CSV será salvo localmente
                var localDirectory = Path.Combine(projectDirectory, "uploads", "csv");
                if (!Directory.Exists(localDirectory))
                    Directory.CreateDirectory(localDirectory);

                // Nome do arquivo local
                var localFilePath = Path.Combine(localDirectory, $"{datasetName}_data.csv");

                // Salva o CSV localmente
                await File.WriteAllTextAsync(localFilePath, csvData);

                return localFilePath;
            }
            catch (Exception ex)
            {
                Notify($"Erro ao salvar o CSV localmente: {ex.Message}");
                return null;
            }
        }

        private async Task<string> ConvertCsvFileToJsonObjectAsync(string path)
        {
            var csv = new List<string[]>();
            var lines = await File.ReadAllLinesAsync(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }


        private string ConvertCsvLineToJsonObject(string[] columns, dynamic line)
        {
            //var listObjResult = new List<Dictionary<string, string>>();

            //var teste = line as IDictionary<string, object>;

            //var objResult = new Dictionary<string, string>();
            //for (int j = 0; j < properties.Length; j++)
            //{
            //    var column = properties[j];
            //    objResult.Add(column, line?["Keys"][j]);
            //}

            return JsonConvert.SerializeObject(line);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
