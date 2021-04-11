using Newtonsoft.Json;

namespace KC.Configurator.Models
{
    class KcConfigurationObject
    {
        [JsonProperty("appDefinition")]
        public AppDefinition AppDefinition {get; set;}
    }
}
