namespace DbMetaTool.Models
{
    public record ProcedureModel
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public List<string> Parameters { get; set; }
    }
}
