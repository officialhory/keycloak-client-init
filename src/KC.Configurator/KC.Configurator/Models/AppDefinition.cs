using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Models
{
    class AppDefinition
    {
        [JsonProperty("clientDefinition")]
        public ClientDefinition ClientDefinition { get; set; }
    }
}
