using DbMetaTool.Builders;
using DbMetaTool.Interfaces;

namespace DbMetaTool
{
    public static class MetadataBuilderFactory
    {
        public static IMetadataBuilder GetBuilder(string format)
        {
            return format.ToLower() switch
            {
                "sql" => new SqlMetadataBuilder(),
                "json" => new JsonMetadataBuilder(),
                "txt" => new TxtMetadataBuilder(),
                _ => throw new ArgumentException("Nieobsługiwany format pliku")
            };
        }
    }
}
