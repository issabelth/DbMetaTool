using Newtonsoft.Json;

namespace DbMetaTool.Builders
{
    public class JsonMetadataBuilder : BaseMetadataBuilder
    {

        public override string Build()
        {
            var result = new Dictionary<string, object>();

            if (_domains?.Any() == true) result.Add("Domains", _domains);
            if (_tables?.Any() == true) result.Add("Tables", _tables);
            if (_procedures?.Any() == true) result.Add("Procedures", _procedures);

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

    }
}