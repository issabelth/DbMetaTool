namespace DbMetaTool.Models
{
    public record DomainModel
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Length { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsNullable { get; set; }
    }
}