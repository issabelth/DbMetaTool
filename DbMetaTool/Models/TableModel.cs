namespace DbMetaTool.Models
{
    public record TableModel
    {
        public string Name { get; set; }
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
    }
}
