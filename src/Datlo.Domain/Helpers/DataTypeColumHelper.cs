using CsvHelper;
using NpgsqlTypes;
using System.Data;

namespace Datlo.Domain.Helpers
{
    public static class DataTypeColumHelper
    {
        public static Dictionary<string, string> GetColumnTypesFromCsvReader(CsvReader csvReader, IEnumerable<string> columns)
        {
            var columnTypes = new Dictionary<string, string>();

            foreach (var column in columns)
            {
                csvReader.Read(); // Lê a primeira linha de dados
                var firstValue = csvReader.GetField(column);

                // Obtem o tipo de dado com base no primeiro valor da coluna
                var columnType = GetType(firstValue);
                columnTypes.Add(column, columnType);
            }

            return columnTypes;
        }

        public static Dictionary<string, string> GetColumnTypes(IEnumerable<string> columns, IEnumerable<dynamic> records)
        {
            try
            {
                var columnTypes = new Dictionary<string, string>();

                foreach (var column in columns)
                {
                    // Encontra a primeira instância não nula e não vazia para determinar o tipo da coluna
                    var firstNonNullValue = records.FirstOrDefault(record => ((IDictionary<string, object>)record)[column] != null &&
                                                                              !string.IsNullOrEmpty(((IDictionary<string, object>)record)[column].ToString()));

                    if (firstNonNullValue != null)
                    {
                        var columnType = GetType(((IDictionary<string, object>)firstNonNullValue)[column]);
                        columnTypes.Add(GetColumnName(column), columnType);
                    }
                    else
                    {
                        // Valor nulo ou vazio, use um tipo padrão (por exemplo, texto)
                        columnTypes.Add(GetColumnName(column), "TEXT");
                    }
                }

                return columnTypes;

            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }

        }

        public static string GetColumnName(string Key)
        {
            return StringHelper.NormalizeString(Key);
        }

        public static string GetType(object value)
        {
            switch (value)
            {
                case int _:
                    return "INTEGER";
                case short _:
                    return "SMALLINT";
                case long _:
                    return "BIGINT";
                case decimal _:
                    return "NUMERIC";
                case float _:
                    return "REAL";
                case double _:
                    return "DOUBLE PRECISION";
                case string _:
                    return "TEXT";
                case DateTime _:
                    return "TIMESTAMP";
                case bool _:
                    return "BOOL";
                case Guid _:
                    return "UUID";
                default:
                    throw new ArgumentException($"Tipo de valor não suportado: {value.GetType().Name}");
            }
        }

        public static NpgsqlDbType GetNpgsqlDbType(string columnType)
        {
            switch (columnType.ToUpper())
            {
                case "INTEGER":
                    return NpgsqlDbType.Integer;
                case "SMALLINT":
                    return NpgsqlDbType.Smallint;
                case "BIGINT":
                    return NpgsqlDbType.Bigint;
                case "SERIAL":
                    return NpgsqlDbType.Integer;
                case "NUMERIC":
                    return NpgsqlDbType.Numeric;
                case "REAL":
                    return NpgsqlDbType.Real;
                case "DOUBLE PRECISION":
                    return NpgsqlDbType.Double;
                case "MONEY":
                    return NpgsqlDbType.Money;
                case "CHAR":
                    return NpgsqlDbType.Char;
                case "VARCHAR":
                    return NpgsqlDbType.Varchar;
                case "TEXT":
                    return NpgsqlDbType.Text;
                case "DATE":
                    return NpgsqlDbType.Date;
                case "TIME":
                    return NpgsqlDbType.Time;
                case "TIMESTAMP":
                    return NpgsqlDbType.Timestamp;
                case "BOOL":
                    return NpgsqlDbType.Boolean;
                case "UUID":
                    return NpgsqlDbType.Uuid;
                default:
                    throw new ArgumentException($"Tipo de coluna não suportado: {columnType}");
            }
        }

        public static DbType GetDbType(string columnType)
        {
            switch (columnType.ToUpper())
            {
                case "INTEGER":
                    return DbType.Int32;
                case "SMALLINT":
                    return DbType.Int16;
                case "BIGINT":
                    return DbType.Int64;
                case "SERIAL":
                    return DbType.Int32;
                case "NUMERIC":
                    return DbType.Decimal;
                case "REAL":
                    return DbType.Single;
                case "DOUBLE PRECISION":
                    return DbType.Double;
                case "MONEY":
                    return DbType.Decimal;
                case "CHAR":
                    return DbType.StringFixedLength;
                case "VARCHAR":
                    return DbType.String;
                case "TEXT":
                    return DbType.String;
                case "DATE":
                    return DbType.Date;
                case "TIME":
                    return DbType.Time;
                case "TIMESTAMP":
                    return DbType.DateTime;
                case "BOOL":
                    return DbType.Boolean;
                case "UUID":
                    return DbType.Guid;
                default:
                    throw new ArgumentException($"Tipo de coluna não suportado: {columnType}");
            }
        }
    }
}
