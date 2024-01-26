using Datlo.Domain.Entities;
using Datlo.Domain.Interfaces.Base;

namespace Datlo.Domain.Interfaces
{
    public interface IDataTypeRepository : IRepository<DataType>
    {
        Task<DataType?> GetAsync(int id);

        Task<List<DataType>?> GetAllAsync();

        Task<DataType?> GetByDatasetNameAsync(string datasetName);

        Task<DataType?> GetByColumnsAsync(Dictionary<string, string> Columns);

        Task InsertDataRangeAsync(string tableName, Dictionary<string, string> columns, IEnumerable<dynamic> records);

        Task CreateTableAsync(string tableName, Dictionary<string, string> columns);

        Task<bool> InsertAsync(DataType model);

        Task<IEnumerable<dynamic>> SearchDataSetByColumnValues(string tableName, string columnName, List<string> columnValues);
    }
}
