namespace Datlo.Domain.Entities
{
    public class DataType : Entity
    {
        public DataType()
        {
            ColumnsMetadata = new Dictionary<string, string>();
        }

        public string Name { get; set; }

        public string? TableReferenceName { get; set; }

        public Dictionary<string, string> ColumnsMetadata { get; set; }
    }
}
