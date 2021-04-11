using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Models
{
    class ProtocolMapperDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("protocolMapper")]
        public string ProtocolMapper { get; set; }

        [JsonProperty("config")]
        public ProtocolMapperConfig ProtocolMapperConfig{ get; set; }
    }

    class ProtocolMapperConfig
    {
        [JsonProperty("multivalued")]
        public bool Multivalued { get; set; }

        [JsonProperty("userinfo.token.claim")]
        public bool UserInfoTokenClaim { get; set; }

        [JsonProperty("id.token.claim")]
        public bool IdTokenClaim{ get; set; }

        [JsonProperty("access.token.claim")]
        public bool AccessTokenClaim { get; set; }

        [JsonProperty("claim.name")]
        public string ClaimName{ get; set; }

        [JsonProperty("jsonType.label")]
        public string JsonTypeLabel{ get; set; }

        [JsonProperty("usermodel.clientRoleMapping.clientId")]
        public string SourceClientId{ get; set; }
    }
}
