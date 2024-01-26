using Dapper;
using Datlo.Domain.Entities;
using Datlo.Domain.Helpers;
using Datlo.Domain.Interfaces;
using Datlo.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Bulk;
using NpgsqlTypes;
using PostgreSQLCopyHelper;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace Datlo.Infra.Data.Repository
{
    public class DataTypeRepository : BaseRepository<DataType>, IDataTypeRepository
    {
        public DataTypeRepository(AppDbContext context) : base(context)
        { }

        public async Task<List<DataType>> GetAllAsync()
        {
            return await Db.DataType.ToListAsync();
        }

        public async Task<DataType> GetAsync(int id)
        {
            return await Db.DataType.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task InsertDataRangeAsync(string tableName, Dictionary<string, string> columns, IEnumerable<dynamic> records)
        {

            /*
             * Deixei esse codigo comentado aqui
             *  foi a primeira versão do insert de range de dados, muito lento
             *  Tambem testei o NpgsqlBulkUploader, mas nao tive sucesso, nao tentei muito tambem
             */
            //using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            //{
            //    await connection.OpenAsync();

            //    foreach (var record in records)
            //    {
            //        await InsertDataAsync(tableName, columns, record);
            //    }
            //}


            var copyHelper = new PostgreSQLCopyHelper<IDictionary<string, object>>(tableName);

            //Nova lista de records, para fazer a conversão de dynamic para IDictionary<string, object>
            var dictionary_records = new List<IDictionary<string, object>>();

       
            foreach (var record in records)
            {
                dictionary_records.Add((IDictionary<string, object>)record);
            }
          
            var first_record = dictionary_records.FirstOrDefault();


            foreach (var property in first_record!)
            {
                if (property.Key.ToLower() != "jsondata")
                    copyHelper.MapText(DataTypeColumHelper.GetColumnName(property.Key), x => x[property.Key].ToString());
                else
                    copyHelper.MapJsonb(DataTypeColumHelper.GetColumnName(property.Key), x => x[property.Key].ToString());
            }

            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                connection.Open();

                copyHelper.SaveAll(connection, dictionary_records);
            }

            return;
        }

        public async Task InsertDataAsync(string tableName, Dictionary<string, string> columns, dynamic record)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                var keys = ((IDictionary<string, string>)columns).Keys;

                // Prepara a consulta SQL para inserir dados
                var sql = $"INSERT INTO {tableName} ({string.Join(", ", keys.ToList())}) VALUES ({string.Join(", ", keys.Select(c => c.ToLower() == "jsondata" ? $"@{c}::jsonb" : $"@{c}"))});";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                foreach (var property in ((IDictionary<string, object>)record))
                {
                    cmd.Parameters.AddWithValue(property.Key, property.Value);
                }

                // Adiciona o parâmetro JsonData
                {
                    var parameter = (NpgsqlParameter)cmd.CreateParameter();
                    parameter.ParameterName = "JsonData";
                    parameter.Value = JsonConvert.SerializeObject(record);
                    parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
                    cmd.Parameters.Add(parameter);
                }

                await cmd.ExecuteNonQueryAsync();
            }

            return;
        }

  

        public async Task<DataType?> GetByDatasetNameAsync(string datasetName)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                connection.Open();

                // Consulta para obter metadados pelo nome do conjunto de dados
                var sql = "SELECT * FROM DataType WHERE DatasetName = @DatasetName";
                var metadata = await connection.QueryFirstOrDefaultAsync<DataType?>(sql, new { DatasetName = datasetName });

                return metadata;
            }
        }

        public async Task<DataType?> GetByColumnsAsync(Dictionary<string, string> Columns)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                connection.Open();

                // Converte as colunas do conjunto de dados em um array ordenado
                var csvColumnsArray = Columns.Keys.OrderBy(k => k).ToArray();

                // Constrói a query SQL dinâmica para comparar os arrays de colunas
                var sql = $"SELECT * FROM DataType WHERE \"ColumnsMetadata\"  = @CsvColumns";

                var datatype = await connection.QueryFirstOrDefaultAsync<DataType?>(sql, new { CsvColumns = Columns });

                return datatype;
            }
        }

        public async Task CreateTableAsync(string tableName, Dictionary<string, string> columns)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                connection.Open();

                var sql = $"CREATE TABLE IF NOT EXISTS {tableName} (";

                foreach (var (columnName, columnType) in columns)
                {
                    if (columnName.ToLower() != "jsondata")
                        sql += $"{columnName} {columnType}, ";
                }

                // Adiciona uma coluna JsonData para armazenar dados em JSONB
                sql += "JsonData JSONB NOT NULL";

                // Adiciona a chave primária (pode precisar ajustar dependendo da estrutura dos dados)
                //sql += $", PRIMARY KEY ({columns.Keys.First()})";

                sql += ");";

                // Executa a query para criar a tabela
                await connection.ExecuteAsync(sql);

                return;
            }
        }

        public async Task<bool> InsertAsync(DataType model)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                // Prepara a consulta SQL para inserir dados
                var sql = $"INSERT INTO DataType (\"Name\", \"TableReferenceName\", \"ColumnsMetadata\", \"CreatedDate\", \"UpdatedDate\") VALUES (@Name, @TableReferenceName, @ColumnsMetadata, @CreatedDate, @UpdatedDate);";

                var queryArguments = new
                {
                    Name = model.Name,
                    TableReferenceName = model.TableReferenceName,
                    ColumnsMetadata = model.ColumnsMetadata,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await connection.ExecuteAsync(sql, queryArguments);
            }

            return true;
        }


        public async Task<IEnumerable<dynamic>> SearchDataSetByColumnValues(string tableName, string columnName, List<string> columnValues)
        {
            using (var connection = new NpgsqlConnection(Db.Database.GetConnectionString()))
            {
                connection.Open();

                // Constrói a consulta SQL dinâmica para buscar com base na coluna e na lista de valores
                var sql = $"SELECT * FROM {tableName} WHERE jsondata ->> @ColumnName = ANY(@ColumnValues)";

                // Executa a consulta parametrizada
                var results = await connection.QueryAsync(sql, new { ColumnName = columnName, ColumnValues = columnValues.ToArray() });

                return results;
            }
        }
    }
}