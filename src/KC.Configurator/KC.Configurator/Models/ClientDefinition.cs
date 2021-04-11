using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Models
{
    class ClientDefinition
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("consentRequired")]
        public bool ConsentRequired { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("rootUrl")]
        public string RootUrl { get; set; }
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }
        [JsonProperty("adminUrl")]
        public string AdminUrl { get; set; }
        [JsonProperty("redirectUris")]
        public List<string> RedirectUris { get; set; }
        [JsonProperty("webOrigins")]
        public List<object> WebOrigins { get; set; }
        [JsonProperty("publicClient")]
        public bool PublicClient { get; set; }
        [JsonProperty("bearerOnly")]
        public bool BearerOnly { get; set; }
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        [JsonProperty("clientAuthenticatorType")]
        public string ClientAuthenticatorType { get; set; }
        [JsonProperty("directAccessGrantsEnabled")]
        public bool DirectAccessGrantsEnabled { get; set; }
        [JsonProperty("authorizationServicesEnabled")]
        public bool AuthorizationServicesEnabled { get; set; }
        [JsonProperty("standardFlowEnabled")]
        public bool StandardFlowEnabled { get; set; }
        [JsonProperty("implicitFlowEnabled")]
        public bool ImplicitFlowEnabled { get; set; }
        [JsonProperty("serviceAccountsEnabled")]
        public bool ServiceAccountsEnabled { get; set; }
        [JsonProperty("defaultRoles")]
        public List<string> DefaultRoles { get; set; }
    }
}
