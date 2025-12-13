using DbMetaTool.Models;

namespace DbMetaTool.Interfaces
{
    public interface IMetadataBuilder
    {
        IMetadataBuilder Reset();
        IMetadataBuilder AddDomain(DomainModel domain);
        IMetadataBuilder AddTable(TableModel table);
        IMetadataBuilder AddProcedure(ProcedureModel procedure);
        string Build();
    }
}