using DbMetaTool.Helpers;
using DbMetaTool.Interfaces;
using DbMetaTool.Models;

namespace DbMetaTool.Builders
{
    public abstract class BaseMetadataBuilder : IMetadataBuilder
    {
        protected readonly List<TableModel> _tables = new();
        protected readonly List<DomainModel> _domains = new();
        protected readonly List<ProcedureModel> _procedures = new();

        public IMetadataBuilder Reset()
        {
            _tables.Clear();
            _domains.Clear();
            _procedures.Clear();
            return this;
        }

        public IMetadataBuilder AddTable(TableModel table)
        {
            _tables.Add(table);
            return this;
        }

        public IMetadataBuilder AddDomain(DomainModel domain)
        {
            domain.Type = FirebirdMapHelper.MapFirebirdType(domain.Type, domain.Length);
            _domains.Add(domain);
            return this;
        }

        public IMetadataBuilder AddProcedure(ProcedureModel procedure)
        {
            _procedures.Add(procedure);
            return this;
        }

        public abstract string Build();
    }
}