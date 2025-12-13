namespace DbMetaTool.Models
{
    public record ColumnModel
    {
        public string Name { get; set; }
        public string DomainName { get; set; }
        public bool IsNullable { get; set; }
    }
}